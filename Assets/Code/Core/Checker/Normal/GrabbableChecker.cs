using RPG2D.Core.Interaction;

namespace RPG2D.Core.Checker
{
    public class GrabbableChecker : CircleChecker
    {
        public IGrabbable detectedGrabbable { get; private set; }

        public override void Check()
        {
            base.Check();
            if (overlapped)
            {
                detectedGrabbable = firstDetectedCollider.GetComponent<IGrabbable>();
            }
            else
            {
                detectedGrabbable = null;
            }
        }
    }
}
