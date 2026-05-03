using UnityEngine;

namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/Dead", fileName = "Dead")]
    public class Dead : State
    {
        protected override void OnEnter(BotGameplayActions a)
        {
            Debug.Log("ENTER DEAD");
        }
    }
}