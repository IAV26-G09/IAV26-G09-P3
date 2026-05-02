using UnityEngine;

namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/RunAway", fileName = "RunAway")]
    public class RunAway : State
    {
        protected override void OnEnter(BotGameplayActions a)
        {
            Debug.Log("ENTRANDO A RUNAWAY");
        }

        protected override void OnUpdate(StateMachine m, float deltaTime)
        {
            m.Owner.Actions.Flee();
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