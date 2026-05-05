using UnityEngine;

namespace HFSM
{
    [CreateAssetMenu(menuName = "HSM/States/Patrol", fileName = "Patrol")]
    public class Patrol : State
    {
        [SerializeField] float minPatrolRadius = 6f;

        protected override void OnEnter(BotGameplayActions a)
        {
            Debug.Log("ENTRANDO A PATROL");
            a.EnableNavMeshAgent();
            a.Sprint(true);

            var agent = a.NavMeshAgent;
            if (agent != null && agent.enabled)
            {
                agent.ResetPath();
                agent.isStopped = false;
            }
        }

        protected override State GetTransition(BotGameplayActions a)
        {
            if (a.Health != null && a.Health.IsCritical())
            {
                State e = FindTransition("Recover");
                Debug.Log("VOY A RECOVER");
                return e;
            }

            if (a.HasEnemyTarget())
            {
                State e = FindTransition("Engage");
                if (e != null)
                {
                    Debug.Log("VOY A ENGAGE");
                    return e;
                }
            }

            if (a.LootRequest)
            {
                if (a.HasNearbyLoot())
                {
                    State e = FindTransition("Loot");
                    if (e != null)
                    {
                        Debug.Log("SALGO DE ENGAGE Y VOY A LOOT");
                        return e;
                    }
                }
                else
                {
                    a.ClearLootRequest();
                }
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
                if (FSM.TryPickRandomNavMeshPointOutsideRadius(a.transform.position, minPatrolRadius, out var dest))
                {
                    Debug.Log("Nuevo punto de ruta");
                    a.TryMoveToWorldPosition(dest);
                }
            }
        }
    }
}
