using HierarchicalStateMachine;
using UnityEngine;

public class HSMBase 
{
    protected FSM fsm;
    private int level; // nivel de la jerarquia

    public void Init(FSM fsm)
    {
        this.fsm = fsm;
    }

    public virtual HSMBase CheckTransitions() { return null; }

    /// <summary>
    /// Cuando la state machine entre a este estado
    /// </summary>
    public virtual void OnEnter()
    {

    }

    /// <summary>
    /// Si esta activo el que se llama en el update
    /// </summary>
    public virtual void OnLogic()
    {

    }

    /// <summary>
    /// Cuando la state machine salga de este estado
    /// </summary>
    public virtual void OnExit()
    {

    }
}
