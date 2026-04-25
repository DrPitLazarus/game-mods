using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace BuffKit
{
    internal class ModMenu : MonoBehaviour
    {
        private static readonly Texture2D _colorWindow = MakeBackgroundTexture(1, 1, new Color(0.2f, 0.2f, 0.2f, 0.92f));
        private static readonly Texture2D _colorTooltip = MakeBackgroundTexture(1, 1, new Color(0f, 0f, 0f, 1f));
        private static readonly float _windowHorizontalPadding = 20f;
        private static readonly float _buttonHorizontalPadding = 4f;
        private static readonly int _resetButtonWidth = 60;
        private static readonly int _windowId = -5;
        private static readonly int _mainFontSize = 18;
        private static Rect _windowRect = new(20, 20, 550, 700);
        private static bool _isVisible = false;
        private static int _toolbarInt = 0;
        private static Vector2 _scrollPosition = new(0, 0);
        private static GUIStyle _styleWindow;
        private static GUIStyle _styleSectionLabel;
        private static GUIStyle _styleLabel;
        private static GUIStyle _styleButton;
        private static GUIStyle _styleToggle;
        private static GUIStyle _styleTextInput;
        private static GUIStyle _styleToggleSkipLabel;
        private static ConfigEntry<KeyboardShortcut> _currentKeyboardShortcutToSet;
        private static IEnumerable<KeyCode> _keysToCheck;
        private float _columnWidth => (_windowRect.width - _windowHorizontalPadding) / 2f;
        private bool _isRecordingKeybind => _currentKeyboardShortcutToSet != null;
        private bool _mouseIsOverWindow => _windowRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));

        private void Awake()
        {
            // Set the window to be centered on screen.
            var screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            _windowRect.x = screenCenter.x - (_windowRect.width / 2f);
            _windowRect.y = screenCenter.y - (_windowRect.height / 2f);
        }

        private void Update()
        {
            if (BuffKit.GeneralToggleModMenuUI.Value.IsDown())
            {
                _isVisible = !_isVisible;
            }

            if (_isVisible && !Cursor.visible)
            {
                Cursor.visible = true;
            }

            if (!_isVisible) return;
            // Disable input when hovering over the window to prevent clicking through.
            if (_mouseIsOverWindow && !_isRecordingKeybind)
            {
                Input.ResetInputAxes();
            }
        }

        private void OnGUI()
        {
            SetupStyles();

            if (ToggleSkip_Patch.IsSkipping)
            {
                GUI.Label(new Rect(Screen.width - 300, Screen.height - 30, 300, 30), $"ToggleSkipping... [{BuffKit.GeneralToggleSkip.Value}]", _styleToggleSkipLabel);
            }

            if (!_isVisible) return;

            _windowRect = GUILayout.Window(_windowId, _windowRect, DrawWindow, $"{PluginInfo.PLUGIN_NAME} {PluginInfo.PLUGIN_VERSION}", _styleWindow);
        }

        private void SetupStyles()
        {
            _styleToggleSkipLabel ??= new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 24,
                alignment = TextAnchor.LowerRight,
            };
            _styleWindow ??= new GUIStyle(GUI.skin.window)
            {
                // Set window background color (focused/unfocused).
                onNormal = { background = _colorWindow },
                normal = { background = _colorWindow },
                fontSize = _mainFontSize
            };
            _styleSectionLabel ??= new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 24,
                margin = new RectOffset(0, 0, 10, 0)
            };
            _styleLabel ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = _mainFontSize
            };
            _styleButton ??= new GUIStyle(GUI.skin.button)
            {
                fontSize = _mainFontSize
            };
            _styleToggle ??= new GUIStyle(GUI.skin.toggle)
            {
                fontSize = _mainFontSize
            };
            _styleTextInput ??= new GUIStyle(GUI.skin.textField)
            {
                fontSize = _mainFontSize
            };
        }

        private void DrawWindow(int windowID)
        {
            // Main window toolbar.
            GUILayout.BeginHorizontal();
            GUILayout.Label("By DrPitLazarus", _styleLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button($"Close [{BuffKit.GeneralToggleModMenuUI.Value}]", _styleButton))
            {
                _isVisible = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            // Tabs.
            _toolbarInt = GUILayout.Toolbar(_toolbarInt, ["Settings", "Tools"], _styleButton);

            GUILayout.Space(10);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            // Content.
            if (_toolbarInt == 0)
            {
                DrawSectionLabel("General");
                DrawSettingEntry(BuffKit.GeneralToggleModMenuUI);
                DrawSettingEntry(BuffKit.GeneralToggleSkip);
                DrawSettingEntry(BuffKit.GeneralToggleUI);
                DrawSettingEntry(BuffKit.GeneralVSync);
                DrawSettingEntry(BuffKit.GeneralFramerateLimit60);
                DrawSettingEntry(BuffKit.GeneralHideRefreshRate);
                DrawSectionLabel("Save Menu");
                DrawSettingEntry(BuffKit.SaveMenuMaxSlots);
                //DrawSettingEntry(BuffKit.SaveMenuModifyOptions);
                DrawSettingEntry(BuffKit.SaveMenuShowSlotNumbers);
                DrawSettingEntry(BuffKit.SaveMenuShowTimeAgo);
                DrawSettingEntry(BuffKit.SaveMenuSortBy);
                //DrawSettingEntry(BuffKit.SaveMenuAutoSaveSystemData);
                DrawSectionLabel("Startup");
                DrawSettingEntry(BuffKit.StartupSkipBootLogos);
                DrawSettingEntry(BuffKit.StartupStopTitleScreenLoop);
                DrawSettingEntry(BuffKit.StartupPlayOpeningMovie);
                DrawSettingEntry(BuffKit.StartupPreferOpening1);
                DrawSectionLabel("Text");
                DrawSettingEntry(BuffKit.TextSpeed);
                DrawSettingEntry(BuffKit.TextVoiceSync);
                DrawSectionLabel("Gameplay");
                DrawSettingEntry(BuffKit.GameplayAlwaysRun);
                DrawSettingEntry(BuffKit.GameplayInfiniteBandage);
                DrawSettingEntry(BuffKit.GameplayInfiniteBatteryItem);
                DrawSettingEntry(BuffKit.GameplayInfiniteStamina);
                DrawSettingEntry(BuffKit.GameplayInfiniteTalisman);
                //DrawSettingToggle(BuffKit.GameplayMenuAnytime);
            }
            else if (_toolbarInt == 1)
            {
                DrawSettingButton(BuffKit.ToolsOpenDataDirectory);
                DrawSettingButton(BuffKit.ToolsOpenGameDirectory);
                DrawSettingButton(BuffKit.ToolsOpenOutputLog);
                DrawSettingButton(BuffKit.ToolsOpenGitHubPage);
                DrawSettingButton(BuffKit.ToolsClearTalismansFromInventory);
            }
            GUILayout.EndScrollView();

            // Footer.
            GUILayout.Label("Hovering over this window will block input to the game. Mostly.", _styleLabel);

            DrawTooltip(_windowRect);

            // Make entire window draggable, must be at the bottom.
            GUI.DragWindow();
        }

        private static void DrawTooltip(Rect area)
        {
            // Slightly modified from: https://github.com/BepInEx/BepInEx.ConfigurationManager/blob/master/ConfigurationManager.Shared/ConfigurationManager.cs#L334
            string tooltip = GUI.tooltip;
            if (!string.IsNullOrEmpty(tooltip))
            {
                var style = new GUIStyle(GUI.skin.box)
                {
                    wordWrap = true,
                    normal = { background = _colorTooltip },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = _mainFontSize
                };

                var content = new GUIContent(tooltip);

                var width = area.width;
                var height = style.CalcHeight(content, width) + 10;

                var mousePosition = Event.current.mousePosition;

                var x = mousePosition.x + width > area.width
                    ? area.width - width
                    : mousePosition.x;

                var y = mousePosition.y + 25 + height > area.height
                    ? mousePosition.y - height
                    : mousePosition.y + 25;

                var position = new Rect(x, y, width, height);
                style.Draw(position, content, -1);
            }
        }

        private void DrawSectionLabel(string label)
        {
            GUILayout.Label(label, _styleSectionLabel);
        }

        private void DrawSettingEntry<T>(T configEntry) where T : ConfigEntryBase
        {
            GUILayout.BeginHorizontal();
            var labelContent = new GUIContent(configEntry.Definition.Key.ToString(), configEntry.Description.Description);
            GUILayout.Label(labelContent, _styleLabel, GUILayout.Width(_columnWidth));

            switch (configEntry)
            {
                case ConfigEntry<bool> boolEntry:
                    {
                        boolEntry.Value = GUILayout.Toggle(boolEntry.Value, boolEntry.Value ? "Enabled" : "Disabled", _styleToggle);
                        GUILayout.FlexibleSpace();
                        var settingIsDefault = boolEntry.Value == (bool)boolEntry.DefaultValue;
                        if (!settingIsDefault && GUILayout.Button("Reset", _styleButton, GUILayout.ExpandWidth(false), GUILayout.Width(_resetButtonWidth)))
                        {
                            boolEntry.Value = (bool)boolEntry.DefaultValue;
                        }
                        break;
                    }
                case ConfigEntry<int> intEntry:
                    {
                        if (configEntry.Description.AcceptableValues is not AcceptableValueRange<int> range)
                        {
                            throw new InvalidOperationException($"Config entry '{intEntry.Definition.Key}' does not have an acceptable value range.");
                        }
                        var min = range.MinValue;
                        var max = range.MaxValue;
                        intEntry.Value = (int)GUILayout.HorizontalSlider(intEntry.Value, min, max, GUILayout.ExpandWidth(true));
                        intEntry.Value = int.Parse(GUILayout.TextField(intEntry.Value.ToString(), _styleTextInput, GUILayout.Width(_resetButtonWidth)));
                        //GUILayout.FlexibleSpace();
                        var settingIsDefault = intEntry.Value == (int)intEntry.DefaultValue;
                        if (settingIsDefault)
                        {
                            GUILayout.Space(_resetButtonWidth + _buttonHorizontalPadding);
                        }
                        if (!settingIsDefault && GUILayout.Button("Reset", _styleButton, GUILayout.Width(_resetButtonWidth)))
                        {
                            intEntry.Value = (int)intEntry.DefaultValue;
                        }
                        break;
                    }
                case ConfigEntry<KeyboardShortcut> keybindEntry:
                    {
                        // Slightly modified from: https://github.com/BepInEx/BepInEx.ConfigurationManager/blob/master/ConfigurationManager.Shared/SettingFieldDrawer.cs#L393
                        // State: Button pressed to set a new key bind.
                        if (ReferenceEquals(_currentKeyboardShortcutToSet, keybindEntry))
                        {
                            GUILayout.Label("Recording, ESC to cancel...", _styleLabel, GUILayout.ExpandWidth(true));
                            // Clear GUI focus.
                            GUIUtility.keyboardControl = -1;
                            var input = UnityInput.Current;
                            _keysToCheck ??= [.. input.SupportedKeyCodes.Except([KeyCode.Mouse0, KeyCode.None])];
                            foreach (var key in _keysToCheck)
                            {
                                if (input.GetKeyUp(key))
                                {
                                    if (key == KeyCode.Escape)
                                    {
                                        _currentKeyboardShortcutToSet = null;
                                        break;
                                    }
                                    BuffKit.Log($"Keybind set: {key}");
                                    keybindEntry.Value = new KeyboardShortcut(key, [.. _keysToCheck.Where(input.GetKey)]);
                                    _currentKeyboardShortcutToSet = null;
                                    break;
                                }
                            }
                            if (GUILayout.Button("Cancel", _styleButton, GUILayout.ExpandWidth(false), GUILayout.Width(_resetButtonWidth + 10)))
                                _currentKeyboardShortcutToSet = null;
                        }
                        // State: Normal display.
                        else
                        {
                            if (GUILayout.Button(keybindEntry.Value.ToString(), _styleButton, GUILayout.ExpandWidth(true)))
                                _currentKeyboardShortcutToSet = keybindEntry;

                            if (GUILayout.Button("Clear", _styleButton, GUILayout.ExpandWidth(false), GUILayout.Width(_resetButtonWidth)))
                            {
                                keybindEntry.Value = KeyboardShortcut.Empty;
                                _currentKeyboardShortcutToSet = null;
                            }
                            var settingIsDefault = keybindEntry.Value.Equals((KeyboardShortcut)keybindEntry.DefaultValue);
                            if (settingIsDefault)
                            {
                                GUILayout.Space(_resetButtonWidth + _buttonHorizontalPadding);
                            }
                            if (!settingIsDefault && GUILayout.Button("Reset", _styleButton, GUILayout.ExpandWidth(false), GUILayout.Width(_resetButtonWidth)))
                            {
                                keybindEntry.Value = (KeyboardShortcut)keybindEntry.DefaultValue;
                            }
                        }
                        break;
                    }
                default:
                    if (configEntry.SettingType.IsEnum)
                    {
                        var enumType = configEntry.BoxedValue.GetType();
                        var currentValue = Convert.ToInt64(configEntry.BoxedValue);
                        var enumValues = Enum.GetValues(configEntry.SettingType);

                        GUILayout.BeginVertical();
                        {
                            //GUILayout.Label($"configEntry: {configEntry}\ncurrentValue: {currentValue} ({Enum.GetName(enumType, configEntry.BoxedValue)})\nenumNames: {String.Join(", ", Enum.GetNames(enumType))}\nenumValues: {enumValues}");
                            foreach (var option in enumValues)
                            {
                                var optionIsCurrentValue = currentValue == Convert.ToInt64(option);
                                var label = "";
                                var field = option.GetType().GetField(option.ToString());
                                label = field?.GetCustomAttributes(typeof(DescriptionAttribute), false).Cast<DescriptionAttribute>().FirstOrDefault()?.Description ?? option.ToString();
                                label = optionIsCurrentValue ? $" > {label} <" : label.ToString();
                                if (GUILayout.Button(label, _styleButton) && !optionIsCurrentValue)
                                {
                                    configEntry.BoxedValue = option;
                                }
                            }
                            var settingIsDefault = configEntry.BoxedValue.Equals(configEntry.DefaultValue);
                            if (!settingIsDefault && GUILayout.Button("Reset", _styleButton))
                            {
                                configEntry.BoxedValue = configEntry.DefaultValue;
                            }
                        }
                        GUILayout.EndVertical();
                    }
                    else
                    {
                        throw new NotSupportedException($"Config entry type '{typeof(T)}' is not supported.");
                    }
                    break;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawSettingButton(ConfigEntry<bool> configEntry)
        {
            var labelContent = new GUIContent(configEntry.Definition.Key.ToString(), configEntry.Description.Description);
            if (GUILayout.Button(labelContent, _styleButton))
            {
                configEntry.Value = true;
            }
        }

        private static Texture2D MakeBackgroundTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D backgroundTexture = new(width, height);

            backgroundTexture.SetPixels(pixels);
            backgroundTexture.Apply();

            return backgroundTexture;
        }
    }
}
