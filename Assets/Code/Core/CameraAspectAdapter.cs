using Cinemachine;
using UnityEngine;

public class CameraAspectAdapter : MonoBehaviour
{
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);

    private CinemachineVirtualCamera _vcam;
    private float _baseOrthoSize;
    private float _lastAspect;

    private void Awake()
    {
        _vcam = GetComponent<CinemachineVirtualCamera>();
        _baseOrthoSize = _vcam.m_Lens.OrthographicSize;
    }

    private void Start() => Apply();

    private void Update()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        if (!Mathf.Approximately(currentAspect, _lastAspect))
            Apply();
    }

    private void Apply()
    {
        float targetAspect = referenceResolution.x / referenceResolution.y;
        float currentAspect = (float)Screen.width / Screen.height;
        _lastAspect = currentAspect;

        _vcam.m_Lens.OrthographicSize = _baseOrthoSize * (targetAspect / currentAspect);
    }
}
