using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;
using Blasphemous.ModdingAPI;
using System.Collections.Generic;
using Blasphemous.ModdingAPI.Files;

namespace PrayerWheel
{
    /// <summary>
    /// Constructs the PrayerWheel Prefab object, and deploys it when needed.
    /// </summary>
    public class PrayerWheelDeployer
    {

        public PrayerWheelDeployer()
        {
            if (!Assemble())
            {
                ModLog.Error("Failed to assemble PrayerWheel!");
                Disable();
                return;
            }

            SpawnManager.OnPlayerSpawn += Deploy;            
        }

        public void Disable()
        {
            SpawnManager.OnPlayerSpawn -= Deploy;
            Remove();
        }

        // --- PrayerWheel structure ---
        // PrayerWheel: GameObject
        //      PrayerWheel: MonoBehaviour
        //      PrayerLeft: GameObject
        //          SpriteRenderer        
        //      PrayerSelected: GameObject
        //          SpriteRenderer
        //      PrayerRight: GameObject
        //          SpriteRenderer

        private GameObject PrayerWheel_Prefab { get; set; }

        private const string PrayerWheelName = "PrayerWheel";
        private const string PrefabName = PrayerWheelName +"_PREFAB";

        public List<int> InputActionsToBlock_KB { get; set; } = new List<int>();
        public List<int> InputActionsToBlock_JOY { get; set; } = new List<int>();
        
        private bool Assemble()
        {
            //ModLog.Info("Assemble: START");

            //ModLog.Info("Assemble: Instantiate object");
            PrayerWheel_Prefab = new GameObject(PrefabName);
            if (null == PrayerWheel_Prefab)
            {
                ModLog.Error("Assemble: Failed to instantiate object!");
                return false;
            }
            PrayerWheel_Prefab.transform.localPosition = new Vector2(0f, 0f);

            PrayerWheelBehaviour behaviour = PrayerWheel_Prefab.AddComponent<PrayerWheelBehaviour>();
            if (null == behaviour)
            {
                ModLog.Error("Assemble: Failed to instantiate behaviour!");
                return false;
            }

            //ModLog.Info("Assemble: Create prayer icons");
            behaviour.PrayerActive = new GameObject(PrayerWheelBehaviour.ICON_PRAYER_ACTIVE_NAME);
            {
                behaviour.PrayerActive.transform.localPosition = new Vector2(0f, 0f);
                SpriteRenderer renderer = behaviour.PrayerActive.AddComponent<SpriteRenderer>();
                renderer.sortingLayerName = "In-Game UI";
                renderer.sortingOrder = PrayerWheelBehaviour.ICON_PRAYER_SORTING_ORDER;

                behaviour.PrayerActive.transform.parent = PrayerWheel_Prefab.transform;
            }

            behaviour.PrayerLeft = GameObject.Instantiate<GameObject>(behaviour.PrayerActive, PrayerWheel_Prefab.transform);
            behaviour.PrayerLeft.name = PrayerWheelBehaviour.ICON_PRAYER_LEFT_NAME;
            behaviour.PrayerLeft.transform.localPosition = new Vector2(PrayerWheelBehaviour.ICON_PRAYER_LEFT_X, PrayerWheelBehaviour.ICON_PRAYER_Y);

            behaviour.PrayerRight = GameObject.Instantiate<GameObject>(behaviour.PrayerActive, PrayerWheel_Prefab.transform);
            behaviour.PrayerRight.name = PrayerWheelBehaviour.ICON_PRAYER_RIGHT_NAME;
            behaviour.PrayerRight.transform.localPosition = new Vector2(PrayerWheelBehaviour.ICON_PRAYER_RIGHT_X, PrayerWheelBehaviour.ICON_PRAYER_Y);


            // Prayer icons for animation
            behaviour.FakePrayerActive = GameObject.Instantiate<GameObject>(behaviour.PrayerActive, PrayerWheel_Prefab.transform);
            behaviour.FakePrayerActive.name = PrayerWheelBehaviour.ICON_FAKEPRAYER_ACTIVE_NAME;
            behaviour.FakePrayerActive.transform.localPosition = new Vector2(PrayerWheelBehaviour.ICON_PRAYER_ACTIVE_X, PrayerWheelBehaviour.ICON_PRAYER_Y);
            {
                SpriteRenderer renderer = behaviour.FakePrayerActive.GetComponent<SpriteRenderer>();
                renderer.sortingOrder = PrayerWheelBehaviour.ICON_FAKEPRAYER_SORTING_ORDER;
            }

            behaviour.FakePrayerLeft = GameObject.Instantiate<GameObject>(behaviour.FakePrayerActive, PrayerWheel_Prefab.transform);
            behaviour.FakePrayerLeft.name = PrayerWheelBehaviour.ICON_FAKEPRAYER_LEFT_NAME;
            behaviour.FakePrayerLeft.transform.localPosition = new Vector2(PrayerWheelBehaviour.ICON_PRAYER_LEFT_X, PrayerWheelBehaviour.ICON_PRAYER_Y);

            behaviour.FakePrayerRight = GameObject.Instantiate<GameObject>(behaviour.FakePrayerActive, PrayerWheel_Prefab.transform);
            behaviour.FakePrayerRight.name = PrayerWheelBehaviour.ICON_FAKEPRAYER_RIGHT_NAME;
            behaviour.FakePrayerRight.transform.localPosition = new Vector2(PrayerWheelBehaviour.ICON_PRAYER_RIGHT_X, PrayerWheelBehaviour.ICON_PRAYER_Y);

            behaviour.FakePrayerIncoming = GameObject.Instantiate<GameObject>(behaviour.FakePrayerActive, PrayerWheel_Prefab.transform);
            behaviour.FakePrayerIncoming.name = PrayerWheelBehaviour.ICON_FAKEPRAYER_INCOMING_NAME;
            behaviour.FakePrayerIncoming.transform.localPosition = new Vector2(PrayerWheelBehaviour.ICON_PRAYER_ACTIVE_X, PrayerWheelBehaviour.ICON_PRAYER_Y);


            //ModLog.Info("Assemble: Create active prayer frame");
            {
                behaviour.PrayerFrame = GameObject.Instantiate<GameObject>(behaviour.PrayerActive, PrayerWheel_Prefab.transform);
                behaviour.PrayerFrame.name = PrayerWheelBehaviour.ICON_FRAME_NAME;

                SpriteImportOptions importOptions = new()
                {
                    Pivot = new Vector2(PrayerWheelBehaviour.ICON_FRAME_PIVOT_X, PrayerWheelBehaviour.ICON_FRAME_PIVOT_Y)
                };

                SpriteRenderer spriteRenderer = behaviour.PrayerFrame.GetComponent<SpriteRenderer>();
                Sprite sprite;

                Main.PrayerWheelMod.FileHandler.LoadDataAsSprite("ActivePrayerFrame.png",
                                                                  out sprite,
                                                                  importOptions);

                spriteRenderer.sprite = sprite;
                spriteRenderer.sortingOrder = PrayerWheelBehaviour.ICON_FRAME_SORTING_ORDER;

            }

            PrayerWheel_Prefab.SetActive(false); // Disable until deployed

            Main.PrayerWheelMod.PrefabHelper.Store(PrayerWheel_Prefab);

            //ModLog.Info("Assemble: DONE");

            return true;
        }

