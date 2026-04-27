using HSM;
using UnityEngine;

namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/Pursue")]
    public class Pursue : State
    {
        public Pursue(StateMachine m, State parent) : base(m, parent)
        {
        }
    }
}

