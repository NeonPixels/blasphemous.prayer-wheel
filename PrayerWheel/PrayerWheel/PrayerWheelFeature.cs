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
    public class PrayerWheelFeature
    {
        public bool IsEnabled { get; private set; }

        public void Enable()
        {
            ModLog.Info("PrayerWheel Enable START");

            if(!Assemble())
            {
                ModLog.Error("Failed to assemble PrayerWheel! Feature can't be enabled!");
                Disable();
                return;
            }

		    SpawnManager.OnPlayerSpawn += Deploy;
            
            // TODO: Make configurable
            Main.PrayerWheelMod.InputHandler.RegisterDefaultKeybindings(new Dictionary<string, KeyCode>()
            {
                { PrayerWheel.INPUT_LEFT_KB, KeyCode.O },
                { PrayerWheel.INPUT_RIGHT_KB, KeyCode.P },
                { PrayerWheel.INPUT_LEFT_JOY, KeyCode.JoystickButton4 },
                { PrayerWheel.INPUT_RIGHT_JOY, KeyCode.JoystickButton5 }
            });

            IsEnabled = true;
            ModLog.Info("PrayerWheel Feature Enabled!");
        }

        public void Disable()
        {
            ModLog.Info("PrayerWheel Disable START");

            SpawnManager.OnPlayerSpawn -= Deploy;
            Remove();

            IsEnabled = false;
            
            ModLog.Info("PrayerWheel Feature Disabled!");
        }

        public PrayerWheelFeature()
        {
            IsEnabled = false;
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

        private InputHandling _inputHandling = new InputHandling();
        
        private bool Assemble()
        {
            ModLog.Info("PrayerWheelFeature::Assemble: START");


            ModLog.Info("PrayerWheelFeature::Assemble: Instantiate object");

            PrayerWheel_Prefab = new GameObject(PrefabName);
            if (null == PrayerWheel_Prefab)
            {
                ModLog.Error("PrayerWheelFeature::Assemble: Failed to instantiate object!");
                return false;
            }
            PrayerWheel_Prefab.transform.localPosition = new Vector2(0f, 0f);

            PrayerWheel behaviour = PrayerWheel_Prefab.AddComponent<PrayerWheel>();
            if (null == behaviour)
            {
                ModLog.Error("PrayerWheelFeature::Assemble: Failed to instantiate behaviour!");
                return false;
            }

            ModLog.Info("PrayerWheelFeature::Assemble: Create prayer icons");
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


            ModLog.Info("PrayerWheelFeature::Assemble: Create active prayer frame");
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

            // TODO: Move this to initialization
            // ModLog.Info("PrayerWheelFeature::Assemble: Get input actions to block from configured bindings");
            // {
            //     behaviour.InputActionsToBlock.Clear();

            //     foreach (KeyCode binding in _inputHandling.GetConfiguredBindings())
            //     {
            //         foreach (int action in _inputHandling.GetAssignedActions(binding))
            //         {
            //             if (behaviour.InputActionsToBlock.Contains(action)) continue;

            //             behaviour.InputActionsToBlock.Add(action);
            //         }
            //     }
            // }


            PrayerWheel_Prefab.SetActive(false); // Disable until deployed

            Main.PrayerWheelMod.PrefabSceneFeature.Store(PrayerWheel_Prefab);

            ModLog.Info("PrayerWheelFeature::Assemble: DONE");

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

           GameObject deployedPrayerWheel = Main.PrayerWheelMod.PrefabSceneFeature.Instantiate(PrefabName, penitent.transform);
            if(null == deployedPrayerWheel)
            {
                ModLog.Error("Failed to instantiate and deploy PrayerWheel!");
                Disable();
                return;
            }

            deployedPrayerWheel.name = PrayerWheelName;
            deployedPrayerWheel.GetComponent<PrayerWheel>().player = penitent;
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
            ModLog.Info("Removing PrayerWheel feature...");

            if(null != PrayerWheel_Prefab)
                Object.Destroy(PrayerWheel_Prefab);

            if(null != Core.Logic.Penitent)
            {
                if(IsDeployed(Core.Logic.Penitent))
                {
                    Object.Destroy(GetDeployed(Core.Logic.Penitent));
                }
            }

            ModLog.Info("PrayerWheel feature Removed!");
        }
    
    }
}