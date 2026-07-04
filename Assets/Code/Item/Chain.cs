using System.Collections.Generic;
using RPG2D.Core.Interaction;
using UnityEngine;

namespace RPG2D.Item
{

    [ExecuteAlways]
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(EdgeCollider2D))]
    public class Chain : MonoBehaviour, IGrabbable, IHookable
    {
        [System.Serializable]
        public struct Node
        {
            public Vector2 pos;
            public Vector2 oldPos;
            public bool isFixed;
        }

        [Header("链条结构设置")]
        [Tooltip("顶部的锚点引用")]
        public Anchor parentAnchor;
        [Tooltip("底部的钩子引用")]
        public ChainHook hookInstance;

        [Range(2, 50)]
        public int segmentCount = 10;
        [Range(0.1f, 2f)]
        public float segmentLength = 0.5f;

        [Header("只读信息")]
        [SerializeField, Unity.Collections.ReadOnly]
        private float totalLength;

        [Header("物理参数设置")]
        [Tooltip("重力参数")]
        public Vector2 gravity = new Vector2(0, -1f);
        [Tooltip("运动阻力（值越小阻力越大）")]
        [Range(0f, 1f)] public float drag = 0.95f;
        [Tooltip("迭代次数越多，链条越不容易拉长。建议 40-60")]
        public int constraintIterations = 50;

        [Header("物理进阶设置")]
        public float swingDamping = 0.7f;
        public float maxSwingImpulse = 2.0f;

        [Header("碰撞设置")]
        public bool enableCollision = true;
        [Tooltip("碰撞目标")]
        public LayerMask collisionLayer;
        [Tooltip("检测敏感度")]
        public float nodeRadius = 0.15f;

        [Header("视觉表现")]
        public Texture2D chainTexture;
        [Range(0.01f, 0.5f)]
        public float chainWidth = 0.1f;
        public int sortingOrder = 0;

        private List<Node> nodes = new List<Node>();
        private LineRenderer lineRenderer;
        private EdgeCollider2D edgeCollider;
        private Material chainMaterial;

        [Header("链条连接")]
        [Tooltip("钩子是否处于连接状态")]
        public bool isHooked; // 由 ChainHook.ConnectTo/Disconnect 同步

        // 尾部是否被固定（钩子钩住了东西）
        public bool isFixedTail => hookInstance != null && hookInstance.hookedTarget != null;

        // 身体是否被钩住
        private ChainHook incomingHook;
        private int hookedNodeIndex = -1;
        public int HookedNodeIndex => hookedNodeIndex;

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

        // --- IHookable 实现 ---
        public Vector2 GetHookAttachPosition()
        {
            if (hookedNodeIndex != -1) return nodes[hookedNodeIndex].pos;
            return transform.position;
        }

        public bool CanBeHooked() => incomingHook == null;

        public void OnHooked(ChainHook hook)
        {
            incomingHook = hook;
            hookedNodeIndex = GetClosestNodeIndex(hook.transform.position);
        }

        public void OnUnhooked()
        {
            incomingHook = null;
            hookedNodeIndex = -1;
        }

        public Chain GetRelatedChain() => this;
        public HookPointType GetHookPointType() => HookPointType.Body;
        // -----------------------

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            edgeCollider = GetComponent<EdgeCollider2D>();
            lineRenderer.useWorldSpace = true;

            if (chainTexture != null)
            {
                chainTexture.wrapMode = TextureWrapMode.Repeat;
                chainMaterial = new Material(Shader.Find("Unlit/Transparent"));
                chainMaterial.mainTexture = chainTexture;
                chainMaterial.color = Color.white;
                lineRenderer.material = chainMaterial;
                lineRenderer.textureMode = LineTextureMode.Tile;
            }

            if (Application.isPlaying)
            {
                InitChain();
            }
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

        [ContextMenu("重置物理链条")]
        public void InitChain()
        {
            nodes.Clear();
            Vector2 startPos = transform.position;

            for (int i = 0; i < segmentCount; i++)
            {
                Vector2 initialPos = startPos + Vector2.down * (i * segmentLength);
                nodes.Add(new Node { pos = initialPos, oldPos = initialPos, isFixed = (i == 0) });
            }

            lineRenderer.positionCount = segmentCount;
        }

        private void UpdateNodes()
        {
            // 头节点固定在变换位置
            var head = nodes[0];
            head.pos = transform.position;
            nodes[0] = head;

            // 被钩住就强制固定尾巴节点
            if (isFixedTail)
            {
                var tail = nodes[nodes.Count - 1];
                tail.pos = hookInstance.hookedTarget.GetHookAttachPosition();
                tail.oldPos = tail.pos;
                nodes[nodes.Count - 1] = tail;
            }

            // 身体中间节点被钩住，强制同步位置
            if (incomingHook != null && hookedNodeIndex != -1)
            {
                var node = nodes[hookedNodeIndex];
                node.pos = incomingHook.transform.position;
                node.oldPos = node.pos;
                nodes[hookedNodeIndex] = node;
            }

            // 物理模拟
            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];
                if (node.isFixed || i == hookedNodeIndex) continue;

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

                    bool aFixed = a.isFixed || i == hookedNodeIndex;
                    bool bFixed = b.isFixed || i + 1 == hookedNodeIndex;

                    if (aFixed)
                    {
                        b.pos -= correction * 2f;
                    }
                    else if (bFixed)
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
                if (node.isFixed || i == hookedNodeIndex) continue;

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
            lineRenderer.startWidth = chainWidth;
            lineRenderer.endWidth = chainWidth;
            lineRenderer.sortingOrder = sortingOrder;

            Vector2[] colliderPoints = new Vector2[nodes.Count];
            float totalDist = 0f;
            for (int i = 0; i < nodes.Count; i++)
            {
                lineRenderer.SetPosition(i, nodes[i].pos);
                colliderPoints[i] = transform.InverseTransformPoint(nodes[i].pos);
                if (i > 0)
                    totalDist += Vector2.Distance(nodes[i].pos, nodes[i - 1].pos);
            }
            edgeCollider.points = colliderPoints;

            if (chainMaterial != null && segmentLength > 0)
                chainMaterial.mainTextureScale = new Vector2(1f / segmentLength, 1);

            // 更新钩子物体的位置和旋转
            if (hookInstance != null && nodes.Count > 0)
            {
                Vector2 lastNodePos = nodes[nodes.Count - 1].pos;
                Vector2 secondLastNodePos = nodes[nodes.Count - 2].pos;

                hookInstance.transform.position = lastNodePos;

                Vector2 dir = (lastNodePos - secondLastNodePos).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                hookInstance.transform.rotation = Quaternion.Euler(0, 0, angle + 90);
            }
        }

        private void UpdateEditorPreview()
        {
            if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

            lineRenderer.startWidth = chainWidth;
            lineRenderer.endWidth = chainWidth;
            lineRenderer.sortingOrder = sortingOrder;
            lineRenderer.positionCount = segmentCount;

            if (chainTexture != null && (chainMaterial == null || chainMaterial.mainTexture != chainTexture))
            {
                if (chainMaterial != null)
                    DestroyImmediate(chainMaterial);
                chainTexture.wrapMode = TextureWrapMode.Repeat;
                chainMaterial = new Material(Shader.Find("Unlit/Transparent"));
                chainMaterial.mainTexture = chainTexture;
                chainMaterial.color = Color.white;
                lineRenderer.material = chainMaterial;
                lineRenderer.textureMode = LineTextureMode.Tile;
            }

            if (chainMaterial != null && segmentLength > 0)
                chainMaterial.mainTextureScale = new Vector2(1f / segmentLength, 1);

            Vector2 startPos = transform.position;
            Vector2 direction = Vector2.down;

            for (int i = 0; i < segmentCount; i++)
            {
                Vector2 nodePos = startPos + direction * (i * segmentLength);
                lineRenderer.SetPosition(i, nodePos);

                if (i == segmentCount - 1 && hookInstance != null)
                {
                    hookInstance.transform.position = nodePos;
                    hookInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (hookedNodeIndex != -1 && Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(nodes[hookedNodeIndex].pos, nodeRadius * 2f);
            }
        }

        private void OnDestroy()
        {
            if (chainMaterial == null) return;
            if (Application.isPlaying)
                Destroy(chainMaterial);
            else
                DestroyImmediate(chainMaterial);
        }

        private void OnValidate()
        {
            totalLength = segmentCount * segmentLength;

            if (parentAnchor == null) parentAnchor = GetComponentInParent<Anchor>();
            if (hookInstance == null) hookInstance = GetComponentInChildren<ChainHook>();
            if (hookInstance != null) hookInstance.ownerChain = this;

            UpdateEditorPreview();
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

            // 4. 身体被钩：别人的钩子钩住了我的身体中间
            if (incomingHook != null)
            {
                incomingHook.Disconnect();
            }
        }

        public void ApplySwingForce(Vector2 targetMousePos, float strength)
        {
            if (isHooked || nodes.Count < 3) return;

            Vector2 anchorPos = transform.position;
            Vector2 mouseDir = (targetMousePos - anchorPos).normalized;
            float verticalFactor = Mathf.Clamp01(Vector2.Dot(mouseDir, Vector2.down) + 0.5f);

            int startIdx = nodes.Count - (nodes.Count / 3);
            for (int i = startIdx; i < nodes.Count; i++)
            {
                Node node = nodes[i];

                Vector2 forceDir = (targetMousePos - node.pos).normalized;
                Vector2 impulse = forceDir * strength * verticalFactor * Time.fixedDeltaTime;

                if (impulse.magnitude > maxSwingImpulse)
                    impulse = impulse.normalized * maxSwingImpulse;

                node.pos += impulse * swingDamping;
                nodes[i] = node;
            }
        }

        public void ApplyRawImpulse(Vector2 impulse)
        {
            if (nodes.Count == 0) return;
            var tail = nodes[nodes.Count - 1];
            tail.oldPos = tail.pos - impulse * Time.fixedDeltaTime;
            nodes[nodes.Count - 1] = tail;
        }

        // ----- 节点查询 -----

        public int GetClosestNodeIndex(Vector2 targetPos)
        {
            int closest = 0;
            float minDist = float.MaxValue;
            for (int i = 0; i < (Application.isPlaying ? nodes.Count : segmentCount); i++)
            {
                Vector2 p = Application.isPlaying ? nodes[i].pos : (Vector2)transform.position + Vector2.down * (i * segmentLength);
                float d = Vector2.Distance(targetPos, p);
                if (d < minDist) { minDist = d; closest = i; }
            }
            return Mathf.Clamp(closest, 0, segmentCount - 2);
        }

        public Vector2 GetNodePos(int index)
        {
            if (Application.isPlaying) return nodes[Mathf.Clamp(index, 0, nodes.Count - 1)].pos;
            return (Vector2)transform.position + Vector2.down * (index * segmentLength);
        }

        public int NodeCount => nodes.Count;
        public float SegLength => segmentLength;
    }
}
