using System;
using System.Collections;
using System.Linq;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.LowLevel;
using Random = UnityEngine.Random;

namespace IAV26.G09.P3
{
[RequireComponent(typeof(BotGameplayActions))]
[DisallowMultipleComponent]
public class HFSM : MonoBehaviour
{
    [Header("HFSM — Depuración")]
    [SerializeField] bool m_LogStateTransitions;

    [Header("HFSM — Estado raíz")]
    [SerializeField]
    private State root;

    private StateMachine machine;
    private string lastPath;

    BotGameplayActions m_Actions;
    public BotGameplayActions Actions => m_Actions;

    // --------------------------------------
    // Ciclo de vida componentes
    // --------------------------------------
    void Awake()
    {
        m_Actions = GetComponent<BotGameplayActions>();
    }

    private void Start()
    {
        InitializeStates();
    }

    // --------------------------------------
    // Máquina de estados — Nucleo de la IA.
    // --------------------------------------
    void Update()
    {
        if (machine != null)
        {
            machine.Tick(Time.deltaTime);

            var path = StatePath(machine.Root.Leaf());

            if (path != lastPath)
            {
                if(m_LogStateTransitions) Debug.Log(path);

                lastPath = path;
            }
        }
    }

    void InitializeStates()
    {
        var builder = new StateMachineBuilder(root);
        machine = builder.Build();
        machine.Owner = this;
    }

    static string StatePath(State s)
    {
        return string.Join(" > ", s.PathToRoot().AsEnumerable().Reverse().Select(n => n.GetType().Name));
    }

    }
}
