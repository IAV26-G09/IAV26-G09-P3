using System.Collections;
using System.Linq;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.LowLevel;

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
// Red (NGO) — Aclaraciones:
//   • La lógica de este bot corre en el SERVIDOR (IsServer). Los clientes solo ven el resultado
//     replicado (NetworkTransform server-authoritative en bots).
//   • No hay que activar cámaras ni AudioListener en instancias que no sean del jugador local.
//
// Vuestra tarea es escribir código aquí de una verdadera máquina de estados jerárquica, :
// que cargue la información de estados, transiciones, condiciones (según salud, según distancia a enemigos, etc.)
// y cuando haya que realizar alguna acción delegar en m_Actions.
// =================================================================================================

[RequireComponent(typeof(BotGameplayActions))]
[DisallowMultipleComponent]
public class FSM : NetworkBehaviour
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
        // El bot no debe competir con el teclado/ratón del jugador humano.
        // Pista: cuando implementéis disparo automático, podréis volver a habilitar
        // PlayerWeaponsManager desde BotGameplayActions.InitializeWeaponSystemsIfNeeded().
        DisableHumanInput();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_Actions = GetComponent<BotGameplayActions>();
        m_Health = GetComponent<Health>();

        if (m_Health != null)
        {
            //m_Health.OnDie += OnBotDied;
            //m_Health.OnHealed += OnBotHealed;
        }

        // Cámaras y listeners solo en el owner (en bots suele ser irrelevante, pero evita conflictos de tipo MPPM).
        if (!IsOwner)
            DisableCameraAndAudioForNonOwner();

        if (IsServer)
            StartCoroutine(ServerInitBotWhenGameplaySceneReady());

        Actions.InitializeWeaponSystemsIfNeeded();
    }

    public override void OnNetworkDespawn()
    {
        StopAllCoroutines();

        if (m_Health != null)
        {
            //m_Health.OnDie -= OnBotDied;
            //m_Health.OnHealed -= OnBotHealed;
        }

        base.OnNetworkDespawn();
    }

    /// <summary>
    /// El host spawnea el player object en cuanto arranca la red; la escena de juego (NavMesh, ActorsManager)
    /// carga justo después. Esperamos a que existan antes de crear el NavMeshAgent.
    /// </summary>
    IEnumerator ServerInitBotWhenGameplaySceneReady()
    {
        const float timeoutSeconds = 45f;
        float elapsed = 0f;

        while (elapsed < timeoutSeconds)
        {
            bool navReady = BotGameplayActions.SceneHasNavMeshData();
            bool actorsReady = Object.FindFirstObjectByType<ActorsManager>() != null;

            if (navReady && actorsReady)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        m_Actions.EnsureNavMeshAgentReady();
        m_Actions.InitializeWeaponSystemsIfNeeded();

        DisableHumanLocomotionThatConflictsWithNavMesh();

        //TransitionTo(BotState.Wandering);

        InitializeStates();
    }

    void DisableHumanInput()
    {
        var inputHandler = GetComponent<PlayerInputHandler>();
        if (inputHandler != null)
            inputHandler.enabled = false;
    }
    void DisableWeaponStak()
    {
        var weapons = GetComponent<PlayerWeaponsManager>();
        if (weapons != null)
            weapons.enabled = false;
    }
    void DisableHumanLocomotionThatConflictsWithNavMesh()
    {
        var pcc = GetComponent<PlayerCharacterController>();
        if (pcc != null)
            pcc.enabled = false;

        var jetpack = GetComponent<Jetpack>();
        if (jetpack != null)
            jetpack.enabled = false;

        var playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null)
            playerInput.enabled = false;

        var cc = GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;
    }
    void DisableCameraAndAudioForNonOwner()
    {
        foreach (var cam in GetComponentsInChildren<Camera>(true))
            cam.enabled = false;

        foreach (var listener in GetComponentsInChildren<AudioListener>(true))
            listener.enabled = false;

        foreach (var cam in GetComponentsInChildren<Camera>(true))
        {
            if (cam != null && cam.gameObject != null)
                cam.gameObject.SetActive(false);
        }
    }

    // ---------------------------------------------------------------------------------------------
    // Máquina de estados — Nucleo de la IA.
    // ---------------------------------------------------------------------------------------------
    void Update()
    {
        if (!IsServer)
            return;

        if (machine != null)
        {
            machine.Tick(Time.deltaTime);

            var path = StatePath(machine.Root.Leaf());

            if (path != lastPath)
            {
                //Debug.Log(path);
                lastPath = path;
            }
        }
    }

    /// <summary>
    /// Ejemplo mínimo: deambular.
    /// </summary>
    void TickWanderingExample()
    {
        if (m_Health != null && m_Health.CurrentHealth <= 0f)
            return;

        var agent = m_Actions.NavMeshAgent;
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (Time.time < m_NextRepathTime)
            return;

        m_NextRepathTime = Time.time + Mathf.Max(0.1f, m_RepathIntervalSeconds);

        bool needsNewTarget = !agent.hasPath || agent.pathPending || m_Actions.HasReachedCurrentDestination();
        if (!needsNewTarget)
            return;

        if (TryPickRandomNavMeshPoint(transform.position, Mathf.Max(2f, m_WanderRadius), out var dest))
            m_Actions.TryMoveToWorldPosition(dest);
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
        //root = new BotRoot(null);
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
