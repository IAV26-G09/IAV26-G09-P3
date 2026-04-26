using UnityEngine;

namespace HSM
{
    public class Paseo : State
    {
        public readonly Idle Idle;
        public readonly Patrol Patrol;

        public Paseo(StateMachine m, State parent) : base(m, parent)
        {
            Idle = new Idle(m, this);
            Patrol = new Patrol(m, this);
        }

        protected override State GetInitialState() => Idle;
    }
}