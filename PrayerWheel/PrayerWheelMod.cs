using System.Collections.Generic;
using System.Linq;
using Blasphemous.ModdingAPI;
using Blasphemous.ModdingAPI.Input;
using Rewired;
using UnityEngine;

namespace PrayerWheel
{
    public class PrayerWheelMod : BlasMod
    {
        public PrayerWheelMod() : base(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_AUTHOR, ModInfo.MOD_VERSION)
        {
            //ModLog.Info($"{ModInfo.MOD_NAME} has been created");
        }

        // --- Utilities ---

        public Utilities.PrefabHelper PrefabHelper { get; private set; }
        public Utilities.CustomInputBlocker CustomInputBlocker { get; private set; }


        // --- Features ---

        public PrayerWheelDeployer PrayerWheelDeployer { get; private set; }

        protected override void OnInitialize()
        {
            LocalizationHandler.RegisterDefaultLanguage("es");
            //ModLog.Info($"{ModInfo.MOD_NAME} has been initialized");

            PrefabHelper        = new Utilities.PrefabHelper();
            CustomInputBlocker  = new Utilities.CustomInputBlocker();
            PrayerWheelDeployer = new PrayerWheelDeployer();

            // Note that there is no guarantee that the Joystick bindings will match an specific button
            // on specific controllers, so players might need to rebind.
            // These bindings have been made with an XBox compatible controller, and match the 
            // left and right shoulder buttons.
            Main.PrayerWheelMod.InputHandler.RegisterDefaultKeybindings(new Dictionary<string, KeyCode>()
            {
                { PrayerWheelBehaviour.INPUT_LEFT_KB, KeyCode.O },
                { PrayerWheelBehaviour.INPUT_RIGHT_KB, KeyCode.P },
                { PrayerWheelBehaviour.INPUT_LEFT_JOY, KeyCode.JoystickButton4 },
                { PrayerWheelBehaviour.INPUT_RIGHT_JOY, KeyCode.JoystickButton5 }
            });
        }

        public void ResetGame()
        { }

        protected override void OnRegisterServices(ModServiceProvider provider)
        { }

        protected override void OnLevelPreloaded(string oldLevel, string newLevel)
        { }

        protected override void OnLevelLoaded(string oldLevel, string newLevel)
        {
            //ModLog.Info($"OnLevelLoaded: {newLevel} has been loaded");

            if (oldLevel == "" && newLevel == "MainMenu")
            {
                UpdateBindingActions();
            }
        }

        /// <summary>
        /// In order to avoid unwanted actions from triggering while the prayer wheel is operating, we need to block
        /// the input actions shared by the specific button bindings. To do this, we need to retrieve said actions
        /// from the configured controller maps.
        /// This method retrieves the bindings defined by the ModAPI InputHandler, and uses them to obtain the associated actions.
        /// These will be later fed to the PrayerWheel behaviour when the prefab is deployed.
        /// Note that we need to call this function once the Main Menu has loadd, otherwise Rewired won't be ready.
        /// </summary>
        private void UpdateBindingActions()
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

        /// <summary>
        /// We can't check the Joystick bindings if no joystick is connected, so this callback will check
        /// as soon as one is detected. And only once.
        /// Note that connecting/disconnecting controllers during gameplay is not recommended, can cause issues.
        /// </summary>
        /// <param name="args">Callback parameter</param>
        private void OnConnectedUpdateJoystickBindingActions(ControllerStatusChangedEventArgs args)
        {
            if (args.controllerType != ControllerType.Joystick) return;

            List<KeyCode> bindings = Utilities.InputMappingHelper.LoadConfiguredInputBindings();

            PrayerWheelDeployer.InputActionsToBlock_JOY.Clear();
            PrayerWheelDeployer.InputActionsToBlock_JOY = Utilities.InputMappingHelper.GetJoystickBindingActions(bindings).Distinct().ToList();
            PrayerWheelDeployer.InputActionsToBlock_JOY.Remove((int)ButtonCode.Interact);

            ReInput.ControllerConnectedEvent -= OnConnectedUpdateJoystickBindingActions;
        }
    }
}
