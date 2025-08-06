using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;
using Blasphemous.ModdingAPI;
using System.Collections.Generic;
using Blasphemous.ModdingAPI.Files;

namespace PrayerWheel
{
    // TODO: Allow selecting prayers without opening the menu

    // Step 1: Player holds "use" button
    // Step 2: Prayer icons appear above Penitent, displaying current, prev and next prayers (icons appear below if penitent is too close to screen top)
    // Step 3: Using left/right keys (or camera stick) the currently selected prayer is swapped


    /// <summary>
    /// TODO
    /// </summary>
    public class PrayerWheelDeployer
    {

        public PrayerWheelDeployer()
        {
            if (!Assemble())
            {
                ModLog.Error("Failed to assemble PrayerWheel! Feature can't be enabled!");
                Disable();
                return;
            }

            SpawnManager.OnPlayerSpawn += Deploy;

            ModLog.Info("PrayerWheel Feature Enabled!");
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
            ModLog.Info("Assemble: START");


            ModLog.Info("Assemble: Instantiate object");

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

            ModLog.Info("Assemble: Create prayer icons");
            behaviour.PrayerActive = new GameObject("PrayerActive");
            {
                behaviour.PrayerActive.transform.localPosition = new Vector2(0f, 0f);
                SpriteRenderer renderer = behaviour.PrayerActive.AddComponent<SpriteRenderer>();
                renderer.sortingLayerName = "In-Game UI";
                renderer.sortingOrder = 0;

                behaviour.PrayerActive.transform.parent = PrayerWheel_Prefab.transform;
            }

            behaviour.PrayerLeft = GameObject.Instantiate<GameObject>(behaviour.PrayerActive, PrayerWheel_Prefab.transform);
            behaviour.PrayerLeft.name = "PrayerLeft";
            behaviour.PrayerLeft.transform.localPosition = new Vector2(-0.8f, 0f);

            behaviour.PrayerRight = GameObject.Instantiate<GameObject>(behaviour.PrayerActive, PrayerWheel_Prefab.transform);
            behaviour.PrayerRight.name = "PrayerRight";
            behaviour.PrayerRight.transform.localPosition = new Vector2(0.8f, 0f);


            ModLog.Info("Assemble: Create active prayer frame");
            {
                behaviour.PrayerFrame = GameObject.Instantiate<GameObject>(behaviour.PrayerActive, PrayerWheel_Prefab.transform);
                behaviour.PrayerFrame.name = "PrayerFrame";

                SpriteImportOptions importOptions = new()
                {
                    Pivot = new Vector2(0.5f, 0.5f)
                };

                SpriteRenderer spriteRenderer = behaviour.PrayerFrame.GetComponent<SpriteRenderer>();
                Sprite sprite;

                Main.PrayerWheelMod.FileHandler.LoadDataAsSprite("ActivePrayerFrame.png",
                                                                  out sprite,
                                                                  importOptions);

                spriteRenderer.sprite = sprite;
                spriteRenderer.sortingOrder = 100;

            }

            PrayerWheel_Prefab.SetActive(false); // Disable until deployed

            Main.PrayerWheelMod.PrefabHelper.Store(PrayerWheel_Prefab);

            ModLog.Info("Assemble: DONE");

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
            ModLog.Info("Deploying PrayerWheel");

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

            ModLog.Info("PrayerWheel Deployed!");
        }

        private void Remove()
        {
            ModLog.Info("Removing PrayerWheel...");

            if(null != PrayerWheel_Prefab)
                Object.Destroy(PrayerWheel_Prefab);

            if(null != Core.Logic.Penitent)
            {
                if(IsDeployed(Core.Logic.Penitent))
                {
                    Object.Destroy(GetDeployed(Core.Logic.Penitent));
                }
            }

            ModLog.Info("PrayerWheel Removed!");
        }
    }
}