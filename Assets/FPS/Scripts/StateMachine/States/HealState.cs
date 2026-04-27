using UnityEngine;

namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/Heal")]
    public class Heal : State
    {
        public Heal(StateMachine m, State parent) : base(m, parent)
        {

        }
    }
}