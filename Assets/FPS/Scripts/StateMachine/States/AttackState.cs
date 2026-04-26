using UnityEngine;

namespace HSM
{
    public class Attack : State
    {
        public Attack(StateMachine m, State parent) : base(m, parent)
        {
        }

        protected override void OnEnter()
        {
            Debug.Log("ENTER ATTACK");
        }

        protected override void OnUpdate(float deltaTime)
        {
            Actions.TryFireCurrentWeaponPrimary(true, true, true);
        }
    }
}

