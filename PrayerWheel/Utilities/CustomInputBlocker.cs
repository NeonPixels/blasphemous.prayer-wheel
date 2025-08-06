using HarmonyLib;
using Framework.Managers;
using Blasphemous.ModdingAPI;
using System.Collections.Generic;

namespace PrayerWheel.Utilities
{
    public class CustomInputBlocker
    {
        private readonly Dictionary<string, int> inputBlockers = new Dictionary<string, int>();
                
        public event Core.SimpleEventParam OnInputLocked;

		public event Core.SimpleEventParam OnInputUnlocked;


		public void SetBlocker(string name, int actionId)
		{

            if (actionId < 0) return;

			if (!inputBlockers.ContainsKey(name))
            {
                inputBlockers.Add(name, actionId);

                if (this.OnInputLocked != null)
                {
                    this.OnInputLocked(actionId);
                }

                ModLog.Info($"Custom Input Blocker ({name})[{actionId}] has been enabled.");
            }						
		}

        public void RemoveBlocker(string name)
        {
            if (inputBlockers.ContainsKey(name))
			{
                RemoveBlocker_Internal(name);
            }
        }

        private void RemoveBlocker_Internal(string name)
        {
            int actionId = inputBlockers[name];
            inputBlockers.Remove(name);

            if (this.OnInputUnlocked != null)
            {
                this.OnInputUnlocked(actionId);
            }
            
            ModLog.Info($"Custom Input Blocker ({name})[{actionId}] has been disabled.");
        }

        public bool HasBlocker(string name)
        {
            return inputBlockers.ContainsKey(name);
        }

        public bool HasBlocker(int actionId)
        {
            if (actionId < 0) return false;

            foreach (int value in inputBlockers.Values)
            {
                if (value == actionId) return true;
            }

            return false;
        }

        public int GetBlockedAction(string name)
        {
            if( inputBlockers.TryGetValue(name, out int value) )
            {
                return value;
            }

            return -1;
        }

		public void RemoveAllBlockers()
		{
            //TODO: out of sync error here
			foreach (string name in inputBlockers.Keys)
            {
                RemoveBlocker_Internal(name);
            }

            inputBlockers.Clear();
			ModLog.Info("All Custom Input Blockers have been removed.");
		}
    }


    // Prevent input for blocked commands from player update
    [HarmonyPatch(typeof(Rewired.Player), "GetButton", typeof(int))]
    class RewiredButton_Patch
    {
        public static bool Prefix(int actionId)
        {
            if(Main.PrayerWheelMod.CustomInputBlocker.HasBlocker(actionId))
            {
                //ModLog.Info($"Input [{actionId}] blocked");
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Rewired.Player), "GetButtonDown", typeof(int))]
    class RewiredButtonDown_Patch
    {
        public static bool Prefix(int actionId)
        {
            if(Main.PrayerWheelMod.CustomInputBlocker.HasBlocker(actionId))
            {
                //ModLog.Info($"Input [{actionId}] blocked");
                return false;
            }

            return true;
        }
    }
}