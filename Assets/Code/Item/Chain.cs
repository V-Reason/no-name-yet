using System.Collections.Generic;
using RPG2D.Core.Interaction;
using UnityEngine;

namespace RPG2D.Item
{

    [ExecuteAlways]
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(EdgeCollider2D))]
    public class Chain : MonoBehaviour, IGrabbable
    {
        private EdgeCollider2D edgeCollider;

        [System.Serializable]
        public struct Node
        {
            public Vector2 pos;
            public Vector2 oldPos;
            public bool isFixed;

            public Node(Vector2 initialPos, bool fixedPoint = false)
            {
                pos = oldPos = initialPos;
                isFixed = fixedPoint;
            }
        }

        [Header("链条结构设置")]
        [Tooltip("锚点")]
        public Transform anchor;
        [Tooltip("链条尾巴")]
        public Transform tailStartPoint;
        [Tooltip("链条段数")]
        public int segmentCount = 10;
        [Tooltip("每段长度")]
        public float segmentLength = 1f;

        [Header("物理参数设置")]
        [Tooltip("重力参数")]
        public Vector2 gravity = new Vector2(0, -1f);
        [Tooltip("运动阻力（值越小阻力越大）")]
        [Range(0f, 1f)] public float drag = 0.95f;
        [Tooltip("物理计算密度")]
        public int constraintIterations = 15;

        [Header("碰撞设置")]
        public bool enableCollision = true;
        [Tooltip("碰撞目标")]
        public LayerMask collisionLayer;
        [Tooltip("检测敏感度")]
        public float nodeRadius = 0.15f;

        private List<Node> nodes = new List<Node>();
        private LineRenderer lineRenderer;

        [Header("链条连接")]
        [Tooltip("顶部的锚点")]
        public Anchor parentAnchor;
        [Tooltip("底部的钩子")]
        public ChainHook hookInstance;
        [Tooltip("钩子是否处于连接状态")]
        public bool isHooked; // 由 ChainHook.ConnectTo/Disconnect 同步

        // 尾部是否被固定（钩子钩住了东西）
        public bool isFixedTail => hookInstance != null && hookInstance.hookedTarget != null;

        // --- IGrabbable 实现 ---
        public GrabType GrabType => GrabType.Linear;
        public bool CanGrab() => true;
        public Transform GetTransform() => transform;
        public Vector2 GetGrabPosition(Vector2 playerPosition)
        {
            int index = GetClosestNodeIndex(playerPosition);
            return GetNodePos(index);
        }
        // -----------------------

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;

            if (Application.isPlaying)
            {
                InitChain();
            }

            edgeCollider = GetComponent<EdgeCollider2D>();
            edgeCollider.isTrigger = true;

            hookInstance = GetComponentInChildren<ChainHook>();
            if (hookInstance != null) hookInstance.ownerChain = this;
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                UpdateEditorPreview();
            }
        }

        private void FixedUpdate()
        {
            if (!Application.isPlaying) return;
            if (nodes.Count == 0) InitChain();

            UpdateNodes();
            ApplyConstraints();
            if (enableCollision) ResolveCollisions();
            DrawChain();
        }

        [ContextMenu("重置链条")]
        public void InitChain()
        {
            nodes.Clear();
            Vector2 startPos = anchor != null ? (Vector2)anchor.position : (Vector2)transform.position;
            Vector2 endPos = tailStartPoint != null ? (Vector2)tailStartPoint.position : startPos + Vector2.down * (segmentCount * segmentLength);

            for (int i = 0; i < segmentCount; i++)
            {
                float t = (float)i / (segmentCount - 1);
                Vector2 initialPos = Vector2.Lerp(startPos, endPos, t);
                nodes.Add(new Node(initialPos, i == 0));
            }

            lineRenderer.positionCount = segmentCount;
        }

        private void UpdateNodes()
        {
            // 头节点固定在锚点
            if (anchor != null)
            {
                var head = nodes[0];
                head.pos = anchor.position;
                nodes[0] = head;
            }

            // 被钩住就强制固定尾巴节点
            if (isFixedTail)
            {
                var tail = nodes[nodes.Count - 1];
                tail.pos = hookInstance.hookedTarget.GetHookAttachPosition();
                tail.oldPos = tail.pos;
                nodes[nodes.Count - 1] = tail;
            }

            // 物理模拟
            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];
                if (node.isFixed) continue;

                Vector2 velocity = (node.pos - node.oldPos) * drag;
                node.oldPos = node.pos;
                node.pos += velocity + gravity * Time.fixedDeltaTime;
                nodes[i] = node;
            }
        }

        private void ApplyConstraints()
        {
            for (int iter = 0; iter < constraintIterations; iter++)
            {
                for (int i = 0; i < nodes.Count - 1; i++)
                {
                    Node a = nodes[i];
                    Node b = nodes[i + 1];

                    float dist = Vector2.Distance(a.pos, b.pos);
                    if (dist == 0) dist = 0.001f;

                    float diff = (segmentLength - dist) / dist;
                    Vector2 correction = (a.pos - b.pos) * diff * 0.5f;

                    if (a.isFixed)
                    {
                        b.pos -= correction * 2f;
                    }
                    else if (b.isFixed)
                    {
                        a.pos += correction * 2f;
                    }
                    else
                    {
                        a.pos += correction;
                        b.pos -= correction;
                    }

                    nodes[i] = a;
                    nodes[i + 1] = b;
                }
            }
        }

        private void ResolveCollisions()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];
                if (node.isFixed) continue;

                Collider2D hit = Physics2D.OverlapCircle(node.pos, nodeRadius, collisionLayer);
                if (hit != null)
                {
                    Vector2 closestPoint = hit.ClosestPoint(node.pos);
                    Vector2 dir = (node.pos - closestPoint).normalized;
                    if (dir == Vector2.zero) dir = Vector2.up;

                    node.pos = closestPoint + dir * nodeRadius;
                    node.oldPos = Vector2.Lerp(node.oldPos, node.pos, 0.5f);
                }
                nodes[i] = node;
            }
        }

        private void DrawChain()
        {
            if (lineRenderer.positionCount != nodes.Count) lineRenderer.positionCount = nodes.Count;
            Vector2[] colliderPoints = new Vector2[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                lineRenderer.SetPosition(i, nodes[i].pos);
                colliderPoints[i] = transform.InverseTransformPoint(nodes[i].pos);
            }
            edgeCollider.points = colliderPoints;

            // 更新钩子物体的位置和旋转
            if (hookInstance != null && nodes.Count > 0)
            {
                Vector2 lastNodePos = nodes[nodes.Count - 1].pos;
                Vector2 secondLastNodePos = nodes[nodes.Count - 2].pos;

                hookInstance.transform.position = lastNodePos;

                Vector2 dir = (lastNodePos - secondLastNodePos).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                hookInstance.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
            }
        }

        private void UpdateEditorPreview()
        {
            if (anchor == null) return;
            if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = segmentCount;
            Vector2 startPos = anchor.position;
            Vector2 endPos = tailStartPoint != null ? (Vector2)tailStartPoint.position : startPos + Vector2.down * (segmentCount * segmentLength);

            for (int i = 0; i < segmentCount; i++)
            {
                float t = (float)i / (segmentCount - 1);
                lineRenderer.SetPosition(i, Vector2.Lerp(startPos, endPos, t));
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                UpdateEditorPreview();
            }
        }

        // ----- 连接/解扣逻辑 -----

        /// <summary>
        /// 同时断开上下两端的连接。
        /// 向上：断开钩在我锚点上的钩子；向下：断开我自己的钩子。
        /// </summary>
        public void TryDisconnectAll()
        {
            // 1. 向下断开：我的钩子钩住了别人
            if (hookInstance != null) hookInstance.Disconnect();

            // 2. 向上断开：别人的钩子钩住了我的锚点
            if (parentAnchor != null && parentAnchor.incomingHook != null)
            {
                parentAnchor.incomingHook.Disconnect();
            }

            // 3. 钩钩相连：我的钩子被另一个钩子钩住
            if (hookInstance != null && hookInstance.incomingHook != null)
            {
                hookInstance.incomingHook.Disconnect();
            }
        }

        public void ApplySwingForce(Vector2 force)
        {
            if (isFixedTail) return;

            for (int i = nodes.Count - 3; i < nodes.Count; i++)
            {
                if (i < 0) continue;
                var node = nodes[i];
                node.pos += force * Time.fixedDeltaTime;
                nodes[i] = node;
            }
        }

        // ----- 节点查询 -----

        public int GetClosestNodeIndex(Vector2 targetPos)
        {
            int closest = 0;
            float minDist = float.MaxValue;
            for (int i = 0; i < nodes.Count; i++)
            {
                float d = Vector2.Distance(targetPos, nodes[i].pos);
                if (d < minDist) { minDist = d; closest = i; }
            }
            return closest;
        }

        public Vector2 GetNodePos(int index) => nodes[Mathf.Clamp(index, 0, nodes.Count - 1)].pos;

        public int NodeCount => nodes.Count;
        public float SegLength => segmentLength;
    }
}
