using Adventure;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Haunted;
using Scene.Map;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BuffKit
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    internal class BuffKit : BaseUnityPlugin
    {
        public static ConfigEntry<bool> StartupSkipBootLogos;
        public static ConfigEntry<bool> StartupStopTitleScreenLoop;
        public static ConfigEntry<int> SaveMenuMaxSlots;
        public static ConfigEntry<bool> SaveMenuShowSlotNumbers;
        public static ConfigEntry<bool> SaveMenuShowTimeAgo;
        public static ConfigEntry<SaveMenuSortByEnum> SaveMenuSortBy;
        public static ConfigEntry<TextSpeedEnum> TextSpeed;
        public static ConfigEntry<bool> TextVoiceSync;
        public static ConfigEntry<KeyboardShortcut> GeneralToggleUI;
        public static ConfigEntry<bool> GameplayInfiniteBandage;
        public static ConfigEntry<bool> GameplayInfiniteStamina;
        public static ConfigEntry<bool> GameplayInfiniteTalisman;
        public static ConfigEntry<bool> ToolsOpenDataDirectory;
        public static ConfigEntry<bool> ToolsOpenGameDirectory;
        public static ConfigEntry<bool> ToolsOpenGitHubPage;
        public static GameObject GameObject;
        private static readonly string _defaultSettingIsVanilla = "Default is the vanilla setting.";
        private static readonly string _defaultSettingIsMod = "Default is the mod setting.";
        private static readonly string _gitHubUrl = "https://github.com/DrPitLazarus/game-mods/tree/master/CorpsePartyBloodDrive_BuffKit#readme";
        private Harmony _harmony;

        private void Awake()
        {
            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            _harmony.PatchAll();
            GameObject = gameObject;
            Log($"{PluginInfo.PLUGIN_NAME} loaded!");

            GameObject.AddComponent<GameplayInventory_Patch>();

            SetupSettings();
        }

        private void SetupSettings()
        {
            StartupSkipBootLogos = Config.Bind("Startup", "SkipBootLogos", true,
                $"Skips the boot logos and go directly to the title screen. \n\n{_defaultSettingIsMod}");
            StartupStopTitleScreenLoop = Config.Bind("Startup", "StopTitleScreenLoop", true,
                $"Prevents the title screen (press any button) from looping back to the logos after 30 seconds. \n\n{_defaultSettingIsMod}");
            SaveMenuMaxSlots = Config.Bind("SaveMenu", "MaxSlots", 50, new ConfigDescription(
                $"Number of save slots to display. Default is 50. Vanilla is 20. Recommend not going over 100 saves, but I won't stop you... \n\n{_defaultSettingIsMod}",
                new AcceptableValueRange<int>(1, 1000)));
            SaveMenuShowSlotNumbers = Config.Bind("SaveMenu", "ShowSlotNumbers", true,
                $"Shows the slot numbers in the save menu. \n\n{_defaultSettingIsMod}");
            SaveMenuShowTimeAgo = Config.Bind("SaveMenu", "ShowTimeAgo", true,
                $"Shows how long ago each save was made in the save menu. Example: 2d 0h 24m ago. \n\n{_defaultSettingIsMod}");
            SaveMenuSortBy = Config.Bind("SaveMenu", "SortBy", SaveMenuSortByEnum.DateTimeDescending,
                $"Changes the sort order of save slots in the save menu. Sort by date will put empty slots at the bottom. \n\n{_defaultSettingIsMod}");
            TextSpeed = Config.Bind("Text", "Speed", TextSpeedEnum._1x,
                $"Changes the speed that text is displayed. Disable TextVoiceSync to apply to voiced text. \n\n{_defaultSettingIsVanilla}");
            TextSpeed.SettingChanged += (_, _) => Lua_Patch.ApplyTextSpeedSetting();
            TextVoiceSync = Config.Bind("Text", "VoiceSync", true,
                $"Text speed is synced to voice audio length if voiced. Otherwise, TextSpeed is used. Disable to use TextSpeed for voiced lines. \n\n{_defaultSettingIsVanilla}");
            TextVoiceSync.SettingChanged += (_, _) => Lua_Patch.ApplyVoiceSyncSetting();
            GeneralToggleUI = Config.Bind("General", "ToggleUI", new KeyboardShortcut(KeyCode.F2),
                $"Key bind to toggle most UI elements so you can look at CGs without obstruction. Works in-game and in the Gallery of Spirits. \n\n{_defaultSettingIsMod}");
            GameplayInfiniteBandage = Config.Bind("Gameplay", "InfiniteBandage", false,
                $"Always have a bandage in your inventory. \n\n{_defaultSettingIsVanilla}");
            GameplayInfiniteStamina = Config.Bind("Gameplay", "InfiniteStamina", false,
                $"Player stamina will not decrease. Run, Rabbit, Run! \n\n{_defaultSettingIsVanilla}");
            GameplayInfiniteTalisman = Config.Bind("Gameplay", "InfiniteTalisman", false,
                $"Always have a talisman in your inventory. \n\n{_defaultSettingIsVanilla}");
            ToolsOpenDataDirectory = Config.Bind("Tools", "OpenDataDirectory", false,
                "Click to open game data directory.");
            ToolsOpenDataDirectory.SettingChanged += (_, _) =>
            {
                if (!ToolsOpenDataDirectory.Value) return; // Only act when set to true.
                ToolsOpenDataDirectory.Value = false; // Reset to false.
                Application.OpenURL(Application.persistentDataPath);
            };
            ToolsOpenGameDirectory = Config.Bind("Tools", "OpenGameDirectory", false,
                "Click to open game install directory.");
            ToolsOpenGameDirectory.SettingChanged += (_, _) =>
            {
                if (!ToolsOpenGameDirectory.Value) return; // Only act when set to true.
                ToolsOpenGameDirectory.Value = false; // Reset to false.
                Application.OpenURL(Path.GetDirectoryName(Application.dataPath));
            };
            ToolsOpenGitHubPage = Config.Bind("Tools", "OpenGitHubPage", false,
                "Click to open the GitHub page for this mod in your browser.");
            ToolsOpenGitHubPage.SettingChanged += (_, _) =>
            {
                if (!ToolsOpenGitHubPage.Value) return; // Only act when set to true.
                ToolsOpenGitHubPage.Value = false; // Reset to false.
                Application.OpenURL(_gitHubUrl);
            };
        }

        /// <summary>
        /// Options for sorting save slots in the save menu.
        /// </summary>
        public enum SaveMenuSortByEnum
        {
            [Description("Date Newest (Default)")]
            DateTimeDescending,
            [Description("Date Oldest")]
            DateTimeAscending,
            [Description("Slot Number (Vanilla)")]
            SlotNumberAscending,
            [Description("Slot Number Largest")]
            SlotNumberDescending,
        }

        /// <summary>
        /// Options for text speed of the message window.
        /// </summary>
        public enum TextSpeedEnum
        {
            [Description("1x")]
            _1x,
            [Description("2x")]
            _2x,
            [Description("4x")]
            _4x,
            [Description("8x")]
            _8x,
            [Description("16x")]
            _16x,
            Instant,
        }

        /// <summary>
        /// Maps TextSpeedEnum values to duration to print each character.
        /// </summary>
        public static Dictionary<TextSpeedEnum, float> TextSpeedValues = new()
        {
            { TextSpeedEnum._1x, 0.05f },
            { TextSpeedEnum._2x, 0.025f },
            { TextSpeedEnum._4x, 0.0125f },
            { TextSpeedEnum._8x, 0.00625f },
            { TextSpeedEnum._16x, 0.003125f },
            { TextSpeedEnum.Instant, 0.0001f },
        };

        /// <summary>
        /// Takes a past DateTime and returns a string representing the time elapsed since then in days, hours, and minutes.
        /// </summary>
        /// <param name="past"></param>
        public static string GetTimeAgo(DateTime past)
        {
            var now = DateTime.Now;
            var difference = now - past;
            if (difference.TotalDays >= 1)
            {
                return $"{difference.Days}d {difference.Hours}h {difference.Minutes}m";
            }
            else if (difference.TotalHours >= 1)
            {
                return $"{difference.Hours}h {difference.Minutes}m";
            }
            else
            {
                return $"{difference.Minutes}m";
            }
        }

        public static void Log(string message)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(1); // Get the calling method frame
            var method = frame.GetMethod();
            var classFullName = method.DeclaringType.FullName;

            UnityEngine.Debug.Log($"{timestamp} [INFO] {classFullName} - {message}");
        }
    }


    [HarmonyPatch]
    internal class Startup_Patch
    {
        /// <summary>
        /// On the logo scene, if enabled, skip it and go straight to the title screen (press any button).
        /// </summary>
        [HarmonyPatch(typeof(LogoControl), nameof(LogoControl.Start))]
        [HarmonyPrefix]
        private static bool Start(LogoControl __instance)
        {
            if (!BuffKit.StartupSkipBootLogos.Value) return true; // Don't skip, run original method.
            BuffKit.Log("Skipping boot logos...");
            // Run replacement coroutine method.
            __instance.StartCoroutine(Start_Patch(__instance));
            return false; // Skip original method.
        }

        /// <summary>
        /// Replacement coroutine method. See above Start() method.
        /// </summary>
        private static IEnumerator Start_Patch(LogoControl __instance)
        {
            // From LogoControl.Start():
            for (int i = 0; i < __instance.logoGraphics.Count; i++)
            {
                __instance.logoGraphics[i].gameObject.SetActive(false);
                __instance.logoGraphics[i].color = Color.white;
            }
            yield return new WaitForSeconds(0.001f); // Need to wait a moment or it gets stuck here.
            Common.Instance().LoadLevel("title", LoadSceneMode.Single);
        }

        /// <summary>
        /// On the title screen (press any button), if enabled, prevents loading the logo scene after 30 seconds.
        /// </summary>
        [HarmonyPatch(typeof(Scene.Title.StatControl), nameof(Scene.Title.StatControl.Update))]
        [HarmonyPrefix]
        private static bool Update(Scene.Title.StatControl __instance)
        {
            if (!BuffKit.StartupStopTitleScreenLoop.Value) return true; // Don't modify, run original method.
            // Original method:
            __instance.get_time += Time.deltaTime;
            List<VitaButtonInput.RewiredAction> list =
            [
                VitaButtonInput.RewiredAction.MenuHorizontal,
                VitaButtonInput.RewiredAction.MenuVertical,
                VitaButtonInput.RewiredAction.MoveHorizontal,
                VitaButtonInput.RewiredAction.MoveVertical,
                VitaButtonInput.RewiredAction.LookHorizontal,
                VitaButtonInput.RewiredAction.LookVertical
            ];
            if ((VitaButtonInput.Instance.IsAnyActionOnce && !VitaButtonInput.Instance.IsAnyActionsFromList(list)) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                __instance.AccessChapter();
            }
            // MODIFIED: Removed timer that would load the logo scene after some time.
            return false; // Skip original method.
        }
    }


    [HarmonyPatch]
    internal class SaveMenu_Patch
    {
        private static int _newMaxSaveSlots;

        /// <summary>
        /// Stub to call the original Menu.OnEnable() method.
        /// </summary>
        /// <param name="instance"></param>
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Menu), nameof(Menu.OnEnable))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void BaseOnEnable(Menu instance) { }

        /// <summary>
        /// Replacement method for SaveMenu.OnEnable() to allow more save slots and other modifications.
        /// </summary>
        [HarmonyPatch(typeof(SaveMenu), nameof(SaveMenu.OnEnable))]
        [HarmonyPrefix]
        private static bool OnEnable(SaveMenu __instance)
        {
            _newMaxSaveSlots = BuffKit.SaveMenuMaxSlots.Value;
            __instance.saveSlots = new SaveSlot[_newMaxSaveSlots];
            var showTimeAgo = BuffKit.SaveMenuShowTimeAgo.Value; // Cache config value.
            Dictionary<int, DateTime> slotDateTimes = [];
            for (int index = 0; index < _newMaxSaveSlots; index++)
            {
                slotDateTimes.Add(index, DateTime.MinValue); // Initialize all to MinValue.
            }
            BuffKit.Log($"_newMaxSaveSlots: {_newMaxSaveSlots}, saveSlots: {__instance.saveSlots.Length}");

            // Original method:
            BaseOnEnable(__instance); // Call base.OnEnable().
            string text = Application.persistentDataPath + "/Saves/slot";
            AssetBundleManager.Instance.LoadBundle("Bundles/textures_saveicon");
            for (int i = 0; i < _newMaxSaveSlots; i++) // MODIFIED LINE: Use new max save slots.
            {
                SaveSlot saveSlot = __instance.selectableList.InstantiateChild<SaveSlot>(__instance.saveSlotPrefab);
                saveSlot.name = "SaveSlot" + (i + 1);
                saveSlot.ListIndex = i;
                if (i == 29) // MODIFIED: Slot 30 is the system save slot. Do not allow selecting it.
                {
                    saveSlot.gameObject.SetActive(false);
                }
                else
                {
                    saveSlot.OnClickWithInt.AddListener(new UnityAction<int>(__instance.OpenConfirmationDialog));
                    saveSlot.onClick.AddListener(new UnityAction(__instance.PlayMenuDecideAudio));
                    saveSlot.UseOnSelectEvent = true;
                    saveSlot.OnSelectEvent.AddListener(new UnityAction(__instance.PlayMenuSelectAudio));
                }
                __instance.saveSlots[i] = saveSlot;
                string text2 = string.Concat(
                [
                text,
                i + 1,
                "/saveheader",
                i + 1,
                ".dat"
                ]);
                var showSlotNumbers = BuffKit.SaveMenuShowSlotNumbers.Value; // MODIFIED: Cache config value.
                if (FileSystem.Exists(text2, true))
                {
                    string[] array = File.ReadAllLines(text2);
                    saveSlot.ChapterTitle.Text = array[0];
                    if (showSlotNumbers)
                    {
                        saveSlot.ChapterTitle.Text += $" - SLOT {i + 1}"; // MODIFIED: Show slot number on active slots.
                    }
                    saveSlot.LocationText.Text = TranslatorService.Instance.GetText(array[1]);
                    saveSlot.DateAndTime.Text = array[2];
                    if (DateTime.TryParse(array[2], out var parsedDateTime))
                    {
                        slotDateTimes[saveSlot.ListIndex] = parsedDateTime; // MODIFIED: Store date time for sorting.
                        if (showTimeAgo)
                        {
                            saveSlot.DateAndTime.Text += $" ({BuffKit.GetTimeAgo(parsedDateTime)} ago)"; // MODIFIED: Show time ago.
                        }
                    }
                    saveSlot.Thumbnail.sprite = AssetBundleManager.Instance.LoadSprite("save_icon/" + array[3] + "_EN");
                    saveSlot.IsEmpty = false;
                }
                else
                {
                    saveSlot.ChapterTitle.Text = TranslatorService.Instance.GetText("savemenu_emptysaveslot");
                    if (showSlotNumbers)
                    {
                        saveSlot.ChapterTitle.Text += $" {i + 1}"; // MODIFIED: Show slot number on empty slots.
                    }
                    saveSlot.LocationText.Text = string.Empty;
                    saveSlot.DateAndTime.Text = string.Empty;
                    saveSlot.Thumbnail.sprite = AssetBundleManager.Instance.LoadSprite("save_icon/Empty");
                }
            }

            __instance.saveSlots = BuffKit.SaveMenuSortBy.Value switch // MODIFIED: Sort save slots based on config option. Empty slots always at bottom for date sorts.
            {
                BuffKit.SaveMenuSortByEnum.DateTimeDescending => [.. __instance.saveSlots.OrderBy(slot => slot.IsEmpty).ThenByDescending(slot => slotDateTimes[slot.ListIndex].Ticks)],
                BuffKit.SaveMenuSortByEnum.DateTimeAscending => [.. __instance.saveSlots.OrderBy(slot => slot.IsEmpty).ThenBy(slot => slotDateTimes[slot.ListIndex].Ticks)],
                BuffKit.SaveMenuSortByEnum.SlotNumberAscending => [.. __instance.saveSlots.OrderBy(slot => slot.ListIndex)],
                BuffKit.SaveMenuSortByEnum.SlotNumberDescending => [.. __instance.saveSlots.OrderByDescending(slot => slot.ListIndex)],
                _ => [.. __instance.saveSlots.OrderBy(slot => slot.IsEmpty).ThenByDescending(slot => slotDateTimes[slot.ListIndex].Ticks)],
            };

            foreach (var slot in __instance.saveSlots) // MODIFIED: Reorder the slots in the UI.
            {
                slot.transform.SetAsLastSibling();
            }

            __instance.selectableList.GetSelectableChildren();
            SaveMenu.MenuState currentState = __instance.CurrentState;
            if (currentState != SaveMenu.MenuState.Load)
            {
                if (currentState == SaveMenu.MenuState.Save)
                {
                    __instance.Title.sprite = __instance.SaveTitleSprite;
                    __instance.ConfirmationDescriptionText.SetTextWithLocalizationKey("savemenu_confirmation_description");
                    __instance.ConfirmButtonText.SetTextWithLocalizationKey("savemenu_overridesave");
                    __instance.CancelButtonText.SetTextWithLocalizationKey("settingsmenu_cancel");
                }
            }
            else
            {
                __instance.Title.sprite = __instance.loadTitleSprite;
                __instance.ConfirmationDescriptionText.SetTextWithLocalizationKey("loadmenu_confirmation_description");
                __instance.ConfirmButtonText.SetTextWithLocalizationKey("_yes");
                __instance.CancelButtonText.SetTextWithLocalizationKey("_no");
            }
            __instance.imageChildren = [.. __instance.GetComponentsInChildren<Image>()];
            __instance.textChildren = [.. __instance.GetComponentsInChildren<TextMeshProUGUI>()];
            __instance.StartCoroutine(__instance.SaveMenuFadeIn());
            return false; // Skip original method.
        }
    }


    [HarmonyPatch]
    internal class ToggleUI_Patch
    {
        private static bool _initialized = false;
        private static bool _shouldShowUi = true;
        private static List<GameObject> _objectsToToggle = [];
        private static List<MonoBehaviour> _componentsToToggle = [];
        private static Image _messageWindowImage;
        private static TextMeshProUGUI _messageWindowText;
        private static TextMeshProUGUI _nameWindowText;
        private static GameObject _messageWindowNextIcon;
        private static GameObject _bustUp;
        private static GameObject _mapNameWindow;
        private static GameObject _choiceWindow;
        private static TextMeshProUGUI _choiceWindowText1;
        private static GameObject _galleryBackButton;
        private static GameObject _gallerySwitchButton;

        /// <summary>
        /// Main initialization patch to get references to UI elements to toggle on/off.
        /// </summary>
        [HarmonyPatch(typeof(AdventureSystem), nameof(AdventureSystem.Start))]
        [HarmonyPostfix]
        private static void AdventureSystem_Start()
        {
            BuffKit.Log("Initializing AdventureSystem_Start()...");
            _objectsToToggle = [
                _messageWindowNextIcon = GameObject.Find("/AdventureSystem/Canvas/MessageWindow/NextIcon"),
                _bustUp = GameObject.Find("/AdventureSystem/Canvas/BustUp"),
                _mapNameWindow = GameObject.Find("/AdventureSystem/Canvas/MapName/Window"),
                _choiceWindow = GameObject.Find("/AdventureSystem/Canvas/ChoiceWindow"),
            ];
            _componentsToToggle = [
                _messageWindowImage = GameObject.Find("/AdventureSystem/Canvas/MessageWindow")?.GetComponent<Image>(),
                _messageWindowText = GameObject.Find("/AdventureSystem/Canvas/MessageWindow/Message")?.GetComponent<TextMeshProUGUI>(),
                _nameWindowText = GameObject.Find("/AdventureSystem/Canvas/NameWindow/Name")?.GetComponent<TextMeshProUGUI>(),
            ];
            _objectsToToggle = [.. _objectsToToggle.Where(obj => obj != null)];
            _componentsToToggle = [.. _componentsToToggle.Where(comp => comp != null)];
            _choiceWindowText1 = _choiceWindow?.transform.GetChild(0)?.GetComponent<TextMeshProUGUI>();
            _initialized = true;
        }

        /// <summary>
        /// Adds references to gallery buttons when opening the album. They get destroyed when leaving the album.
        /// </summary>
        [HarmonyPatch(typeof(Scene.BonusMenu.Album), nameof(Scene.BonusMenu.Album.OnEnable))]
        [HarmonyPostfix]
        private static void Album_OnEnable()
        {
            _galleryBackButton = GameObject.Find("/MenuCanvas/BonusMenuParent/GalleryOfSpirits/ImageInspect/Back");
            _gallerySwitchButton = GameObject.Find("/MenuCanvas/BonusMenuParent/GalleryOfSpirits/ImageInspect/SwitchImage");
        }

        /// <summary>
        /// Sets the position of the map name window to the outer_position (hidden) so the toggle doesn't show it.
        /// </summary>
        [HarmonyPatch(typeof(MapNameWindow), nameof(MapNameWindow.Initialize))]
        [HarmonyPostfix]
        private static void MapNameWindow_Initialize(MapNameWindow __instance)
        {
            __instance.window_.rectTransform.position = __instance.outer_position_;
        }

        /// <summary>
        /// Update method to check for key press and apply hiding/showing the UI.
        /// </summary>
        [HarmonyPatch(typeof(AdventureSystem), nameof(AdventureSystem.Update))]
        [HarmonyPostfix]
        private static void Update()
        {
            if (!_initialized) return;
            if (BuffKit.GeneralToggleUI.Value.IsDown())
            {
                _shouldShowUi = !_shouldShowUi;
                BuffKit.Log($"Toggling UI... _shouldShowUI: {_shouldShowUi}.");
                ApplySetActive();
            }
            // Make sure UI is hidden when it should be.
            if (!_shouldShowUi)
            {
                ApplySetActive();
            }
        }

        /// <summary>
        /// Applies the SetActive and enabled states based on _shouldShowUi.
        /// </summary>
        private static void ApplySetActive()
        {
            if (_shouldShowUi)
            {
                // Reactivate certain UI elements.
                _bustUp.SetActive(true);
                _mapNameWindow.SetActive(true);
                var choice1HasTextSet = !_choiceWindowText1.text.Contains("icon_off");
                _choiceWindow.SetActive(choice1HasTextSet);
                var messageWindowHasText = _messageWindowText.text != string.Empty;
                _messageWindowImage.enabled = messageWindowHasText;
                _messageWindowText.enabled = messageWindowHasText;
                _messageWindowNextIcon.SetActive(messageWindowHasText); // Didn't see a way to check status... might be fine to show it.
                var nameWindowHasText = _nameWindowText.text != string.Empty && _nameWindowText.text != "Name of Character";
                _nameWindowText.enabled = messageWindowHasText && nameWindowHasText;
            }
            else
            {
                // Deactivate all UI elements.
                foreach (var obj in _objectsToToggle)
                {
                    TrySetActive(obj, _shouldShowUi);
                }
                foreach (var component in _componentsToToggle)
                {
                    if (component.gameObject == null) continue;
                    component.enabled = _shouldShowUi;
                }
            }
            // Always deactivate/activate gallery buttons if they exist.
            TrySetActive(_galleryBackButton, _shouldShowUi);
            TrySetActive(_gallerySwitchButton, _shouldShowUi);
        }

        /// <summary>
        /// Attempts to set the active state of the specified <see cref="GameObject"/>.
        /// </summary>
        /// <param name="obj">The <see cref="GameObject"/> whose active state is to be set. If <paramref name="obj"/> is <see
        /// langword="null"/>, the method does nothing.</param>
        /// <param name="state">The desired active state.</param>
        private static void TrySetActive(GameObject obj, bool state)
        {
            if (obj == null) return;
            obj.SetActive(state);
        }
    }


    [HarmonyPatch]
    internal class Gameplay_Patch
    {
        /// <summary>
        /// If enabled, sets players stamina to max for infinite stamina. Run, Rabbit, Run!
        /// </summary>
        [HarmonyPatch(typeof(MapPlayerCharacter), nameof(MapPlayerCharacter.SetMoveVector))]
        [HarmonyPostfix]
        private static void SetMoveVector(MapPlayerCharacter __instance)
        {
            if (!BuffKit.GameplayInfiniteStamina.Value) return; // Do nothing.
            __instance.stamina = __instance.max_stamina;
        }
    }


    internal class GameplayInventory_Patch : MonoBehaviour
    {
        private static readonly float _checkInterval = 1f;
        private static readonly string _bandageItemId = "item04";
        private static readonly string _talismanItemId = "item11";

        private void Start()
        {
            BuffKit.Log("Starting...");
            StartCoroutine(CheckInventory());
        }

        /// <summary>
        /// Main coroutine to check inventory and add items if they are missing and the corresponding config option is enabled.
        /// </summary>
        private IEnumerator CheckInventory()
        {
            while (true)
            {
                yield return new WaitForSeconds(_checkInterval);
                var gameData = Common.Instance().GameData;
                if (gameData == null) continue;

                if (BuffKit.GameplayInfiniteBandage.Value)
                {
                    if (!gameData.items.Contains(_bandageItemId))
                    {
                        BuffKit.Log("InfiniteBandage: not found, adding one...");
                        gameData.AddItem(_bandageItemId);
                    }
                }
                if (BuffKit.GameplayInfiniteTalisman.Value)
                {
                    if (!gameData.items.Contains(_talismanItemId))
                    {
                        BuffKit.Log("InfiniteTalisman: not found, adding one...");
                        gameData.AddItem(_talismanItemId);
                    }
                }
            }
        }
    }


    [HarmonyPatch]
    internal class Lua_Patch
    {
        /// <summary>
        /// Apply settings for text speed and voice sync after the HauntedLua is initialized.
        /// </summary>
        [HarmonyPatch(typeof(HauntedLua), nameof(HauntedLua.Initialize))]
        [HarmonyPostfix]
        private static void Initialize()
        {
            BuffKit.Log("Applying text speed and voice sync settings...");
            ApplyTextSpeedSetting();
            ApplyVoiceSyncSetting();
        }

        /// <summary>
        /// Overwrites the default message speed with the configured text speed value.
        /// </summary>
        public static void ApplyTextSpeedSetting()
        {
            var speedValue = BuffKit.TextSpeedValues[BuffKit.TextSpeed.Value];
            AdventureSystem.Instance()?.lua?.DoString($"Adventure.deffult_message_speed_ = {speedValue}");
        }

        /// <summary>
        /// Overwrites the Adventure:SetMessage function to include a voice_sync variable that uses the configured setting.
        /// </summary>
        public static void ApplyVoiceSyncSetting()
        {
            var voiceSyncLuaValue = BuffKit.TextVoiceSync.Value.ToString().ToLower();
            var luaFunction = @$"function Adventure:SetMessage(localization_key, voice_path , any_message_speed)
    voice_sync = {voiceSyncLuaValue} -- MODIFIED: Add voice_sync variable.
	--デフォルトのメッセージ速度を保持する
	self.message_speed_ = self.deffult_message_speed_

	local localizedMessage = self.adventure_system_:GetLocalizedText(localization_key)
	localizedMessage = StringEncode(localizedMessage)	
	
	if voice_path == nil then
		voice_path = ''
	end
	self.voice_path_ = voice_path
		
	-- 音声が有る場合
	if voice_path ~= '' then
		-- 音声再生
		local voice_time = Audio.PlayVoice(voice_path)			
		--print('voice_time: ', voice_time)
		
		if voice_time > 0.0 and voice_sync then -- MODIFIED: Add voice_sync check.
			-- 音声同期のメッセージ速度に変更
			local message_len = localizedMessage:len()
			self.message_speed_ = voice_time / message_len			
			--print('message_speed_: ', self.message_speed_)
		end
	end

	-- 再生速度指定がある場合、指定速度に設定する
	if any_message_speed ~= nil then
		self.message_speed_ = any_message_speed	
		--print('message_speed_: ', self.message_speed_)
	end
	
	--self.emphasis_text_:SetText('')
	--self.emphasis_message_ = nil
	
	getmetatable(self).__index.SetMessage(self, localizedMessage)


	--デフォルトのメッセージ速度にする
	--self.message_speed_ = default_msg_spd

	--メッセージログ追加
	self:AddMessageLog(localization_key)
	
	--音声停止
	Audio.StopVoice()

	--メッセージ送り音
	Audio.PlaySystem( 3 )
end";
            AdventureSystem.Instance()?.lua?.DoString(luaFunction);
        }
    }
}