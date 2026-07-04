using RPG2D.Core.Interaction;
using RPG2D.Item;
using UnityEngine;

namespace RPG2D.Character.Player
{
    public class ClimbState : PlayerState
    {
        private Chain currentChain;
        private int currentIdx;
        private float segmentProgress;

        private bool isInputInverted;

        public ClimbState(StateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            stateMachine.animatorWrapper.SetBool(stateMachine.animatorWrapper.IsClimbing, true);

            var target = stateMachine.detector.checkData.TargetGrabbable;
            TransitionToGrabbable(target);
        }

        private void TransitionToGrabbable(IGrabbable target)
        {
            if (target == null) return;

            if (target is Anchor anchor)
            {
                currentChain = anchor.attachedChain;
                currentIdx = 0;
                segmentProgress = 0f;
            }
            else if (target is Chain chain)
            {
                currentChain = chain;
                currentIdx = currentChain.GetClosestNodeIndex(stateMachine.transform.position);
                currentIdx = Mathf.Clamp(currentIdx, 0, currentChain.NodeCount - 2);
                segmentProgress = 0.5f;
            }
            else if (target.GrabType == GrabType.Static)
            {
                stateMachine.transform.position = target.GetGrabPosition(stateMachine.transform.position);
                currentChain = null;
            }

            if (currentChain != null)
            {
                stateMachine.rb.velocity = Vector2.zero;
                stateMachine.rb.isKinematic = true;
            }
        }

        public override void OnUpdate()
        {
            // Q键：解开所有连接
            if (Input.GetKeyDown(KeyCode.Q) && currentChain != null)
            {
                currentChain.TryDisconnectAll();
                isInputInverted = false;
                return;
            }

            // 跳跃离开
            if (Input.GetKeyDown(KeyCode.Space))
            {
                stateMachine.SwitchState<IdleState>();
                return;
            }

            if (currentChain != null)
            {
                HandleSwingLogic();
                HandleClimbMovement();
            }
        }

        private bool isSwinging;

        private void HandleSwingLogic()
        {
            bool shouldSwing = currentIdx <= 1 && Input.GetMouseButton(0);

            if (shouldSwing && !isSwinging)
            {
                isSwinging = true;
                stateMachine.animatorWrapper.SetBool(stateMachine.animatorWrapper.IsClimbing, false);
                stateMachine.animatorWrapper.SetBool(stateMachine.animatorWrapper.IsSwinging, true);
            }
            else if (!shouldSwing && isSwinging)
            {
                isSwinging = false;
                stateMachine.animatorWrapper.SetBool(stateMachine.animatorWrapper.IsSwinging, false);
                stateMachine.animatorWrapper.SetBool(stateMachine.animatorWrapper.IsClimbing, true);
            }

            if (shouldSwing)
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                currentChain.ApplySwingForce(mouseWorldPos, stateMachine.actorData.swingPower);
            }
        }

        private void HandleClimbMovement()
        {
            float rawInputY = stateMachine.controller.inputData.Move.y;
            if (Mathf.Abs(rawInputY) < 0.05f) { isInputInverted = false; }

            float effectiveInputY = isInputInverted ? -rawInputY : rawInputY;

            if (Mathf.Abs(effectiveInputY) < 0.01f)
            {
                UpdatePosition();
                return;
            }

            float delta = -effectiveInputY * stateMachine.actorData.climbSpeed * Time.deltaTime / currentChain.SegLength;
            segmentProgress += delta;

            // --- 向上跨越检测 ---
            if (segmentProgress < 0f)
            {
                if (currentIdx > 0)
                {
                    currentIdx--;
                    segmentProgress = 0.95f;
                }
                else
                {
                    // 在 Index 0 (顶部) 只能检查顶部的 Anchor
                    if (currentChain.parentAnchor != null && currentChain.parentAnchor.incomingHook != null)
                        PerformChainSwitch(currentChain.parentAnchor.incomingHook.ownerChain, HookPointType.Tail);
                    else
                        segmentProgress = 0f;
                }
            }
            // --- 向下跨越检测 ---
            else if (segmentProgress > 1f)
            {
                if (currentIdx < currentChain.NodeCount - 2)
                {
                    currentIdx++;
                    segmentProgress = 0.05f;
                }
                else
                {
                    // 情况 A：我主动钩住了别人
                    var activeTarget = currentChain.hookInstance?.hookedTarget;
                    // 情况 B：别人钩住了我的钩子
                    var passiveIncoming = currentChain.hookInstance?.incomingHook;

                    if (activeTarget != null)
                        PerformChainSwitch(activeTarget.GetRelatedChain(), activeTarget.GetHookPointType());
                    else if (passiveIncoming != null)
                        PerformChainSwitch(passiveIncoming.ownerChain, HookPointType.Tail);
                    else
                        segmentProgress = 1f;
                }
            }

            UpdatePosition();
        }

        private void PerformChainSwitch(Chain newChain, HookPointType entryPoint)
        {
            currentChain = newChain;

            if (entryPoint == HookPointType.Head)
            {
                currentIdx = 0;
                segmentProgress = 0.1f;
                isInputInverted = false;
            }
            else if (entryPoint == HookPointType.Body)
            {
                currentIdx = newChain.HookedNodeIndex;
                currentIdx = Mathf.Clamp(currentIdx, 0, currentChain.NodeCount - 2);
                segmentProgress = 0.5f;
                isInputInverted = false;
            }
            else
            {
                currentIdx = currentChain.NodeCount - 2;
                segmentProgress = 0.9f;
                isInputInverted = true;
                Debug.Log("<color=yellow>进入尾部连接：输入临时反转模式启用</color>");
            }
        }

        private void UpdatePosition()
        {
            Vector2 pA = currentChain.GetNodePos(currentIdx);
            Vector2 pB = currentChain.GetNodePos(currentIdx + 1);
            stateMachine.transform.position = Vector2.Lerp(pA, pB, segmentProgress);

            Vector2 chainDir = (pB - pA).normalized;
            float moveInputY = stateMachine.controller.inputData.Move.y;
            Vector2 facingDir = (moveInputY > 0.1f) ? -chainDir : chainDir;
            stateMachine.actor.RotateVisual(facingDir);
        }

        public override void Exit()
        {
            base.Exit();
            stateMachine.rb.isKinematic = false;
            stateMachine.animatorWrapper.SetBool(stateMachine.animatorWrapper.IsClimbing, false);
            stateMachine.animatorWrapper.SetBool(stateMachine.animatorWrapper.IsSwinging, false);
            isSwinging = false;
        }
    }
}
