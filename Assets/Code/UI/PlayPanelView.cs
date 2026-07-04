using UnityEngine;
using TMPro;

public class PlayPanelView : MonoBehaviour
{
    [Header("UI 引用")]
    public TextMeshProUGUI depthText;

    private void Update()
    {
        depthText.text = $"{Blackboard.Instance.currentDepth:F1} M";
    }
}
