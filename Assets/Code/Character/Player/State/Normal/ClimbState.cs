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

        public ClimbState(StateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
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

        private void HandleSwingLogic()
        {
            // 甩动逻辑：在链条顶端时可以通过鼠标甩动
            if (currentIdx <= 1 && segmentProgress < 0.5f && Input.GetMouseButton(0))
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 dir = (mousePos - (Vector2)stateMachine.transform.position).normalized;
                currentChain.ApplySwingForce(dir * stateMachine.actorData.swingPower * Mathf.Abs(Mathf.Sin(Time.time * 2)));
            }
        }

        private void HandleClimbMovement()
        {
            float inputY = stateMachine.controller.inputData.Move.y;

            if (Mathf.Abs(inputY) < 0.01f)
            {
                UpdatePosition();
                return;
            }

            float delta = -inputY * stateMachine.actorData.climbSpeed * Time.deltaTime / currentChain.SegLength;
            segmentProgress += delta;

            // --- 向上跨越 ---
            if (segmentProgress < 0f)
            {
                if (currentIdx > 0)
                {
                    currentIdx--;
                    segmentProgress = 0.95f;
                }
                else
                {
                    // 已经到顶，检查上方是否有链条钩住了我的锚点
                    var upHook = currentChain.parentAnchor?.incomingHook;
                    if (upHook != null)
                    {
                        currentChain = upHook.ownerChain;
                        currentIdx = currentChain.NodeCount - 2;
                        segmentProgress = 0.95f;
                        Debug.Log("<color=yellow>无缝向上切换</color>");
                    }
                    else
                    {
                        segmentProgress = 0f;
                    }
                }
            }
            // --- 向下跨越 ---
            else if (segmentProgress > 1f)
            {
                if (currentIdx < currentChain.NodeCount - 2)
                {
                    currentIdx++;
                    segmentProgress = 0.05f;
                }
                else
                {
                    // 已经到底，检查钩子是否钩住了东西
                    var target = currentChain.hookInstance?.hookedTarget;
                    if (target != null && target.GetRelatedChain() != null)
                    {
                        currentChain = target.GetRelatedChain();
                        currentIdx = 0;
                        segmentProgress = 0.05f;
                        Debug.Log("<color=yellow>无缝向下切换</color>");
                    }
                    else
                    {
                        segmentProgress = 1f;
                    }
                }
            }

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            Vector2 pA = currentChain.GetNodePos(currentIdx);
            Vector2 pB = currentChain.GetNodePos(currentIdx + 1);
            stateMachine.transform.position = Vector2.Lerp(pA, pB, segmentProgress);
        }

        public override void Exit()
        {
            stateMachine.rb.isKinematic = false;
        }
    }
}
