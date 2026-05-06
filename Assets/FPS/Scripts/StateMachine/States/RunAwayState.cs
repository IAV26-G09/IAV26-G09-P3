using UnityEngine;

namespace IAV26.G09.P3
{
    [CreateAssetMenu(menuName = "HSM/States/RunAway", fileName = "RunAway")]
    public class RunAway : State
    {
        [SerializeField] float runAwayAngularSpeed = 720f;
        [SerializeField] float normalAngularSpeed = 120f;
        [SerializeField] float runAwayAcceleration = 55f;

        float m_PreviousAcceleration;
        bool m_HasPreviousAcceleration;

        protected override void OnEnter(BotGameplayActions a)
        {
            if (a.NavMeshAgent != null)
            {
                a.NavMeshAgent.angularSpeed = runAwayAngularSpeed;
                m_PreviousAcceleration = a.NavMeshAgent.acceleration;
                m_HasPreviousAcceleration = true;
                a.NavMeshAgent.acceleration = Mathf.Max(m_PreviousAcceleration, runAwayAcceleration);
                a.Sprint(true);
            }
        }

        protected override void OnUpdate(BotGameplayActions a, float deltaTime)
        {
            a.Flee();
        }

        protected override void OnExit(BotGameplayActions a)
        {
            if (a.NavMeshAgent != null)
            {
                a.NavMeshAgent.angularSpeed = normalAngularSpeed;
                if (m_HasPreviousAcceleration)
                    a.NavMeshAgent.acceleration = m_PreviousAcceleration;

                a.Sprint(false);
            }

            m_HasPreviousAcceleration = false;
        }

        protected override State GetTransition(BotGameplayActions a)
        {
            if (a.SeesHealth)
            {
                State e = Transitions.Find(x => x.stateName.Contains("Heal"));
                return e;
            }

            if (!a.SeesEnemy || !a.HasKnownEnemy())
            {
                return FindTransition("Patrol");
            }

            return null;
        }
    }
}
