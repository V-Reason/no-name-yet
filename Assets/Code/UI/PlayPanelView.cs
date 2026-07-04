using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayPanelView : MonoBehaviour
{
    [Header("深度 UI")]
    public TextMeshProUGUI depthText;

    [Header("磁铁 UI")]
    public Image magnetIcon;
    [SerializeField] private Color magnetDimColor = new Color(0.35f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color magnetNormalColor = Color.white;
    [SerializeField] private Color magnetBrightColor = Color.white;
    [SerializeField] private float magnetBrightScale = 1.15f;

    private void Update()
    {
        depthText.text = $"{Blackboard.Instance.currentDepth:F1} M";
        UpdateMagnetIcon();
    }

    private void UpdateMagnetIcon()
    {
        if (magnetIcon == null) return;

        if (Blackboard.Instance.isMagnetActive)
        {
            magnetIcon.color = magnetBrightColor;
            magnetIcon.transform.localScale = Vector3.one * magnetBrightScale;
        }
        else if (Blackboard.Instance.hasMagnet)
        {
            magnetIcon.color = magnetNormalColor;
            magnetIcon.transform.localScale = Vector3.one;
        }
        else
        {
            magnetIcon.color = magnetDimColor;
            magnetIcon.transform.localScale = Vector3.one;
        }
    }
}
