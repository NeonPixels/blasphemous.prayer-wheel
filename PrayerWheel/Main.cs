using BepInEx;

namespace PrayerWheel
{
    [BepInPlugin(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_VERSION)]
    [BepInDependency("Blasphemous.ModdingAPI", "3.0.0")]
    public class Main : BaseUnityPlugin
    {
        public static Main Instance { get; private set; }
        public static PrayerWheelMod PrayerWheelMod { get; private set; }

        private void Start()
        {
            Instance = this;
            PrayerWheelMod = new PrayerWheelMod();
        }
    }
}
