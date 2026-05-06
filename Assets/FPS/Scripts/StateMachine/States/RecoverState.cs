using UnityEngine;

namespace IAV26.G09.P3
{
    [CreateAssetMenu(menuName = "HSM/States/Recover", fileName = "Recover")]
    public class Recover : State
    {
        protected override State GetTransition(BotGameplayActions a)
        {
            if (a.Health != null && a.Health.IsCritical())
                return null;

            if (a.HasEnemyTarget())
                return FindTransition("Engage");

            return FindTransition("Patrol");
        }
    }
}
