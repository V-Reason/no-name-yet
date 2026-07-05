using UnityEngine;

namespace RPG2D.Character.Monster.Fish
{
    public enum FishDirection
    {
        Left = -1,
        Right = 1,
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class Fish : MonoBehaviour
    {
        [Header("移动")]
        [SerializeField, Min(0f)] private float baseSpeed = 1.5f;
        [SerializeField, Min(0f)] private float speedVariance = 0.5f;

        [Header("垂直偏航")]
        [SerializeField, Min(0f)] private float verticalAmplitude = 2f;
        [SerializeField, Min(0.01f)] private float verticalFrequency = 0.3f;
        [SerializeField, Min(0f)] private float verticalPhaseVariance = 1f;

        [Header("大小")]
        [SerializeField, Min(0.01f)] private float baseScale = 1f;
        [SerializeField, Min(0f)] private float scaleVariance = 0.3f;

        [Header("回收")]
        [SerializeField, Min(0f)] private float despawnMargin = 3f;

        private float _speed;
        private FishDirection _direction;
        private float _centerY;
        private float _verticalPhase;
        private float _verticalFrequency;
        private FishSpawner _spawner;

        public void Init(FishSpawner spawner, FishDirection direction, float centerY)
        {
            _spawner = spawner;
            _direction = direction;
            _centerY = centerY;
            _speed = baseSpeed + Random.Range(-speedVariance, speedVariance);
            _verticalPhase = Random.Range(0f, verticalPhaseVariance * Mathf.PI * 2f);
            _verticalFrequency = verticalFrequency + Random.Range(-0.1f, 0.1f);

            float scale = baseScale + Random.Range(-scaleVariance, scaleVariance);
            transform.localScale = new Vector3(scale, scale, 1f);

            ApplyFacing();
        }

        private void Update()
        {
            float verticalOffset = Mathf.Sin(_verticalPhase + Time.time * _verticalFrequency) * verticalAmplitude;
            float horizontalMove = (int)_direction * _speed * Time.deltaTime;

            Vector3 pos = transform.position;
            pos.x += horizontalMove;
            pos.y = _centerY + verticalOffset;
            transform.position = pos;

            if (IsOutOfViewport())
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

        private bool IsOutOfViewport()
        {
            Camera cam = Camera.main;
            if (cam == null) return false;

            if (_direction == FishDirection.Right)
            {
                float rightEdge = cam.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
                return transform.position.x > rightEdge + despawnMargin;
            }
            else
            {
                float leftEdge = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
                return transform.position.x < leftEdge - despawnMargin;
            }
        }

        private void ApplyFacing()
        {
            Vector3 scale = transform.localScale;
            float absX = Mathf.Abs(scale.x);
            scale.x = (_direction == FishDirection.Right ? absX : -absX);
            transform.localScale = scale;
        }
    }
}
