using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;
using Blasphemous.ModdingAPI.Input;
using Blasphemous.ModdingAPI;
using DG.Tweening;
using Framework.Inventory;
using System.Collections.Generic;

namespace PrayerWheel
{
    // Step 1: Player holds "use" button
    // Step 2: Prayer icons appear, displaying current, prev and next prayers
    // Step 3: Using configured left/right inputs, the currently selected prayer is swapped

    // TODO:
    // - Audio
    // - Block wheel in specific situations (Isidora, Eternal Processions, Golden Visages?, DEMAKE)
    // - Refine InputBlocks that affect wheel (allow swapping during boss intros?)
    // - Slide icons when swapping
    // - Clean up code

    public class PrayerWheel : MonoBehaviour
    {
        // ----- Properties -----

        public Penitent player = null;

        public float timeInputInteractHold = 0.5f;


        public static string INPUT_LEFT_KB = "PrayerWheel_Input_Left_KB";
        public static string INPUT_RIGHT_KB = "PrayerWheel_Input_Right_KB";
        public static string INPUT_LEFT_JOY = "PrayerWheel_Input_Left_JOY";
        public static string INPUT_RIGHT_JOY = "PrayerWheel_Input_Right_JOY";

        public List<int> InputActionsToBlock { get; set; } = new List<int>();
        public static int ACTION_LEFT_KB = -1;
        public static int ACTION_RIGHT_KB = -1;
        public static int ACTION_LEFT_JOY = RewiredConsts.Action.Flask;
        public static int ACTION_RIGHT_JOY = RewiredConsts.Action.Parry;

        // ----- Private Properties -----

        private bool IsInteractButtonHeld { get; set; }


        private string[] InputBlockers = { "DIALOG", "FADE", "BLOCK_UNTIL_FPS_STABLE", "INTERACTABLE", "INVENTORY", "UIBLOCKING" }; //TODO: Add more blockers?

        private float deltaTimeButtonHeld = 0.0f;

        // ----- Private Methods -----

        // TODO: Some of the functions below might be redundant. For example, CurrentState == Playing might overlap with some input blockers

        private bool IsInputBlocked
        {
            get
            {
                foreach (string blockType in InputBlockers)
                {
                    if(Core.Input.HasBlocker(blockType))
                        return true;
                }

                return false;
            }
        }

        private bool IsPlayerInValidState
        {
            get
            {
                return    null != player
                    && !player.Status.Dead;
                    // TODO: Add more states?
            }
        }

        private bool IsGameInValidState
        {
            get
            {
                return Core.Logic.CurrentState == LogicStates.Playing;
            }
        }

        private void ResetHoldStatus()
        {
            // if(IsInteractButtonHeld)
            // {
            //     ModLog.Info($"{name}: Hold status RESET:"
            //                 + (IsInputBlocked?" InputBlocked":"")
            //                 + (!IsPlayerInValidState?" PlayerNotInValidState":"")
            //                 + (!IsGameInValidState?" GameNotInValidState":"")
            //                 + (Main.PrayerWheelMod.InputHandler.GetButtonUp(ButtonCode.Interact)?" ButtonReleased":"")
            //             );
            // }

            deltaTimeButtonHeld = 0.0f;
            IsInteractButtonHeld = false;
        }

        private void CheckIfInteractButtonIsHeld() // TODO: Add LongPress or Hold check to input manager?
        {
            if (    Main.PrayerWheelMod.InputHandler.GetButtonUp(ButtonCode.Interact)
                 || IsInputBlocked
                 || !IsPlayerInValidState
                 || !IsGameInValidState )
            {
                ResetHoldStatus();
                return;
            }

            if (Main.PrayerWheelMod.InputHandler.GetButton(ButtonCode.Interact))
            {                
                deltaTimeButtonHeld += Time.deltaTime;
                if (deltaTimeButtonHeld >= timeInputInteractHold && !IsInteractButtonHeld)
                {
                    deltaTimeButtonHeld = 0.0f;
                    IsInteractButtonHeld = true;                    
                }
            }
        }


