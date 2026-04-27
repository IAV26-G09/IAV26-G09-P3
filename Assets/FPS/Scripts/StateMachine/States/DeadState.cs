using UnityEngine;

namespace HSM
{
    [CreateAssetMenu(menuName = "HSM/States/Dead")]
    public class Dead : State
    {
        public Dead(StateMachine m, State parent) : base(m, parent)
        {

        }
    }
}