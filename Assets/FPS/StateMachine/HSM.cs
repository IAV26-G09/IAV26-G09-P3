using UnityEngine;

public class HSM
{
    public HSMBase CurrentState { get; private set; }

    public void SetState(HSMBase newState)
    {
        CurrentState?.OnExit();
        CurrentState = newState;
        CurrentState?.OnEnter();
    }

    public void Update()
    {
        if (CurrentState == null) return;

        //var next = CurrentState.CheckTransitions();
        //if (next != null)
        //{
        //    SetState(next);
        //    return;
        //}

        CurrentState.OnLogic();
    }
}