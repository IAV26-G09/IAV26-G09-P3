using System.IO;
using UnityEditor;
using UnityEngine;

namespace HFSM
{
    [CreateAssetMenu(menuName = "HSM/States/Attack", fileName = "Attack")]
    public class Attack : State
    {
        [SerializeField] float attackRange = 14f;

        protected override void OnEnter(BotGameplayActions a)
        {
            Debug.Log("ENTER ATTACK");
            a.StopNavigation();
        }

        protected override void OnExit(BotGameplayActions a)
        {
            // release fire when we leave attack
            a.TryFireCurrentWeaponPrimary(false, false, true);
        }

        protected override State GetTransition(BotGameplayActions a)
        {
            if (a.Health != null && a.Health.CurrentHealth <= a.Health.CriticalHealthRatio)
                return FindTransition("Recover");

            if (!a.HasEnemyTarget())
                return FindTransition("Patrol");

            if (!a.CanAttackCurrentEnemy(attackRange))
                return FindTransition("Pursue");

            return null;
        }

        protected override void OnUpdate(StateMachine m, float deltaTime)
        {
            var actions = m.Owner.Actions;

            if (!actions.HasEnemyTarget())
                return;

            actions.StopNavigation();
            actions.FaceCurrentEnemy();

            bool fired = actions.TryFireCurrentWeaponPrimary(true, true, false);
            if (!fired)
                actions.TryReloadActiveWeapon();
        }
    }
}

