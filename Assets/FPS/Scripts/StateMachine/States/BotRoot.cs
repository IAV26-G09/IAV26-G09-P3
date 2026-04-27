using System.Collections.Generic;
using UnityEngine;

namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/BotRoot")]
    public class BotRoot : State
    {
        public readonly Dead dead;
        public readonly Alive alive;

        protected override State GetTransition(BotGameplayActions a)
        {
            return null;
        }
    }
}

