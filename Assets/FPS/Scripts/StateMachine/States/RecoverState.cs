using UnityEngine;

namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/Recover")]
    public class Recover : State
    {
        private RunAway run;
        private Heal heal;

        protected override State GetInitialState() => run;
    }
}