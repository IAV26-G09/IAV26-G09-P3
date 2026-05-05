using System;
using HFSM;
using NUnit.Framework.Internal;
using Unity.FPS.AI;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Gestor de acciones para el bot jugador (<c>UCM_Bot</c>).
/// <para>
/// Propósito: Ejemplo de clase que centraliza en un solo sitio llamadas con nombre claro ("ir aquí", "disparar",
/// "cambiar arma", etc.) para que la <see cref="FSM"/> (u otra IA) no tenga que conocer todos los
/// detalles de <see cref="PlayerCharacterController"/>, <see cref="PlayerWeaponsManager"/>, etc.
/// </para>
/// <para>
/// <b>Importante (diseño actual del proyecto):</b> el prefab del bot suele desactivar
/// <see cref="PlayerInputHandler"/> y <see cref="PlayerWeaponsManager"/> en <c>Awake</c> para que no
/// compitan con el teclado/ratón. Eso retrasa la inicialización de armas hasta que alguien vuelva a
/// habilitar <see cref="PlayerWeaponsManager"/> (por ejemplo desde
/// <see cref="InitializeWeaponSystemsIfNeeded"/>). ¡Ojo, sin armas inicializadas, los métodos de combate
/// no tendrán efecto!
/// </para>
/// </summary>
[DisallowMultipleComponent]
public class BotGameplayActions : MonoBehaviour
{
    // -------- NAVEGACION
    [Header("Navegación (NavMeshAgent)")]
    [Tooltip("Si no hay agente en el prefab, se crea uno en tiempo de ejecución al inicializar.")]
    [SerializeField] bool m_AutoCreateNavMeshAgent = true;

    [SerializeField] float m_DefaultStoppingDistance = 1.5f;

    NavMeshAgent m_NavMeshAgent;

    PlayerCharacterController m_PlayerCc;

    Vector3 m_LastWorldPosForAnim;
    bool m_HasLastWorldPosForAnim;

    private Transform m_Transform;

    public NavMeshAgent NavMeshAgent => m_NavMeshAgent; // Referencia al agente de navegación del bot (puede ser null antes de inicializar)

    // -------- COMBATE
    [Header("Combate")]
    [Tooltip("Si es true, en InitializeWeaponSystemsIfNeeded se habilita PlayerWeaponsManager para que ejecute Start y cree las armas iniciales.")]
    [SerializeField] bool m_EnableWeaponManagerForBot = true;

    PlayerWeaponsManager m_Weapons;
    Health m_Health;

    public Health Health => m_Health; // Vida del personaje, útil para transiciones

    // -------- CAMPO VISION
    [Header("Campo de vision")]
    [SerializeField]
    private float radioVision = 10.0f;
    [SerializeField]
    [Range(0.0f, 180.0f)] // para evitar que puedan ver mas alla de un angulo de vision de 180 grados
    private float angleVision = 30.0f;
    [SerializeField]
    private bool debug = true;

    private SphereCollider m_SphereCollider;

    public bool SeesHealth { get; set; }
    public bool SeesEnemy { get; set; }

    private Transform m_HealthTransform;
    private Transform m_EnemyTransform;
    private Transform m_WeaponTransform;

    private bool m_SeesWeapon;
    private bool m_WeaponPickedUp;
    private bool m_LootRequest;
    public bool SeesWeapon => m_SeesWeapon;
    public bool LootRequest => m_LootRequest;

    public Transform HealthTransform => m_HealthTransform;
    public Transform EnemyTransform => m_EnemyTransform;
    public Transform WeaponTransform => m_WeaponTransform;

    // -------- HUIDA
    private float m_Speed;
    private float m_FleeSpeed;

    // -------- MUERTE
    [SerializeField]
    private Transform spawn;

    void Awake()
    {
        EventManager.AddListener<PickupEvent>(OnPickUp);

        m_Health = GetComponent<Health>();
        m_PlayerCc = GetComponent<PlayerCharacterController>();
        m_Weapons = GetComponent<PlayerWeaponsManager>();
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        m_SphereCollider = GetComponent<SphereCollider>();

        m_Speed = m_NavMeshAgent.speed;
        m_FleeSpeed = (m_Speed * 2);

        m_Transform = GetComponent<Transform>();

        if (m_SphereCollider != null) m_SphereCollider.radius = radioVision;
    }

