using UnityEngine;
using static Unity.Netcode.Components.AttachableBehaviour;
public class CombatState : HSMBase
{
    HSM subMachine = new HSM();

    HSMBase chase;
    HSMBase attack;

    public override void OnEnter()
    {
        //chase = new ChaseState();
        //attack = new AttackState();

        chase.Init(fsm);
        attack.Init(fsm);

        subMachine.SetState(chase);
    }

    public override void OnLogic()
    {
        subMachine.Update();
    }

    public override HSMBase CheckTransitions()
    {
        //if (fsm.Health.CurrentHealth <= 0)
        //    return fsm.GetDeadState();

        return null;
    }
}
