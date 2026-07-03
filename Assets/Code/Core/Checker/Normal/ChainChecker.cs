namespace RPG2D.Core.Checker
{
    public class ChainChecker : CircleChecker
    {
        public Chain detectedChain { get; private set; }

        public override void Check()
        {
            base.Check();
            if (overlapped)
            {
                detectedChain = firstDetectedCollider.GetComponent<Chain>();
            }
            else
            {
                detectedChain = null;
            }
        }
    }
}
