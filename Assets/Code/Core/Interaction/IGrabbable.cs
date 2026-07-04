using UnityEngine;

namespace RPG2D.Core.Interaction
{
    public enum GrabType { Static, Linear }
    public interface IGrabbable
    {
        GrabType GrabType { get; }
        Vector2 GetGrabPosition(Vector2 playerPosition);
        Transform GetTransform();
        bool CanGrab();
    }
}
