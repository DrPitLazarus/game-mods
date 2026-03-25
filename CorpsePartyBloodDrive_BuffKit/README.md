# CorpsePartyBloodDrive_BuffKit

My mod for Corpse Party: Blood Drive. 

A collection of small quality-of-life improvements and gameplay tweaks. Made for a friend who introduced me to the Corpse Party series.

## Download
Visit the [release page](https://github.com/DrPitLazarus/game-mods/releases/tag/CorpsePartyBloodDrive_BuffKit@0.4.0). Current version is 0.4.0.

<details>
<summary>Older Versions</summary>

- [0.3.0](https://github.com/DrPitLazarus/game-mods/releases/tag/CorpsePartyBloodDrive_BuffKit@0.3.0)
- [0.2.1](https://github.com/DrPitLazarus/game-mods/releases/tag/CorpsePartyBloodDrive_BuffKit@0.2.1)
- [0.2.0](https://github.com/DrPitLazarus/game-mods/releases/tag/CorpsePartyBloodDrive_BuffKit@0.2.0)
- [0.1.0](https://github.com/DrPitLazarus/game-mods/releases/tag/CorpsePartyBloodDrive_BuffKit@0.1.0)
</details>  

## Features
Press F10 in-game to open the mod settings menu.

:star: denotes mod enhancements enabled by default.

### General
- **ToggleModMenuUI** :star: [F10 by default]
  - Key bind to toggle the mod settings menu.
- **ToggleSkip** :star: [K by default]
  - Key bind to toggle skip dialogue. Displays text overlay in the bottom-right when active.
- **ToggleUI** :star: [H by default]
  - Key bind to toggle most UI elements so you can look at CGs without obstruction. Works in-game and in the Gallery of Spirits.
- **VSync** :star: [Disabled by default] 
  - Use your display's refresh rate. Vanilla is enabled by default.
- **FramerateLimit60** :star: [Enabled by default] 
  - Limit framerate to 60 FPS when VSync is disabled. Otherwise unlimited.
- **HideRefreshRate** :star: [Enabled by default] 
  - Removes the refresh rate from the resolution text as it is not used in the game and just takes up space. Example: 1920x1080@360 becomes 1920x1080.
### Save Menu
- **MaxSlots** :star: [50 by default] 
  - Number of save slots to display. Default is 50. Vanilla is 20. Recommend not going over 100 saves, but I won't stop you... Increases time to load the save menu.  
	Fewer save slots won't delete data and only displays slots up to it.
  - Settings: 1-1,000.
- **ShowSlotNumbers** :star: [Enabled by default] 
  - Shows slot numbers in the save menu.
- **ShowTimeAgo** :star: [Enabled by default]
  - Shows how long ago each save was made in the save menu. Example: 2d 0h 24m ago.
- **SortBy** :star: [Date Newest by default]
  - Changes the sort order of save slots in the save menu. Sort by date will put empty slots at the bottom. 
  - When Saving, the first empty save slot is moved to the top. 
  - When Loading, empty save slots are hidden.
  - Settings: Date Newest, Date Oldest, Slot Number (Vanilla), Slot Number Largest.
### Startup
- **SkipBootLogos** :star: [Enabled by default] 
  - Skips startup logos when launching the game.
- **StopTitleScreenLoop** :star: [Enabled by default] 
  - Prevents the title screen (press any button) from looping back to the logos after 30 seconds.
### Text
- **Speed** [1x by default]
  - Changes the speed that text is displayed. Disable TextVoiceSync to apply to voiced text. 
  - Settings: 1x, 2x, 4x, 8x, 16x, Instant.
- **VoiceSync** :star: [Disabled by default]
  - Text speed is synced to voice audio length if voiced. Otherwise, TextSpeed is used. Disable to use TextSpeed for voiced lines. Vanilla is enabled by default.
### Gameplay
- **AlwaysRun** [Disabled by default] 
  - Run by default. Use run key to walk.
- **InfiniteBandage** [Disabled by default]
  - Always have a bandage in your inventory.
- **InfiniteBatteryItem** [Disabled by default] 
  - Always have a battery in your inventory. For those who want the thrilling and immersive experience of manually reloading your flashlight battery instead of the vanilla infinite battery... weirdo. This doesn't toggle the vanilla infinite battery option.
- **InfiniteStamina** [Disabled by default] 
  - Player stamina will not decrease. Run, Rabbit, Run!
- **InfiniteTalisman** [Disabled by default] 
  - Always have a talisman in your inventory.
### Tools
- **OpenDataDirectory**
  - Opens game data directory. `%UserProfile%\AppData\LocalLow\XSEED\Corpse Party_ Blood Drive\`
- **OpenGameDirectory**
  - Opens game install directory.
- **OpenOutputLog**
  - Opens the Unity output log file. `%UserProfile%\AppData\LocalLow\XSEED\Corpse Party_ Blood Drive\output_log.txt`
- **OpenGitHubPage**
  - Opens the GitHub page for this mod in your browser.

## Notes

Other features that could be added, but currently do not have time to implement:
- AutoSaveSystemData: Automatically press yes to save system data when prompted. Had bugs but currently don't have time to investigate.
- Add Delete and Edit Location Text options to the save/load menu.
- Ability to open the ESC menu while in cutscenes and dialogue.
- Ability to save the game at any time.
- Chibi model and animation viewer would be pretty cool.

![ModMenuScreenshots](https://github.com/user-attachments/assets/bf0ee022-264b-4c96-a557-76c9412054ce)
