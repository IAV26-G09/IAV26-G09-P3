using UnityEngine;

namespace IAV26.G09.P3
{
    [CreateAssetMenu(menuName = "HSM/States/Dead", fileName = "Dead")]
    public class Dead : State
    {
        protected override void OnEnter(BotGameplayActions a)
        {
            a.ResetStateOnDead();
        }
    }
}