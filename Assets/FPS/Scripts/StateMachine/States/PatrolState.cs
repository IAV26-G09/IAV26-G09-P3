using UnityEngine;

namespace IAV26.G09.P3
{
    [CreateAssetMenu(menuName = "HSM/States/Patrol", fileName = "Patrol")]
    public class Patrol : State
    {
        int m_LastWaypointIndex = -1;

        protected override void OnEnter(BotGameplayActions a)
        {
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
                TryMoveToNextWaypoint(a);
            }
        }

        bool TryMoveToNextWaypoint(BotGameplayActions a)
        {
            int waypointCount = a.GetPatrolWaypointCount();
            if (waypointCount <= 0)
                return false;

            int attempts = waypointCount;
            while (attempts-- > 0)
            {
                int waypointIndex = PickRandomWaypointIndex(waypointCount);
                if (!a.TryGetPatrolWaypointPosition(waypointIndex, out var waypointPosition))
                    continue;

                if (!a.TryMoveToWorldPosition(waypointPosition))
                    continue;

                m_LastWaypointIndex = waypointIndex;
                return true;
            }

            return false;
        }

        int PickRandomWaypointIndex(int waypointCount)
        {
            if (waypointCount <= 1)
                return 0;

            int index = Random.Range(0, waypointCount - 1);
            if (m_LastWaypointIndex >= 0 && index >= m_LastWaypointIndex)
            {
                index++;
            }

            return index;
        }
    }
}
