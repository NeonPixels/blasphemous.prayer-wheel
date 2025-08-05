using System;
using System.Collections.Generic;
using Blasphemous.ModdingAPI;
using Blasphemous.ModdingAPI.Input;
using Rewired;
using UnityEngine;
using System.Linq;

namespace PrayerWheel
{

    public class InputHandling
    {
        public InputHandling()
        {

        }


        public List<KeyCode> GetConfiguredBindings()
        {
            // TODO: Iterate all mappings, list actions that match current code
            ModLog.Info("GetConfiguredBindings START");

            Dictionary<string, KeyCode> bindings = ReflectionHelper.GetInstanceField(typeof(InputHandler), Main.PrayerWheelMod.InputHandler, "_keybindings") as Dictionary<string, KeyCode>;

            if(bindings == null) ModLog.Info("NULL");
            else ModLog.Info($"Size: {bindings.Count}");

            foreach (string key in bindings.Keys)
            {
                ModLog.Info($"GetConfiguredBindings Key:{key} Value:{bindings[key].ToString()}");
            }

            List<KeyCode> retList = new List<KeyCode>();

            return retList;
        }

        public List<int> GetAssignedActions(KeyCode binding)
        {
            // TODO: Iterate all mappings, list actions that match current code

            List<int> retList = new List<int>();

            return retList;
        }

        public void RemoveInvalidActionBinding(int action)
        {
            // TODO: Iterate bindings, see if they apply to action, remove them, log error
        }

        public KeyCode ConvertJoystickToKeyCode(int number, int button)
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




        // // Summary:
        // //     Specifies which keybindings will be loaded and registers their defaults
        // public void RegisterDefaultKeybindings(Dictionary<string, KeyCode> defaults)
        // {
        //     if (_registered)
        //     {
        //         ModLog.Warn("InputHandler has already been registered!", _mod);
        //         return;
        //     }

        //     _registered = true;
        //     foreach (KeyValuePair<string, KeyCode> @default in defaults)
        //     {
        //         _keybindings.Add(@default.Key, @default.Value);
        //     }

        //     DeserializeKeybindings(_mod.FileHandler.LoadKeybindings());
        //     _mod.FileHandler.SaveKeybindings(SerializeKeyBindings());
        // }

        // //
        // // Summary:
        // //     When saving the keybindings to a file, convert them to a list of strings
        // private string[] SerializeKeyBindings()
        // {
        //     string[] array = new string[_keybindings.Count];
        //     int num = 0;
        //     foreach (KeyValuePair<string, KeyCode> keybinding in _keybindings)
        //     {
        //         array[num++] = $"{keybinding.Key}: {keybinding.Value}";
        //     }

        //     return array;
        // }

        // //
        // // Summary:
        // //     When loading the keybindings from a file, convert and validate their keycodes
        // private void DeserializeKeybindings(string[] keys)
        // {
        //     foreach (string text in keys)
        //     {
        //         int num = text.IndexOf(':');
        //         if (num < 0)
        //         {
        //             continue;
        //         }

        //         string text2 = text.Substring(0, num).Trim();
        //         string value = text.Substring(num + 1).Trim();
        //         if (_keybindings.ContainsKey(text2))
        //         {
        //             try
        //             {
        //                 object obj = Enum.Parse(typeof(KeyCode), value);
        //                 _keybindings[text2] = (KeyCode)obj;
        //             }
        //             catch
        //             {
        //                 ModLog.Error("Keybinding '" + text2 + "' is invalid.  Using default instead.", _mod);
        //             }
        //         }
        //     }
        // }
    }
}