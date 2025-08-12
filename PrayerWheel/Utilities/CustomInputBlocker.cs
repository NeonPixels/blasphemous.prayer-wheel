using HarmonyLib;
using Framework.Managers;
using System.Collections.Generic;

namespace PrayerWheel.Utilities
{
    /// <summary>
    /// Allows blocking input for specific actions.
    /// Example: Blocking players from jumping, or from using flasks.
    /// Note: Currently, only Rewired.GetButton and Rewired.GetButtonDown are blocked,
    /// might need expansion down the line.
    /// </summary>
    public class CustomInputBlocker
    {
        private readonly Dictionary<string, int> inputBlockers = new Dictionary<string, int>();

        public event Core.SimpleEventParam OnInputLocked;

        public event Core.SimpleEventParam OnInputUnlocked;


        //private Mutex _mutex = new Mutex();

        public void SetBlocker(string name, int actionId)
        {
            if (actionId < 0) return;

            //_mutex.WaitOne();

            if (!inputBlockers.ContainsKey(name))
            {
                inputBlockers.Add(name, actionId);

                if (this.OnInputLocked != null)
                {
                    this.OnInputLocked(actionId);
                }

                //ModLog.Info($"Custom Input Blocker ({name})[{actionId}] has been enabled.");
            }

            //_mutex.ReleaseMutex();
        }

        public void RemoveBlocker(string name)
        {
            //_mutex.WaitOne();

            if (inputBlockers.ContainsKey(name))
            {
                RemoveBlocker_Internal(name);
            }

            //_mutex.ReleaseMutex();
        }

        private void RemoveBlocker_Internal(string name)
        {
            int actionId = inputBlockers[name];
            inputBlockers.Remove(name);

            if (this.OnInputUnlocked != null)
            {
                this.OnInputUnlocked(actionId);
            }

            //ModLog.Info($"Custom Input Blocker ({name})[{actionId}] has been disabled.");
        }

        public bool HasBlocker(string name)
        {
            //_mutex.WaitOne();

            bool result = inputBlockers.ContainsKey(name);

            //_mutex.ReleaseMutex();

            return result;
        }

        public bool HasBlocker(int actionId)
        {
            if (actionId < 0) return false;

            //_mutex.WaitOne();

            bool result = false;

            foreach (int value in inputBlockers.Values)
            {
                if (value == actionId)
                {
                    result = true;
                    break;
                }
            }

            //_mutex.ReleaseMutex();

            return result;
        }

        public int GetBlockedAction(string name)
        {
            //_mutex.WaitOne();

            int result = -1;

            if (inputBlockers.TryGetValue(name, out int value))
            {
                result = value;
            }

            //_mutex.ReleaseMutex();

            return result;
        }

        // TODO: Out of sync exception when using this in the Update method of the behaviour
        //       Mutexes didn't help
        public void RemoveAllBlockers()
        {
            //_mutex.WaitOne();

            foreach (string name in inputBlockers.Keys)
            {
                RemoveBlocker_Internal(name);
            }

            //ModLog.Info("All Custom Input Blockers have been removed.");

            //_mutex.ReleaseMutex();
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