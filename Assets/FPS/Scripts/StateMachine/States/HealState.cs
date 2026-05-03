using UnityEngine;

namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/Heal", fileName = "Heal")]
    public class Heal : State
    {
        private float pHealth;
        bool noHealing = false;

        protected override void OnEnter(BotGameplayActions a)
        {
            Debug.Log("ENTRANDO A HEAL");
            pHealth = a.Health.CurrentHealth;

            if (a.HealthTransform != null)
            {
                a.TryMoveToWorldPosition(a.HealthTransform.position);
            }
            else
            {
                noHealing = true;
            }
        }

        protected override State GetTransition(BotGameplayActions a)
        {
            Debug.Log(a.Health.CurrentHealth + " " + pHealth);

            State e = Transitions.Find(x => x.stateName.Contains("Patrol"));

            if (a.Health.CurrentHealth > pHealth || noHealing)
            {
                Debug.Log("RECUPERADA");
                noHealing = false;
                return e;
            }

            return null;
        }

        protected override void OnUpdate(StateMachine m, float deltaTime)
        {
            
        }
    }
}