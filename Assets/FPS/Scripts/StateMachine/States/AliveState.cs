using UnityEngine;

namespace IAV26.G09.P3
{
    [CreateAssetMenu(menuName = "HSM/States/Alive", fileName = "Alive")]
    public class Alive : State
    {
        protected override State GetTransition(BotGameplayActions a)
        {
            //Debug.Log(a.Health.CurrentHealth);

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
            Debug.Log("ENTER ALIVE");
            a.Sprint(true);
            //a.Health.CurrentHealth = 10;
        }
    }
}