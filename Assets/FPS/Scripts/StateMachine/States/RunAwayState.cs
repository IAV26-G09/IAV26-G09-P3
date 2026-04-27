using UnityEngine;

namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/RunAway")]
    public class RunAway : State
    {
        public RunAway(StateMachine m, State parent) : base(m, parent)
        {

        }
    }
}