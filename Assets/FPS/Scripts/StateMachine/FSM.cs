using System;
using System.Collections;
using System.Linq;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.LowLevel;
using Random = UnityEngine.Random;

/*
 * Se trata como un arbol:
 * 
 *             STATEMACHINE
 *             /          \
 *         paseo         combat
 *         /   \           ...
 *     idle   moving   
 *
 */

namespace HSM
{
// =================================================================================================
// FSM — Plantilla de máquina de estados SIMPLIFICADA Y A FUEGO EN EL CÓDIGO para UCM_Bot
// =================================================================================================
// Objetivo:
//   • Separar "qué decide la IA" (esta clase) de "cómo se ejecutan las acciones en el juego"
//     (ver BotGameplayActions).
//
// Vuestra tarea es escribir código aquí de una verdadera máquina de estados jerárquica:
// que cargue la información de estados, transiciones, condiciones (según salud, según distancia a enemigos, etc.)
// y cuando haya que realizar alguna acción delegar en m_Actions.
// =================================================================================================

[RequireComponent(typeof(BotGameplayActions))]
[DisallowMultipleComponent]
public class FSM : MonoBehaviour
{
    [Header("FSM — parámetros del ejemplo Wandering")]
    [Tooltip("Radio alrededor de la posición actual para elegir un nuevo punto aleatorio en NavMesh.")]
    [SerializeField] float m_WanderRadius = 25f;

    [Tooltip("Cada cuántos segundos, como máximo, se reconsidera el destino.")]
    [SerializeField] float m_RepathIntervalSeconds = 1.25f;

    [Header("FSM — depuración")]
    [SerializeField] bool m_LogStateTransitions;

    float m_NextRepathTime;

    Health m_Health;
    BotGameplayActions m_Actions;

    // HFSM
    [SerializeField]
    private State root;

    private StateMachine machine;
    private string lastPath;

    public BotGameplayActions Actions => m_Actions;

    // ---------------------------------------------------------------------------------------------
    // Ciclo de vida red / componentes
    // ---------------------------------------------------------------------------------------------
    void Awake()
    {
        m_Actions = GetComponent<BotGameplayActions>();
    }

    private void Start()
    {
        InitializeStates();
    }

    // ---------------------------------------------------------------------------------------------
    // Máquina de estados — Nucleo de la IA.
    // ---------------------------------------------------------------------------------------------
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

    // ---------------------------------------------------------------------------------------------
    // Utilidades NavMesh (podrían moverse a BotGameplayActions si preferís no tener nada de lógica aquí)
    // ---------------------------------------------------------------------------------------------
    static public bool TryPickRandomNavMeshPoint(Vector3 origin, float radius, out Vector3 result)
    {
        for (int i = 0; i < 20; i++)
        {
            var rnd = Random.insideUnitSphere * radius;
            var candidate = origin + rnd;
            if (NavMesh.SamplePosition(candidate, out var hit, 2.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = origin;
        return false;
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