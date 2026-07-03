using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class VerletChain2D : MonoBehaviour
{
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

    [Header("Chain Structure")]
    public Transform anchor;
    public Transform tailStartPoint;
    public int segmentCount = 10;
    public float segmentLength = 1f;

    [Header("Physics Settings")]
    public Vector2 gravity = new Vector2(0, -1f);
    [Range(0f, 1f)] public float drag = 0.95f;
    public int constraintIterations = 15; // 提高迭代次数让链子更结实

    [Header("Collision Settings")]
    public bool enableCollision = true;
    public LayerMask collisionLayer;
    public float nodeRadius = 0.15f;

    private List<Node> nodes = new List<Node>();
    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        // 关键：强制使用世界坐标，防止渲染位置偏移或塌陷
        lineRenderer.useWorldSpace = true;

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

    [ContextMenu("Reset Chain")]
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
        if (anchor != null)
        {
            var head = nodes[0];
            head.pos = anchor.position;
            nodes[0] = head;
        }

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

    // 修正后的约束算法：使用比例修正法
    private void ApplyConstraints()
    {
        for (int iter = 0; iter < constraintIterations; iter++)
        {
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Node a = nodes[i];
                Node b = nodes[i + 1];

                float dist = Vector2.Distance(a.pos, b.pos);
                if (dist == 0) dist = 0.001f; // 防止除以0

                // 计算误差比例
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
        for (int i = 0; i < nodes.Count; i++)
        {
            lineRenderer.SetPosition(i, nodes[i].pos);
        }
    }

    private void UpdateEditorPreview()
    {
        if (anchor == null) return;
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.useWorldSpace = true; // 预览时也确保使用世界坐标
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
}
