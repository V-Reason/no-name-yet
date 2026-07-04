using UnityEngine;

namespace RPG2D.Core.Shader
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ColliderToMesh : MonoBehaviour
    {
        [SerializeField, Min(0.001f)]
        private float edgeFadeDistance = 0.3f;

        [SerializeField, Range(8, 128)]
        private int circleSegments = 48;

        [SerializeField, Range(4, 40)]
        private int boxSubdivisions = 16;

        private void Start()
        {
            GenerateMesh();
        }

        [ContextMenu("Generate Mesh")]
        public void GenerateMesh()
        {
            Mesh mesh = null;

            if (TryGetComponent<CircleCollider2D>(out var circle))
                mesh = BuildCircleMesh(circle);
            else if (TryGetComponent<BoxCollider2D>(out var box))
                mesh = BuildBoxMesh(box);
            else if (TryGetComponent<PolygonCollider2D>(out var poly))
                mesh = BuildPolygonMesh(poly);

            if (mesh == null)
                return;

            GetComponent<MeshFilter>().sharedMesh = mesh;
            AssignMaterial();
        }

        private void AssignMaterial()
        {
            var renderer = GetComponent<MeshRenderer>();
            if (renderer.sharedMaterial != null)
                return;

#if UNITY_EDITOR
            var mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Code/Core/Shader/Turbulence.mat");
            if (mat != null)
                renderer.sharedMaterial = mat;
#endif
        }

        // ── Circle ────────────────────────────────────

        private Mesh BuildCircleMesh(CircleCollider2D circle)
        {
            int vertCount = circleSegments + 1;
            var verts = new Vector3[vertCount];
            var colors = new Color[vertCount];
            var tris = new int[circleSegments * 3];
            var uv = new Vector2[vertCount];

            float radius = circle.radius;
            Vector2 center = circle.offset;

            // Center vertex (alpha = 1)
            verts[0] = center;
            colors[0] = Color.white;
            uv[0] = new Vector2(0.5f, 0.5f);

            for (int i = 0; i < circleSegments; i++)
            {
                float angle = i * Mathf.PI * 2f / circleSegments;
                float x = center.x + Mathf.Cos(angle) * radius;
                float y = center.y + Mathf.Sin(angle) * radius;
                verts[i + 1] = new Vector3(x, y, 0);
                colors[i + 1] = new Color(1, 1, 1, 0);
                uv[i + 1] = new Vector2(
                    Mathf.Cos(angle) * 0.5f + 0.5f,
                    Mathf.Sin(angle) * 0.5f + 0.5f);

                tris[i * 3] = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = (i + 1) % circleSegments + 1;
            }

            var mesh = new Mesh { name = "CircleColliderMesh" };
            mesh.vertices = verts;
            mesh.colors = colors;
            mesh.uv = uv;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            return mesh;
        }

        // ── Box ───────────────────────────────────────

        private Mesh BuildBoxMesh(BoxCollider2D box)
        {
            int res = boxSubdivisions;
            int vertsX = res + 1;
            int vertsY = res + 1;
            int vertCount = vertsX * vertsY;

            var verts = new Vector3[vertCount];
            var colors = new Color[vertCount];
            var uv = new Vector2[vertCount];
            var tris = new int[res * res * 6];

            Vector2 half = box.size * 0.5f;
            Vector2 origin = box.offset - half;

            for (int y = 0; y < vertsY; y++)
            {
                for (int x = 0; x < vertsX; x++)
                {
                    int idx = y * vertsX + x;
                    float fx = x / (float)res;
                    float fy = y / (float)res;

                    Vector2 localPos = origin + new Vector2(fx * box.size.x, fy * box.size.y);
                    verts[idx] = new Vector3(localPos.x, localPos.y, 0);
                    uv[idx] = new Vector2(fx, fy);

                    float distLeft = localPos.x - origin.x;
                    float distRight = (origin.x + box.size.x) - localPos.x;
                    float distBottom = localPos.y - origin.y;
                    float distTop = (origin.y + box.size.y) - localPos.y;
                    float minEdgeDist = Mathf.Min(distLeft, distRight, distBottom, distTop);
                    float alpha = Mathf.Clamp01(minEdgeDist / Mathf.Max(edgeFadeDistance, 0.001f));
                    colors[idx] = new Color(1, 1, 1, alpha);
                }
            }

            int triIdx = 0;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    int bl = y * vertsX + x;
                    int br = bl + 1;
                    int tl = (y + 1) * vertsX + x;
                    int tr = tl + 1;

                    tris[triIdx++] = bl;
                    tris[triIdx++] = tl;
                    tris[triIdx++] = br;
                    tris[triIdx++] = tl;
                    tris[triIdx++] = tr;
                    tris[triIdx++] = br;
                }
            }

            var mesh = new Mesh { name = "BoxColliderMesh" };
            mesh.vertices = verts;
            mesh.colors = colors;
            mesh.uv = uv;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            return mesh;
        }

        // ── Polygon ───────────────────────────────────

        private Mesh BuildPolygonMesh(PolygonCollider2D poly)
        {
            if (poly.pathCount == 0 || poly.GetTotalPointCount() < 3)
                return null;

            Vector2[] path = poly.GetPath(0);
            int n = path.Length;
            if (n < 3) return null;

            Vector3 offset = poly.offset;

            // Check convexity to decide triangulation strategy
            bool convex = IsPathConvex(path);

            if (convex)
                return BuildConvexPolygonMesh(path, offset, poly);
            else
                return BuildConcavePolygonMesh(path, offset, poly);
        }

        private bool IsPathConvex(Vector2[] path)
        {
            int n = path.Length;
            if (n < 3) return false;
            bool? sign = null;
            for (int i = 0; i < n; i++)
            {
                Vector2 a = path[i];
                Vector2 b = path[(i + 1) % n];
                Vector2 c = path[(i + 2) % n];
                float cross = (b.x - a.x) * (c.y - b.y) - (b.y - a.y) * (c.x - b.x);
                if (Mathf.Abs(cross) < 1e-6f) continue;
                bool cur = cross > 0;
                if (sign == null) sign = cur;
                else if (sign.Value != cur) return false;
            }
            return true;
        }

        private Mesh BuildConvexPolygonMesh(Vector2[] path, Vector3 offset, PolygonCollider2D poly)
        {
            int n = path.Length;

            // Compute centroid (guaranteed inside for convex polygon)
            Vector2 centroid = Vector2.zero;
            for (int i = 0; i < n; i++)
                centroid += path[i];
            centroid = centroid / n + (Vector2)offset;

            // Vertices: center + path points
            var verts = new Vector3[n + 1];
            var colors = new Color[n + 1];
            var uv = new Vector2[n + 1];

            // Center vertex
            verts[0] = centroid;
            float centerDist = MinDistanceToPolygonEdges(poly, centroid);
            float centerAlpha = Mathf.Clamp01(centerDist / Mathf.Max(edgeFadeDistance, 0.001f));
            colors[0] = new Color(1, 1, 1, centerAlpha);

            // Path vertices (boundary, alpha = 0)
            for (int i = 0; i < n; i++)
            {
                verts[i + 1] = path[i] + (Vector2)offset;
                colors[i + 1] = new Color(1, 1, 1, 0);
            }

            // Triangles: fan from center
            var tris = new int[n * 3];
            for (int i = 0; i < n; i++)
            {
                tris[i * 3]     = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = (i + 1) % n + 1;
            }

            // UV from bounding box
            ComputeUVFromBounds(verts, uv);

            var mesh = new Mesh { name = "PolygonColliderMesh" };
            mesh.vertices = verts;
            mesh.colors = colors;
            mesh.uv = uv;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            return mesh;
        }

        private Mesh BuildConcavePolygonMesh(Vector2[] path, Vector3 offset, PolygonCollider2D poly)
        {
            int n = path.Length;

            // Triangle fan from first vertex
            var verts = new Vector3[n];
            var colors = new Color[n];
            var uv = new Vector2[n];
            var tris = new int[(n - 2) * 3];

            for (int i = 0; i < n; i++)
                verts[i] = path[i] + (Vector2)offset;

            for (int i = 0; i < n - 2; i++)
            {
                tris[i * 3]     = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = i + 2;
            }

            ComputeUVFromBounds(verts, uv);

            var mesh = new Mesh { name = "PolygonColliderMesh" };
            mesh.vertices = verts;
            mesh.uv = uv;
            mesh.triangles = tris;
            mesh.RecalculateBounds();

            // Compute edge distance colors + subdivide for interior points
            ApplyEdgeColors(mesh, poly);
            mesh = SubdivideOnce(mesh, poly);
            mesh = SubdivideOnce(mesh, poly);

            return mesh;
        }

        private void ComputeUVFromBounds(Vector3[] verts, Vector2[] uv)
        {
            if (verts.Length == 0) return;
            Vector3 min = verts[0];
            Vector3 max = verts[0];
            for (int i = 1; i < verts.Length; i++)
            {
                min = Vector3.Min(min, verts[i]);
                max = Vector3.Max(max, verts[i]);
            }
            Vector3 size = max - min;
            float invX = Mathf.Abs(size.x) > 1e-6f ? 1f / size.x : 1f;
            float invY = Mathf.Abs(size.y) > 1e-6f ? 1f / size.y : 1f;
            for (int i = 0; i < verts.Length; i++)
            {
                uv[i] = new Vector2(
                    (verts[i].x - min.x) * invX,
                    (verts[i].y - min.y) * invY);
            }
        }

        private void ApplyEdgeColors(Mesh mesh, PolygonCollider2D poly)
        {
            Vector3[] verts = mesh.vertices;
            Color[] colors = new Color[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                float dist = MinDistanceToPolygonEdges(poly, verts[i]);
                float alpha = Mathf.Clamp01(dist / Mathf.Max(edgeFadeDistance, 0.001f));
                colors[i] = new Color(1, 1, 1, alpha);
            }
            mesh.colors = colors;
        }

        private Mesh SubdivideOnce(Mesh mesh, PolygonCollider2D poly)
        {
            Vector3[] verts = mesh.vertices;
            Vector2[] uv = mesh.uv;
            int[] tris = mesh.triangles;

            int triCount = tris.Length / 3;
            int newVertCount = verts.Length + triCount * 3;
            var newVerts = new Vector3[newVertCount];
            var newColors = new Color[newVertCount];
            var newUV = new Vector2[newVertCount];
            var newTris = new int[triCount * 4 * 3];

            // Copy original verts
            for (int i = 0; i < verts.Length; i++)
            {
                newVerts[i] = verts[i];
                if (mesh.colors != null && i < mesh.colors.Length)
                    newColors[i] = mesh.colors[i];
                else
                    newColors[i] = Color.white;
                if (mesh.uv != null && i < mesh.uv.Length)
                    newUV[i] = mesh.uv[i];
            }

            int nextVert = verts.Length;
            int nextTri = 0;

            for (int t = 0; t < triCount; t++)
            {
                int i0 = tris[t * 3];
                int i1 = tris[t * 3 + 1];
                int i2 = tris[t * 3 + 2];

                Vector3 v0 = newVerts[i0];
                Vector3 v1 = newVerts[i1];
                Vector3 v2 = newVerts[i2];

                // Midpoints
                Vector3 m01 = (v0 + v1) * 0.5f;
                Vector3 m12 = (v1 + v2) * 0.5f;
                Vector3 m20 = (v2 + v0) * 0.5f;

                int im01 = nextVert++;
                int im12 = nextVert++;
                int im20 = nextVert++;

                newVerts[im01] = m01;
                newVerts[im12] = m12;
                newVerts[im20] = m20;

                float d01 = MinDistanceToPolygonEdges(poly, m01);
                float d12 = MinDistanceToPolygonEdges(poly, m12);
                float d20 = MinDistanceToPolygonEdges(poly, m20);
                newColors[im01] = new Color(1, 1, 1, Mathf.Clamp01(d01 / Mathf.Max(edgeFadeDistance, 0.001f)));
                newColors[im12] = new Color(1, 1, 1, Mathf.Clamp01(d12 / Mathf.Max(edgeFadeDistance, 0.001f)));
                newColors[im20] = new Color(1, 1, 1, Mathf.Clamp01(d20 / Mathf.Max(edgeFadeDistance, 0.001f)));

                // UV for midpoints
                Vector2 uv0 = newUV[i0];
                Vector2 uv1 = newUV[i1];
                Vector2 uv2 = newUV[i2];
                newUV[im01] = (uv0 + uv1) * 0.5f;
                newUV[im12] = (uv1 + uv2) * 0.5f;
                newUV[im20] = (uv2 + uv0) * 0.5f;

                // 4 new triangles
                newTris[nextTri++] = i0;
                newTris[nextTri++] = im01;
                newTris[nextTri++] = im20;
                newTris[nextTri++] = i1;
                newTris[nextTri++] = im12;
                newTris[nextTri++] = im01;
                newTris[nextTri++] = i2;
                newTris[nextTri++] = im20;
                newTris[nextTri++] = im12;
                newTris[nextTri++] = im01;
                newTris[nextTri++] = im12;
                newTris[nextTri++] = im20;
            }

            var newMesh = new Mesh { name = mesh.name + "_Subdiv" };
            newMesh.vertices = newVerts;
            newMesh.colors = newColors;
            newMesh.uv = newUV;
            newMesh.triangles = newTris;
            newMesh.RecalculateBounds();
            return newMesh;
        }

        // ── Distance helpers ──────────────────────────

        private float MinDistanceToPolygonEdges(PolygonCollider2D poly, Vector2 localPoint)
        {
            float minDist = float.MaxValue;

            for (int pathIndex = 0; pathIndex < poly.pathCount; pathIndex++)
            {
                Vector2[] path = poly.GetPath(pathIndex);
                if (path.Length < 2)
                    continue;

                for (int i = 0; i < path.Length; i++)
                {
                    Vector2 a = path[i] + poly.offset;
                    Vector2 b = path[(i + 1) % path.Length] + poly.offset;
                    float dist = DistToSegment(localPoint, a, b);
                    if (dist < minDist)
                        minDist = dist;
                }
            }

            return minDist < float.MaxValue ? minDist : 0f;
        }

        private static float DistToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            if (ab.sqrMagnitude < 1e-9f)
                return Vector2.Distance(point, a);

            float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(point, a + ab * t);
        }
    }
}
