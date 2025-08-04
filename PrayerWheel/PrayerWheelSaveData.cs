using System;
using Blasphemous.ModdingAPI.Persistence;

namespace PrayerWheel
{
    [Serializable]
    public class PrayerWheelSaveData : SaveData
    {
        public PrayerWheelSaveData() : base("ID_PRAYER_WHEEL")
        { }

        public Config config;
    }
}
