using UnityEngine;

namespace HSM
{
    public class BotRoot : State
    {
        public readonly Dead dead;
        public readonly Alive alive;

        public BotRoot(StateMachine m)
            : base(m, null)
        {
            dead = new Dead(m, this);
            alive = new Alive(m, this);
        }

        protected override State GetInitialState() => alive;

        protected override State GetTransition()
        {
            return null;
        }
    }
}

