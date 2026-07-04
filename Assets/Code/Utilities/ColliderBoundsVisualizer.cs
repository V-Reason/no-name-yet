using UnityEngine;

namespace EditorTool
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class ColliderBoundsVisualizer : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private Color _color = new(0f, 1f, 0.5f, 0.8f);
        [SerializeField] private bool _showOnlyWhenSelected;

        private void OnDrawGizmos()
        {
            if (_showOnlyWhenSelected) return;
            DrawColliderBounds();
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showOnlyWhenSelected) return;
            DrawColliderBounds();
        }

        private void DrawColliderBounds()
        {
            Gizmos.color = _color;

            // --- 3D Colliders ---
            if (TryGetComponent<BoxCollider>(out var box))
                DrawBox(box);
            else if (TryGetComponent<SphereCollider>(out var sphere))
                DrawSphere(sphere);
            else if (TryGetComponent<CapsuleCollider>(out var capsule))
                DrawCapsule(capsule);
            else if (TryGetComponent<MeshCollider>(out var mesh))
                DrawBounds(mesh.bounds);

            // --- 2D Colliders ---
            if (TryGetComponent<BoxCollider2D>(out var box2D))
                DrawBox2D(box2D);
            else if (TryGetComponent<CircleCollider2D>(out var circle2D))
                DrawCircle2D(circle2D);
            else if (TryGetComponent<CapsuleCollider2D>(out var capsule2D))
                DrawCapsule2D(capsule2D);
            else if (TryGetComponent<PolygonCollider2D>(out var poly2D))
                DrawPolygon2D(poly2D);
            else if (TryGetComponent<EdgeCollider2D>(out var edge2D))
                DrawEdge2D(edge2D);
            else if (TryGetComponent<CompositeCollider2D>(out var comp2D))
                DrawComposite2D(comp2D);
        }

        // ── 3D ──────────────────────────────────────

        private static void DrawBox(BoxCollider c)
        {
            var m = Matrix4x4.TRS(c.transform.position, c.transform.rotation, c.transform.lossyScale);
            var prev = Gizmos.matrix;
            Gizmos.matrix = m;
            Gizmos.DrawWireCube(c.center, c.size);
            Gizmos.matrix = prev;
        }

        private static void DrawSphere(SphereCollider c)
        {
            var lossy = c.transform.lossyScale;
            var radius = c.radius * Mathf.Max(lossy.x, lossy.y, lossy.z);
            Gizmos.DrawWireSphere(c.transform.TransformPoint(c.center), radius);
        }

        private static void DrawCapsule(CapsuleCollider c)
        {
            var lossy = c.transform.lossyScale;
            var worldCenter = c.transform.TransformPoint(c.center);
            var rotation = c.transform.rotation;

            var radius = c.radius * Mathf.Max(lossy.x, lossy.z);
            var height = Mathf.Max(0, c.height * lossy.y - radius * 2f);
            var half = height * 0.5f;

            var axis = c.direction switch
            {
                0 => rotation * Vector3.right,
                1 => rotation * Vector3.up,
                _ => rotation * Vector3.forward,
            };

            var top = worldCenter + axis * half;
            var bottom = worldCenter - axis * half;

            Gizmos.DrawWireSphere(top, radius);
            Gizmos.DrawWireSphere(bottom, radius);
            if (height > 0f) DrawCapsuleConnect(top, bottom, radius, axis);
        }

        private static void DrawCapsuleConnect(Vector3 top, Vector3 bottom, float radius, Vector3 axis)
        {
            var perp = Vector3.Cross(axis, Mathf.Abs(Vector3.Dot(axis, Vector3.up)) > 0.99f ? Vector3.forward : Vector3.up).normalized;
            var perp2 = Vector3.Cross(axis, perp).normalized;
            for (int i = 0; i < 4; i++)
            {
                var a = Quaternion.AngleAxis(i * 90f, axis);
                var dir = a * perp * radius;
                Gizmos.DrawLine(top + dir, bottom + dir);
            }
        }

        private static void DrawBounds(Bounds b)
        {
            Gizmos.DrawWireCube(b.center, b.size);
        }

        // ── 2D ──────────────────────────────────────

        private static void DrawBox2D(BoxCollider2D c)
        {
            var m = Matrix4x4.TRS(c.transform.position, c.transform.rotation, c.transform.lossyScale);
            var prev = Gizmos.matrix;
            Gizmos.matrix = m;
            Gizmos.DrawWireCube((Vector3)c.offset, (Vector3)c.size);
            Gizmos.matrix = prev;
        }

        private static void DrawCircle2D(CircleCollider2D c)
        {
            var lossy = c.transform.lossyScale;
            var radius = c.radius * Mathf.Max(lossy.x, lossy.y);
            var count = Mathf.CeilToInt(radius * 16f);
            count = Mathf.Clamp(count, 16, 128);

            var center = c.transform.TransformPoint((Vector3)c.offset);
            var prev = Vector3.zero;
            for (int i = 0; i <= count; i++)
            {
                var angle = i * Mathf.PI * 2f / count;
                var pt = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                if (i > 0) Gizmos.DrawLine(prev, pt);
                prev = pt;
            }
        }

        private static void DrawCapsule2D(CapsuleCollider2D c)
        {
            var lossy = c.transform.lossyScale;
            var worldCenter = c.transform.TransformPoint(c.offset);
            var radius = c.size.x * 0.5f * lossy.x;
            var height = c.size.y * lossy.y;
            var half = Mathf.Max(0, height * 0.5f - radius);

            var axis = c.direction == CapsuleDirection2D.Vertical
                ? (Vector2)c.transform.up
                : (Vector2)c.transform.right;

            var top = (Vector2)worldCenter + axis * half;
            var bottom = (Vector2)worldCenter - axis * half;

            DrawWireArc2D(top, radius, 0f, 360f);
            DrawWireArc2D(bottom, radius, 0f, 360f);

            var perp = Vector2.Perpendicular(axis) * radius;
            Gizmos.DrawLine(top + perp, bottom + perp);
            Gizmos.DrawLine(top - perp, bottom - perp);
        }

        private static void DrawPolygon2D(PolygonCollider2D c)
        {
            var t = c.transform;
            for (int p = 0; p < c.pathCount; p++)
            {
                var path = c.GetPath(p);
                for (int i = 0; i < path.Length; i++)
                {
                    var a = t.TransformPoint((Vector3)path[i]);
                    var b = t.TransformPoint((Vector3)path[(i + 1) % path.Length]);
                    Gizmos.DrawLine(a, b);
                }
            }
        }

        private static void DrawEdge2D(EdgeCollider2D c)
        {
            var t = c.transform;
            var pts = c.points;
            for (int i = 0; i < pts.Length - 1; i++)
                Gizmos.DrawLine(t.TransformPoint((Vector3)pts[i]), t.TransformPoint((Vector3)pts[i + 1]));
        }

        private static void DrawComposite2D(CompositeCollider2D c)
        {
            var t = c.transform;
            for (int p = 0; p < c.pathCount; p++)
            {
                var path = new Vector2[c.GetPathPointCount(p)];
                c.GetPath(p, path);
                for (int i = 0; i < path.Length - 1; i++)
                    Gizmos.DrawLine(t.TransformPoint((Vector3)path[i]), t.TransformPoint((Vector3)path[i + 1]));
                Gizmos.DrawLine(t.TransformPoint((Vector3)path[path.Length - 1]), t.TransformPoint((Vector3)path[0]));
            }
        }

        // ── helper ──────────────────────────────────

        private static void DrawWireArc2D(Vector2 center, float radius, float startAngle, float endAngle)
        {
            var count = Mathf.CeilToInt(radius * 16f);
            count = Mathf.Clamp(count, 12, 64);
            var prev = Vector2.zero;
            for (int i = 0; i <= count; i++)
            {
                var a = Mathf.Lerp(startAngle, endAngle, i / (float)count) * Mathf.Deg2Rad;
                var pt = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                if (i > 0) Gizmos.DrawLine(prev, pt);
                prev = pt;
            }
        }
    }
}