        private GameObject GetDeployed(Penitent penitent)        
        {
            Transform deployed = penitent.transform.Find(PrayerWheelName);

            return deployed == null? null : deployed.gameObject;
        }

        private bool IsDeployed(Penitent penitent)
        {
            return null != GetDeployed(penitent);
        }

        private void Deploy(Penitent penitent)
        {
            //ModLog.Info("Deploying PrayerWheel");

            if(null == penitent)
            {
                ModLog.Error("Can't deploy PrayerWheel, invalid Penitent");
                return;
            }

            if(null == PrayerWheel_Prefab)
            {
                ModLog.Error("Can't deploy PrayerWheel, not assembled!");
                return;
            }

            if(IsDeployed(penitent))
            {
                ModLog.Info("PrayerWheel already deployed");
                return;
            }

            GameObject deployedPrayerWheel = Main.PrayerWheelMod.PrefabHelper.Instantiate(PrefabName, penitent.transform);
            if (null == deployedPrayerWheel)
            {
                ModLog.Error("Failed to instantiate and deploy PrayerWheel!");
                Disable();
                return;
            }

            deployedPrayerWheel.name = PrayerWheelName;
            deployedPrayerWheel.GetComponent<PrayerWheelBehaviour>().penitent = penitent;
            deployedPrayerWheel.GetComponent<PrayerWheelBehaviour>().InputActionsToBlock_KB = InputActionsToBlock_KB;
            deployedPrayerWheel.GetComponent<PrayerWheelBehaviour>().InputActionsToBlock_JOY = InputActionsToBlock_JOY;
            deployedPrayerWheel.SetActive(true);

            if(!IsDeployed(penitent))
            {
                ModLog.Error("Failed to deploy PrayerWheel!");
                Disable();
                return;
            }           

            //ModLog.Info("PrayerWheel Deployed!");
        }

        private void Remove()
        {
            //ModLog.Info("Removing PrayerWheel...");

            if(null != PrayerWheel_Prefab)
                Object.Destroy(PrayerWheel_Prefab);

            if(null != Core.Logic.Penitent)
            {
                if(IsDeployed(Core.Logic.Penitent))
                {
                    Object.Destroy(GetDeployed(Core.Logic.Penitent));
                }
            }

            //ModLog.Info("PrayerWheel Removed!");
        }
    }
}