using System;
using IAV26.G09.P3;
using NUnit.Framework.Internal;
using Unity.FPS.AI;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.AI;

namespace IAV26.G09.P3
{
    /// <summary>
    /// Gestor de acciones para el bot jugador.
    /// </summary>
    [DisallowMultipleComponent]
    public class BotGameplayActions : MonoBehaviour
    {
        // -------- NAVEGACION
        [Header("Navegación (NavMeshAgent)")]
        [Tooltip("Si no hay agente en el prefab, se crea uno en tiempo de ejecución al inicializar.")]
        [SerializeField]
        bool m_AutoCreateNavMeshAgent = true;

        [SerializeField] float m_DefaultStoppingDistance = 1.5f;

        NavMeshAgent m_NavMeshAgent;

        PlayerCharacterController m_PlayerCc;

        private Transform m_Transform;

        public NavMeshAgent NavMeshAgent =>
            m_NavMeshAgent; // Referencia al agente de navegación del bot (puede ser null antes de inicializar)

        // -------- COMBATE
        [Header("Combate")]
        [Tooltip(
            "Si es true, en InitializeWeaponSystemsIfNeeded se habilita PlayerWeaponsManager para que ejecute Start y cree las armas iniciales.")]
        [SerializeField]
        bool m_EnableWeaponManagerForBot = true;

        PlayerWeaponsManager m_Weapons;
        Health m_Health;

        public Health Health => m_Health; // Vida del personaje, útil para transiciones

        // -------- CAMPO VISION
        [Header("Campo de vision")] [SerializeField]
        private float radioVision = 10.0f;

        [SerializeField]
        [Range(0.0f, 180.0f)]
        // para evitar que puedan ver mas alla de un angulo de vision de 180 grados
        private float angleVision = 30.0f;

        [SerializeField] private bool debug = true;

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

        public Transform HealthTransform => m_HealthTransform;
        public Transform EnemyTransform => m_EnemyTransform;
        public Transform WeaponTransform => m_WeaponTransform;

        // -------- HUIDA
        private float m_Speed;
        private float m_FleeSpeed;

        // -------- MUERTE
        [Header("Spawn, inicial y tras morir")]
        [SerializeField] Transform[] m_Waypoints;

        void Awake()
        {
            EventManager.AddListener<PickupEvent>(OnPickUp);
            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);

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

        void Start()
        {
            MoveToRandomSpawnPoint();
        }

        private void OnTriggerStay(Collider other)
        {
            // si es colision con algo que no nos interese no hace nada
            if (other.GetComponent<HealthPickup>() == null &&
                other.GetComponentInParent<EnemyController>() == null &&
                other.GetComponent<WeaponPickup>() == null)
            {
                return;
            }

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
                        SeesHealth = true;
                        m_HealthTransform = other.GetComponent<Transform>();
                    }
                    else
                    {
                        SeesHealth = false;
                        m_HealthTransform = null;
                    }

                    // si con lo que choca es enemy
                    if (hit.collider.GetComponentInParent<EnemyController>() != null
                        || hit.collider.GetComponent<EnemyController>() != null)
                    {
                        EnemyController seenEnemy = hit.collider.GetComponent<EnemyController>();
                        if (seenEnemy == null)
                            seenEnemy = hit.collider.GetComponentInParent<EnemyController>();

                        if (seenEnemy != null)
                        {
                            if (m_EnemyTransform == null || m_EnemyTransform == seenEnemy.transform)
                            {
                                SeesEnemy = true;
                                m_EnemyTransform = seenEnemy.transform;
                            }
                        }
                    }
                    else
                    {
                        EnemyController otherEnemy = other.GetComponentInParent<EnemyController>();
                        if (otherEnemy != null && m_EnemyTransform == otherEnemy.transform)
                        {
                            SeesEnemy = false;
                        }
                    }

                    bool otherIsWeapon = other.GetComponent<WeaponPickup>() != null;

