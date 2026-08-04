# Seagull Stop It Now

A Valheim mod that rewards your archery skills by playing a randomized clip from the famous 'Seagulls - Stop It Now' by Bad Lip Reading.

## Features
* **Kill Confirmed:** Instantly triggers an audio clip when you land the killing blow on a seagull (works with bows, magic, or melee).
* **Distance Independent:** The audio plays at the player's location, meaning you will hear it loud and clear even if you snipe a bird from 50 meters away.
* **Weighted Randomizer:** Includes 5 distinct audio clips. Four of them play commonly, while one special clip (`thatsgood.wav`) acts as a "rare" drop that plays less frequently.

## Installation (Mod Manager - Recommended)
1. Ensure you have [r2modman](https://valheim.thunderstore.io/package/ebkr/r2modman/) installed.
2. Click **Install with Mod Manager** at the top of this page.
3. Launch the game!

## Installation (Manual)
1. Ensure you have [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) installed.
2. Download the latest `.zip` file for this mod.
3. Extract the contents of the archive.
4. Move the entire `SeagullStopItNow` folder (which contains the `.dll` and the five `.wav` files) into your `Valheim/BepInEx/plugins/` directory.

## Customization
If you want to use your own sounds in the future, you can simply replace the included `.wav` files inside the `plugins/SeagullStopItNow/` folder with your own. Ensure they share the exact same filenames (`hmhah.wav`, `stopitnow.wav`, etc.) and are saved in standard **16-bit PCM WAV** format.

## Changelog
* **v1.0.0**
  * Initial release.
* **v1.0.2**
  * Added very low chance that the full song plays
* **v1.0.3**
* **v1.0.4**
  * Added support for multiplayer. The sound will now play at the location of the player that killed the Seagull.
* **v1.0.5**
  * Moved multiplayer support to RPC call as previous solution was not working. For sound to play from other player's kill, they must also have the mod. The sound will play at the killer's location.