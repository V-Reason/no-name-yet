using UnityEngine;

namespace RPG2D.Character.Monster.Jellyfish
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Jellyfish : MonoBehaviour
    {
        [Header("移动")]
        [SerializeField, Min(0f)] private float baseSpeed = 0.5f;
        [SerializeField, Min(0f)] private float speedVariance = 0.3f;

        [Header("水平漂移")]
        [SerializeField, Min(0f)] private float driftAmplitude = 0.5f;
        [SerializeField, Min(0.01f)] private float driftFrequency = 0.5f;

        [Header("大小")]
        [SerializeField, Min(0.01f)] private float baseScale = 1f;
        [SerializeField, Min(0f)] private float scaleVariance = 0.3f;

        [Header("回收")]
        [SerializeField, Min(0f)] private float despawnMargin = 2f;

        private float _speed;
        private float _noiseOffset;
        private JellyfishSpawner _spawner;

        public void Init(JellyfishSpawner spawner)
        {
            _spawner = spawner;
            _speed = baseSpeed + Random.Range(-speedVariance, speedVariance);
            _noiseOffset = Random.Range(0f, 100f);

            float scale = baseScale + Random.Range(-scaleVariance, scaleVariance);
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        private void Update()
        {
            float drift = (Mathf.PerlinNoise(_noiseOffset + Time.time * driftFrequency, 0f) - 0.5f)
                * 2f * driftAmplitude;
            transform.position += new Vector3(drift, _speed, 0f) * Time.deltaTime;

            if (IsAboveViewport())
            {
                PoolManager.Instance.Despawn(gameObject);
            }
        }

        private void OnDisable()
        {
            if (_spawner != null)
            {
                _spawner.NotifyDespawned();
                _spawner = null;
            }
        }

        private bool IsAboveViewport()
        {
            Camera cam = Camera.main;
            if (cam == null) return false;

            float viewportTop = cam.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
            return transform.position.y > viewportTop + despawnMargin;
        }
    }
}
