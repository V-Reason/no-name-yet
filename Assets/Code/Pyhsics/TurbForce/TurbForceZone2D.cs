using UnityEngine;
using System.Collections.Generic;
using RPG2D.Core.Checker;
using RPG2D.Core.Interaction;

namespace RPG2D.Pyhsics.TurbForce
{
    /// <summary>
    /// 固定 2D 湍流力场, 主动检测区域内目标并施加固定方向和强度的力.
    /// </summary>
    [RequireComponent(typeof(AreaTargetChecker2D))]
    [ExecuteAlways]
    public class TurbForceZone2D : MonoBehaviour
    {
        private readonly HashSet<IForceReceiver> appliedReceivers = new();

        [SerializeField, Min(0f)]
        private float forceMagnitude = 10f;

        [SerializeField]
        private bool useEdgeFalloff = true;

        [SerializeField, Min(0.01f)]
        private float edgeFalloffDistance = 1f;

        [SerializeField, Range(0f, 1f)]
        private float edgeForceMultiplier = 0f;

        private static readonly int ShaderPropFlowDirection = Shader.PropertyToID("_FlowDirection");

        private AreaTargetChecker2D targetChecker;
        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock propertyBlock;

        /// <summary>
        /// 获取当前区域提供的湍流力, 方向自动跟随 Transform 的 Y 轴朝向, 强度由 forceMagnitude 控制.
        /// </summary>
        public Vector2 Force => (Vector2)transform.up * forceMagnitude;

        private void Reset()
        {
            EnsureTriggerCollider();
        }

        private void Awake()
        {
            targetChecker = GetComponent<AreaTargetChecker2D>();
            EnsureTriggerCollider();
            CacheTurbulenceMaterial();
        }

        private void LateUpdate()
        {
            SyncFlowDirection();
        }

        private void OnValidate()
        {
            forceMagnitude = Mathf.Max(0f, forceMagnitude);
            edgeFalloffDistance = Mathf.Max(0.01f, edgeFalloffDistance);
            edgeForceMultiplier = Mathf.Clamp01(edgeForceMultiplier);
            EnsureTriggerCollider();
        }

        private void FixedUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ApplyForceToTargets();
        }

        /// <summary>
        /// 设置湍流强度, 方向由 Transform 的 Y 轴朝向自动决定.
        /// </summary>
        public void SetForce(float magnitude)
        {
            forceMagnitude = Mathf.Max(0f, magnitude);
        }

        /// <summary>
        /// 检测力场范围内的目标, 并对每个受力接口对象施加一次湍流力.
        /// </summary>
        private void ApplyForceToTargets()
        {
            if (targetChecker == null)
            {
                targetChecker = GetComponent<AreaTargetChecker2D>();
            }

            if (targetChecker == null)
            {
                return;
            }

            appliedReceivers.Clear();
            targetChecker.Check();

            for (int i = 0; i < targetChecker.HitCount; i++)
            {
                Collider2D targetCollider = targetChecker[i];
                if (!ForceReceiverResolver2D.TryGetUniqueReceiver(targetCollider, appliedReceivers, out IForceReceiver receiver))
                {
                    continue;
                }

                Vector2 targetPosition = ForceReceiverResolver2D.GetForceSamplePosition(targetCollider);
                Vector2 force = GetForceAt(targetPosition);
                if (force.sqrMagnitude <= Mathf.Epsilon)
                {
                    continue;
                }

                receiver.ApplyForce(force);
            }
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

        private void CacheTurbulenceMaterial()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }
        }

        private void SyncFlowDirection()
        {
            if (meshRenderer == null)
            {
                return;
            }

            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetVector(ShaderPropFlowDirection, -Vector2.up);
            meshRenderer.SetPropertyBlock(propertyBlock);
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 force = Force;
            if (force.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            Gizmos.color = Color.red;
            Vector3 start = transform.position;
            Vector3 end = start + (Vector3)force;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.08f);
        }
    }
}
