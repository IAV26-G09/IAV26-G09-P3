using UnityEngine;

namespace HFSM
{
    [CreateAssetMenu(menuName = "HSM/States/Patrol", fileName = "Patrol")]
    public class Patrol : State
    {
        protected override void OnEnter(BotGameplayActions a)
        {
            Debug.Log("ENTRANDO A PATROL");
        }

        protected override State GetTransition(BotGameplayActions a)
        {
            if (a.Health != null && a.Health.CurrentHealth <= a.Health.CriticalHealthRatio)
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

            return null;
        }

        protected override void OnUpdate(StateMachine m, float deltaTime)
        {
            var actions = m.Owner.Actions;
            var agent = actions.NavMeshAgent;
            if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            {
                Debug.Log("NO TENGO NAVMESH!!!!!!!!!!!!!!!");
                return;
            }

            if (!agent.hasPath || (agent.hasPath && actions.HasReachedCurrentDestination()))
            {
                if (FSM.TryPickRandomNavMeshPoint(actions.transform.position, 20f, out var dest))
                {
                    Debug.Log("Nuevo punto de ruta");

                    actions.TryMoveToWorldPosition(dest);
                }
            }
        }
    }
}
