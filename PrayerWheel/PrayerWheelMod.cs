using System;
using System.Collections.Generic;
using System.Linq;
using Blasphemous.ModdingAPI;
using Blasphemous.ModdingAPI.Input;
using Blasphemous.ModdingAPI.Persistence;
using Framework.Managers;
using Rewired;
using UnityEngine;

namespace PrayerWheel
{
    public class PrayerWheelMod : BlasMod, IPersistentMod
    {
        public string PersistentID => "ID_PRAYER_WHEEL";

        // Save file info
        public Config GameSettings { get; private set; }

        public PrayerWheelMod() : base(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_AUTHOR, ModInfo.MOD_VERSION)
        {
            GameSettings = new Config();

            ModLog.Info($"{ModInfo.MOD_NAME} has been created <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
        }

        // --- Utilities ---

        public Utilities.PrefabHelper PrefabHelper { get; private set; }
        public Utilities.CustomInputBlocker CustomInputBlocker { get; private set; }


        // --- Features ---

        public PrayerWheelDeployer PrayerWheelDeployer { get; private set; }

        protected override void OnInitialize()
        {
            LocalizationHandler.RegisterDefaultLanguage("es");
            ModLog.Info($"{ModInfo.MOD_NAME} has been initialized");

            PrefabHelper        = new Utilities.PrefabHelper();
            CustomInputBlocker  = new Utilities.CustomInputBlocker();
            PrayerWheelDeployer = new PrayerWheelDeployer();
            
            Main.PrayerWheelMod.InputHandler.RegisterDefaultKeybindings(new Dictionary<string, KeyCode>()
            {
                { PrayerWheelBehaviour.INPUT_LEFT_KB, KeyCode.O },
                { PrayerWheelBehaviour.INPUT_RIGHT_KB, KeyCode.P },
                { PrayerWheelBehaviour.INPUT_LEFT_JOY, KeyCode.JoystickButton4 },
                { PrayerWheelBehaviour.INPUT_RIGHT_JOY, KeyCode.JoystickButton5 }
            });
        }

        public SaveData SaveGame()
        {
            return new PrayerWheelSaveData
            {
                config = GameSettings
            };
        }

        public void LoadGame(SaveData data)
        {
            PrayerWheelSaveData saveGameData = data as PrayerWheelSaveData;

            GameSettings = saveGameData.config;
        }

        public void ResetGame()
        {
            GameSettings = new Config();
        }

        protected override void OnRegisterServices(ModServiceProvider provider)
        {

        }

        protected override void OnLevelPreloaded(string oldLevel, string newLevel)
        {

        }

        protected override void OnLevelLoaded(string oldLevel, string newLevel)
        {
            ModLog.Info($"OnLevelLoaded: {newLevel} has been loaded");

            if (oldLevel == "" && newLevel == "MainMenu")
            {
                List<KeyCode> bindings = Utilities.InputMappingHelper.LoadConfiguredInputBindings();

                PrayerWheelDeployer.InputActionsToBlock_KB = Utilities.InputMappingHelper.GetKeyboardBindingActions(bindings).Distinct().ToList();

                PrayerWheelDeployer.InputActionsToBlock_KB.Remove((int)ButtonCode.Interact);

                if (Utilities.InputMappingHelper.AreJoysticksAvailable())
                {
                    PrayerWheelDeployer.InputActionsToBlock_JOY = Utilities.InputMappingHelper.GetJoystickBindingActions(bindings).Distinct().ToList();

                    PrayerWheelDeployer.InputActionsToBlock_JOY.Remove((int)ButtonCode.Interact);
                }
                else
                {
                    ReInput.ControllerConnectedEvent += OnConnectedUpdateJoystickBindingActions;
                }
            }
        }

        private void OnConnectedUpdateJoystickBindingActions(ControllerStatusChangedEventArgs args)
        {
            if (args.controllerType != ControllerType.Joystick) return;

            List<KeyCode> bindings = Utilities.InputMappingHelper.LoadConfiguredInputBindings();

            PrayerWheelDeployer.InputActionsToBlock_JOY.Clear();
            PrayerWheelDeployer.InputActionsToBlock_JOY = Utilities.InputMappingHelper.GetJoystickBindingActions(bindings).Distinct().ToList();
            PrayerWheelDeployer.InputActionsToBlock_JOY.Remove((int)ButtonCode.Interact);

            ReInput.ControllerConnectedEvent -= OnConnectedUpdateJoystickBindingActions;
        }

        // private List<KeyCode> LoadConfiguredInputBindings()
        // {
        //     //ModLog.Info("LoadConfiguredInputBindings START");

        //     Dictionary<string, KeyCode> bindings = ReflectionHelper.GetInstanceField(typeof(InputHandler), Main.PrayerWheelMod.InputHandler, "_keybindings") as Dictionary<string, KeyCode>;

        //     if (bindings == null) return null;

        //     List<KeyCode> retList = new List<KeyCode>();

        //     foreach (string key in bindings.Keys)
        //     {
        //         //ModLog.Info($"Key:{key} Value:{bindings[key].ToString()}");
        //         retList.Add(bindings[key]);
        //     }

        //     //ModLog.Info("LoadConfiguredInputBindings END");

        //     return retList;
        // }

        // private List<int> GetInputBindingActions(List<KeyCode> bindings)
        // {
        //     List<int> actions = new List<int>();

        //     //actions.AddCollection(Utilities.InputMappingHelper.GetKeyboardBindingActions(bindings));
        //     GetJoystickBindingActions(bindings, actions);


        //     return actions;
        // }

        // private void GetKeyboardBindingActions(List<KeyCode> bindings, List<int> actions)
        // {
        //     Player player = Rewired.ReInput.players.GetPlayer(0);
        //     List<string> actionNames = Core.ControlRemapManager.GetAllActionNamesInOrder();

        //     foreach (string actionName in actionNames)
        //     {
        //         foreach (ActionElementMap aem in player.controllers.maps.ButtonMapsWithAction(ControllerType.Keyboard, actionName, false))
        //         {
        //             InputAction action = ReInput.mapping.GetAction(aem.actionId);
        //             if (action == null) continue; // invalid Action
        //             if (aem.keyCode == KeyCode.None) continue; // there is no key assigned

        //             if (bindings.Contains(aem.keyCode))
        //             {
        //                 actions.Add(aem.actionId);
        //                 ModLog.Info($"Added Keyboard Action '{actionName}' for KeyCode '{aem.keyCode.ToString()}'");
        //             }
        //         }
        //     }
        // }

        // private void GetJoystickBindingActions(List<KeyCode> bindings, List<int> actions)
        // {
        //     ModLog.Info("GetJoystickBindingActions START");

        //     Player player = Rewired.ReInput.players.GetPlayer(0);

        //     Dictionary<int, List<int>> joystickBindings = ParseJoystickBindings(bindings);

        //     ModLog.Info("JoystickBindings:");
        //     foreach (int key in joystickBindings.Keys)
        //     {
        //         string values = "";
        //         foreach (int value in joystickBindings[key]) values += (value + " ");

        //         ModLog.Info($"[{key}][{values}]");
        //     }

        //     // All elements mapped to all joysticks in the player
        //     foreach (Joystick joystick in player.controllers.Joysticks)
        //     {
        //         ModLog.Info("Joystick: " + joystick.id);
        //         // TODO: If no joystick is connected, add callback to ReInput.ControllerConnectedEvent?

        //         if (joystickBindings[joystick.id].Count == 0) continue;

        //         // Loop over all Joystick Maps in the Player for this Joystick
        //         foreach (JoystickMap map in player.controllers.maps.GetMaps<JoystickMap>(joystick.id))
        //         {
        //             // Loop over all button maps
        //             foreach (ActionElementMap aem in map.ButtonMaps)
        //             {
        //                 if (joystickBindings[joystick.id].Contains(aem.elementIndex))
        //                 {
        //                     if (aem.actionId >= 0)
        //                     {
        //                         actions.Add(aem.actionId);

        //                         ModLog.Info("Added action " + aem.actionId + " for Joystick " + joystick.id + " Button " + aem.elementIndex + ": " + ReInput.mapping.GetAction(aem.actionId).name);
        //                     }
        //                 }
        //             }
        //         }
        //     }
            
        // }

        // private Dictionary<int, List<int>> ParseJoystickBindings(List<KeyCode> bindings)
        // {
        //     Dictionary<int, List<int>> retDict = new Dictionary<int, List<int>>();
        //     for (int idx = 0; idx < 9; idx++)
        //     {
        //         retDict[idx] = new List<int>();
        //     }

        //     foreach (KeyCode binding in bindings)
        //     {
        //         if (binding < KeyCode.JoystickButton0) continue;

        //         int value = (int)binding - (int)KeyCode.JoystickButton0;

        //         int joystickIndex = value / 20;
        //         int buttonIndex = value % 20;

        //         ModLog.Info($"{binding.ToString()}: Joy {joystickIndex} Button {buttonIndex}");
        //         if (!retDict[joystickIndex].Contains(buttonIndex))
        //         {
        //             retDict[joystickIndex].Add(buttonIndex);

        //             ModLog.Info($"Joystick: {joystickIndex} List Count: {retDict[joystickIndex].Count}");
        //         }
        //     }

        //     return retDict;
        // }



        // public KeyCode ConvertJoystickToKeyCode(int number, int button)
        // {
        //     if (number < 0 || number > 8 || button < 0 || button > 19)
        //     {
        //         // TODO: Log error?
        //         return KeyCode.None;
        //     }

        //     int offset = (int)KeyCode.JoystickButton0;
        //     int range = (int)KeyCode.Joystick1Button0 - (int)KeyCode.JoystickButton0;

        //     int value = offset + (number * range) + button;

        //     return (KeyCode)value;
        // }

        // private void InitializeInputBlockers(List<KeyCode> bindings)
        // {
        //     Player player = Rewired.ReInput.players.GetPlayer(0);

        //     List<string> actionNames = Core.ControlRemapManager.GetAllActionNamesInOrder();




        //     foreach (string actionName in actionNames)
        //     {
        //         ModLog.Info($" Joystick mappings for: {actionName}");

        //         // All elements mapped to all joysticks in the player
        //         foreach (Joystick joystick in player.controllers.Joysticks)
        //         {

        //             // Loop over all Joystick Maps in the Player for this Joystick
        //             foreach (JoystickMap map in player.controllers.maps.GetMaps<JoystickMap>(joystick.id))
        //             {
        //                 // Loop over all button maps
        //                 foreach (ActionElementMap aem in map.ButtonMaps)
        //                 {
        //                     if (ReInput.mapping.GetAction(aem.actionId).name != actionName) continue;
        //                     ModLog.Info(aem.elementIdentifierName + " is assigned to Button " + aem.elementIndex + " with the Action " + ReInput.mapping.GetAction(aem.actionId).name);
        //                 }

        //                 // Loop over all axis maps
        //                 foreach (ActionElementMap aem in map.AxisMaps)
        //                 {
        //                     if (ReInput.mapping.GetAction(aem.actionId).name != actionName) continue;
        //                     ModLog.Info(aem.elementIdentifierName + " is assigned to Axis " + aem.elementIndex + " with the Action " + ReInput.mapping.GetAction(aem.actionId).name);
        //                 }

        //                 // Loop over all element maps of any type
        //                 foreach (ActionElementMap aem in map.AllMaps)
        //                 {
        //                     if (ReInput.mapping.GetAction(aem.actionId).name != actionName) continue;

        //                     if (aem.elementType == ControllerElementType.Axis)
        //                     {
        //                         ModLog.Info(aem.elementIdentifierName + " is assigned to Axis " + aem.elementIndex + " with the Action " + ReInput.mapping.GetAction(aem.actionId).name);
        //                     }
        //                     else if (aem.elementType == ControllerElementType.Button)
        //                     {
        //                         ModLog.Info(aem.elementIdentifierName + " is assigned to Button " + aem.elementIndex + " with the Action " + ReInput.mapping.GetAction(aem.actionId).name);
        //                     }
        //                 }
        //             }
        //         }


        //         ModLog.Info($" Keyboard buttons for: {actionName}");

        //         // Log the keyboard keys assigned to an Action manually
        //         foreach (ActionElementMap aem in player.controllers.maps.ButtonMapsWithAction(ControllerType.Keyboard, actionName, false))
        //         {
        //             InputAction action = ReInput.mapping.GetAction(aem.actionId);
        //             if (action == null) continue; // invalid Action
        //             if (aem.keyCode == KeyCode.None) continue; // there is no key assigned

        //             ModLog.Info($"   KeyCode: {aem.keyCode.ToString()}");

        //             string descriptiveName = action.descriptiveName; // get the descriptive name of the Action

        //             // Create a string name that contains the primary key and any modifier keys
        //             string key = aem.keyCode.ToString(); // get the primary key code as a string
        //             if (aem.modifierKey1 != ModifierKey.None) key += " + " + aem.modifierKey1.ToString();
        //             if (aem.modifierKey2 != ModifierKey.None) key += " + " + aem.modifierKey2.ToString();
        //             if (aem.modifierKey3 != ModifierKey.None) key += " + " + aem.modifierKey3.ToString();

        //             // Treat axis-type Actions differently than button-type Actions because axis contribution could be positive or negative
        //             // It's generally safe to assume positive contribution for button-type Actions
        //             if (action.type == InputActionType.Axis) // this is an axis-type Action
        //             {

        //                 // Determine if it contributes to the positive or negative value of the Action
        //                 if (aem.axisContribution == Pole.Positive) // positive
        //                 {
        //                     descriptiveName = !string.IsNullOrEmpty(action.positiveDescriptiveName) ?
        //                         action.positiveDescriptiveName :  // use the positive name if one exists
        //                         action.descriptiveName + " +"; // use the descriptive name with sign appended if not
        //                 }
        //                 else  // negative
        //                 {
        //                     descriptiveName = !string.IsNullOrEmpty(action.negativeDescriptiveName) ?
        //                         action.negativeDescriptiveName :  // use the negative name if one exists
        //                         action.descriptiveName + " -"; // use the descriptive name with sign appended if not
        //                 }
        //             }

        //             ModLog.Info("   " + descriptiveName + " is assigned to " + key);
        //         }
        //     }
        // }
    }
}
