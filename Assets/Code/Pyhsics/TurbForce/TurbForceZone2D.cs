using UnityEngine;

namespace RPG2D.Pyhsics.TurbForce
{
    /// <summary>
    /// 固定 2D 湍流力区域, 为进入区域的接收器提供固定方向和强度的力.
    /// </summary>
    public class TurbForceZone2D : MonoBehaviour
    {
        [SerializeField]
        private Vector2 forceDirection = Vector2.up;

        [SerializeField, Min(0f)]
        private float forceMagnitude = 10f;

        [SerializeField]
        private bool useEdgeFalloff = true;

        [SerializeField, Min(0.01f)]
        private float edgeFalloffDistance = 1f;

        [SerializeField, Range(0f, 1f)]
        private float edgeForceMultiplier = 0f;

        /// <summary>
        /// 获取当前区域提供的湍流力, 方向会被归一化后再乘以强度.
        /// </summary>
        public Vector2 Force
        {
            get
            {
                if (forceDirection.sqrMagnitude <= Mathf.Epsilon || forceMagnitude <= 0f)
                {
                    return Vector2.zero;
                }

                return forceDirection.normalized * forceMagnitude;
            }
        }

        private void Reset()
        {
            EnsureTriggerCollider();
        }

        private void Awake()
        {
            EnsureTriggerCollider();
        }

        private void OnValidate()
        {
            forceMagnitude = Mathf.Max(0f, forceMagnitude);
            edgeFalloffDistance = Mathf.Max(0.01f, edgeFalloffDistance);
            edgeForceMultiplier = Mathf.Clamp01(edgeForceMultiplier);
            EnsureTriggerCollider();
        }

        /// <summary>
        /// 设置湍流方向和强度, 用于运行时配置或测试固定力结果.
        /// </summary>
        public void SetForce(Vector2 direction, float magnitude)
        {
            forceDirection = direction;
            forceMagnitude = Mathf.Max(0f, magnitude);
        }

        /// <summary>
        /// 根据世界坐标获取该点受到的湍流力, 开启边缘衰减时靠近边界会减弱.
        /// </summary>
        public Vector2 GetForceAt(Vector2 worldPosition)
        {
            if (!ContainsPoint(worldPosition))
            {
                return Vector2.zero;
            }

            return Force * GetEdgeFalloffMultiplier(worldPosition);
        }

        /// <summary>
        /// 判断世界坐标点是否位于当前湍流区域的碰撞体范围内.
        /// </summary>
        public bool ContainsPoint(Vector2 worldPosition)
        {
            Collider2D zoneCollider = GetComponent<Collider2D>();
            return zoneCollider != null && zoneCollider.OverlapPoint(worldPosition);
        }

        /// <summary>
        /// 计算边缘衰减倍率, 距离边界越近倍率越低, 深入区域后恢复满强度.
        /// </summary>
        private float GetEdgeFalloffMultiplier(Vector2 worldPosition)
        {
            if (!useEdgeFalloff)
            {
                return 1f;
            }

            Collider2D zoneCollider = GetComponent<Collider2D>();
            if (zoneCollider == null || !TryGetDistanceToEdge(zoneCollider, worldPosition, out float distanceToEdge))
            {
                return 1f;
            }

            float falloffProgress = Mathf.Clamp01(distanceToEdge / edgeFalloffDistance);
            return Mathf.Lerp(edgeForceMultiplier, 1f, falloffProgress);
        }

        /// <summary>
        /// 按碰撞体类型计算目标点到湍流区域边缘的距离.
        /// </summary>
        private bool TryGetDistanceToEdge(Collider2D zoneCollider, Vector2 worldPosition, out float distanceToEdge)
        {
            switch (zoneCollider)
            {
                case CircleCollider2D circleCollider:
                    distanceToEdge = GetDistanceToCircleEdge(circleCollider, worldPosition);
                    return true;
                case BoxCollider2D boxCollider:
                    distanceToEdge = GetDistanceToBoxEdge(boxCollider, worldPosition);
                    return true;
                case PolygonCollider2D polygonCollider:
                    distanceToEdge = GetDistanceToPolygonEdge(polygonCollider, worldPosition);
                    return true;
                default:
                    distanceToEdge = 0f;
                    return false;
            }
        }

