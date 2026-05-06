using UnityEngine;

namespace IAV26.G09.P3
{
    [CreateAssetMenu(menuName = "HSM/States/Engage", fileName = "Engage")]
    public class Engage : State
    {
        bool requestLootOnExit;

        protected override void OnEnter(BotGameplayActions a)
        {
            Debug.Log("ENTRANDO A ENGAGE");
            requestLootOnExit = false;
            a.ClearLootRequest();
        }

        protected override State GetInitialState()
        {
            return FindTransition("Pursue");
        }

        protected override void OnExit(BotGameplayActions a)
        {
            if (requestLootOnExit)
            {
                a.RequestLoot();
                requestLootOnExit = false;
            }

            a.ResetView();
            //a.SeesEnemy = false;
            a.ForgetEnemy();
        }

        protected override State GetTransition(BotGameplayActions a)
        {
            if (a.Health != null && a.Health.IsCritical())
            {
                requestLootOnExit = false;
                State e = FindTransition("Recover");
                Debug.Log("VOY A RECOVER");
                return e;
            }

            if (!a.HasEnemyTarget())
            {
                if (a.HasKnownEnemy())
                    return null;

                requestLootOnExit = true;
                State e = FindTransition("Patrol");
                if (e != null)
                {
                    Debug.Log("VOY A PATROL");
                    return e;
                }
            }

            return null;
        }
    }
}
