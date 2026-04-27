using System.Collections.Generic;
using UnityEngine;

namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/BotRoot")]
    public class BotRoot : State
    {
        public readonly Dead dead;
        public readonly Alive alive;

        public BotRoot(StateMachine m)
            : base(m, null)
        {
            //dead = new Dead(m, this);
            //alive = new Alive(m, this);
        }

        protected override State GetTransition()
        {
            return null;
        }
    }
}

