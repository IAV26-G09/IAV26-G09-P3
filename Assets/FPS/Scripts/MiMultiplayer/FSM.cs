using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Unity.FPS.Gameplay;
using Unity.FPS.Game;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador automático (bot) para un jugador.
/// Pensado para ponerse en el prefab `UCM_Bot` en lugar de `ClientPlayerMove`.
/// </summary>
public class FSM : NetworkBehaviour
{
    [Header("Autopilot")]
    [SerializeField] float wanderRadius = 25f;
    [SerializeField] float repathIntervalSeconds = 1.25f;
    [SerializeField] float stoppingDistance = 1.5f;

    NavMeshAgent m_Agent;
    float m_NextRepathTime;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Solo mueve el bot en la instancia que lo posee (normalmente el host/servidor en vuestro setup).
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        DisableManualControlComponents();
        EnsureAgent();
    }

    void DisableManualControlComponents()
    {
        var inputHandler = GetComponent<PlayerInputHandler>();
        if (inputHandler != null) inputHandler.enabled = false;

        var pcc = GetComponent<PlayerCharacterController>();
        if (pcc != null) pcc.enabled = false;

        var jetpack = GetComponent<Jetpack>();
        if (jetpack != null) jetpack.enabled = false;

        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = false;

        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
    }

    void EnsureAgent()
    {
        if (m_Agent == null)
            m_Agent = GetComponent<NavMeshAgent>();
        if (m_Agent == null)
            m_Agent = gameObject.AddComponent<NavMeshAgent>();

        m_Agent.enabled = true;
        m_Agent.stoppingDistance = Mathf.Max(0.25f, stoppingDistance);
        m_Agent.autoBraking = true;
        m_Agent.updatePosition = true;
        m_Agent.updateRotation = true;
    }

    void Update()
    {
        if (!IsOwner) return;
        if (m_Agent == null || !m_Agent.enabled) return;
        if (!m_Agent.isOnNavMesh) return;

        if (Time.time < m_NextRepathTime) return;
        m_NextRepathTime = Time.time + Mathf.Max(0.1f, repathIntervalSeconds);

        bool needsNewTarget = !m_Agent.hasPath || m_Agent.pathPending ||
                              m_Agent.remainingDistance <= (m_Agent.stoppingDistance + 0.35f);
        if (!needsNewTarget) return;

        if (TryPickRandomNavMeshPoint(transform.position, Mathf.Max(2f, wanderRadius), out var dest))
            m_Agent.SetDestination(dest);
    }

    static bool TryPickRandomNavMeshPoint(Vector3 origin, float radius, out Vector3 result)
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
}

