using RPG2D.Core.Data;
using RPG2D.Core.Interaction;
using RPG2D.Item;
using UnityEngine;

namespace RPG2D.Character.Player
{
    public class ClimbState : PlayerState
    {
        private Chain chain;
        private int currentIdx;
        private float segmentProgress; // 0-1 进度

        public ClimbState(StateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            var grabbable = stateMachine.detector.checkData.TargetGrabbable;

            Debug.Log($"{grabbable?.GetTransform()}");

            if (grabbable.GrabType == GrabType.Static)
            {
                // 锁定到锚点
                stateMachine.transform.position = grabbable.GetGrabPosition(stateMachine.transform.position);
            }
            else if (grabbable.GrabType == GrabType.Linear)
            {
                chain = grabbable as Chain;
                // 锁定到最近的节点
                currentIdx = chain.GetClosestNodeIndex(stateMachine.transform.position);
                currentIdx = UnityEngine.Mathf.Min(currentIdx, chain.segmentCount - 2);
                segmentProgress = 0.5f;

            }

            // 减速
            stateMachine.rb.velocity = UnityEngine.Vector2.zero;
            stateMachine.rb.isKinematic = true; // 物理静默，防止推开链子
        }

        public override void OnUpdate()
        {
            // 退出条件：按交互键或跳跃键（这里根据你的输入逻辑定）
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space))
            {
                stateMachine.SwitchState<IdleState>();
                return;
            }

            HandleSwingLogic();
            HandleClimbMovement();
        }

        // 在 ClimbState.cs 的 OnUpdate 中添加：
        private void HandleSwingLogic()
        {
            // 只有在链条顶端（锚点）才能甩
            if (currentIdx > 1) return;

            if (Input.GetMouseButton(0)) // 按住左键甩动
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 dir = (mousePos - (Vector2)stateMachine.transform.position).normalized;

                // 这里的力大小可以根据需求调整
                float swingPower = stateMachine.actorData.swingPower;
                chain.ApplySwingForce(dir * swingPower * Mathf.Abs(Mathf.Sin(Time.time)));
            }

            // 解开钩子逻辑
            if (Input.GetKeyDown(KeyCode.Q))
            {
                // 检测玩家是否在钩子附近
                float distToHook = Vector2.Distance(stateMachine.transform.position, chain.GetHookPosition());
                if (distToHook < 1.5f) // 距离阈值
                {
                    chain.Disconnect();
                }
            }
        }

        private void HandleClimbMovement()
        {
            // 获取垂直输入 (W/Up 为正, S/Down 为负)
            float inputY = stateMachine.controller.inputData.Move.y;
            if (UnityEngine.Mathf.Abs(inputY) < 0.01f) return;

            // --- 核心修复：在这里加一个负号 ---
            // 因为索引 0 是顶端，向上爬意味着我们要减小索引/进度
            float delta = -inputY * stateMachine.actorData.climbSpeed * UnityEngine.Time.deltaTime / chain.segmentLength;

            segmentProgress += delta;

            // 跨段逻辑处理
            if (segmentProgress > 1f)
            {
                // 进度 > 1 说明在向索引大的方向（下方）移动
                if (currentIdx < chain.segmentCount - 2)
                {
                    currentIdx++;
                    segmentProgress -= 1f;
                }
                else
                {
                    segmentProgress = 1f; // 到达链条最末端
                }
            }
            else if (segmentProgress < 0f)
            {
                // 进度 < 0 说明在向索引小的方向（上方）移动
                if (currentIdx > 0)
                {
                    currentIdx--;
                    segmentProgress += 1f;
                }
                else
                {
                    segmentProgress = 0f; // 到达锚点
                }
            }

            // 插值设置位置
            UnityEngine.Vector2 posA = chain.GetNodePos(currentIdx);
            UnityEngine.Vector2 posB = chain.GetNodePos(currentIdx + 1);
            stateMachine.transform.position = UnityEngine.Vector2.Lerp(posA, posB, segmentProgress);
        }

        public override void Exit()
        {
            stateMachine.rb.isKinematic = false;
        }
    }
}
