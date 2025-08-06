
using Blasphemous.ModdingAPI;
using UnityEngine;

namespace PrayerWheel.Utilities
{
    /// <summary>
    /// Utility class that allows storing GameObjects as prefabs inside the Main plugin object, to be instantiated as needed.
    /// For extra safety, objects will be stored inside an empty GameObject named using the MOD_ID
    /// </summary>
    public class PrefabHelper
    {
        private string _prefabContainerName = ModInfo.MOD_ID + "_Prefabs";

        private GameObject _prefabContainer = null;

        public PrefabHelper()
        {
            _prefabContainer = Main.Instance.transform.Find(_prefabContainerName)?.gameObject;
            if (null == _prefabContainer)
            {
                _prefabContainer = new GameObject(_prefabContainerName);
                _prefabContainer.transform.SetParent(Main.Instance.transform);
            }
        }

        public void Store(GameObject prefab, bool replace = false)
        {
            GameObject previousPrefab = _prefabContainer.transform.Find(prefab.name)?.gameObject;

            if (null != previousPrefab)
            {
                if (!replace)
                {
                    ModLog.Error($"Cannot store prefab '{prefab.name}', another prefab with the same name already exists!");
                    return;
                }
                else
                {
                    // ModLog.Info($"Replacing previous instance of prefab '{prefab.name}'");
                    Object.Destroy(previousPrefab);
                }
            }

            prefab.transform.SetParent(_prefabContainer.transform, true);
        }

        public void Remove(string name)
        {
            GameObject previousPrefab = _prefabContainer.transform.Find(name)?.gameObject;

            if (null != previousPrefab)
            {
                // ModLog.Info($"Removing instance of prefab '{name}'");
                Object.Destroy(previousPrefab);
            }
            else
            {
                ModLog.Error($"Cannot remove prefab '{name}', it doesn't exist");
            }
        }

        public GameObject Instantiate(string name, Transform parent)
        {
            GameObject prefab = _prefabContainer.transform.Find(name)?.gameObject;

            if (null == prefab)
            {
                ModLog.Error($"Cannot instantiate prefab '{name}', it doesn't exist!");
                return null;
            }

            GameObject newInstance = GameObject.Instantiate(prefab, parent);
            // ModLog.Info("Instantiating Stored Prefab:");
            // ModLog.Info("  Prefab:");
            // ModLog.Info($"    Name: '{prefab.name}' Parent: '{prefab.transform.parent.name}' Scene: '{prefab.scene.name}'");
            // ModLog.Info("  New Instance:");
            // ModLog.Info($"    Name: '{newInstance.name}' Parent: '{newInstance.transform.parent.name}' Scene: {newInstance.scene.name}");

            return newInstance;
        }
    }
}