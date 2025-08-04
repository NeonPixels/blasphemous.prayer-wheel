using System;
using System.Collections.Generic;
using Blasphemous.ModdingAPI;
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

        // --- Libraries and Dependencies ---

        public PrefabsScene.PrefabsSceneFeature PrefabSceneFeature { get; private set; }
        public CustomInputBlocker.CustomInputBlocker CustomInputBlocker { get; private set; }

        // --- Features ---

        public PrayerWheelFeature PrayerWheelFeature { get; private set; }

        protected override void OnInitialize()
        {
            LocalizationHandler.RegisterDefaultLanguage("es");
            ModLog.Info($"{ModInfo.MOD_NAME} has been initialized");

            PrefabSceneFeature = new PrefabsScene.PrefabsSceneFeature();
            PrefabSceneFeature.Enable();

            CustomInputBlocker = new CustomInputBlocker.CustomInputBlocker();
            CustomInputBlocker.Enable();

            PrayerWheelFeature = new PrayerWheelFeature();
            PrayerWheelFeature.Enable();
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
                InitializeInputBlockers();
            }
        }

        private void InitializeInputBlockers()
        { 
            Player player = Rewired.ReInput.players.GetPlayer(0);

            List<string> actionNames = Core.ControlRemapManager.GetAllActionNamesInOrder();

            


            foreach (string actionName in actionNames)
            {
                ModLog.Info($" Joystick mappings for: {actionName}");

                // All elements mapped to all joysticks in the player
                foreach (Joystick joystick in player.controllers.Joysticks)
                {

                    // Loop over all Joystick Maps in the Player for this Joystick
                    foreach (JoystickMap map in player.controllers.maps.GetMaps<JoystickMap>(joystick.id))
                    {
                        // Loop over all button maps
                        foreach (ActionElementMap aem in map.ButtonMaps)
                        {
                            if (ReInput.mapping.GetAction(aem.actionId).name != actionName) continue;
                            ModLog.Info(aem.elementIdentifierName + " is assigned to Button " + aem.elementIndex + " with the Action " + ReInput.mapping.GetAction(aem.actionId).name);
                        }

                        // Loop over all axis maps
                        foreach (ActionElementMap aem in map.AxisMaps)
                        {
                            if (ReInput.mapping.GetAction(aem.actionId).name != actionName) continue;
                            ModLog.Info(aem.elementIdentifierName + " is assigned to Axis " + aem.elementIndex + " with the Action " + ReInput.mapping.GetAction(aem.actionId).name);
                        }

                        // Loop over all element maps of any type
                        foreach (ActionElementMap aem in map.AllMaps)
                        {
                            if (ReInput.mapping.GetAction(aem.actionId).name != actionName) continue;

                            if (aem.elementType == ControllerElementType.Axis)
                            {
                                ModLog.Info(aem.elementIdentifierName + " is assigned to Axis " + aem.elementIndex + " with the Action " + ReInput.mapping.GetAction(aem.actionId).name);
                            }
                            else if (aem.elementType == ControllerElementType.Button)
                            {
                                ModLog.Info(aem.elementIdentifierName + " is assigned to Button " + aem.elementIndex + " with the Action " + ReInput.mapping.GetAction(aem.actionId).name);
                            }
                        }
                    }
                }


                ModLog.Info($" Keyboard buttons for: {actionName}");

                // Log the keyboard keys assigned to an Action manually
                foreach (ActionElementMap aem in player.controllers.maps.ButtonMapsWithAction(ControllerType.Keyboard, actionName, false))
                {
                    InputAction action = ReInput.mapping.GetAction(aem.actionId);
                    if (action == null) continue; // invalid Action
                    if (aem.keyCode == KeyCode.None) continue; // there is no key assigned

                    ModLog.Info($"   KeyCode: {aem.keyCode.ToString()}");

                    string descriptiveName = action.descriptiveName; // get the descriptive name of the Action

                    // Create a string name that contains the primary key and any modifier keys
                    string key = aem.keyCode.ToString(); // get the primary key code as a string
                    if (aem.modifierKey1 != ModifierKey.None) key += " + " + aem.modifierKey1.ToString();
                    if (aem.modifierKey2 != ModifierKey.None) key += " + " + aem.modifierKey2.ToString();
                    if (aem.modifierKey3 != ModifierKey.None) key += " + " + aem.modifierKey3.ToString();

                    // Treat axis-type Actions differently than button-type Actions because axis contribution could be positive or negative
                    // It's generally safe to assume positive contribution for button-type Actions
                    if (action.type == InputActionType.Axis) // this is an axis-type Action
                    {

                        // Determine if it contributes to the positive or negative value of the Action
                        if (aem.axisContribution == Pole.Positive) // positive
                        {
                            descriptiveName = !string.IsNullOrEmpty(action.positiveDescriptiveName) ?
                                action.positiveDescriptiveName :  // use the positive name if one exists
                                action.descriptiveName + " +"; // use the descriptive name with sign appended if not
                        }
                        else  // negative
                        {
                            descriptiveName = !string.IsNullOrEmpty(action.negativeDescriptiveName) ?
                                action.negativeDescriptiveName :  // use the negative name if one exists
                                action.descriptiveName + " -"; // use the descriptive name with sign appended if not
                        }
                    }

                    ModLog.Info("   " + descriptiveName + " is assigned to " + key);
                }
            }
        }
    }
}