    private void OnTriggerStay(Collider other)
    {
        // si es colision con algo que no nos interese no hace nada
        if (other.GetComponent<HealthPickup>() == null &&
            other.GetComponent<EnemyController>() == null &&
            other.GetComponent<WeaponPickup>() == null)
        {
            return;
        }

        //Debug.Log(other.GetComponent<Transform>().position + " " + other.gameObject.name);

        // calculo del angulo desde delante
        Vector3 directionToColl = other.GetComponent<Transform>().position - m_Transform.position;
        float angleToPlayer = Vector3.Angle(m_Transform.forward, directionToColl);

        // si estas dentro del campo de vision
        if (angleToPlayer <= angleVision)
        {
            // si no hay nada entre el avatar y lo que me interesa
            if (Physics.Raycast(m_Transform.position, directionToColl.normalized, out RaycastHit hit, radioVision))
            {
                // si con lo que choca es health pickup
                if (hit.collider.GetComponent<HealthPickup>() != null)
                {
                    Debug.Log("veo poti");

                    SeesHealth = true;
                    m_HealthTransform = other.GetComponent<Transform>();
                }
                else
                {
                    Debug.Log("NO veo poti");

                    SeesHealth = false;
                    m_HealthTransform = null;
                }

                // si con lo que choca es enemy
                if (hit.collider.GetComponentInParent<EnemyController>() != null
                    || hit.collider.GetComponent<EnemyController>() != null)
                {
                    Debug.Log("veo enemigo, raycast a: " + hit.collider.gameObject.name);

                    SeesEnemy = true;
                    m_EnemyTransform = other.GetComponent<Transform>();
                }
                else
                {
                    Debug.Log("NO veo enemigo, raycast a: " + hit.collider.gameObject.name);

                    SeesEnemy = false;
                    m_EnemyTransform = null;
                }

                bool otherIsWeapon = other.GetComponent<WeaponPickup>() != null;

                // si con lo que choca es weapon pickup
                if (hit.collider.GetComponentInParent<WeaponPickup>() != null
                    || hit.collider.GetComponent<WeaponPickup>() != null)
                {
                    Debug.Log("veo arma");

                    m_SeesWeapon = true;
                    m_WeaponTransform = other.GetComponent<Transform>();
                }
                else if (otherIsWeapon)
                {
                    m_SeesWeapon = false;
                    m_WeaponTransform = null;
                }
            }
            else
            {
                SeesHealth = false;
                m_HealthTransform = null;

                SeesEnemy = false;
                m_EnemyTransform = null;

                if (other.GetComponent<WeaponPickup>() != null)
                {
                    m_SeesWeapon = false;
                    m_WeaponTransform = null;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<HealthPickup>() != null)
        {
            SeesHealth = false;
            m_HealthTransform = null;
        }

        else if (other.GetComponent<EnemyController>() != null)
        {

            Debug.Log("pierdo de vista");

            SeesEnemy = false;
            m_EnemyTransform = null;
        }
        else if (other.GetComponent<WeaponPickup>() != null)
        {
            m_SeesWeapon = false;
            m_WeaponTransform = null;
        }
    }

    void OnPickUp(PickupEvent evt)
    {
        if (evt.Pickup.GetComponent<HealthPickup>() != null)
        {
            SeesHealth = false;
        }

        if (evt.Pickup.GetComponent<WeaponPickup>() != null)
        {
            bool wasTargetWeapon = m_WeaponTransform != null && evt.Pickup.transform == m_WeaponTransform;

            m_SeesWeapon = false;
            m_WeaponTransform = null;
            m_WeaponPickedUp = wasTargetWeapon;
        }
    }

    private void Update()
    {
        //if(m_SeesHealth) Debug.Log("VEO POTI");

        // Debug
        if (debug)
        {
            Transform t = GetComponentInParent<Transform>();
            float r = radioVision / 2;
            float a = angleVision / 2;

            Vector3 v1 = Vector3.RotateTowards(t.forward, t.right * -1, a * Mathf.Deg2Rad, 0);
            Vector3 v2 = Vector3.RotateTowards(t.forward, t.right, a * Mathf.Deg2Rad, 0);

            Debug.DrawRay(t.position, t.forward * r, Color.white, 0.1f);
            Debug.DrawRay(t.position, v1 * r, Color.yellow, 0.1f);
            Debug.DrawRay(t.position, v2 * r, Color.yellow, 0.1f);
        }
    }

    void OnEnable()
    {
        m_HasLastWorldPosForAnim = false;
    }

    /// <summary>
    /// Llamar desde la IA en el servidor cuando queráis asegurar que el stack de armas está listo
    /// para usar <see cref="SwitchToWeaponSlot"/> / <see cref="TryFireCurrentWeaponPrimary"/>.
    /// </summary>
    public void InitializeWeaponSystemsIfNeeded()
    {
        if (!m_EnableWeaponManagerForBot || m_Weapons == null)
            return;

        if (!m_Weapons.enabled)
            m_Weapons.enabled = true;
    }

    /// <summary>
    /// Comprueba si la escena activa tiene datos de NavMesh bakeados (p. ej. ya cargó el mapa de juego).
    /// Útil para no llamar a <see cref="NavMeshAgent"/> mientras sigue activa la escena de menú.
    /// </summary>
    public static bool SceneHasNavMeshData()
    {
        var tri = NavMesh.CalculateTriangulation();
        return tri.indices != null && tri.indices.Length >= 3;
    }

    /// <summary>
    /// Prepara o configura el <see cref="NavMeshAgent"/> para el modo bot (servidor).
    /// No crea el componente hasta que exista NavMesh en escena; si el transform aún no está sobre la malla,
    /// intenta un <see cref="NavMeshAgent.Warp"/> al punto más cercano.
    /// </summary>
    public void EnsureNavMeshAgentReady()
    {
        if (m_NavMeshAgent == null && m_AutoCreateNavMeshAgent)
        {
            if (!SceneHasNavMeshData())
                return;

            m_NavMeshAgent = gameObject.AddComponent<NavMeshAgent>();
        }

        if (m_NavMeshAgent == null)
            return;

        m_NavMeshAgent.enabled = true;
        m_NavMeshAgent.stoppingDistance = Mathf.Max(0.25f, m_DefaultStoppingDistance);
        m_NavMeshAgent.autoBraking = true;
        m_NavMeshAgent.updatePosition = true;
        m_NavMeshAgent.updateRotation = true;

        // Radio pequeño: si el jugador ya está bien posicionado en un sótano, un radio grande podía
        // proyectar a otra capa de NavMesh más alta (otra planta) y provocar desplazamientos raros.
        if (!m_NavMeshAgent.isOnNavMesh &&
            NavMesh.SamplePosition(transform.position, out var hit, 2.5f, NavMesh.AllAreas))
        {
            m_NavMeshAgent.Warp(hit.position);
        }
    }

    void LateUpdate()
    {
        // UCM_Bot desactiva PlayerCharacterController; el Animator no recibe Forward/Strafe. Replicamos
        // la idea del PCC usando la velocidad real del transform (válida en servidor y clientes vía red).
        if (GetComponent<FSM>() == null)
            return;

        DriveThirdPersonLocomotionAnimator();
    }

    void DriveThirdPersonLocomotionAnimator()
    {
        if (m_PlayerCc == null)
            return;

        //var anim = m_PlayerCc.CharacterAnimator;
        /*
        if (anim == null)
            return;

        if (m_Health != null && m_Health.CurrentHealth <= 0f)
            return;

        if (anim.GetBool("IsDead"))
            return;
        */

        float dt = Time.deltaTime;
        if (dt < 1e-5f)
            return;

        if (!m_HasLastWorldPosForAnim)
        {
            m_LastWorldPosForAnim = transform.position;
            m_HasLastWorldPosForAnim = true;
            return;
        }

        Vector3 worldVel = (transform.position - m_LastWorldPosForAnim) / dt;
        m_LastWorldPosForAnim = transform.position;

        Vector3 localVel = transform.InverseTransformDirection(worldVel);
        float maxSpd = Mathf.Max(0.01f, m_PlayerCc.MaxSpeedOnGround);
        float forward = Mathf.Clamp(localVel.z / maxSpd, -1f, 1f);
        float strafe = Mathf.Clamp(localVel.x / maxSpd, -1f, 1f);

        /*
        anim.SetFloat("Forward", forward, 0.12f, dt);
        anim.SetFloat("Strafe", strafe, 0.12f, dt);
        anim.SetBool("IsGrounded", true);
        anim.SetBool("IsAiming", m_Weapons != null && m_Weapons.IsAiming);*/
    }

    public bool Flee()
    {
        if (SeesEnemy)
        {
            if (EnemyTransform != null)
            {
                Vector3 lineal = m_Transform.position - EnemyTransform.transform.position;

                if (lineal.magnitude > radioVision)
                {
                    m_NavMeshAgent.speed = m_Speed;
                    return false;
                }

                lineal.Normalize();

                Vector3 fleeDestination = m_Transform.position + lineal * radioVision;
                Sprint(true);
                TryMoveToWorldPosition(fleeDestination);
            }
        }

        return true;
    }

    public void Respawn()
    {
        m_Transform.position = spawn.position;
    }

    /// <summary>Ordena moverse hacia un punto del mundo (debe ser alcanzable por NavMesh).</summary>
    /// <returns><c>true</c> si se pudo fijar un destino válido.</returns>
    public bool TryMoveToWorldPosition(Vector3 worldPosition)
    {
        if (m_NavMeshAgent == null || !m_NavMeshAgent.isActiveAndEnabled)
            return false;
        if (!m_NavMeshAgent.isOnNavMesh)
            return false;

        m_NavMeshAgent.isStopped = false;
        return m_NavMeshAgent.SetDestination(worldPosition);
    }

    /// <summary>Detiene la navegación y cancela el path actual.</summary>
    public void StopNavigation()
    {
        if (m_NavMeshAgent == null || !m_NavMeshAgent.enabled)
            return;

        m_NavMeshAgent.isStopped = true;
        m_NavMeshAgent.ResetPath();
    }

    /// <summary>Desactiva por completo el agente (p. ej. al morir).</summary>
    public void DisableNavMeshAgent()
    {
        if (m_NavMeshAgent == null)
            return;

        StopNavigation();
        m_NavMeshAgent.enabled = false;
    }

    /// <summary>Vuelve a habilitar el agente tras un respawn.</summary>
    public void EnableNavMeshAgent()
    {
        EnsureNavMeshAgentReady();
        if (m_NavMeshAgent != null && m_NavMeshAgent.enabled)
            m_NavMeshAgent.isStopped = false;
    }

    /// <summary>Distancia restante aproximada en el path actual (o infinito si no hay path).</summary>
    public float GetPathRemainingDistance()
    {
        if (m_NavMeshAgent == null || !m_NavMeshAgent.enabled || !m_NavMeshAgent.hasPath)
            return float.PositiveInfinity;
        return m_NavMeshAgent.remainingDistance;
    }

    /// <summary>¿Ha llegado (aprox.) al destino con el umbral del agente?</summary>
    public bool HasReachedCurrentDestination()
    {
        if (m_NavMeshAgent == null || !m_NavMeshAgent.enabled)
            return true;
        if (m_NavMeshAgent.pathPending)
            return false;
        return !m_NavMeshAgent.hasPath || m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance + 0.35f;
    }

    /// <summary>
    /// Gira el cuerpo del bot (eje Y) para mirar hacia un punto del suelo.
    /// Útil antes de disparar. No modifica la inclinación vertical de la cámara del prefab humano
    /// (eso sigue ligado al stack de FPS); solo alinea el forward horizontal.
    /// </summary>
    public void FaceTowardsWorldPoint(Vector3 worldPoint)
    {
        Vector3 flat = worldPoint - transform.position;
        flat.y = 0f;
        if (flat.sqrMagnitude < 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(flat.normalized, Vector3.up);
    }

    /// <summary>
    /// Gira el cuerpo hacia una dirección horizontal (XZ).
    /// </summary>
    public void SetFacingDirection(Vector3 horizontalDirection)
    {
        horizontalDirection.y = 0f;
        if (horizontalDirection.sqrMagnitude < 0.0001f)
            return;
        transform.rotation = Quaternion.LookRotation(horizontalDirection.normalized, Vector3.up);
    }

    // --- Armas (vía APIs públicas del proyecto)

    /// <summary>Índice de arma activa, o -1 si ninguna.</summary>
    public int GetActiveWeaponSlotIndex()
    {
        return m_Weapons != null ? m_Weapons.ActiveWeaponIndex : -1;
    }

    /// <summary>Referencia al arma activa (puede ser null).</summary>
    public WeaponController GetActiveWeaponOrNull()
    {
        return m_Weapons != null ? m_Weapons.GetActiveWeapon() : null;
    }

    /// <summary>Cambia al arma del slot (0..8 según el array interno del manager).</summary>
    public void SwitchToWeaponSlot(int slotIndex, bool force = false)
    {
        if (m_Weapons == null)
            return;
        m_Weapons.SwitchToWeaponIndex(slotIndex, force);
    }

    /// <summary>Pasa al siguiente/previo arma equipada (orden circular del manager).</summary>
    public void SwitchToNextWeaponInInventory()
    {
        if (m_Weapons == null)
            return;
        m_Weapons.SwitchWeapon(ascendingOrder: true);
    }

    public void SwitchToPreviousWeaponInInventory()
    {
        if (m_Weapons == null)
            return;
        m_Weapons.SwitchWeapon(ascendingOrder: false);
    }

    /// <summary>
    /// Disparo primario del arma activa usando la misma API que el input humano acaba llamando.
    /// Pasad los tres flags como en un botón: pulsación, mantener, soltar.
    /// </summary>
    public bool TryFireCurrentWeaponPrimary(bool pressedDown, bool held, bool released)
    {
        var w = GetActiveWeaponOrNull();
        if (w == null)
            return false;
        return w.HandleShootInputs(pressedDown, held, released);
    }

    /// <summary>Inicia la recarga del arma activa (animación + estado interno del arma).</summary>
    public void TryReloadActiveWeapon()
    {
        var w = GetActiveWeaponOrNull();
        if (w == null || w.IsReloading)
            return;
        if (!w.AutomaticReload && w.CurrentAmmoRatio < 1f)
            w.StartReloadAnimation();
    }

    // --- "Intención" de movimiento estilo FPS (útil para conectar la IA aquí) ---------
    /// <summary>
    /// Valores que un humano produce con WASD + sprint + agacharse. El proyecto <i>no</i> los lee
    /// todavía desde aquí: están expuestos para que podáis redirigirlos a
    /// <see cref="PlayerInputHandler"/> en un futuro, cuando queráis hilar muy fino para IMITAR la forma de controlar al personaje de un humano.
    /// </summary>
    public struct LocomotionIntent
    {
        /// <summary>En espacio local del jugador: X strafe, Z adelante/atrás (como GetMoveInput).</summary>
        public Vector3 Move;

        public bool Sprint;
        public bool Crouch;
        public bool JumpPressed;
        public bool AimHeld;
    }

    /// <summary>Buffer de intención que la FSM puede rellenar; integración con PCC pendiente.</summary>
    public LocomotionIntent BufferedLocomotion;

    /// <summary>Fija la intención de movimiento para un posible puente futuro con <see cref="PlayerInputHandler"/>.</summary>
    public void SetLocomotionIntent(in LocomotionIntent intent)
    {
        BufferedLocomotion = intent;
    }

    /// <summary>Ejemplo de uso: moverse "como teclas" hacia delante/derecha en espacio local.</summary>
    public void SetLocomotionIntentSimple(Vector2 xz, bool sprint, bool crouch, bool jump, bool aim)
    {
        BufferedLocomotion = new LocomotionIntent
        {
            Move = new Vector3(xz.x, 0f, xz.y),
            Sprint = sprint,
            Crouch = crouch,
            JumpPressed = jump,
            AimHeld = aim
        };
    }

    // --- Consultas rápidas (podéis añadir más si lo consideráis necesario

    /// <summary>¿Sigue vivo el bot?</summary>
    public bool IsAlive()
    {
        return m_Health == null || m_Health.CurrentHealth > 0f;
    }

    public EnemyController GetCurrentEnemyController()
    {
        if (m_EnemyTransform == null)
            return null;

        return m_EnemyTransform.GetComponentInParent<EnemyController>();
    }

    public bool HasEnemyTarget()
    {
        var enemy = GetCurrentEnemyController();
        if (enemy == null)
            return false;

        // SeesEnemy viene de trigger/raycast y puede fluctuar un frame; mantenemos validacion por LoS.
        return SeesEnemy || HasCurrentEnemySight(radioVision + 2f);
    }

    public Vector3 GetCurrentEnemyAimPosition()
    {
        var enemy = GetCurrentEnemyController();
        if (enemy == null)
            return transform.position;

        return enemy.transform.position;
    }

    public float GetDistanceToCurrentEnemy()
    {
        var enemy = GetCurrentEnemyController();
        if (enemy == null)
            return Single.PositiveInfinity;

        return Vector3.Distance(transform.position, enemy.transform.position);
    }

    public bool HasCurrentEnemySight(float maxDistance = 100f)
    {
        var enemy = GetCurrentEnemyController();
        if (enemy == null)
            return false;

        var origin = transform.position + Vector3.up * 1.3f;
        var target = GetCurrentEnemyAimPosition();
        var direction = target - origin;
        float distance = Mathf.Min(direction.magnitude, maxDistance);

        if (distance <= 0.001f)
            return true;

        direction /= direction.magnitude;

        if (!Physics.Raycast(origin, direction, out var hit, distance))
            return false;

        return hit.collider != null && hit.collider.GetComponentInParent<EnemyController>() == enemy;
    }

    public bool CanAttackCurrentEnemy(float maxRange)
    {
        if (!HasEnemyTarget())
            return false;

        if (GetDistanceToCurrentEnemy() > maxRange)
            return false;

        return HasCurrentEnemySight(maxRange + 2f);
    }

    public bool TryMoveToCurrentEnemy()
    {
        var enemy = GetCurrentEnemyController();
        if (enemy == null)
            return false;

        return TryMoveToWorldPosition(enemy.transform.position);
    }

    public void FaceCurrentEnemy()
    {
        FaceTowardsWorldPoint(GetCurrentEnemyAimPosition());
    }

    public void FaceCurrentHealth()
    {
        if (m_HealthTransform != null)
            FaceTowardsWorldPoint(m_HealthTransform.position);
    }

    public void FaceCurrentWeapon()
    {
        if (m_WeaponTransform != null)
            FaceTowardsWorldPoint(m_WeaponTransform.position);
    }

    public bool ResetWeaponPickedUp()
    {
        if (!m_WeaponPickedUp)
            return false;

        m_WeaponPickedUp = false;
        return true;
    }

    public void RequestLoot()
    {
        m_LootRequest = true;
    }

    public void ClearLootRequest()
    {
        m_LootRequest = false;
    }

    public bool HasNearbyLoot()
    {
        bool hasWeaponLoot = m_SeesWeapon && m_WeaponTransform != null;
        bool hasHealthLoot = SeesHealth && m_HealthTransform != null;
        return hasWeaponLoot || hasHealthLoot;
    }

    public void ClearWeaponTarget()
    {
        m_SeesWeapon = false;
        m_WeaponTransform = null;
    }

    public void ForgetEnemy()
    {
        m_EnemyTransform = null;
    }

    public void Sprint(bool s)
    {
        m_NavMeshAgent.speed = s ? m_FleeSpeed : m_Speed;
    }
}
