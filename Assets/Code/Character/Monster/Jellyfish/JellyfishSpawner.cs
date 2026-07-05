using System.Collections;
using UnityEngine;

namespace RPG2D.Character.Monster.Jellyfish
{
    public class JellyfishSpawner : MonoBehaviour
    {
        [Header("生成")]
        [SerializeField] private GameObject jellyfishPrefab;
        [SerializeField, Min(1)] private int maxCount = 5;
        [SerializeField, Min(0.1f)] private float spawnInterval = 3f;
        [SerializeField, Min(0f)] private float intervalVariance = 1f;

        [Header("生成区域")]
        [SerializeField, Range(0f, 0.3f)] private float viewportMarginX = 0.1f;
        [SerializeField, Min(0f)] private float spawnBelowViewport = 2f;

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
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(viewportMarginX, 0, 0));
            Vector3 bottomRight = cam.ViewportToWorldPoint(new Vector3(1f - viewportMarginX, 0, 0));

            float spawnX = Random.Range(bottomLeft.x, bottomRight.x);
            float spawnY = bottomLeft.y - spawnBelowViewport;

            GameObject obj = PoolManager.Instance.Spawn(
                jellyfishPrefab,
                new Vector3(spawnX, spawnY, 0),
                Quaternion.identity);

            var jellyfish = obj.GetComponent<Jellyfish>();
            if (jellyfish != null)
            {
                _aliveCount++;
                jellyfish.Init(this);
            }
        }

        public void NotifyDespawned()
        {
            _aliveCount = Mathf.Max(0, _aliveCount - 1);
        }
    }
}
