using RPG2D.Core.Controller;
using RPG2D.Core.Data;
using RPG2D.Core.StateMachine;
using UnityEngine;

namespace RPG2D.Core.Actor
{
    /// <summary>
    /// 方向枚举
    /// </summary>
    public enum Direction
    {
        Right = 1,
        Middle = 0,
        Left = -1,
    }

    /// <summary>
    /// 玩家对象行为执行器，以及数值存放（血量，体力等）
    /// 不用于逻辑判断，仅仅会执行相应动作，
    /// 是否应该执行交给上层处理
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class BaseActor<TActorData> :
        MonoBehaviour, IActor
        where TActorData : ActorData
    {
        // 内部参数
        protected TActorData actorData;
        protected IController controller;

        // 对外接口
        public Rigidbody2D rb { get; protected set; }
        [HideInInspector]
        public Direction currFacing { get; protected set; } = Direction.Right;
        [HideInInspector]
        public bool canMove = true;
        [HideInInspector]
        public bool canFlip = true;

        protected virtual void Start()
        {
            // 目前为空
        }

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            actorData = GetComponent<IActorStateMachine<TActorData>>().actorData;
            controller = GetComponent<IController>();
        }

        // 禁用接口
        [System.Obsolete("该方法已禁用", true)]
        private void OnUpdate() { }


        // 应用速度
        public virtual void ApplyVelocity(Vector2 velocity) =>
            rb.velocity = velocity;
        // 应用冲量
        public virtual void ApplyImpulse(Vector2 impulse) =>
            rb.AddForce(impulse, ForceMode2D.Impulse);
        // 应用力
        public virtual void ApplyForce(Vector2 force) =>
            rb.AddForce(force, ForceMode2D.Force);
        // 停止
        public virtual void Stop() =>
            rb.velocity = Vector2.zero;

        // 移动
        public virtual void Move()
        {
            if (!canMove) return;

            // inputData内含角色移动的速度参数
            InputData input = controller.inputData;

            float axis = input.Move.x;
            if (Mathf.Abs(axis) <= 0.01f) return;

            float targetSpeed = axis * actorData.moveSpeed; // 目标速度

            bool isMovingSameDir = Mathf.Sign(axis) == Mathf.Sign(rb.velocity.x);
            bool isReachedTarget = Mathf.Abs(rb.velocity.x) >= Mathf.Abs(targetSpeed);

            if (isMovingSameDir && isReachedTarget) return;

            float speedDif = targetSpeed - rb.velocity.x;        // 速度差
            float impulse = speedDif * rb.mass / 10;             // 加速动量

            ApplyImpulse(Vector2.right * impulse);

            HandleFlip(axis);
        }

        // 处理转向
        public virtual void HandleFlip(float axis)
        {
            // 控制转向
            if (axis == 0) return;

            Direction tarFacing = axis > 0 ? Direction.Right : Direction.Left;
            if (tarFacing != currFacing)
            {
                FlipTo(tarFacing);
                // Flip();
            }
        }

        // 转向
        public virtual void Flip()
        {
            if (!canFlip) return;
            currFacing = currFacing == Direction.Left ? Direction.Right : Direction.Left;
            transform.localScale = new Vector3((float)currFacing, transform.localScale.y, transform.localScale.z);
        }

        public virtual void FlipTo(Direction tarFacing)
        {
            if (!canFlip) return;
            currFacing = tarFacing;
            transform.localScale = new Vector3((float)currFacing, transform.localScale.y, transform.localScale.z);
        }

        // 限制速度
        public void CapSpeed(float maxSpeed)
        {
            float sqrMax = maxSpeed * maxSpeed;
            float sqrVel = rb.velocity.sqrMagnitude;
            if (sqrVel > sqrMax)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }

    }
}
