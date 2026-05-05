using UnityEngine;

namespace HFSM
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
            Debug.Log("ENTRANDO A RUNAWAY");

            if (a.NavMeshAgent != null)
            {
                a.NavMeshAgent.angularSpeed = runAwayAngularSpeed;
                m_PreviousAcceleration = a.NavMeshAgent.acceleration;
                m_HasPreviousAcceleration = true;
                a.NavMeshAgent.acceleration = Mathf.Max(m_PreviousAcceleration, runAwayAcceleration);
                a.Sprint(true);
            }
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

        protected override void OnUpdate(BotGameplayActions a, float deltaTime)
        {
            if (a.Flee())
            {
                //Debug.Log("HUYENDO");
            }
        }

        protected override State GetTransition(BotGameplayActions a)
        {
            if (a.SeesHealth)
            {
                Debug.Log("VOY A HEAL");
                State e = Transitions.Find(x => x.stateName.Contains("Heal"));
                return e;
            }

            return null;
        }
    }
}
