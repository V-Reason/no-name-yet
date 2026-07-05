using System.Collections;
using UnityEngine;

namespace RPG2D.Character.Monster.Fish
{
    public class FishSpawner : MonoBehaviour
    {
        [Header("生成")]
        [SerializeField] private GameObject fishPrefab;
        [SerializeField, Min(1)] private int maxCount = 3;
        [SerializeField, Min(0.1f)] private float spawnInterval = 2f;
        [SerializeField, Min(0f)] private float intervalVariance = 1f;

        [Header("生成区域")]
        [SerializeField, Range(0f, 0.4f)] private float viewportMarginY = 0.15f;
        [SerializeField, Min(0f)] private float spawnOutsideMargin = 2f;

        private int _aliveCount;

        private void Start()
        {
            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                if (_aliveCount < maxCount)
                {
                    TrySpawn();
                }

                float delay = spawnInterval + Random.Range(-intervalVariance, intervalVariance);
                yield return new WaitForSeconds(Mathf.Max(0.1f, delay));
            }
        }

        private void TrySpawn()
        {
            if (fishPrefab == null)
            {
                Debug.LogWarning($"[{name}] FishSpawner.fishPrefab 未赋值", this);
                return;
            }

            Camera cam = Camera.main;
            if (cam == null) return;

            FishDirection direction = Random.value > 0.5f ? FishDirection.Right : FishDirection.Left;

            float spawnX;
            if (direction == FishDirection.Right)
            {
                Vector3 leftEdge = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
                spawnX = leftEdge.x - spawnOutsideMargin;
            }
            else
            {
                Vector3 rightEdge = cam.ViewportToWorldPoint(new Vector3(1, 0, 0));
                spawnX = rightEdge.x + spawnOutsideMargin;
            }

            Vector3 bottomEdge = cam.ViewportToWorldPoint(new Vector3(0, viewportMarginY, 0));
            Vector3 topEdge = cam.ViewportToWorldPoint(new Vector3(0, 1f - viewportMarginY, 0));
            float spawnY = Random.Range(bottomEdge.y, topEdge.y);

            GameObject obj = PoolManager.Instance.Spawn(
                fishPrefab,
                new Vector3(spawnX, spawnY, 0),
                Quaternion.identity);

            var fish = obj.GetComponent<Fish>();
            if (fish != null)
            {
                _aliveCount++;
                fish.Init(this, direction, spawnY);
            }
        }

        public void NotifyDespawned()
        {
            _aliveCount = Mathf.Max(0, _aliveCount - 1);
        }
    }
}
