using UnityEngine;

namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/Heal", fileName = "Heal")]
    public class Heal : State
    {
        protected override void OnEnter(BotGameplayActions a)
        {
            Debug.Log("ENTRANDO A HEAL");
        }

        protected override State GetTransition(BotGameplayActions a)
        {
            return null;
        }

        protected override void OnUpdate(StateMachine m, float deltaTime)
        {
            var actions = m.Owner.Actions;
            var agent = actions.NavMeshAgent;
            if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            {
                Debug.Log("NO TENGO NAVMESH");
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