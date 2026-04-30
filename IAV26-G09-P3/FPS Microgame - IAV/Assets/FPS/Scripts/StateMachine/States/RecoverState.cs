using UnityEngine;

namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/Recover", fileName = "Recover")]
    public class Recover : State
    {
        private RunAway run;
        private Heal heal;

        protected override State GetInitialState() => run;
    }
}