using UnityEngine;

namespace HSM
{
    public class Combat : State
    {
        private bool paseo = true;

        public Combat(StateMachine m, State parent) : base(m, parent)
        {
        }

        protected override State GetTransition()
        {
            if (paseo)
            {
                Debug.Log("VOY A PASEO");
                paseo = false;
                return ((BotRoot)Parent).Paseo;
            }
            else
            {
                return null;
            }
        }

        protected override void OnEnter()
        {
            Debug.Log("ENTRANDO A COMBAT");
        }
    }
}