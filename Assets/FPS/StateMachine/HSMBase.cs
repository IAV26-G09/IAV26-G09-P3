using UnityEngine;

public class HSMBase
{
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
