using UnityEngine;

namespace HSM
{
    public class Idle : State
    {
        float counter = 0.0f;
        float time = 5.0f;

        public Idle(StateMachine m, State parent) : base(m, parent)
        {
        }

        protected override void OnEnter()
        {
            Debug.Log("ENTRANDO A IDLE");
        }

        protected override void OnExit()
        {
            Actions.StopNavigation();
        }

        protected override State GetTransition()
        {
            if (counter >= time)
            {
                counter = 0f;

                //Debug.Log("VOY A PATROL");
                //return ((Dead)Parent).Patrol;
            }
            return null;
        }

        protected override void OnUpdate(float deltaTime)
        {
            counter += deltaTime;
        }
    }

}