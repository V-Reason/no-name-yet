using System.Collections;
using UnityEngine;

namespace RPG2D.Character.Monster.Jellyfish
{
    public class JellyfishV2Spawner : MonoBehaviour, IJellyfishSpawner
    {
        [Header("生成")]
        [SerializeField] private GameObject jellyfishPrefab;
        [SerializeField, Min(1)] private int maxCount = 6;
        [SerializeField, Min(0.1f)] private float spawnInterval = 4f;
        [SerializeField, Min(0f)] private float intervalVariance = 1.5f;

        [Header("成组生成")]
        [SerializeField, Min(1)] private int groupSizeMin = 1;
        [SerializeField, Min(1)] private int groupSizeMax = 3;
        [SerializeField, Min(0f)] private float groupSpreadX = 1.5f;
        [SerializeField, Min(0f)] private float groupStaggerDelay = 0.3f;

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
                    StartCoroutine(SpawnGroup());
                }

                float delay = spawnInterval + Random.Range(-intervalVariance, intervalVariance);
                yield return new WaitForSeconds(Mathf.Max(0.1f, delay));
            }
        }

        private IEnumerator SpawnGroup()
        {
            int groupSize = Random.Range(groupSizeMin, groupSizeMax + 1);
            float centerX = GetSpawnX();

            for (int i = 0; i < groupSize; i++)
            {
                if (_aliveCount >= maxCount) break;

                float offsetX = groupSize > 1
                    ? Mathf.Lerp(-groupSpreadX, groupSpreadX, i / (float)(groupSize - 1))
                    : 0f;

                SpawnOne(centerX + offsetX);

                if (i < groupSize - 1)
                    yield return new WaitForSeconds(groupStaggerDelay);
            }
        }

        private float GetSpawnX()
        {
            Camera cam = Camera.main;
            if (cam == null) return 0f;

            Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(viewportMarginX, 0, 0));
            Vector3 bottomRight = cam.ViewportToWorldPoint(new Vector3(1f - viewportMarginX, 0, 0));
            return Random.Range(bottomLeft.x, bottomRight.x);
        }

        private void SpawnOne(float spawnX)
        {
            if (jellyfishPrefab == null)
            {
                Debug.LogWarning($"[{name}] JellyfishV2Spawner.jellyfishPrefab 未赋值", this);
                return;
            }

            Camera cam = Camera.main;
            if (cam == null) return;

            float spawnY = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).y - spawnBelowViewport;

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
