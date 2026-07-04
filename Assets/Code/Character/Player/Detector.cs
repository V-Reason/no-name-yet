using UnityEngine;
using RPG2D.Core.Checker;
using RPG2D.Core.Actor;
using System;
using RPG2D.Core.Interaction;
using RPG2D.Item;

namespace RPG2D.Character.Player
{
    [RequireComponent(typeof(InteractableChecker))]
    public class Detector : RPG2D.Core.Detector.BaseDetector<PlayerCheckData>
    {
        private Rigidbody2D rb;
        private InteractableChecker InteractableChecker;

        public bool IsMoving => controller.inputData.Move != Vector2.zero;

        protected void Start()
        {
            rb = GetComponent<StateMachine>().rb;
            InteractableChecker = GetComponent<InteractableChecker>();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            UpdateCheckData();
        }

        private void UpdateCheckData()
        {
            checkData.IsMoving = IsMoving;
            checkData.Velocity = rb.velocity;

            checkData.CanGrab = InteractableChecker.IsConditionMet && InteractableChecker.detectedGrabbable != null;
            checkData.TargetGrabbable = InteractableChecker.detectedGrabbable;
        }
    }
}
