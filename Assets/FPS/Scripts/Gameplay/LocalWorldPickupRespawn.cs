using UnityEngine;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// Respawn local tras un delay. Opcional: prefab del Project en "World Pickup Prefab".
    /// Si está vacío, se clona este GameObject como plantilla (sin Netcode) la primera vez que hace falta.
    /// </summary>
    public class LocalWorldPickupRespawn : MonoBehaviour
    {
        [Tooltip("Opcional: arrastra el prefab desde la carpeta Project (no la instancia en la escena).")]
        [SerializeField] GameObject m_WorldPickupPrefab;

        [Tooltip("Segundos hasta que vuelve a aparecer (solo en esta máquina).")]
        [SerializeField] float m_DelaySeconds = 30f;

        GameObject m_LocalSpawnTemplate;

        /// <summary>
        /// Evita recursión: Instantiate(this) dispara Awake en el clon en el mismo frame.
        /// </summary>
        static int s_BuildDepth;

        public float DelaySeconds
        {
            get => m_DelaySeconds;
            set => m_DelaySeconds = value;
        }

        public void SetWorldPrefab(GameObject prefab)
        {
            m_WorldPickupPrefab = prefab;
        }

        void Awake()
        {
            // El clon interno de Instantiate(this) despierta en medio de BuildHiddenLocalTemplate: no re-entrar.
            if (s_BuildDepth > 0)
                return;

            BuildHiddenLocalTemplate();
        }

        void BuildHiddenLocalTemplate()
        {
            if (m_LocalSpawnTemplate != null) return;

            s_BuildDepth++;
            try
            {
                var holder = new GameObject("~PickupRespawnHolder_" + gameObject.GetInstanceID());
                holder.SetActive(false);
                Object.DontDestroyOnLoad(holder);

                m_LocalSpawnTemplate = Instantiate(gameObject, holder.transform);
                m_LocalSpawnTemplate.name = gameObject.name + "_LocalRespawnTemplate";
                m_LocalSpawnTemplate.SetActive(false);

                StripNetcodeAndNetworkPickupSync(m_LocalSpawnTemplate);
            }
            finally
            {
                s_BuildDepth--;
            }
        }

        static void StripNetcodeAndNetworkPickupSync(GameObject root)
        {
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null) continue;
                var t = mb.GetType();
                if (t.Name == "NetworkPickupSync")
                {
                    Object.Destroy(mb);
                    continue;
                }

                var ns = t.Namespace;
                if (ns == "Unity.Netcode")
                    Object.Destroy(mb);
            }
        }

        /// <summary>
        /// Programa un clon del pickup en la posición/rotación actuales de este objeto.
        /// </summary>
        public void TryScheduleRespawnAtCurrentTransform()
        {
            if (m_WorldPickupPrefab == null && m_LocalSpawnTemplate == null)
                BuildHiddenLocalTemplate();

            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;
            Transform parent = transform.parent;

            if (m_WorldPickupPrefab != null && !m_WorldPickupPrefab.scene.IsValid())
            {
                Unity.FPS.Game.LocalRespawnScheduler.Schedule(m_WorldPickupPrefab, pos, rot, m_DelaySeconds, parent);
                return;
            }

            if (m_LocalSpawnTemplate != null)
            {
                Unity.FPS.Game.LocalRespawnScheduler.Schedule(m_LocalSpawnTemplate, pos, rot, m_DelaySeconds, null);
                return;
            }

            Debug.LogWarning(
                "[LocalWorldPickupRespawn] No se pudo programar el respawn: sin plantilla ni prefab válido. ¿Hay componente en este pickup?",
                this);
        }
    }
}
