using UnityEngine;

namespace IAV26.G09.P3
{
    [CreateAssetMenu(menuName = "HSM/States/Heal", fileName = "Heal")]
    public class Heal : State
    {
        private float pHealth;
        bool noHealing = false;

        protected override void OnEnter(BotGameplayActions a)
        {
            pHealth = a.Health.CurrentHealth;
            
            a.Sprint(true);

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
            State patrol = FindTransition("Patrol");

            if (noHealing)
            {
                noHealing = false;
                a.SeesHealth = false;
                return patrol;
            }

            bool healed = a.Health.CurrentHealth > pHealth;
            bool hasHealthNearby = a.SeesHealth && a.HealthTransform != null;

            if (healed && !hasHealthNearby)
            {
                a.SeesHealth = false;
                return patrol;
            }

            if (healed && hasHealthNearby)
            {
                pHealth = a.Health.CurrentHealth;
            }

            return null;
        }
    }
}