using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;
using Blasphemous.ModdingAPI.Input;
using Blasphemous.ModdingAPI;
using DG.Tweening;
using Framework.Inventory;
using System.Collections.Generic;
using Rewired;
using Gameplay.GameControllers.Bosses.Isidora;
using Gameplay.UI;

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

    public class PrayerWheelBehaviour : MonoBehaviour
    {
        // ----- Properties -----

        public Penitent penitent = null;

        // Input

        public float timeInputInteractHold = 0.5f;

        public static string INPUT_LEFT_KB { get; } = "PrayerWheel_Input_Left_KB";
        public static string INPUT_RIGHT_KB { get; } = "PrayerWheel_Input_Right_KB";
        public static string INPUT_LEFT_JOY { get; } = "PrayerWheel_Input_Left_JOY";
        public static string INPUT_RIGHT_JOY { get; } = "PrayerWheel_Input_Right_JOY";

        public List<int> InputActionsToBlock_KB { get; set; } = new List<int>();
        public List<int> InputActionsToBlock_JOY { get; set; } = new List<int>();

        // GUI

        public static string ICON_PRAYER_LEFT_NAME { get; } = "PrayerLeft";
        public static string ICON_PRAYER_RIGHT_NAME { get; } = "PrayerRight";
        public static string ICON_PRAYER_ACTIVE_NAME { get; } = "PrayerActive";
        public static string ICON_FRAME_NAME { get; } = "PrayerFrame";
        public static string ICON_FAKEPRAYER_LEFT_NAME { get; } = "FakePrayerLeft";
        public static string ICON_FAKEPRAYER_RIGHT_NAME { get; } = "FakePrayerRight";
        public static string ICON_FAKEPRAYER_ACTIVE_NAME { get; } = "FakePrayerActive";
        public static string ICON_FAKEPRAYER_INCOMING_NAME { get; } = "FakePrayerIncoming";

        public static float ICON_PRAYER_LEFT_X { get; } = -0.8f;
        public static float ICON_PRAYER_ACTIVE_X { get; } = 0.0f;
        public static float ICON_PRAYER_RIGHT_X { get; } = 0.8f;
        public static float ICON_PRAYER_Y { get; } = 0.0f;
        public static float ICON_FRAME_PIVOT_X { get; } = 0.5f;
        public static float ICON_FRAME_PIVOT_Y { get; } = 0.5f;

        public static int ICON_PRAYER_SORTING_ORDER { get; } = 0;
        public static int ICON_FAKEPRAYER_SORTING_ORDER { get; } = 50;
        public static int ICON_FRAME_SORTING_ORDER { get; } = 100;




        // ----- Private Properties -----

        private bool IsInteractButtonHeld { get; set; }


        // Base game input blockers that prevent activating the prayer wheel
        private string[] InputBlockers = { "DIALOG", "FADE", "BLOCK_UNTIL_FPS_STABLE", "INTERACTABLE", "INVENTORY", "UIBLOCKING", "dog_block" };
         //TODO: Add more blockers?

        private float deltaTimeButtonHeld = 0.0f;


        // ----- Private Methods -----

        // TODO: Some of the functions below might be redundant. For example, CurrentState == Playing might overlap with some input blockers

        private bool IsInputBlocked
        {
            get
            {
                foreach (string blockType in InputBlockers)
                {
                    if (Core.Input.HasBlocker(blockType))
                        return true;
                }

                return false;
            }
        }

        private bool IsPlayerInValidState
        {
            get
            {
                return null != penitent
                    && !penitent.Status.Dead;
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

        /// <summary>
        /// In situations where the player is not allowed to use the inventory,
        /// the prayerwheel should be disabled
        /// </summary>
        private bool IsInventoryAllowed
        {
            get
            {
                return UIController.instance.CanOpenInventory;
            }
        }

        /// <summary>
        /// In the base game you can't swap prayers during the Isidora fight, so 
        /// the prayer wheel is also disabled.
        /// </summary>
        /// <returns>true if Isidora boss fight is active, false otherwise</returns>
        private bool IsInIsidoraBossfight()
        {
            return (Core.LevelManager.currentLevel.LevelName.Equals("D01BZ08S01")
                    || Core.LevelManager.currentLevel.LevelName.Equals("D22Z01S18"))
                    && (bool)UnityEngine.Object.FindObjectOfType<IsidoraBehaviour>();
        }

        private void ResetHoldStatus()
        {
            // if(IsInteractButtonHeld)
            // {
            //     ModLog.Info($"{name}: Hold status RESET:"
            //                 + (IsInputBlocked?" InputBlocked":"")
            //                 + (!IsPlayerInValidState?" PlayerNotInValidState":"")
            //                 + (!IsGameInValidState?" GameNotInValidState":"")
            //                 + (!IsInventoryAllowed?" InventoryLocked":"")
            //                 + (IsInIsidoraBossfight?" IsInIsidoraBossfight":"")
            //                 + (Main.PrayerWheelMod.InputHandler.GetButtonUp(ButtonCode.Interact)?" ButtonReleased":"")
            //             );
            // }

            deltaTimeButtonHeld = 0.0f;
            IsInteractButtonHeld = false;
        }

        private void CheckIfInteractButtonIsHeld() // TODO: Add LongPress or Hold check to input manager?
        {
            if (Main.PrayerWheelMod.InputHandler.GetButtonUp(ButtonCode.Interact)
                 || IsInputBlocked
                 || !IsPlayerInValidState
                 || !IsGameInValidState
                 || !IsInventoryAllowed
                 || IsInIsidoraBossfight())
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

        // Fake prayer icons used for animation
        public GameObject FakePrayerActive { get; set; }
        public GameObject FakePrayerLeft { get; set; }
        public GameObject FakePrayerRight { get; set; }
        public GameObject FakePrayerIncoming { get; set; }

        private Prayer _emptyPrayer = new Prayer();

        private bool IsOverlayVisible { get; set; }
        private bool IsOverlayTransitioning { get{ return _overlayTransitionAnimations > 0; } }

        private const float _overlayFadeTime = 0.1f;


        private Color _colorVisibleActive = new Color(1f, 1f, 1f, 1f);
        private Color _colorHalfVisible   = new Color(1f, 1f, 1f, 0.5f);
        private Color _colorInvisible     = new Color(1f, 1f, 1f, 0f);



        private void ShowPrayerIcons()
        {
            PrayerActive.GetComponent<SpriteRenderer>().color = _colorVisibleActive;
            PrayerLeft.GetComponent<SpriteRenderer>().color   = _colorHalfVisible;
            PrayerRight.GetComponent<SpriteRenderer>().color  = _colorHalfVisible;
        }

        private void HidePrayerIcons()
        {
            PrayerActive.GetComponent<SpriteRenderer>().color = _colorInvisible;
            PrayerLeft.GetComponent<SpriteRenderer>().color   = _colorInvisible;
            PrayerRight.GetComponent<SpriteRenderer>().color  = _colorInvisible;
        }

        private void HideFakePrayerIcons()
        {
            FakePrayerActive.GetComponent<SpriteRenderer>().color   = _colorInvisible;
            FakePrayerLeft.GetComponent<SpriteRenderer>().color     = _colorInvisible;
            FakePrayerRight.GetComponent<SpriteRenderer>().color    = _colorInvisible;
            FakePrayerIncoming.GetComponent<SpriteRenderer>().color = _colorInvisible;
        }

        private void ShowOverlay()
        { 
            IsOverlayVisible = true;

            HideFakePrayerIcons();
            PrayerFrame.GetComponent<SpriteRenderer>().color = _colorVisibleActive;
            ShowPrayerIcons();

            //ModLog.Info($"{name}: Overlay visible!");
        }

        private void HideOverlay()
        { 
            IsOverlayVisible = false;            

            HideFakePrayerIcons();
            PrayerFrame.GetComponent<SpriteRenderer>().color = _colorInvisible;
            HidePrayerIcons();

            //ModLog.Info($"{name}: Overlay invisible!");
        }


        private int _overlayTransitionAnimations = 0;

        private void OnOverlayTransitionAnimationStart()
        {
            _overlayTransitionAnimations++;
        }

        private void StartTransitionToShowOverlay()
        {
            if (IsOverlayVisible) return;

            if (!IsOverlayTransitioning)
            {
                UpdatePrayers();

                PrayerFrame.GetComponent<SpriteRenderer>().DOFade(_colorVisibleActive.a, _overlayFadeTime)
                                                            .OnPlay(OnOverlayTransitionAnimationStart)
                                                            .OnComplete(OnAnimationToShowOverlayComplete);
                PrayerActive.GetComponent<SpriteRenderer>().DOFade(_colorVisibleActive.a, _overlayFadeTime)
                                                            .OnPlay(OnOverlayTransitionAnimationStart)
                                                            .OnComplete(OnAnimationToShowOverlayComplete);
                PrayerLeft.GetComponent<SpriteRenderer>().DOFade(_colorHalfVisible.a, _overlayFadeTime)
                                                            .OnPlay(OnOverlayTransitionAnimationStart)
                                                            .OnComplete(OnAnimationToShowOverlayComplete);
                PrayerRight.GetComponent<SpriteRenderer>().DOFade(_colorHalfVisible.a, _overlayFadeTime)
                                                            .OnPlay(OnOverlayTransitionAnimationStart)
                                                            .OnComplete(OnAnimationToShowOverlayComplete);
                // TODO: Audio.Appear();
            }
        }

        private void OnAnimationToShowOverlayComplete()
        {
            _overlayTransitionAnimations--;

            if (_overlayTransitionAnimations < 0)
            {
                ModLog.Error("OnAnimationToShowOverlayComplete: Too many animations ended!");
                _overlayTransitionAnimations = 0;
            }

            if (IsOverlayTransitioning) return;

            ShowOverlay();            
        }

        private void StartTransitionToHideOverlay()
        {
            if (!IsOverlayVisible) return;
            if (IsSwapAnimationRunning) return;

            if (!IsOverlayTransitioning)
            {
                PrayerFrame.GetComponent<SpriteRenderer>().DOFade(_colorInvisible.a, _overlayFadeTime)
                                                            .OnPlay(OnOverlayTransitionAnimationStart)
                                                            .OnComplete(OnAnimationToHideOverlayComplete);
                PrayerActive.GetComponent<SpriteRenderer>().DOFade(_colorInvisible.a, _overlayFadeTime)
                                                            .OnPlay(OnOverlayTransitionAnimationStart)
                                                            .OnComplete(OnAnimationToHideOverlayComplete);
                PrayerLeft.GetComponent<SpriteRenderer>().DOFade(_colorInvisible.a, _overlayFadeTime)
                                                            .OnPlay(OnOverlayTransitionAnimationStart)
                                                            .OnComplete(OnAnimationToHideOverlayComplete);
                PrayerRight.GetComponent<SpriteRenderer>().DOFade(_colorInvisible.a, _overlayFadeTime)
                                                            .OnPlay(OnOverlayTransitionAnimationStart)
                                                            .OnComplete(OnAnimationToHideOverlayComplete);
                // TODO: Audio.Disappear();
            }
        }

        private void OnAnimationToHideOverlayComplete()
        {
            _overlayTransitionAnimations--;

            if (_overlayTransitionAnimations < 0)
            {
                ModLog.Error("OnAnimationToHideOverlayComplete: Too many animations ended!");
                _overlayTransitionAnimations = 0;
            }

            if (IsOverlayTransitioning) return;

            HideOverlay();            
        }

        // -- Blockers --

        private bool _inputActionsBlocked = false;

        private void BlockActionInputs()
        {
            if (_inputActionsBlocked) return;

            switch (GetActiveControllerType())
            {
                case ControllerType.Joystick:
                    {
                        //ModLog.Info("Set Joystick blockers");
                        foreach (int action in InputActionsToBlock_JOY)
                        {
                            Main.PrayerWheelMod.CustomInputBlocker.SetBlocker("PRAYERWHEEL_BLOCK_" + action, action);
                        }

                        break;
                    }
                default:
                    {
                        //ModLog.Info("Set Keyboard blockers");
                        foreach (int action in InputActionsToBlock_KB)
                        {
                            Main.PrayerWheelMod.CustomInputBlocker.SetBlocker("PRAYERWHEEL_BLOCK_" + action, action);
                        }

                        break;
                    }
            }

            _inputActionsBlocked = true;
        }

        private void UnblockActionInputs()
        {
            if (!_inputActionsBlocked) return;

            foreach (int action in InputActionsToBlock_KB)
            {
                Main.PrayerWheelMod.CustomInputBlocker.RemoveBlocker("PRAYERWHEEL_BLOCK_" + action);
            }
            foreach (int action in InputActionsToBlock_JOY)
            {
                Main.PrayerWheelMod.CustomInputBlocker.RemoveBlocker("PRAYERWHEEL_BLOCK_" + action);
            }

            _inputActionsBlocked = false;
        }

        // -- Prayers --

        private int GetEquippedPrayerIndex()
        {
            int idx = 0;
            foreach (Prayer prayer in Core.InventoryManager.GetPrayersOwned())
            {
                if (Core.InventoryManager.IsPrayerEquipped(prayer))
                    return idx;

                idx++;
            }

            return -1;
        }

        private bool IsAnyPrayerEquipped()
        {
            return GetEquippedPrayerIndex() >= 0;
        }

        private Prayer GetPrayerActive()
        {
            if (Core.InventoryManager.GetPrayersOwned().Count <= 0) return _emptyPrayer;

            if (!Core.InventoryManager.IsAnyPrayerEquipped()) return _emptyPrayer;

            return Core.InventoryManager.GetPrayerInSlot(0);
        }

        private Prayer GetPrayerLeft()
        {
            if (Core.InventoryManager.GetPrayersOwned().Count <= 0) return _emptyPrayer;

            int equippedIndex = GetEquippedPrayerIndex();
            int leftIndex = equippedIndex - 1;

            // If we only have one prayer and it is equipped, don't show prayer on the left
            if (Core.InventoryManager.GetPrayersOwned().Count == 1 && IsAnyPrayerEquipped()) return _emptyPrayer;

            // Wrap-around
            if (leftIndex < 0)
            {
                leftIndex = Core.InventoryManager.GetPrayersOwned().Count - 1;
            }

            return Core.InventoryManager.GetPrayersOwned()[leftIndex];
        }

        private Prayer GetPrayerRight()
        {
            if (Core.InventoryManager.GetPrayersOwned().Count <= 0) return _emptyPrayer;

            int equippedIndex = GetEquippedPrayerIndex();
            int rightIndex = equippedIndex + 1;

            // If we only have one prayer and it is equipped, don't show prayer on the right
            if (Core.InventoryManager.GetPrayersOwned().Count == 1 && IsAnyPrayerEquipped()) return _emptyPrayer;

            // Wrap-around
            if (rightIndex >= Core.InventoryManager.GetPrayersOwned().Count)
            {
                rightIndex = 0;
            }

            return Core.InventoryManager.GetPrayersOwned()[rightIndex];
        }
        
        private Prayer GetPrayerIncomingLeft()
        {
            if (Core.InventoryManager.GetPrayersOwned().Count <= 0) return _emptyPrayer;

            int equippedIndex = GetEquippedPrayerIndex();

            if (Core.InventoryManager.GetPrayersOwned().Count == 1 && IsAnyPrayerEquipped()) return _emptyPrayer;

            int leftIncomingIndex = equippedIndex<0? -2 : equippedIndex - 2;

            // Wrap-around
            if (leftIncomingIndex < 0)
            {
                leftIncomingIndex += Core.InventoryManager.GetPrayersOwned().Count;
            }
    
            return Core.InventoryManager.GetPrayersOwned()[leftIncomingIndex];
        }

        private Prayer GetPrayerIncomingRight()
        {
            if (Core.InventoryManager.GetPrayersOwned().Count <= 0) return _emptyPrayer;

            int equippedIndex = GetEquippedPrayerIndex();

            if (Core.InventoryManager.GetPrayersOwned().Count == 1 && IsAnyPrayerEquipped()) return _emptyPrayer;

            int rightIncomingIndex = equippedIndex<0? 1 : equippedIndex + 2;

            // Wrap-around
            if (rightIncomingIndex >= Core.InventoryManager.GetPrayersOwned().Count)
            {
                rightIncomingIndex -= Core.InventoryManager.GetPrayersOwned().Count;
            }
    
            return Core.InventoryManager.GetPrayersOwned()[rightIncomingIndex];
        }

        private void UpdatePrayers()
        {
            PrayerActive.GetComponent<SpriteRenderer>().sprite = GetPrayerActive().picture;
            PrayerLeft.GetComponent<SpriteRenderer>().sprite = GetPrayerLeft().picture;
            PrayerRight.GetComponent<SpriteRenderer>().sprite = GetPrayerRight().picture;
        }

        private void SwapLeft()
        {
            if (!IsOverlayVisible) return;
            if (IsOverlayTransitioning) return;
            if (IsSwapAnimationRunning) return;

            SlidePrayersLeft();

            Prayer nextPrayer = GetPrayerLeft();
            if (!Core.InventoryManager.IsPrayerOwned(nextPrayer))
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
            if (!IsOverlayVisible) return;
            if (IsOverlayTransitioning) return;
            if (IsSwapAnimationRunning) return;

            SlidePrayersRight();

            Prayer nextPrayer = GetPrayerRight();
            if (!Core.InventoryManager.IsPrayerOwned(nextPrayer))
            {
                // ModLog.Info("Can't swap to empty slot");
                return;
            }

            Core.InventoryManager.SetPrayerInSlot(0, nextPrayer);
            UpdatePrayers();

            // ModLog.Info("Swapping to the right");
        }

        // ----- Prayer Swap animation -----


        private int _swapAnimations = 0;

        private bool IsSwapAnimationRunning { get { return _swapAnimations > 0; } }

        private const float _slideAnimationRunTime = 0.2f;

        private void OnSwapAnimationStarted()
        {
            _swapAnimations++;
        }

        private void OnSwapAnimationEnded()
        {
            _swapAnimations--;

            if (IsSwapAnimationRunning) return;

            ShowPrayerIcons();
            HideFakePrayerIcons();            
        }

        private void SlidePrayersLeft()
        {
            if (!IsSwapAnimationRunning)
            {
                HidePrayerIcons();

                SpriteRenderer FakePrayerActiveRenderer   = FakePrayerActive.GetComponent<SpriteRenderer>();
                SpriteRenderer FakePrayerLeftRenderer     = FakePrayerLeft.GetComponent<SpriteRenderer>();
                SpriteRenderer FakePrayerRightRenderer    = FakePrayerRight.GetComponent<SpriteRenderer>();
                SpriteRenderer FakePrayerIncomingRenderer = FakePrayerIncoming.GetComponent<SpriteRenderer>();

                FakePrayerActiveRenderer.sprite   = GetPrayerActive().picture;
                FakePrayerLeftRenderer.sprite     = GetPrayerLeft().picture;
                FakePrayerRightRenderer.sprite    = GetPrayerRight().picture;
                FakePrayerIncomingRenderer.sprite = GetPrayerIncomingLeft().picture;

                FakePrayerActiveRenderer.color   = _colorVisibleActive;
                FakePrayerLeftRenderer.color     = _colorHalfVisible;
                FakePrayerRightRenderer.color    = _colorHalfVisible;
                FakePrayerIncomingRenderer.color = _colorInvisible;

                FakePrayerActive.transform.localPosition   = new Vector2(ICON_PRAYER_ACTIVE_X, ICON_PRAYER_Y);
                FakePrayerLeft.transform.localPosition     = new Vector2(ICON_PRAYER_LEFT_X, ICON_PRAYER_Y);
                FakePrayerRight.transform.localPosition    = new Vector2(ICON_PRAYER_RIGHT_X, ICON_PRAYER_Y);
                FakePrayerIncoming.transform.localPosition = new Vector2(ICON_PRAYER_LEFT_X, ICON_PRAYER_Y);

                FakePrayerActiveRenderer.DOFade(_colorHalfVisible.a, _slideAnimationRunTime)
                                            .OnPlay(OnSwapAnimationStarted)
                                            .OnComplete(OnSwapAnimationEnded);
                FakePrayerLeftRenderer.DOFade(_colorVisibleActive.a, _slideAnimationRunTime)
                                        .OnPlay(OnSwapAnimationStarted)
                                        .OnComplete(OnSwapAnimationEnded);
                if (IsAnyPrayerEquipped()) FakePrayerRightRenderer.DOFade(_colorInvisible.a, _slideAnimationRunTime)
                                                                    .OnPlay(OnSwapAnimationStarted)
                                                                    .OnComplete(OnSwapAnimationEnded);
                FakePrayerIncomingRenderer.DOFade(_colorHalfVisible.a, _slideAnimationRunTime)
                                            .OnPlay(OnSwapAnimationStarted)
                                            .OnComplete(OnSwapAnimationEnded);

                FakePrayerLeft.transform.DOLocalMoveX(ICON_PRAYER_ACTIVE_X, _slideAnimationRunTime).SetEase(Ease.OutCubic)
                                            .OnPlay(OnSwapAnimationStarted)
                                            .OnComplete(OnSwapAnimationEnded);
                FakePrayerActive.transform.DOLocalMoveX(ICON_PRAYER_RIGHT_X, _slideAnimationRunTime).SetEase(Ease.OutCubic)
                                            .OnPlay(OnSwapAnimationStarted)
                                            .OnComplete(OnSwapAnimationEnded);
            }
        }

        private void SlidePrayersRight()
        {
            if (!IsSwapAnimationRunning)
            {
                HidePrayerIcons();

                SpriteRenderer FakePrayerActiveRenderer   = FakePrayerActive.GetComponent<SpriteRenderer>();
                SpriteRenderer FakePrayerLeftRenderer     = FakePrayerLeft.GetComponent<SpriteRenderer>();
                SpriteRenderer FakePrayerRightRenderer    = FakePrayerRight.GetComponent<SpriteRenderer>();
                SpriteRenderer FakePrayerIncomingRenderer = FakePrayerIncoming.GetComponent<SpriteRenderer>();

                FakePrayerActiveRenderer.sprite   = GetPrayerActive().picture;
                FakePrayerLeftRenderer.sprite     = GetPrayerLeft().picture;
                FakePrayerRightRenderer.sprite    = GetPrayerRight().picture;
                FakePrayerIncomingRenderer.sprite = GetPrayerIncomingRight().picture;

                FakePrayerActiveRenderer.color   = _colorVisibleActive;
                FakePrayerLeftRenderer.color     = _colorHalfVisible;
                FakePrayerRightRenderer.color    = _colorHalfVisible;
                FakePrayerIncomingRenderer.color = _colorInvisible;

                FakePrayerActive.transform.localPosition   = new Vector2(ICON_PRAYER_ACTIVE_X, ICON_PRAYER_Y);
                FakePrayerLeft.transform.localPosition     = new Vector2(ICON_PRAYER_LEFT_X, ICON_PRAYER_Y);
                FakePrayerRight.transform.localPosition    = new Vector2(ICON_PRAYER_RIGHT_X, ICON_PRAYER_Y);
                FakePrayerIncoming.transform.localPosition = new Vector2(ICON_PRAYER_RIGHT_X, ICON_PRAYER_Y);

                FakePrayerActiveRenderer.DOFade(_colorHalfVisible.a, _slideAnimationRunTime)
                                            .OnPlay(OnSwapAnimationStarted)
                                            .OnComplete(OnSwapAnimationEnded);
                if (IsAnyPrayerEquipped()) FakePrayerLeftRenderer.DOFade(_colorInvisible.a, _slideAnimationRunTime)
                                                                    .OnPlay(OnSwapAnimationStarted)
                                                                    .OnComplete(OnSwapAnimationEnded);
                FakePrayerRightRenderer.DOFade(_colorVisibleActive.a, _slideAnimationRunTime)
                                            .OnPlay(OnSwapAnimationStarted)
                                            .OnComplete(OnSwapAnimationEnded);
                FakePrayerIncomingRenderer.DOFade(_colorHalfVisible.a, _slideAnimationRunTime)
                                            .OnPlay(OnSwapAnimationStarted)
                                            .OnComplete(OnSwapAnimationEnded);

                FakePrayerRight.transform.DOLocalMoveX(0.0f, _slideAnimationRunTime).SetEase(Ease.OutCubic)
                                            .OnPlay(OnSwapAnimationStarted)
                                            .OnComplete(OnSwapAnimationEnded);
                FakePrayerActive.transform.DOLocalMoveX(ICON_PRAYER_LEFT_X, _slideAnimationRunTime).SetEase(Ease.OutCubic)
                                            .OnPlay(OnSwapAnimationStarted)
                                            .OnComplete(OnSwapAnimationEnded);
            }
        }


        // ----- MonoBehaviour methods -----

        private void Awake()
        {
            //ModLog.Info($"{name}: PrayerWheel Awaking: Resetting hold status");

            IsOverlayVisible = false;
            _overlayTransitionAnimations = 0;

            ResetHoldStatus();

            //ModLog.Info($"{name}: PrayerWheel Awake");
        }

        private void Start()
        {
            //ModLog.Info($"{name}: PrayerWheel Starting...");

            if (null == penitent)
            {
                ModLog.Error($"{name}: No player assigned to PrayerWheel!");
            }

            // Reassign elements, as cloning the prefab breaks these links
            PrayerFrame  = this.gameObject.transform.Find(ICON_FRAME_NAME).gameObject;
            PrayerActive = this.gameObject.transform.Find(ICON_PRAYER_ACTIVE_NAME).gameObject;
            PrayerLeft   = this.gameObject.transform.Find(ICON_PRAYER_LEFT_NAME).gameObject;
            PrayerRight  = this.gameObject.transform.Find(ICON_PRAYER_RIGHT_NAME).gameObject;
            FakePrayerActive    = this.gameObject.transform.Find(ICON_FAKEPRAYER_ACTIVE_NAME).gameObject;
            FakePrayerLeft      = this.gameObject.transform.Find(ICON_FAKEPRAYER_LEFT_NAME).gameObject;
            FakePrayerRight     = this.gameObject.transform.Find(ICON_FAKEPRAYER_RIGHT_NAME).gameObject;
            FakePrayerIncoming  = this.gameObject.transform.Find(ICON_FAKEPRAYER_INCOMING_NAME).gameObject;

            UpdatePrayers();
            HideOverlay();

            //ModLog.Info($"{name}: PrayerWheel Started!");
        }

        private void Update()
        {
            if (null == penitent)
            {
                return;
            }

            CheckIfInteractButtonIsHeld();

            // Check if button is being held, but do nothing if we don't have prayers
            if (IsInteractButtonHeld && Core.InventoryManager.GetPrayersOwned().Count > 0)
            {
                //ModLog.Info($"{name}: Holding down interact button...");
                BlockActionInputs();

                StartTransitionToShowOverlay();

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
                UnblockActionInputs();

                StartTransitionToHideOverlay();
            }
        }

        void OnDestroy()
        {
            ResetHoldStatus();
            UnblockActionInputs();
            //ModLog.Info($"{name}: PrayerWheel Destroyed");
        }

        private ControllerType GetActiveControllerType()
        {
            return Rewired.ReInput.players.GetPlayer(0)
                        .controllers.maps.GetMap(Core.Input.ActiveController.type,
                                                 Core.Input.ActiveController.id,
                                                 "Default",
                                                 "Default").controllerType;
        }
    }
}