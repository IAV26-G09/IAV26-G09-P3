using UnityEngine;

namespace HSM
{
    public class Engage : State
    {
        private Attack attack;
        private Pursue pursue;

        protected override State GetInitialState() => attack;

        public Engage(StateMachine m, State parent) : base(m, parent)
        {
            attack = new Attack(m, this);
            pursue = new Pursue(m, this);
        }
    }
}