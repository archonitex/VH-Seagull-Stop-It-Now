using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace SeagullStopItNow;

[BepInPlugin("com.archonite.seagullstopitnow", "Seagull Stop It Now", "1.0.0")]
public class SeagullPlugin : BaseUnityPlugin
{
    private static SeagullPlugin Instance;
    
    private class SoundEntry
    {
        public string FileName;
        public int Weight;
        public AudioClip Clip;

        public SoundEntry(string fileName, int weight)
        {
            FileName = fileName;
            Weight = weight;
        }
    }

    private static List<SoundEntry> deathSounds = new List<SoundEntry>()
    {
        new SoundEntry("hmhah.wav", 10),
        new SoundEntry("hmhah2.wav", 10),
        new SoundEntry("stopitnow.wav", 10),
        new SoundEntry("stopitnow2.wav", 10),
        new SoundEntry("thatsgood.wav", 2) 
    };
    
    private readonly Harmony harmony = new Harmony("com.archonite.seagullstopitnow");

    private void Awake()
    {
        Instance = this;
        LoadAllAudio();
        harmony.PatchAll();
        Logger.LogInfo("SeagullStopItNow has loaded!");
    }

    public static void LogMessage(string msg)
    {
        if (Instance != null) Instance.Logger.LogInfo(msg);
    }

    private void LoadAllAudio()
    {
        string dir = Path.GetDirectoryName(Info.Location);

        foreach (var sound in deathSounds)
        {
            string audioPath = Path.Combine(dir, sound.FileName);
            sound.Clip = LoadWavFile(audioPath);
            
            if (sound.Clip != null)
            {
                Logger.LogInfo($"Successfully loaded {sound.FileName} (Duration: {sound.Clip.length}s).");
            }
            else
            {
                Logger.LogError($"Failed to load audio from {audioPath}");
            }
        }
    }

    private AudioClip LoadWavFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            LogMessage($"WAV file not found at: {filePath}");
            return null;
        }

        byte[] fileBytes = File.ReadAllBytes(filePath);
        
        if (fileBytes.Length < 44)
        {
            LogMessage($"File too small to be a valid WAV: {filePath}");
            return null;
        }

        int channels = fileBytes[22];
        int sampleRate = BitConverter.ToInt32(fileBytes, 24);
        int bitsPerSample = fileBytes[34];

        int dataIndex = -1;
        for (int i = 12; i < fileBytes.Length - 4; i++)
        {
            if (fileBytes[i] == 'd' && fileBytes[i+1] == 'a' && fileBytes[i+2] == 't' && fileBytes[i+3] == 'a')
            {
                dataIndex = i + 8;
                break;
            }
        }

        if (dataIndex == -1 || dataIndex >= fileBytes.Length)
        {
            LogMessage($"Invalid WAV file (no data chunk found): {filePath}");
            return null;
        }

        int dataSize = BitConverter.ToInt32(fileBytes, dataIndex - 4);
        int bytesPerSample = bitsPerSample / 8;
        if (bytesPerSample <= 0) bytesPerSample = 2;

        int sampleCount = dataSize / bytesPerSample;
        int sampleCountPerChannel = sampleCount / channels;

        float[] audioData = new float[sampleCount];

        if (bitsPerSample == 16)
        {
            for (int i = 0, dest = dataIndex; i < sampleCount && dest + 1 < fileBytes.Length; i++, dest += 2)
            {
                short sampleValue = BitConverter.ToInt16(fileBytes, dest);
                audioData[i] = sampleValue / 32768f;
            }
        }
        else if (bitsPerSample == 8)
        {
            for (int i = 0, dest = dataIndex; i < sampleCount && dest < fileBytes.Length; i++, dest++)
            {
                sbyte sampleValue = (sbyte)fileBytes[dest];
                audioData[i] = sampleValue / 128f;
            }
        }
        else
        {
            LogMessage($"Unsupported bits per sample ({bitsPerSample}) in {filePath}. Must be 16-bit PCM.");
            return null;
        }

        AudioClip clip = AudioClip.Create(Path.GetFileNameWithoutExtension(filePath), sampleCountPerChannel, channels, sampleRate, false);
        clip.SetData(audioData, 0);
        return clip;
    }

    public static void PlayDeathSound(Vector3 position)
    {
        var validSounds = deathSounds.FindAll(s => s.Clip != null);
        
        if (validSounds.Count == 0)
        {
            LogMessage("ERROR: No valid audio clips found to play!");
            return;
        }

        int totalWeight = 0;
        foreach (var s in validSounds)
        {
            totalWeight += s.Weight;
        }

        int randomVal = UnityEngine.Random.Range(0, totalWeight);
        
        AudioClip selectedClip = null;
        
        foreach (var s in validSounds)
        {
            if (randomVal < s.Weight)
            {
                selectedClip = s.Clip;
                break;
            }
            randomVal -= s.Weight;
        }

        if (selectedClip != null)
        {
            LogMessage($"Playing audio clip: {selectedClip.name} at {position}");
            AudioSource.PlayClipAtPoint(selectedClip, position, 2.0f);
        }
    }
}

[HarmonyPatch(typeof(Character), nameof(Character.Damage))]
public class Character_Damage_Patch
{
    static void Prefix(Character __instance, HitData hit, out bool __state)
    {
        __state = __instance.GetHealth() > 0f;
    }

    static void Postfix(Character __instance, HitData hit, bool __state)
    {
        if (__state && __instance.GetHealth() <= 0f)
        {
            string objName = __instance.gameObject.name;
            if (objName.Contains("Seagal"))
            {
                Character attacker = hit.GetAttacker();
                if (attacker != null && attacker == Player.m_localPlayer)
                {
                    SeagullPlugin.LogMessage("Seagull killed via Character patch!");
                    //SeagullPlugin.PlayDeathSound(__instance.transform.position);
                    SeagullPlugin.PlayDeathSound(Player.m_localPlayer.transform.position);
                }
            }
        }
    }
}

[HarmonyPatch(typeof(Destructible), nameof(Destructible.Damage))]
public class Destructible_Damage_Patch
{
    static void Postfix(Destructible __instance, HitData hit)
    {
        if (__instance != null)
        {
            string objName = __instance.gameObject.name;
            
            if (objName.Contains("Seagal"))
            {
                SeagullPlugin.LogMessage($"Destructible Seagal hit/destroyed: {objName}");
                
                if (hit != null)
                {
                    Character attacker = hit.GetAttacker();
                    if (attacker != null && attacker == Player.m_localPlayer)
                    {
                        SeagullPlugin.LogMessage("Local player killed the seagull via Destructible patch!");
                        //SeagullPlugin.PlayDeathSound(__instance.transform.position); //This plays the sound at the Seagull's position, which may be far away from the player. Consider using the player's position instead.
                        SeagullPlugin.PlayDeathSound(Player.m_localPlayer.transform.position);
                    }
                }
            }
        }
    }
}