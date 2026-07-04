using UnityEngine;
using RPG2D.Core.Checker;
using RPG2D.Core.Actor;
using System;

namespace RPG2D.Character.Player
{
    [RequireComponent(typeof(GrabbableChecker))]
    public class Detector : RPG2D.Core.Detector.BaseDetector<PlayerCheckData>
    {
        private Rigidbody2D rb;
        private GrabbableChecker grabbableChecker;

        public bool IsMoving => controller.inputData.Move != Vector2.zero;

        protected void Start()
        {
            rb = GetComponent<StateMachine>().rb;
            grabbableChecker = GetComponent<GrabbableChecker>();
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

            checkData.CanGrab = grabbableChecker.IsConditionMet;
            checkData.TargetGrabbable = grabbableChecker.detectedGrabbable;
        }
    }
}
