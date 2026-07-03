using UnityEngine;
using RPG2D.Core.Checker;
using RPG2D.Core.Actor;
using System;

namespace RPG2D.Character.Player
{
    public class Detector : RPG2D.Core.Detector.BaseDetector<PlayerCheckData>
    {
        private Rigidbody2D rb;

        public bool IsMoving => controller.inputData.Move != Vector2.zero;

        protected void Start()
        {
            rb = GetComponent<StateMachine>().rb;
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
        }
    }
}
