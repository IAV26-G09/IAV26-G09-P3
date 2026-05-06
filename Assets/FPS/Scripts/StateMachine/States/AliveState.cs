using UnityEngine;

namespace IAV26.G09.P3
{
    [CreateAssetMenu(menuName = "HSM/States/Alive", fileName = "Alive")]
    public class Alive : State
    {
        protected override State GetTransition(BotGameplayActions a)
        {
            if (a.Health.HasDied)
            {
                a.Health.HasDied = false;
                a.Respawn();
                State e = Transitions.Find(x => x.stateName.Contains("Dead"));
                return e;
            }

            return null;
        }

        protected override void OnEnter(BotGameplayActions a)
        {
            a.Sprint(true);
        }
    }
}