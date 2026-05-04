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

        protected override State GetInitialState()
        {
            return FindTransition("Pursue");
        }

        protected override void OnExit(BotGameplayActions a)
        {
            //a.SeesEnemy = false;
            a.ForgetEnemy();
        }

        protected override State GetTransition(BotGameplayActions a)
        {
            if (a.Health != null && a.Health.CurrentHealth <= a.Health.CriticalHealthRatio)
            {
                State e = FindTransition("Recover");
                Debug.Log("VOY A RECOVER");
                return e;
            }

            if (!a.HasEnemyTarget())
            {
                State e = FindTransition("Patrol");
                if (e != null)
                {
                    Debug.Log("VOY A PATROL");
                    return e;
                }
            }

            return null;
        }
    }
}