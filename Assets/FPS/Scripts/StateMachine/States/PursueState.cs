using HFSM;
using UnityEngine;

namespace HFSM
{
    [CreateAssetMenu(menuName = "HSM/States/Pursue", fileName = "Pursue")]
    public class Pursue : State
    {
        [SerializeField] float attackRange = 14f;

        protected override void OnEnter(BotGameplayActions a)
        {
            Debug.Log("ENTRANDO A PURSUE");
        }

        protected override State GetTransition(BotGameplayActions a)
        {
            if (a.Health != null && a.Health.IsCritical())
                return FindTransition("Recover");

            if (!a.HasEnemyTarget())
            {
                if (a.SeesEnemy)
                    return null;

                return FindTransition("Patrol");
            }

            if (a.CanAttackCurrentEnemy(attackRange))
                return FindTransition("Attack");

            return null;
        }

        protected override void OnUpdate(BotGameplayActions a, float deltaTime)
        {
            if (!a.HasEnemyTarget())
                return;

            a.TryMoveToCurrentEnemy();
            a.FaceCurrentEnemy();
        }
    }
}

