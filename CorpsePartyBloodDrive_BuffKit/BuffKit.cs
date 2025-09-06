using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Haunted;
using Scene.Map;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public static ConfigEntry<bool> GameplayInfiniteStamina;
        public static ConfigEntry<bool> ToolsOpenDataDirectory;
        public static ConfigEntry<bool> ToolsOpenGameDirectory;
        private Harmony _harmony;

        private void Awake()
        {
            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            _harmony.PatchAll();
            Log($"{PluginInfo.PLUGIN_NAME} loaded!");

            SetupSettings();
        }

        private void SetupSettings()
        {
            StartupSkipBootLogos = Config.Bind("Startup", "SkipBootLogos", true, "If true, skips the boot logos.");
            StartupStopTitleScreenLoop = Config.Bind("Startup", "StopTitleScreenLoop", true, "If true, prevents the title screen (press any button) from looping back to the logos after 30 seconds.");
            SaveMenuMaxSlots = Config.Bind("SaveMenu", "MaxSlots", 50, new ConfigDescription(
                "Maximum number of save slots. Default is 50. Original is 20. Recommend not going over 100 saves, but I won't stop you...",
                new AcceptableValueRange<int>(1, 1000)));
            SaveMenuShowSlotNumbers = Config.Bind("SaveMenu", "ShowSlotNumbers", true, "If true, shows the slot numbers in the save menu.");
            GameplayInfiniteStamina = Config.Bind("Gameplay", "InfiniteStamina", false, "If true, player stamina will not decrease. Run, Rabbit, Run!");
            ToolsOpenDataDirectory = Config.Bind("Tools", "OpenDataDirectory", false, "Click to open game data directory.");
            ToolsOpenDataDirectory.SettingChanged += (_, _) =>
            {
                if (!ToolsOpenDataDirectory.Value) return; // Only act when set to true.
                ToolsOpenDataDirectory.Value = false; // Reset to false.
                Application.OpenURL(Application.persistentDataPath);
            };
            ToolsOpenGameDirectory = Config.Bind("Tools", "OpenGameDirectory", false, "Click to open game install directory.");
            ToolsOpenGameDirectory.SettingChanged += (_, _) =>
            {
                if (!ToolsOpenGameDirectory.Value) return; // Only act when set to true.
                ToolsOpenGameDirectory.Value = false; // Reset to false.
                Application.OpenURL(Path.GetDirectoryName(Application.dataPath));
            };
        }

        public static void Log(string message)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(1); // Get the calling method frame
            var method = frame.GetMethod();
            var classFullName = method.DeclaringType.FullName;
            var methodName = method.Name;

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
                if (i != 29) // MODIFIED: Slot 30 is the system save slot. Do not allow selecting it.
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
            return false;
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
}