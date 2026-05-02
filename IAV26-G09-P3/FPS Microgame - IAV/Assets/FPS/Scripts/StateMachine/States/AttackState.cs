using System.IO;
using UnityEditor;
using UnityEngine;

namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/Attack", fileName = "Attack")]
    public class Attack : State
    {
        protected override void OnEnter(BotGameplayActions a)
        {
            Debug.Log("ENTER ATTACK");
        }


        protected override void OnUpdate(StateMachine m, float deltaTime)
        {
            m.Owner.Actions.TryFireCurrentWeaponPrimary(true, true, true);

            // si baja de x vida -> huye
            if (m.Owner.Actions.Health.CurrentHealth <= m.Owner.Actions.Health.CriticalHealthRatio)
            {

            }
        }
    }
}