                    // si con lo que choca es weapon pickup
                    if (hit.collider.GetComponentInParent<WeaponPickup>() != null
                        || hit.collider.GetComponent<WeaponPickup>() != null)
                    {
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

                    EnemyController otherEnemy = other.GetComponentInParent<EnemyController>();
                    if (otherEnemy != null && m_EnemyTransform == otherEnemy.transform)
                        SeesEnemy = false;

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

            else if (other.GetComponentInParent<EnemyController>() != null)
            {
                EnemyController otherEnemy = other.GetComponentInParent<EnemyController>();
                if (otherEnemy != null && m_EnemyTransform == otherEnemy.transform)
                {
                    SeesEnemy = false;
                }
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
            // Debug campo de vision
#if UNITY_EDITOR
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
#endif
        }

        /// <summary>
        /// Comprueba si la escena activa tiene datos de NavMesh bakeados (p. ej. ya cargo el mapa de juego).
        /// Útil para no llamar a <see cref="NavMeshAgent"/> mientras sigue activa la escena de menu.
        /// </summary>
        public static bool SceneHasNavMeshData()
        {
            var tri = NavMesh.CalculateTriangulation();
            return tri.indices != null && tri.indices.Length >= 3;
        }

        /// <summary>
        /// Prepara o configura el <see cref="NavMeshAgent"/> para el modo bot.
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
            if (GetComponent<HFSM>() == null)
                return;
        }

        public void Flee()
        {
            if (SeesEnemy)
            {
                if (EnemyTransform != null)
                {
                    Vector3 lineal = m_Transform.position - EnemyTransform.transform.position;

                    if (lineal.magnitude > radioVision)
                    {
                        m_NavMeshAgent.speed = m_Speed;

                        return;
                    }

                    lineal.Normalize();

                    Vector3 fleeDestination = m_Transform.position + lineal * radioVision;
                    Sprint(true);
                    TryMoveToWorldPosition(fleeDestination);
                }
            }
        }

