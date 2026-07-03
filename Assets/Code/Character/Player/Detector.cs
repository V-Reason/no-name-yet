using UnityEngine;
using RPG2D.Core.Checker;
using RPG2D.Core.Actor;
using System;

namespace RPG2D.Character.Player
{
    [RequireComponent(typeof(ChainChecker))]
    public class Detector : RPG2D.Core.Detector.BaseDetector<PlayerCheckData>
    {
        private Rigidbody2D rb;
        private ChainChecker chainChecker;

        public bool IsMoving => controller.inputData.Move != Vector2.zero;

        protected void Start()
        {
            rb = GetComponent<StateMachine>().rb;
            chainChecker = GetComponent<ChainChecker>();
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

            checkData.CanGrabChain = chainChecker.IsConditionMet;
            checkData.TargetChain = chainChecker.detectedChain;
        }
    }
}
