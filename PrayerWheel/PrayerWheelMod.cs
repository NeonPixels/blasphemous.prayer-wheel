using Blasphemous.ModdingAPI;
using Blasphemous.ModdingAPI.Persistence;

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
    }
}