        public void Respawn()
        {
            MoveToRandomSpawnPoint();
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

        /// <summary>Vuelve a habilitar el agente tras un respawn.</summary>
        public void EnableNavMeshAgent()
        {
            EnsureNavMeshAgentReady();
            if (m_NavMeshAgent != null && m_NavMeshAgent.enabled)
                m_NavMeshAgent.isStopped = false;
        }

        /// <summary>¿Ha llegado (aprox.) al destino con el umbral del agente?</summary>
        public bool HasReachedCurrentDestination()
        {
            if (m_NavMeshAgent == null || !m_NavMeshAgent.enabled)
                return true;
            if (m_NavMeshAgent.pathPending)
                return false;
            return !m_NavMeshAgent.hasPath ||
                   m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance + 0.35f;
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

        /// <summary>Referencia al arma activa (puede ser null).</summary>
        public WeaponController GetActiveWeaponOrNull()
        {
            return m_Weapons != null ? m_Weapons.GetActiveWeapon() : null;
        }

        public bool TrySwitchToLoadedWeapon(int minimumAmmo)
        {
            if (m_Weapons == null)
                return false;

            if (m_Weapons.WeaponSwitchDelay > 0f)
                m_Weapons.WeaponSwitchDelay = 0f;

            int currentSlot = m_Weapons.ActiveWeaponIndex;

            for (int i = 0; i < 9; i++)
            {
                if (i == currentSlot)
                    continue;

                var candidate = m_Weapons.GetWeaponAtSlotIndex(i);
                if (candidate == null)
                    continue;

                int ammo = candidate.GetCurrentAmmo();
                if (ammo < minimumAmmo)
                    continue;

                m_Weapons.SwitchToWeaponIndex(i, true);
                return true;
            }

            return false;
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

            return HasCurrentEnemySight(radioVision);
        }

        public bool HasKnownEnemy()
        {
            return GetCurrentEnemyController() != null;
        }

        public Vector3 GetCurrentEnemyAimPosition()
        {
            var enemy = GetCurrentEnemyController();
            if (enemy == null)
                return transform.position;

            var actor = enemy.GetComponent<Actor>();
            if (actor != null && actor.AimPoint != null)
                return actor.AimPoint.position;

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

            return HasCurrentEnemySight(maxRange);
        }

        public bool TryMoveToCurrentEnemy()
        {
            var enemy = GetCurrentEnemyController();
            if (enemy == null)
                return false;

            return TryMoveToWorldPosition(enemy.transform.position);
        }

        private void FaceCurrentEnemy()
        {
            Vector3 aimPoint = GetCurrentEnemyAimPosition();
            FaceTowardsWorldPoint(aimPoint);
            FaceViewTowardsWorldPoint(aimPoint);
        }

        public void TryFaceEnemy()
        {
            if (HasCurrentEnemySight(radioVision))
            {
                FaceCurrentEnemy();
                return;
            }

            if (m_NavMeshAgent != null)
            {
                Vector3 moveDir = m_NavMeshAgent.desiredVelocity;
                moveDir.y = 0f;
                if (moveDir.sqrMagnitude > 0.0001f)
                    SetFacingDirection(moveDir);
            }

            ResetView();
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

        public void ClearLootRequest()
        {
            m_LootRequest = false;
        }

        public void ClearWeaponTarget()
        {
            m_SeesWeapon = false;
            m_WeaponTransform = null;
        }

        public void ForgetEnemy()
        {
            m_EnemyTransform = null;
            SeesEnemy = false;
        }

        public void ResetStateOnDead()
        {
            SeesHealth = false;
            SeesEnemy = false;
            m_SeesWeapon = false;

            m_HealthTransform = null;
            m_EnemyTransform = null;
            m_WeaponTransform = null;

            m_WeaponPickedUp = false;
            m_LootRequest = false;

            ResetView();

            if (m_NavMeshAgent != null && m_NavMeshAgent.enabled)
            {
                m_NavMeshAgent.ResetPath();
                m_NavMeshAgent.isStopped = false;
            }
        }

        public void Sprint(bool s)
        {
            m_NavMeshAgent.speed = s ? m_FleeSpeed : m_Speed;
        }

        void FaceViewTowardsWorldPoint(Vector3 worldPoint)
        {
            if (m_PlayerCc == null || m_PlayerCc.PlayerCamera == null)
                return;

            Transform cameraTransform = m_PlayerCc.PlayerCamera.transform;
            Vector3 dir = worldPoint - cameraTransform.position;
            if (dir.sqrMagnitude < 0.0001f)
                return;

            Vector3 localDir = transform.InverseTransformDirection(dir.normalized);
            float pitch = -Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;

            cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }

        public void ResetView()
        {
            if (m_PlayerCc == null || m_PlayerCc.PlayerCamera == null)
                return;

            m_PlayerCc.PlayerCamera.transform.localEulerAngles = Vector3.zero;
        }

        public int GetPatrolWaypointCount()
        {
            return m_Waypoints.Length;
        }

        public bool TryGetPatrolWaypointPosition(int index, out Vector3 position)
        {
            position = transform.position;

            if (m_Waypoints == null)
                return false;

            if (index < 0 || index >= m_Waypoints.Length)
                return false;

            var waypoint = m_Waypoints[index];
            if (waypoint == null)
                return false;

            position = waypoint.position;
            return true;
        }

        void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (evt == null || evt.Enemy == null || m_EnemyTransform == null)
                return;

            EnemyController currentEnemy = GetCurrentEnemyController();
            if (currentEnemy == null)
            {
                ForgetEnemy();
                return;
            }

            if (evt.Enemy == currentEnemy.gameObject)
            {
                ForgetEnemy();
            }
        }

        bool MoveToRandomSpawnPoint()
        {
            if (m_Waypoints == null || m_Waypoints.Length == 0)
                return false;

            Transform point = m_Waypoints[UnityEngine.Random.Range(0, m_Waypoints.Length)];
            if (point == null)
                return false;

            m_Transform.position = point.position;
            return true;
        }
    }
}
