# New Safety Help

A mod that allows adding/modifying Entity entries from the game Home Safety Hotline.

# How to Install:
1. Download MelonLoader (Automated Installer) from the official website: [MelonLoader Download](https://melonwiki.xyz/#/?id=requirements)

2. Alternative Download Page: ﻿[MelonLoader Releases](https://github.com/LavaGang/MelonLoader/releases/)

3. Run MelonLoader and select **Home Safety Hotline** as the game. You can choose any version, however note, that it is best to use the latest.

4. Move **NewSafetyHelp.dll** (From ﻿[Releases](https://github.com/Gasterbuzzer/NewSafetyHelp/releases)) to the new **Mods** folder in your Games Directory. You can find your games Mod folder by pressing the folder icon MelonLoader.
(Alternatively: Right mouse click the game in Steam with → Manage → Browse Local Files)

6. Start your game and enjoy.

# Using the Mod
See the Wiki here on GitHub [Link](https://github.com/404)

List of JSON Keys (All values are optional. But if it has (Optional) it means most Entries do not need it or it is handled by the mod itself for you):

## Main Values
 - "monster_name" (Example: "TEST_NAME"): Name of the monster.
 - "monster_description" (Example: "DESCRIPTION"): Description of the monster, allows special formatting.
 - "monster_portrait_image_name" (Example: "image.png"): Name of the image inside the same folder.
 - "monster_audio_clip_name" (Example: "sound.wav"): Name of the sound file inside the same folder.
 - "monster_id" (Optional) (INT (Examples: 0-9999)): ID for the monster, best to leave it empty and let the mod handle it for you. Mostly usefull for replacing entries.

## Phobias
 - "spider_phobia" (Optional) (Examples: true, false): If to hide the image of the entry if spider phobia is enabled.
 - "darkness_phobia" (Optional) (Examples: true, false): If to hide the image of the entry if darkness phobia is enabled.
 - "dog_phobia" (Optional) (Examples: true, false): If to hide the image of the entry if dog phobia is enabled.
 - "holes_phobia" (Optional) (Examples: true, false): If to hide the image of the entry if holes phobia is enabled.
 - "insect_phobia" (Optional) (Examples: true, false): If to hide the image of the entry if insect phobia is enabled.
 - "watching_phobia" (Optional) (Examples: true, false): If to hide the image of the entry if watching phobia is enabled.
 - "tight_space_phobia" (Optional) (Examples: true, false): If to hide the image of the entry if tight spaces phobia is enabled.

## DLC: Seasonal Worker
 - "only_dlc" (Examples: true, false): If to only show in the DLC.
 - "include_dlc" (Examples: true, false): If to **also** include in the DLC.

# Caller Arcade Mode Values (Values of the caller for the Entry)
 - "arcade_calls" (List) (Example: ["Hello, I have this problem...", "Hi, I think I have this problem..."]): List of text that appear when called in Arcade (Training) Mode.
 - "caller_audio_clip_name" (Example: "sound.wav"): Audio played when called for this entry.

# Caller Main Campaing Values (Values of the caller for the Entry)
 - "caller_audio_clip_name" (Example: "sound.wav"): Audio played when called for this entry.
 - "caller_name" (Example: "John Safety"): Name of the caller.
 - "caller_transcript" (Example: "Hello, I need help."): Transcript of the caller in text form.
 - "caller_image_name" (Example: "image.png"): Name of the image for the caller inside the same folder.

## Special Values
 - "replace_entry" (Examples: true, false): Replace Entry instead of adding it.
 - "access_level" (Examples (INT): 0-5): Which access level is needed when playing the main campaign (0 => No permissions needed).
 - "include_campaign" (Examples: true, false): If to include it in the main campaign, has a 20% to replace a call.

# Building
Project can be built with: .NET Framework 4.7.2

Please note, a build script has been added to automatically move the built mod to the games directory, for this to work, you have to provide a textfile named "config.txt" in the top most level. 
It should contain the full absolute path to the games **Mods** folder.
Other than that, enjoy.

# MelonLoader Information
Has been tested with version 0.5.7 and above. As of release, the newest version is 0.6.6. If any versions higher or lower do not work, do not be afraid to contact me.
