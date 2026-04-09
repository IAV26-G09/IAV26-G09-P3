using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>
    /// Opcional: tras morir el enemigo, vuelve a instanciarse localmente tras un delay (misma posición inicial).
    /// Asigna el mismo prefab de enemigo que quieras respawnear.
    /// </summary>
    public class EnemyLocalRespawn : MonoBehaviour
    {
        [Tooltip("Opcional: prefab desde la carpeta Project. Si arrastras la instancia de la escena (o lo dejas vacío), el respawn clona el enemigo al morir (como los items).")]
        [SerializeField] GameObject m_EnemyPrefab;

        [SerializeField] float m_DelaySeconds = 30f;

        Vector3 m_InitialPosition;
        Quaternion m_InitialRotation;
        Transform m_Parent;

        void Awake()
        {
            m_InitialPosition = transform.position;
            m_InitialRotation = transform.rotation;
            m_Parent = transform.parent;
        }

        /// <summary>Llamado desde EnemyController al morir (antes de Destroy).</summary>
        public void OnEnemyDiedScheduleLocalRespawn()
        {
            // Prefab del Project: instanciar desde asset (referencia estable tras Destroy).
            if (m_EnemyPrefab != null && !m_EnemyPrefab.scene.IsValid())
            {
                LocalRespawnService.ScheduleEnemyPrefab(m_EnemyPrefab, m_InitialPosition, m_InitialRotation,
                    m_DelaySeconds, m_Parent);
                return;
            }

            // Instancia en escena o campo vacío: clonar la jerarquía viva en este frame (igual que pickups).
            LocalRespawnService.ScheduleEnemyCloneAfterDestroy(gameObject, m_InitialPosition, m_InitialRotation,
                m_DelaySeconds, m_Parent);
        }
    }
}