        // -- Show/Hide overlay --
       
        public GameObject PrayerFrame { get; set; }
        public GameObject PrayerActive { get; set; }
        public GameObject PrayerLeft { get; set; }
        public GameObject PrayerRight { get; set; }

        private Prayer _emptyPrayer = new Prayer();

        private bool IsOverlayVisible {get; set;}
        private bool IsOverlayTransitioning {get; set;}

        private const float _overlayFadeTime = 0.1f;


        private Color _colorVisibleActive = Color.white;
        private Color _colorVisibleSide = new Color(1f,1f,1f,0.5f);
        private Color _colorInvisible = new Color(1f,1f,1f,0f);
        

        private void OverlayInTransition()
        {
            IsOverlayTransitioning = true;
        }

        private void ShowOverlay()
        {
            if(IsOverlayVisible) return;

            if (!IsOverlayTransitioning)
            {
                UpdatePrayers();

                PrayerFrame.GetComponent<SpriteRenderer>().DOFade(_colorVisibleActive.a, _overlayFadeTime)
                                                          .OnComplete(OverlayVisible).OnUpdate(OverlayInTransition);
                PrayerActive.GetComponent<SpriteRenderer>().DOFade(_colorVisibleActive.a, _overlayFadeTime)
                                                           .OnComplete(OverlayVisible).OnUpdate(OverlayInTransition);
                PrayerLeft.GetComponent<SpriteRenderer>().DOFade(_colorVisibleSide.a, _overlayFadeTime)
                                                         .OnComplete(OverlayVisible).OnUpdate(OverlayInTransition);
                PrayerRight.GetComponent<SpriteRenderer>().DOFade(_colorVisibleSide.a, _overlayFadeTime)
                                                          .OnComplete(OverlayVisible).OnUpdate(OverlayInTransition);
                // TODO: Audio.Appear();
            }
        }

        private void OverlayVisible()
        {
            IsOverlayVisible = true;
            IsOverlayTransitioning = false;

            PrayerFrame.GetComponent<SpriteRenderer>().color = _colorVisibleActive;
            PrayerActive.GetComponent<SpriteRenderer>().color = _colorVisibleActive;
            PrayerLeft.GetComponent<SpriteRenderer>().color   = _colorVisibleSide;
            PrayerRight.GetComponent<SpriteRenderer>().color  = _colorVisibleSide;

            //ModLog.Info($"{name}: Overlay visible!");
        }

        private void HideOverlay()
        {
            if(!IsOverlayVisible) return;

            if (!IsOverlayTransitioning)
            {
                PrayerFrame.GetComponent<SpriteRenderer>().DOFade(_colorInvisible.a, _overlayFadeTime)
                                                          .OnComplete(OverlayInvisible).OnUpdate(OverlayInTransition);
                PrayerActive.GetComponent<SpriteRenderer>().DOFade(_colorInvisible.a, _overlayFadeTime)
                                                           .OnComplete(OverlayInvisible).OnUpdate(OverlayInTransition);
                PrayerLeft.GetComponent<SpriteRenderer>().DOFade(_colorInvisible.a, _overlayFadeTime)
                                                         .OnComplete(OverlayInvisible).OnUpdate(OverlayInTransition);
                PrayerRight.GetComponent<SpriteRenderer>().DOFade(_colorInvisible.a, _overlayFadeTime)
                                                          .OnComplete(OverlayInvisible).OnUpdate(OverlayInTransition);
                // TODO: Audio.Disappear();
            }
        }

        private void OverlayInvisible()
        {
            IsOverlayVisible = false;
            IsOverlayTransitioning = false;

            PrayerFrame.GetComponent<SpriteRenderer>().color = _colorInvisible;
            PrayerActive.GetComponent<SpriteRenderer>().color = _colorInvisible;
            PrayerLeft.GetComponent<SpriteRenderer>().color   = _colorInvisible;
            PrayerRight.GetComponent<SpriteRenderer>().color  = _colorInvisible;

            //ModLog.Info($"{name}: Overlay invisible!");
        }

