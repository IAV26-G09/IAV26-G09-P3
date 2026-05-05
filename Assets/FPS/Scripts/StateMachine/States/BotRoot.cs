using System.Collections.Generic;
using UnityEngine;

namespace IAV26.G09.P3
{
    [CreateAssetMenu(menuName = "HSM/States/BotRoot", fileName = "BotRoot")]
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

