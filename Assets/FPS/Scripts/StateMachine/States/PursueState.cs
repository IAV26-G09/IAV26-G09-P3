using IAV26.G09.P3;
using UnityEngine;

namespace IAV26.G09.P3
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
                if (a.HasKnownEnemy())
                    return null;

                return FindTransition("Patrol");
            }

            if (a.CanAttackCurrentEnemy(attackRange))
                return FindTransition("Attack");

            return null;
        }

        protected override void OnUpdate(BotGameplayActions a, float deltaTime)
        {
            if (!a.HasKnownEnemy())
                return;

            a.TryMoveToCurrentEnemy();
            a.TryFaceEnemy();
        }
    }
}

