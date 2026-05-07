using UnityEngine;

namespace IAV26.G09.P3
{
    [CreateAssetMenu(menuName = "HSM/States/Patrol", fileName = "Patrol")]
    public class Patrol : State
    {
        protected override void OnEnter(BotGameplayActions a)
        {
            a.EnableNavMeshAgent();

            var agent = a.NavMeshAgent;
            if (agent != null && agent.enabled)
            {
                agent.ResetPath();
                agent.isStopped = false;
            }
        }

        protected override State GetTransition(BotGameplayActions a)
        {
            if (a.Health != null && a.Health.IsCritical() && a.SeesHealth)
            {
                State e = FindTransition("Recover");
                return e;
            }

            if (a.HasEnemyTarget())
            {
                State e = FindTransition("Engage");
                if (e != null)
                {
                    return e;
                }
            }

            if (a.SeesWeapon && a.WeaponTransform != null)
            {
                State e = FindTransition("Loot");
                if (e != null)
                    return e;
            }

            return null;
        }

        protected override void OnUpdate(BotGameplayActions a, float deltaTime)
        {
            var agent = a.NavMeshAgent;
            if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            {
                Debug.Log("ERROR EN LA NAVMESH");
                return;
            }

            if (!agent.hasPath || (agent.hasPath && a.HasReachedCurrentDestination()))
            {
                a.TryMoveToNextWaypoint();
            }
        }
    }
}
