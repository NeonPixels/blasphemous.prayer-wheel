
using System.Collections;
using Blasphemous.ModdingAPI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PrayerWheel.PrefabsScene
{

    // TODO: Add a new additive scene where prefabs can be stored

    public class PrefabsSceneFeature
    {
        public bool IsEnabled { get; private set;}

        public void Enable()
        {
            _scene = SceneManager.CreateScene("PrefabStorage");
            

            IsEnabled = true;
        }

        public void Disable()
        {
            Main.Instance.StartCoroutine(UnloadScene());
        }

        private IEnumerator UnloadScene()
        {
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(_scene);

            while(!asyncUnload.isDone)
            {
                yield return null;
            }

            IsEnabled = false;
        }

        private Scene _scene;

        public void Store(GameObject prefab, string path = "")
        {
            if(!IsEnabled) return;

            prefab.transform.parent = null;
            SceneManager.MoveGameObjectToScene(prefab, _scene);
            //TODO: Move to path

        }

        public GameObject Instantiate(string path, Transform parent)
        {
            GameObject newInstance = null;
            // TODO: Manage path
            foreach(GameObject prefab in _scene.GetRootGameObjects())
            {
                if(prefab.name == path)
                {
                    newInstance = GameObject.Instantiate(prefab, parent);
                    ModLog.Info($"Prefab({path}) scene: {prefab.scene.name}");
                    ModLog.Info($"New instance({newInstance.name}) scene: {newInstance.scene.name}");
                }
            }

            return newInstance;
        }

    }
}