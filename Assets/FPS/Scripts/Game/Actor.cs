using UnityEngine;

namespace Unity.FPS.Game
{
    // This class contains general information describing an actor (player or enemies).
    // It is mostly used for AI detection logic and determining if an actor is friend or foe
    public class Actor : MonoBehaviour
    {
        [Tooltip("Represents the affiliation (or team) of the actor. Actors of the same affiliation are friendly to each other")]
        public int Affiliation;

        [Tooltip("Represents point where other actors will aim when they attack this actor")]
        public Transform AimPoint;

        ActorsManager m_ActorsManager;
        bool m_RegisteredWithManager;

        void Start()
        {
            TryRegisterWithActorsManager();
        }

        void Update()
        {
            if (m_RegisteredWithManager)
                return;

            // Multijugador: el jugador puede spawnear antes de que cargue la escena de juego (menú → PrisonScene).
            TryRegisterWithActorsManager();
        }

        void TryRegisterWithActorsManager()
        {
            if (m_RegisteredWithManager)
                return;

            m_ActorsManager = GameObject.FindFirstObjectByType<ActorsManager>();
            if (m_ActorsManager == null)
                return;

            if (!m_ActorsManager.Actors.Contains(this))
                m_ActorsManager.Actors.Add(this);

            m_RegisteredWithManager = true;
        }

        void OnDestroy()
        {
            if (m_ActorsManager != null && m_ActorsManager.Actors.Contains(this))
                m_ActorsManager.Actors.Remove(this);
        }
    }
}