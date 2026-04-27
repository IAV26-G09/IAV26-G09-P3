using UnityEngine;

namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/Alive")]
    public class Alive : State
    {
        protected override State GetTransition(BotGameplayActions a)
        {
            //if (paseo)
            //{
            //    Debug.Log("VOY A PASEO");
            //    paseo = false;
            //    return ((BotRoot)Parent).Dead;
            //}
            return null;
        }

        protected override void OnEnter(BotGameplayActions a)
        {
            Debug.Log("ENTER ALIVE");
        }

        //protected override State GetInitialState() => engage;
    }
}