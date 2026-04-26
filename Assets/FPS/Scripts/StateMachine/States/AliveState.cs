using UnityEngine;

namespace HSM
{
    public class Alive : State
    {
        private Patrol patrol;
        private Engage engage;
        private Recover recover;
        private Loot loot;

        public Alive(StateMachine m, State parent) : base(m, parent)
        {
            patrol = new Patrol(m, this);
            engage = new Engage(m, this);
            recover = new Recover(m, this);
            loot = new Loot(m, this);
        }

        protected override State GetTransition()
        {
            //if (paseo)
            //{
            //    Debug.Log("VOY A PASEO");
            //    paseo = false;
            //    return ((BotRoot)Parent).Dead;
            //}
            return null;
        }

        protected override void OnEnter()
        {
            Debug.Log("ENTER ALIVE");
        }

        protected override State GetInitialState() => engage;
    }
}