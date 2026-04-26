using UnityEngine;

namespace HSM
{
public class BotRoot : State
{
    public readonly Paseo Paseo;
    public readonly Combat Combat;

    private bool combat = false;

    public BotRoot(StateMachine m)
        : base(m, null)
    {
        Paseo = new Paseo(m, this);
        Combat = new Combat(m, this);
    }

    protected override State GetInitialState() => Paseo;

    protected override State GetTransition()
    {
        if (combat)
        {
            Debug.Log("VOY A COMBATE");
            combat = false;
            return Combat;
        }
        else
        {
            return null;
        }
    }
}
}

