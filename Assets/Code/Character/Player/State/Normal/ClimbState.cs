using RPG2D.Core.Interaction;
using RPG2D.Item;
using UnityEngine;

namespace RPG2D.Character.Player
{
    public class ClimbState : PlayerState
    {
        private Chain chain;
        private int currentIdx;
        private float segmentProgress;

        public ClimbState(StateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            var grabbable = stateMachine.detector.checkData.TargetGrabbable;
            InitializeClimb(grabbable);
        }

        // 提取初始化逻辑，方便切换锁链时复用
        private void InitializeClimb(IGrabbable grabbable)
        {
            if (grabbable == null) return;

            IGrabbable actualTarget = grabbable;

            // --- 核心修复：重定向逻辑 ---
            if (grabbable is Anchor anchor && anchor.attachedChain != null)
            {
                // 如果抓的是锚点，但锚点连着锁链，则视为抓住了锁链
                actualTarget = anchor.attachedChain;
            }
            // -------------------------

            if (actualTarget.GrabType == GrabType.Static)
            {
                stateMachine.transform.position = actualTarget.GetGrabPosition(stateMachine.transform.position);
                chain = null;
            }
            else if (actualTarget.GrabType == GrabType.Linear)
            {
                chain = actualTarget as Chain;

                // 如果是从锚点重定向过来的，直接设为第0个节点
                if (grabbable is Anchor)
                {
                    currentIdx = 0;
                    segmentProgress = 0f;
                }
                else
                {
                    currentIdx = chain.GetClosestNodeIndex(stateMachine.transform.position);
                    currentIdx = Mathf.Clamp(currentIdx, 0, chain.segmentCount - 2);
                    segmentProgress = 0.5f;
                }
            }

            stateMachine.rb.velocity = Vector2.zero;
            stateMachine.rb.isKinematic = true;
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                stateMachine.SwitchState<IdleState>();
                return;
            }

            if (chain != null)
            {
                HandleSwingLogic();
                HandleClimbMovement();
            }
        }

        private void HandleSwingLogic()
        {
            if (chain == null) return;

            // --- 统一的 Q 键逻辑 ---
            if (Input.GetKeyDown(KeyCode.Q))
            {
                // 逻辑：如果在顶端且有东西勾着我，断开上面的；否则断开下面的
                if (currentIdx <= 1 && segmentProgress < 0.5f && chain.incomingHook != null)
                {
                    Debug.Log("断开上方连接");
                    chain.incomingHook.ownerChain.Disconnect();
                }
                else if (chain.isHooked)
                {
                    Debug.Log("断开下方连接");
                    chain.Disconnect();
                }
                return; // 处理完 Q 立即返回
            }

            // 甩动逻辑
            if (currentIdx <= 1 && Input.GetMouseButton(0))
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 dir = (mousePos - (Vector2)stateMachine.transform.position).normalized;
                chain.ApplySwingForce(dir * stateMachine.actorData.swingPower * Mathf.Abs(Mathf.Sin(Time.time * 2)));
            }
        }

        private void HandleClimbMovement()
        {
            float inputY = stateMachine.controller.inputData.Move.y;

            if (Mathf.Abs(inputY) < 0.01f)
            {
                UpdatePlayerPositionOnChain();
                return;
            }

            float delta = -inputY * stateMachine.actorData.climbSpeed * Time.deltaTime / chain.segmentLength;
            segmentProgress += delta;

            // --- 向下跨段 (已经实现) ---
            if (segmentProgress > 1f)
            {
                if (currentIdx < chain.segmentCount - 2)
                {
                    currentIdx++;
                    segmentProgress -= 1f;
                }
                else if (chain.isHooked && chain.HookedTarget != null)
                {
                    var nextGrabbable = (chain.HookedTarget as Component)?.GetComponentInParent<IGrabbable>();
                    if (nextGrabbable != null && nextGrabbable != (IGrabbable)chain)
                    {
                        InitializeClimb(nextGrabbable);
                        currentIdx = 0;
                        segmentProgress = 0.1f;
                        return;
                    }
                }
                else { segmentProgress = 1f; }
            }
            // --- 需求修复：向上跨段 (回到上一条锁链) ---
            else if (segmentProgress < 0f)
            {
                if (currentIdx > 0)
                {
                    currentIdx--;
                    segmentProgress += 1f;
                }
                else if (chain.incomingHook != null) // 如果有上一级锁链钩着我
                {
                    Chain prevChain = chain.incomingHook.ownerChain;
                    InitializeClimb(prevChain);
                    // 修正：回到上一条链子的倒数第二个节点，进度设为末尾
                    currentIdx = prevChain.segmentCount - 2;
                    segmentProgress = 0.95f;
                    Debug.Log("<color=yellow>回爬成功</color>");
                    return;
                }
                else
                {
                    segmentProgress = 0f;
                }
            }

            UpdatePlayerPositionOnChain();
        }

        private void UpdatePlayerPositionOnChain()
        {
            Vector2 posA = chain.GetNodePos(currentIdx);
            Vector2 posB = chain.GetNodePos(currentIdx + 1);
            stateMachine.transform.position = Vector2.Lerp(posA, posB, segmentProgress);
        }

        public override void Exit()
        {
            stateMachine.rb.isKinematic = false;
        }
    }
}
