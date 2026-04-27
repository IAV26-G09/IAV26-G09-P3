using UnityEngine;

namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/Patrol")]
    public class Patrol : State
    {
        public Patrol(StateMachine m, State parent) : base(m, parent)
        {
        }

        protected override void OnEnter()
        {
            Debug.Log("ENTRANDO A PATROL");
        }

        protected override State GetTransition()
        {
            var agent = Actions.NavMeshAgent;
            if (agent.hasPath && Actions.HasReachedCurrentDestination())
            {
                //Debug.Log("VOY A IDLE");
                //return ((Dead)Parent).Idle;
            }

            return null;
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            var actions = Actions;
            var agent = actions.NavMeshAgent;
            if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            {
                Debug.Log("NO TENGO NAVMESH!!!!!!!!!!!!!!!");
                return;
            }

            if (!agent.hasPath || (agent.hasPath && Actions.HasReachedCurrentDestination()))
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