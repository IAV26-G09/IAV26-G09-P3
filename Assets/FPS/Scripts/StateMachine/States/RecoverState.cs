using UnityEngine;

namespace HSM
{
    public class Recover : State
    {
        private RunAway run;
        private Heal heal;

        protected override State GetInitialState() => run;

        public Recover(StateMachine m, State parent) : base(m, parent)
        {
            run = new RunAway(m, this);
            heal = new Heal(m, this);
        }
    }
}