        // -- Prayers --

        private int GetEquippedPrayerIndex()
        {
            int idx = 0;
            foreach(Prayer prayer in Core.InventoryManager.GetPrayersOwned())
            {
                if(Core.InventoryManager.IsPrayerEquipped(prayer))
                    return idx;
             
                idx++;
            }

            return -1;
        }

        private Prayer GetPrayerActive()
        {
            if(Core.InventoryManager.GetPrayersOwned().Count <= 0) return _emptyPrayer;

            if(!Core.InventoryManager.IsAnyPrayerEquipped()) return _emptyPrayer;
            
            return Core.InventoryManager.GetPrayerInSlot(0);
        }

        private Prayer GetPrayerLeft()
        {
            if(Core.InventoryManager.GetPrayersOwned().Count <= 0) return _emptyPrayer;

            int equippedIndex = GetEquippedPrayerIndex();
            int rightIndex = equippedIndex - 1;
            
            // If we only have one prayer and it is equipped, don't show prayer on the left
            if(Core.InventoryManager.GetPrayersOwned().Count == 1 && equippedIndex >= 0) return _emptyPrayer;

            // Wrap-around
            if(rightIndex < 0) return Core.InventoryManager.GetPrayersOwned()[Core.InventoryManager.GetPrayersOwned().Count - 1];
            
            return Core.InventoryManager.GetPrayersOwned()[rightIndex];
        }

        private Prayer GetPrayerRight()
        {
            if(Core.InventoryManager.GetPrayersOwned().Count <= 0) return _emptyPrayer;

            int equippedIndex = GetEquippedPrayerIndex();
            int rightIndex = equippedIndex + 1;
            
            // If we only have one prayer and it is equipped, don't show prayer on the right
            if(Core.InventoryManager.GetPrayersOwned().Count == 1 && equippedIndex >= 0) return _emptyPrayer;

            // Wrap-around
            if(rightIndex >= Core.InventoryManager.GetPrayersOwned().Count) return Core.InventoryManager.GetPrayersOwned()[0];
            
            return Core.InventoryManager.GetPrayersOwned()[rightIndex];
        }

        private void UpdatePrayers()
        {
            PrayerActive.GetComponent<SpriteRenderer>().sprite = GetPrayerActive().picture;
            PrayerLeft.GetComponent<SpriteRenderer>().sprite   = GetPrayerLeft().picture;
            PrayerRight.GetComponent<SpriteRenderer>().sprite  = GetPrayerRight().picture;
        }

        private void SwapLeft()
        {
            Prayer nextPrayer = GetPrayerLeft();
            if(!Core.InventoryManager.IsPrayerOwned(nextPrayer))
            {
                // ModLog.Info("Can't swap to empty slot");
                return;
            }
            
            Core.InventoryManager.SetPrayerInSlot(0, nextPrayer);
            UpdatePrayers();

            // ModLog.Info("Swapping to the left");
        }

        private void SwapRight()
        {
            Prayer nextPrayer = GetPrayerRight();
            if(!Core.InventoryManager.IsPrayerOwned(nextPrayer))
            {
                // ModLog.Info("Can't swap to empty slot");
                return;
            }
            
            Core.InventoryManager.SetPrayerInSlot(0, nextPrayer);
            UpdatePrayers();

            // ModLog.Info("Swapping to the right");
        }


        // ----- MonoBehaviour methods -----

        private void Awake()
		{
            ModLog.Info($"{name}: PrayerWheel Awaking: Resetting hold status");
            
            IsOverlayVisible       = false;
            IsOverlayTransitioning = false;

            ResetHoldStatus();

            ModLog.Info($"{name}: PrayerWheel Awake");
        }

