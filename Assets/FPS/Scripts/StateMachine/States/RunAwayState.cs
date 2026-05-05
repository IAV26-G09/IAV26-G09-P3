using UnityEngine;

namespace HFSM
{
    [CreateAssetMenu(menuName = "HSM/States/RunAway", fileName = "RunAway")]
    public class RunAway : State
    {
        [SerializeField] float runAwayAngularSpeed = 720f;
        [SerializeField] float normalAngularSpeed = 120f;

        protected override void OnEnter(BotGameplayActions a)
        {
            Debug.Log("ENTRANDO A RUNAWAY");

            if (a.NavMeshAgent != null)
            {
                a.NavMeshAgent.angularSpeed = runAwayAngularSpeed;
                a.Sprint(true);
            }
        }

        protected override void OnExit(BotGameplayActions a)
        {
            if (a.NavMeshAgent != null)
            {
                a.NavMeshAgent.angularSpeed = normalAngularSpeed;
                a.Sprint(false);
            }
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
