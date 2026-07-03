using System;
using System.Collections;
using UnityEngine;

namespace RPG2D.Core.Actor
{
    /// <summary>
    /// 视觉特效组件
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFX : MonoBehaviour
    {
        [Header("材质配置")]
        [Tooltip("受击高亮材质")]
        [SerializeField] private Material flashMat;
        [Tooltip("单次闪烁持续时间")]
        [SerializeField] private float flashDuration = 0.1f;
        [Tooltip("循环闪烁的频率")]
        [SerializeField] private float blinkInterval = 0.1f;

        private SpriteRenderer sr;
        private Material originalMat;
        private Color originalColor;

        // 预缓存设计，GC友好
        private Coroutine activeFlashCoroutine;
        private Coroutine activeBlinkCoroutine;
        private WaitForSeconds flashWait;
        private WaitForSeconds blinkWait;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            originalMat = sr.material;
            originalColor = sr.color;

            flashWait = new(flashDuration);
            blinkWait = new(blinkInterval);
        }

        /// <summary>
        /// 单次闪烁
        /// </summary>
        public void PlayFlash()
        {
            if (activeFlashCoroutine != null) StopCoroutine(activeFlashCoroutine);
            activeFlashCoroutine = StartCoroutine(FlashCoroutine());
        }

        /// <summary>
        /// 开始循环闪烁
        /// </summary>
        public void StartBlink(Color blinkColor)
        {
            if (activeBlinkCoroutine != null) StopCoroutine(activeBlinkCoroutine);
            activeBlinkCoroutine = StartCoroutine(BlinkCoroutine(blinkColor));
        }

        /// <summary>
        /// 停止循环闪烁
        /// </summary>
        public void StopBlink()
        {
            if (activeBlinkCoroutine != null)
            {
                StopCoroutine(activeBlinkCoroutine);
                activeBlinkCoroutine = null;
            }
            sr.color = originalColor;
        }

        private IEnumerator FlashCoroutine()
        {
            sr.material = flashMat;
            yield return flashWait;
            sr.material = originalMat;
            activeFlashCoroutine = null;
        }

        private IEnumerator BlinkCoroutine(Color blinkColor)
        {
            while (true)
            {
                sr.color = (sr.color != blinkColor) ? blinkColor : originalColor;
                yield return blinkWait;
            }
        }

        // 隐藏时防止协程未关闭
        private void OnDisable()
        {
            StopBlink();
            if (activeFlashCoroutine != null)
            {
                StopCoroutine(activeFlashCoroutine);
                sr.material = originalMat;
                activeFlashCoroutine = null;
            }
        }
    }
}