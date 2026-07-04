using RPG2D.Core.Interaction;

namespace RPG2D.Core.Checker
{
    public class InteractableChecker : CircleChecker
    {
        public IGrabbable detectedGrabbable { get; private set; }

        public override void Check()
        {
            base.Check(); // 执行原有的 OverlapCircle 检测
            if (overlapped)
            {
                // 关键：在命中的物体上找接口，不论它是 Anchor 还是 Chain
                detectedGrabbable = firstDetectedCollider.GetComponent<IGrabbable>();
            }
            else
            {
                detectedGrabbable = null;
            }
        }
    }
}
