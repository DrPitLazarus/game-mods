# CorpsePartyBloodDrive_BuffKit

My mod for Corpse Party: Blood Drive. 

A collection of small quality-of-life improvements and gameplay tweaks.

## Download
Visit the [release page](https://github.com/DrPitLazarus/game-mods/releases/tag/CorpsePartyBloodDrive_BuffKit@0.3.0). Current version is 0.3.0.

<details>
<summary>Older Versions</summary>

- [0.2.1](https://github.com/DrPitLazarus/game-mods/releases/tag/CorpsePartyBloodDrive_BuffKit@0.2.1)
- [0.2.0](https://github.com/DrPitLazarus/game-mods/releases/tag/CorpsePartyBloodDrive_BuffKit@0.2.0)
- [0.1.0](https://github.com/DrPitLazarus/game-mods/releases/tag/CorpsePartyBloodDrive_BuffKit@0.1.0)
</details>  

## Features
Uses BepInEx Configuration Manager. Press F1 in-game to open the configuration menu.

- **StartupSkipBootLogos** [Enabled by default] 
  - Skips startup logos when launching the game.
  - Default is the mod setting.
- **StartupStopTitleScreenLoop** [Enabled by default] 
  - Prevents the title screen (press any button) from looping back to the logos after 30 seconds.
  - Default is the mod setting.
- **SaveMenuMaxSlots** [50 by default] 
  - Number of save slots to display. Default is 50. Vanilla is 20. Recommend not going over 100 saves, but I won't stop you...
  - Default is the mod setting.
  - Settings: 1-1,000.
- **SaveMenuShowSlotNumbers** [Enabled by default] 
  - Shows slot numbers in the save menu.
  - Default is the mod setting.
- **SaveMenuShowTimeAgo** [Enabled by default]
  - Shows how long ago each save was made in the save menu. Example: 2d 0h 24m ago.
  - Default is the mod setting.
- **SaveMenuSortBy** [Date Newest by default]
  - Changes the sort order of save slots in the save menu. Sort by date will put empty slots at the bottom.
  - Default is the mod setting.
  - Settings: Date Newest, Date Oldest, Slot Number (Vanilla), Slot Number Largest.
- **TextSpeed** [1x by default]
  - Changes the speed that text is displayed. Disable TextVoiceSync to apply to voiced text. 
  - Default is the vanilla setting.
  - Settings: 1x, 2x, 4x, 8x, 16x, Instant.
- **TextVoiceSync** [Enabled by default]
  - Text speed is synced to voice audio length if voiced. Otherwise, TextSpeed is used. Disable to use TextSpeed for voiced lines. 
  - Default is the vanilla setting.
- **GeneralToggleUI** [F2 by default]
  - Key bind to toggle most UI elements so you can look at CGs without obstruction. Works in-game and in the Gallery of Spirits.
  - Default is the mod setting.
- **GameplayInfiniteBandage** [Disabled by default]
  - Always have a bandage in your inventory.
  - Default is the vanilla setting.
- **GameplayInfiniteStamina** [Disabled by default] 
  - Player stamina will not decrease. Run, Rabbit, Run!
  - Default is the vanilla setting.
- **GameplayInfiniteTalisman** [Disabled by default] 
  - Always have a talisman in your inventory.
  - Default is the vanilla setting.
- **ToolsOpenDataDirectory**
  - Opens game data directory. `%UserProfile%\AppData\LocalLow\XSEED\Corpse Party_ Blood Drive\`
- **ToolsOpenGameDirectory**
  - Opens game install directory.
- **ToolsOpenGitHubPage**
  - Opens the GitHub page for this mod in your browser.
