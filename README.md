![Version](https://img.shields.io/github/v/release/Flazhik/Jukebox)
![Licence](https://img.shields.io/github/license/Flazhik/Jukebox)

# Jukebox / Cybergrind Music Explorer

### Enhanced music browser and player for Cyber Grind

![Header](https://i.imgur.com/rg17BUp.png)

#### _This mod doesn't affect leaderboards and doesn't alter your gameplay: your score will be saved_

- [Installation](#installation)
- [Basics](#basics)
- [Mod settings](#settings)
- Features
    - [Playback menu](#playback-menu)
    - [Current track HUD](#current-track-hud)
    - [Switching playlists](#switching-playlists)
    - [Clear playlist / Load all tracks from a folder](#clear-playlist--load-all-tracks-from-a-folder)
    - [Effects replacement](#effects-replacement)
    - [Calm & Battle themes](#calm-and-battle-themes)
    - [Segmented tracks](#segmented-tracks)
- [Additional credits](#additional-credits)

# Installation

1. Download the **BepInEx** release from [here](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21). It's
   recommended to choose **BepInEx_x64** release unless you're certain you have a 32-bit system
2. Extract the contents of **BepInEx** archive in your local **ULTRAKILL** folder. If you're not sure where this folder
   is located, find **ULTRAKILL** in your Steam Library > Right mouse click > **Properties** > **Local files** > **Browse**
3. Download the Jukebox
   archive [here](https://github.com/Flazhik/Jukebox/releases/download/v2.0.0/Jukebox.v2.0.0.zip), then
   extract its contents at **ULTRAKILL/BepInEx/plugins** (create *plugins* folder manually in case it's missing)

You can also use r2modman for that
## Basics

All the basic functionality is available by pressing **F4** while in Cyber Grind and ~ while in arena.

## Settings
![Jukebox Menu](https://i.imgur.com/z9lhr0h.png)

All the settings for the mod are available by pressing Menu hotkey (F4 by default). The main screen of the menu contains the following options:

**Playback**

1. **Show current track panel indefinitely**: once checked, this option will make the "Now playing" panel to be present the whole time the track is playing
2. **Custom tracks volume boost**: if your track is still too quiet, you can always add up to 10dB to it. Please use it carefully
3. **Always play an intro part**: intro section of the song is always going to be played despite the track position in the playlist
4. **Discord & Steam integration**: shows the current track name alongside wave number in Discord and Steam

**Playlist editing**

1. **Prevent duplicate tracks**: prevents tracks duplication. Once checked deletes existing duplicates
2. **Enable tracks preview**: allows to listen to the first 5 seconds of a track by clicking on it in the terminal

For other settings like key bindings and Themes, use the respective tab.

## Playback menu
![Playback menu](https://i.imgur.com/1qaFgPT.png)

Allows you to switch between the tracks mid-game. Available by pressing **~** by default.

If the whole menu is an overkill for you, just switch between tracks using **Next track** hotkey.

## Current track HUD
![Current track HUD](https://i.imgur.com/jf79lzj.png)

A HUD element that shows currently played track's info. Available for all the HUD variants.

## Switching playlists

![Playlist selection](https://i.imgur.com/hIZdvai.png)

This feature allows you to switch between up to 6 different playlists

## Clear playlist / Load all tracks from a folder

Allows to wipe the current playlist or add all the tracks from the currently selected folder simultaneously.

## Effects replacement

![Effects replacement](https://i.imgur.com/Y3Q2Dmg.png)

A new Cybergrind config menu called "Sound Effects" is added into the Terminal which, obviously enough, allows you to replace a certain sound effects.

Please remember the files used for sound effects must be placed at `ULTRAKILL/Cybergrind/Effects`

## Calm and battle themes

You know how in original ULTRAKILL campaign there's a light, calm theme playing when there are no enemies around, followed by an intense variation once the battle has started?
Now you can enable it in Cyber Grind for original ULTRAKILL soundtrack and create your own tracks.


But unlike the campaign mode, you can decide when to play the calm theme by yourself (otherwise it would've been stupid since enemies are almost always present in Cyber Grind).
Go to <b>Themes</b> tab and you'll see three controls:

1. **Enable calm themes**: disable if you're not interested in this feature
2. **Enemies threshold for calm theme to play**: the battle theme will play unless there's less or equal enemies currently alive
3. **Play battle theme when there's <= N of these enemies around** enemies threshold may be a reliable way to control when the calm theme is playing, but what if, say, these two enemies who's left are Mindflayer and Insurrectionist? Is it really a good occasion to switch to the calm theme? That's why you can change a threshold for each enemy individually or disable it.

![Example of the calm theme settings](https://i.imgur.com/W65jDjk.png)

_"Play calm theme when there's 4 or less enemies around. Unless there's more that one Insurrectionist or more than 2 Mindflayers"_

Alright, but what about custom tracks?

Well, to create such a track, use the same technique as for intros and loops: postfixes in your track file name.
Any track is treated as a "battle theme" by default. To add its calm variation, add another, similarly named track with **_calm** postfix. Again, these files must have the same extension!

And what about segmented tracks? They already contain two files! How do I even make a calm variation to its intro and loop? Can I add a calm theme only to loop or only to intro specifically?
Yes, you can. Customize it the way you want. Calm loop part can either have a **_calm** or **_calmloop** postfix, but intro has to end with **_calmintro**.
More on that in the Manual section of the menu.

## Segmented tracks
You can also add multi-segmented looped tracks with an intro and loop parts.
In order to make such a track, place your intro and loop files in the same folder and add _intro and _loop postfixes into their file names respectively.
Please note that these parts must have the same extension!

![Looped track example](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/204979ac-2fe7-44b8-9c99-1892610a98b8)

Like this.

If everything's been done correctly, it now should be displayed as a single track.

## Additional Credits:

Created by [Flazhik](https://github.com/Flazhik)

Special thanks to [eternalUnion](https://github.com/eternalUnion) for VanityReprised

#### Jukebox uses following libraries:
- [TagLibSharp](https://github.com/mono/taglib-sharp) - Licensed under [LGPL-2.1](https://github.com/mono/taglib-sharp/blob/main/COPYING)