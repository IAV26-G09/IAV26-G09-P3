using UnityEngine;

namespace IAV26.G09.P3
{
    [CreateAssetMenu(menuName = "HSM/States/Engage", fileName = "Engage")]
    public class Engage : State
    {
        protected override State GetInitialState()
        {
            return FindTransition("Pursue");
        }

        protected override void OnExit(BotGameplayActions a)
        {
            a.ResetView();
            a.ForgetEnemy();
        }

        protected override State GetTransition(BotGameplayActions a)
        {
            if (a.Health != null && a.Health.IsCritical())
            {
                State e = FindTransition("Recover");
                return e;
            }

            if (!a.HasEnemyTarget())
            {
                if (a.HasKnownEnemy())
                    return null;

                State e = FindTransition("Patrol");
                if (e != null)
                {
                    return e;
                }
            }

            return null;
        }
    }
}