        private void Start()
        {
            ModLog.Info($"{name}: PrayerWheel Starting...");

            if(null == player)
            {
                ModLog.Error($"{name}: No player assigned to PrayerWheel!");
            }

            // Reassign elements, as cloning the prefab breaks these links
            PrayerFrame  = this.gameObject.transform.Find("PrayerFrame").gameObject;
            PrayerActive = this.gameObject.transform.Find("PrayerActive").gameObject;
            PrayerLeft   = this.gameObject.transform.Find("PrayerLeft").gameObject;
            PrayerRight  = this.gameObject.transform.Find("PrayerRight").gameObject;
            
            UpdatePrayers();
            OverlayInvisible();

            ModLog.Info($"{name}: PrayerWheel Started!");
        }

        // TODO: Disable feature in specific situations, like the fight with Isidora or the final scene in the eternal processions

        private void Update()
        {
            if(null == player)
            {
                return;
            }

            CheckIfInteractButtonIsHeld();

            // Check if button is being held, but do nothing if we don't have prayers
            if (IsInteractButtonHeld && Core.InventoryManager.GetPrayersOwned().Count > 0)
            {
                //ModLog.Info($"{name}: Holding down interact button...");
                ShowOverlay();

                foreach (int action in InputActionsToBlock)
                { 
                    Main.PrayerWheelMod.CustomInputBlocker.SetBlocker("PRAYERWHEEL_BLOCK_" + action, action);
                }

                // TODO: Use above
                Main.PrayerWheelMod.CustomInputBlocker.SetBlocker("PRAYERWHEEL_SCROLL_LEFT_KB", PrayerWheel.ACTION_LEFT_KB);
                Main.PrayerWheelMod.CustomInputBlocker.SetBlocker("PRAYERWHEEL_SCROLL_LEFT_JOY", PrayerWheel.ACTION_LEFT_JOY);
                Main.PrayerWheelMod.CustomInputBlocker.SetBlocker("PRAYERWHEEL_SCROLL_RIGHT_KB", PrayerWheel.ACTION_RIGHT_KB);
                Main.PrayerWheelMod.CustomInputBlocker.SetBlocker("PRAYERWHEEL_SCROLL_RIGHT_JOY", PrayerWheel.ACTION_RIGHT_JOY);
                
                // for(int idx = (int)KeyCode.Joystick1Button0; idx <= (int)KeyCode.Joystick1Button19; idx++)
                // {
                //     if(UnityEngine.Input.GetKeyDown((KeyCode)idx)) ModLog.Info($"Joystick 1 Button {idx} down");
                // }

                if (Main.PrayerWheelMod.InputHandler.GetKeyDown(INPUT_LEFT_KB) || Main.PrayerWheelMod.InputHandler.GetKeyDown(INPUT_LEFT_JOY))
                {
                    SwapLeft();
                }
                else if (Main.PrayerWheelMod.InputHandler.GetKeyDown(INPUT_RIGHT_KB) || Main.PrayerWheelMod.InputHandler.GetKeyDown(INPUT_RIGHT_JOY))
                {
                    SwapRight();
                }
            }
            else
            {
                foreach (int action in InputActionsToBlock)
                { 
                    Main.PrayerWheelMod.CustomInputBlocker.RemoveBlocker("PRAYERWHEEL_BLOCK_" + action);
                }

                // TODO: Use above
                Main.PrayerWheelMod.CustomInputBlocker.RemoveBlocker("PRAYERWHEEL_SCROLL_LEFT_KB");
                Main.PrayerWheelMod.CustomInputBlocker.RemoveBlocker("PRAYERWHEEL_SCROLL_LEFT_JOY");
                Main.PrayerWheelMod.CustomInputBlocker.RemoveBlocker("PRAYERWHEEL_SCROLL_RIGHT_KB");
                Main.PrayerWheelMod.CustomInputBlocker.RemoveBlocker("PRAYERWHEEL_SCROLL_RIGHT_JOY");

                HideOverlay();
            }
        }

        void OnDestroy()
        {
            ResetHoldStatus();
            ModLog.Info($"{name}: PrayerWheel Destroyed");
        }
    }
}