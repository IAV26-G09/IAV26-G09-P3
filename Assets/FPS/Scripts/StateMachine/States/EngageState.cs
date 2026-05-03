using UnityEngine;

namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/Engage", fileName = "Engage")]
    public class Engage : State
    {
        protected override void OnEnter(BotGameplayActions a)
        {
            Debug.Log("ENTRANDO A ENGAGE");
        }

        protected override State GetTransition(BotGameplayActions a)
        {
            // si baja de x vida -> recover
            if (a.Health.CurrentHealth <= a.Health.CriticalHealthRatio)
            {
                State e = Transitions.Find(x => x.stateName.Contains("Recover"));
                Debug.Log("VOY A RECOVER");
                return e;
            }

            return null;
        }
    }
}