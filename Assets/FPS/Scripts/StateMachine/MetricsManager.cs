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
        [SerializeField] private TMPro.TMP_Text fps; // fps

        // muertes/asesinatos
        int m_Muertes;
        int m_Asesinatos;

        // fps
        private int frameRate = 60;

        // Variables de timer de framerate
        int m_frameCounter = 0;
        float m_timeCounter = 0.0f;
        float m_lastFramerate = 0.0f;
        float m_refreshTime = 0.5f;

        private void Awake()
        {
            EventManager.AddListener<PlayerDeathEvent>(OnPlayerDeath);
            EventManager.AddListener<EnemyKillEvent>(OnEnemyKill);

            m_Muertes = 0;
            m_Asesinatos = 0;
        }

        private void Start()
        {
            Application.targetFrameRate = frameRate;
        }

        private void Update()
        {
            // Timer para mostrar el frameRate a intervalos
            if (m_timeCounter < m_refreshTime)
            {
                m_timeCounter += Time.deltaTime;
                m_frameCounter++;
            }
            else
            {
                m_lastFramerate = (float)m_frameCounter / m_timeCounter;
                m_frameCounter = 0;
                m_timeCounter = 0.0f;
            }

            // Texto con el framerate y 2 decimales
            if (fps != null)
            {
                fps.text = (((int)(m_lastFramerate * 100 + .5) / 100.0)).ToString("N0");
            }
        }

        private void ChangeFrameRate()
        {
            if (frameRate == 30)
            {
                frameRate = 60;
                Application.targetFrameRate = 60;
            }
            else
            {
                frameRate = 30;
                Application.targetFrameRate = 30;
            }
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