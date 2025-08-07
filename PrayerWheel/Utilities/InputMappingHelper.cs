using System.Collections.Generic;
using Blasphemous.ModdingAPI;
using Blasphemous.ModdingAPI.Input;
using Rewired;
using UnityEngine;
using Framework.Managers;

namespace PrayerWheel.Utilities
{
    public static class InputMappingHelper
    {
        public static bool AreJoysticksAvailable()
        {
            Player player = Rewired.ReInput.players.GetPlayer(0);
            return player.controllers.Joysticks.Count > 0;
        }

        public static List<KeyCode> LoadConfiguredInputBindings()
        {
            Dictionary<string, KeyCode> bindings = ReflectionHelper.GetInstanceField(typeof(InputHandler), Main.PrayerWheelMod.InputHandler, "_keybindings") as Dictionary<string, KeyCode>;

            if (bindings == null)
            {
                ModLog.Error("InputMappingHelper::LoadConfiguredInputBindings: Failed to load bindings!");
                return null;
            }

            List<KeyCode> retList = new List<KeyCode>();

            if (bindings.Keys.Count == 0)
            {
                ModLog.Error("InputMappingHelper::LoadConfiguredInputBindings: No bindings to load, has the InputHandler completed loading?");
                return retList;
            }

            foreach (string key in bindings.Keys)
            {
                retList.Add(bindings[key]);
            }

            return retList;
        }

        public static List<int> GetKeyboardBindingActions(List<KeyCode> bindings)
        {
            Player player = Rewired.ReInput.players.GetPlayer(0);
            List<string> actionNames = Core.ControlRemapManager.GetAllActionNamesInOrder();
            List<int> actions = new List<int>();

            foreach (string actionName in actionNames)
            {
                foreach (ActionElementMap aem in player.controllers.maps.ButtonMapsWithAction(ControllerType.Keyboard, actionName, false))
                {
                    InputAction action = ReInput.mapping.GetAction(aem.actionId);
                    if (action == null) continue; // invalid Action
                    if (aem.keyCode == KeyCode.None) continue; // there is no key assigned

                    if (bindings.Contains(aem.keyCode))
                    {
                        actions.Add(aem.actionId);
                        ModLog.Info($"InputMappingHelper::GetKeyboardBindingActions: Added Keyboard Action '{actionName}' Id '{aem.actionId}' for KeyCode '{aem.keyCode.ToString()}'");
                    }
                }
            }

            return actions;
        }

        public static List<int> GetJoystickBindingActions(List<KeyCode> bindings)
        {
            ModLog.Info("GetJoystickBindingActions START");

            Player player = Rewired.ReInput.players.GetPlayer(0);
            Dictionary<int, List<int>> joystickBindings = ParseJoystickBindings(bindings);
            List<int> actions = new List<int>();

            // ModLog.Info("JoystickBindings:");
            // foreach (int key in joystickBindings.Keys)
            // {
            //     string values = "";
            //     foreach (int value in joystickBindings[key]) values += (value + " ");

            //     ModLog.Info($"[{key}][{values}]");
            // }

            // All elements mapped to all joysticks in the player
            foreach (Joystick joystick in player.controllers.Joysticks)
            {
                //ModLog.Info("Joystick: " + joystick.id);

                if (joystickBindings[joystick.id].Count == 0) continue;

                // Loop over all Joystick Maps in the Player for this Joystick
                foreach (JoystickMap map in player.controllers.maps.GetMaps<JoystickMap>(joystick.id))
                {
                    // Loop over all button maps
                    foreach (ActionElementMap aem in map.ButtonMaps)
                    {
                        if (joystickBindings[joystick.id].Contains(aem.elementIndex))
                        {
                            if (aem.actionId >= 0)
                            {
                                actions.Add(aem.actionId);

                                ModLog.Info("Added action " + aem.actionId + " for Joystick " + joystick.id + " Button " + aem.elementIndex + ": " + ReInput.mapping.GetAction(aem.actionId).name);
                            }
                        }
                    }
                }
            }

            return actions;
        }

        public static Dictionary<int, List<int>> ParseJoystickBindings(List<KeyCode> bindings)
        {
            Dictionary<int, List<int>> retDict = new Dictionary<int, List<int>>();
            for (int idx = 0; idx < 9; idx++)
            {
                retDict[idx] = new List<int>();
            }

            foreach (KeyCode binding in bindings)
            {
                if (binding < KeyCode.JoystickButton0) continue;

                int value = (int)binding - (int)KeyCode.JoystickButton0;

                int joystickIndex = value / 20;
                int buttonIndex = value % 20;

                //ModLog.Info($"{binding.ToString()}: Joy {joystickIndex} Button {buttonIndex}");
                if (!retDict[joystickIndex].Contains(buttonIndex))
                {
                    retDict[joystickIndex].Add(buttonIndex);

                    //ModLog.Info($"Joystick: {joystickIndex} List Count: {retDict[joystickIndex].Count}");
                }
            }

            return retDict;
        }

        public static KeyCode ConvertJoystickToKeyCode(int number, int button)
        {
            if (number < 0 || number > 8 || button < 0 || button > 19)
            {
                // TODO: Log error?
                return KeyCode.None;
            }

            int offset = (int)KeyCode.JoystickButton0;
            int range = (int)KeyCode.Joystick1Button0 - (int)KeyCode.JoystickButton0;

            int value = offset + (number * range) + button;

            return (KeyCode)value;
        }
    }
}