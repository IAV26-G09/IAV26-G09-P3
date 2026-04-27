using UnityEngine;


namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/Loot")]
    public class Loot : State
    {
        public Loot(StateMachine m, State parent) : base(m, parent)
        {
        }
    }
}