        /// <summary>
        /// 计算目标点到圆形碰撞体边缘的距离.
        /// </summary>
        private float GetDistanceToCircleEdge(CircleCollider2D circleCollider, Vector2 worldPosition)
        {
            Vector2 center = circleCollider.transform.TransformPoint(circleCollider.offset);
            Vector3 scale = circleCollider.transform.lossyScale;
            float radiusScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
            float worldRadius = circleCollider.radius * radiusScale;
            return Mathf.Max(0f, worldRadius - Vector2.Distance(worldPosition, center));
        }

        /// <summary>
        /// 计算目标点到矩形碰撞体边缘的距离.
        /// </summary>
        private float GetDistanceToBoxEdge(BoxCollider2D boxCollider, Vector2 worldPosition)
        {
            Vector2 halfSize = boxCollider.size * 0.5f;
            Vector2 topLeft = BoxPointToWorld(boxCollider, new Vector2(-halfSize.x, halfSize.y));
            Vector2 topRight = BoxPointToWorld(boxCollider, new Vector2(halfSize.x, halfSize.y));
            Vector2 bottomRight = BoxPointToWorld(boxCollider, new Vector2(halfSize.x, -halfSize.y));
            Vector2 bottomLeft = BoxPointToWorld(boxCollider, new Vector2(-halfSize.x, -halfSize.y));

            float minDistance = GetDistanceToSegment(worldPosition, topLeft, topRight);
            minDistance = Mathf.Min(minDistance, GetDistanceToSegment(worldPosition, topRight, bottomRight));
            minDistance = Mathf.Min(minDistance, GetDistanceToSegment(worldPosition, bottomRight, bottomLeft));
            minDistance = Mathf.Min(minDistance, GetDistanceToSegment(worldPosition, bottomLeft, topLeft));
            return minDistance;
        }

        /// <summary>
        /// 将矩形碰撞体的本地顶点转换为世界坐标.
        /// </summary>
        private Vector2 BoxPointToWorld(BoxCollider2D boxCollider, Vector2 localPoint)
        {
            return boxCollider.transform.TransformPoint(localPoint + boxCollider.offset);
        }

        /// <summary>
        /// 计算目标点到多边形碰撞体最近边的距离.
        /// </summary>
        private float GetDistanceToPolygonEdge(PolygonCollider2D polygonCollider, Vector2 worldPosition)
        {
            float minDistance = float.MaxValue;

            for (int pathIndex = 0; pathIndex < polygonCollider.pathCount; pathIndex++)
            {
                Vector2[] pathPoints = polygonCollider.GetPath(pathIndex);
                if (pathPoints.Length < 2)
                {
                    continue;
                }

                for (int i = 0; i < pathPoints.Length; i++)
                {
                    Vector2 start = PolygonPointToWorld(polygonCollider, pathPoints[i]);
                    Vector2 end = PolygonPointToWorld(polygonCollider, pathPoints[(i + 1) % pathPoints.Length]);
                    float distance = GetDistanceToSegment(worldPosition, start, end);
                    minDistance = Mathf.Min(minDistance, distance);
                }
            }

            return minDistance < float.MaxValue ? minDistance : 0f;
        }

        /// <summary>
        /// 将多边形碰撞体的本地顶点转换为世界坐标.
        /// </summary>
        private Vector2 PolygonPointToWorld(PolygonCollider2D polygonCollider, Vector2 localPoint)
        {
            return polygonCollider.transform.TransformPoint(localPoint + polygonCollider.offset);
        }

        /// <summary>
        /// 计算目标点到线段的最短距离.
        /// </summary>
        private float GetDistanceToSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
        {
            Vector2 segment = segmentEnd - segmentStart;
            if (segment.sqrMagnitude <= Mathf.Epsilon)
            {
                return Vector2.Distance(point, segmentStart);
            }

            float t = Vector2.Dot(point - segmentStart, segment) / segment.sqrMagnitude;
            Vector2 closestPoint = segmentStart + segment * Mathf.Clamp01(t);
            return Vector2.Distance(point, closestPoint);
        }

        /// <summary>
        /// 确保区域碰撞体以 Trigger 形式工作, 避免湍流区域阻挡对象移动.
        /// </summary>
        private void EnsureTriggerCollider()
        {
            Collider2D zoneCollider = GetComponent<Collider2D>();
            if (zoneCollider == null)
            {
                return;
            }

            zoneCollider.isTrigger = true;
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 force = Force;
            if (force.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Vector3 start = transform.position;
            Vector3 end = start + (Vector3)force.normalized;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.08f);
        }
    }
}
