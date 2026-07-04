using RPG2D.Item;
using UnityEngine;

public class MagnetBlackboardBridge : MonoBehaviour
{
    [SerializeField] private PlayerItemHolder holder;

    private void OnEnable()
    {
        if (holder != null)
        {
            holder.OnMagnetChanged += HandleMagnetChanged;
        }
    }

    private void OnDisable()
    {
        if (holder != null)
        {
            holder.OnMagnetChanged -= HandleMagnetChanged;
        }
    }

    private void Start()
    {
        if (holder != null)
        {
            Blackboard.Instance.hasMagnet = holder.HasMagnet;
        }
    }

    private void Update()
    {
        if (holder == null) return;

        var active = holder.ActiveHeldItem;
        if (active != null && active.IsExpired)
        {
            Blackboard.Instance.isMagnetActive = false;
        }
    }

    private void HandleMagnetChanged(bool hasMagnet)
    {
        Blackboard.Instance.hasMagnet = hasMagnet;
        Blackboard.Instance.isMagnetActive = !hasMagnet;
    }
}
