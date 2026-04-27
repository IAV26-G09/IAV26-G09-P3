using UnityEngine;

namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/Attack")]
    public class Attack : State
    {
        public Attack(StateMachine m, State parent) : base(m, parent)
        {
        }

        protected override void OnEnter()
        {
            Debug.Log("ENTER ATTACK");
        }

        protected override void OnUpdate(StateMachine m, float deltaTime)
        {
            Debug.Log("DISPAROOOOOOOOOOOOOOOOOOOOOOOO");
            m.Owner.Actions.TryFireCurrentWeaponPrimary(true, true, true);
        }
    }
}

