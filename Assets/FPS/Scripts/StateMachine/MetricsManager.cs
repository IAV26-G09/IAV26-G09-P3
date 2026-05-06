using System;
using TMPro;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace IAV26.G09.P3
{
    public class MetricsManager : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text muertes; // veces que se ha muerto
        [SerializeField] private TMPro.TMP_Text asesinatos; // veces que ha matado

        int m_Muertes;
        int m_Asesinatos;

        private void Awake()
        {
            EventManager.AddListener<PlayerDeathEvent>(OnPlayerDeath);
            EventManager.AddListener<EnemyKillEvent>(OnEnemyKill);

            m_Muertes = 0;
            m_Asesinatos = 0;
        }

        void OnPlayerDeath(PlayerDeathEvent evt)
        {
            m_Muertes += 1;
            muertes.text = m_Muertes.ToString("N0");
        }

        void OnEnemyKill(EnemyKillEvent evt)
        {
            m_Asesinatos += 1;
            asesinatos.text = m_Asesinatos.ToString("N0");
        }
    }
}