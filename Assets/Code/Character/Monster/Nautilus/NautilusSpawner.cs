using System.Collections;
using UnityEngine;

namespace RPG2D.Character.Monster.Nautilus
{
    public class NautilusSpawner : MonoBehaviour
    {
        [Header("生成")]
        [SerializeField] private GameObject nautilusPrefab;
        [SerializeField, Min(1)] private int maxCount = 5;
        [SerializeField, Min(0.1f)] private float spawnInterval = 3f;
        [SerializeField, Min(0f)] private float intervalVariance = 1f;

        [Header("生成区域")]
        [SerializeField, Range(0f, 0.3f)] private float viewportMarginX = 0.1f;
        [SerializeField, Min(0f)] private float spawnAboveViewport = 2f;

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
            if (nautilusPrefab == null)
            {
                Debug.LogWarning($"[{name}] NautilusSpawner.nautilusPrefab 未赋值", this);
                return;
            }

            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 topLeft = cam.ViewportToWorldPoint(new Vector3(viewportMarginX, 1, 0));
            Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1f - viewportMarginX, 1, 0));

            float spawnX = Random.Range(topLeft.x, topRight.x);
            float spawnY = topLeft.y + spawnAboveViewport;

            GameObject obj = PoolManager.Instance.Spawn(
                nautilusPrefab,
                new Vector3(spawnX, spawnY, 0),
                Quaternion.identity);

            var nautilus = obj.GetComponent<Nautilus>();
            if (nautilus != null)
            {
                _aliveCount++;
                nautilus.Init(this);
            }
        }

        public void NotifyDespawned()
        {
            _aliveCount = Mathf.Max(0, _aliveCount - 1);
        }
    }
}
