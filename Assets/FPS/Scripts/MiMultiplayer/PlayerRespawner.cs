using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    public class PlayerRespawner : NetworkBehaviour
    {
        private Health m_Health;
        private PlayerCharacterController m_CharacterController;
        PlayerNameTag m_LastPlayerAttacker;

        void Awake()
        {
            m_Health = GetComponent<Health>();
            m_CharacterController = GetComponent<PlayerCharacterController>();

            // Nos suscribimos a nuestra propia muerte
            if (m_Health != null)
            {
                m_Health.OnDie += HandleDeath;
                m_Health.OnDamaged += OnDamaged;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (m_Health != null)
            {
                m_Health.OnDie -= HandleDeath;
                m_Health.OnDamaged -= OnDamaged;
            }
        }

        void OnDamaged(float damage, GameObject damageSource)
        {
            // Solo guardamos atacante si es un jugador (para evitar dar kills cuando te mata un bot/entorno).
            if (damageSource == null)
            {
                m_LastPlayerAttacker = null;
                return;
            }

            var attackerTag = damageSource.GetComponentInParent<PlayerNameTag>();
            if (attackerTag != null && attackerTag != GetComponent<PlayerNameTag>())
            {
                m_LastPlayerAttacker = attackerTag;
            }
            else
            {
                // Daño de bot/entorno/no-jugador: no debe dar kill a nadie.
                m_LastPlayerAttacker = null;
            }
        }

        void HandleDeath()
        {
            // El servidor contabiliza muertes/killers (si aplica)
            if (IsServer)
            {
                var myTag = GetComponent<PlayerNameTag>();
                if (myTag != null)
                    myTag.Deaths.Value++;

                if (m_LastPlayerAttacker != null && myTag != null && m_LastPlayerAttacker != myTag)
                    m_LastPlayerAttacker.Kills.Value++;
            }

            // Si yo muero, yo inicio la cuenta atrás en mi pantalla
            if (!IsOwner) return;

            StartCoroutine(DeathRoutine());
        }

        IEnumerator DeathRoutine()
        {
            GameFlowManager flowManager = FindFirstObjectByType<GameFlowManager>();
            CanvasGroup fadeCanvas = flowManager != null ? flowManager.EndGameFadeCanvasGroup : null;

            fadeCanvas.gameObject.SetActive(false);

            yield return new WaitForSeconds(4f);

            fadeCanvas.gameObject.SetActive(true);

            // Le pedimos al servidor (Host) que nos busque un sitio para reaparecer
            RequestRespawnServerRpc();
        }

        [ServerRpc]
        void RequestRespawnServerRpc(ServerRpcParams rpcParams = default)
        {
            // 1. EL SERVIDOR: Busca todos los puntos de aparición en el mapa
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("RespawnPoint");
            Vector3 spawnPos = new Vector3(0, 5, 0); // Por si se te olvida poner puntos, caes del cielo
            Quaternion spawnRot = Quaternion.identity;

            // 2. Elige uno al azar
            if (spawnPoints.Length > 0)
            {
                int randomIndex = Random.Range(0, spawnPoints.Length);
                spawnPos = spawnPoints[randomIndex].transform.position;
                spawnRot = spawnPoints[randomIndex].transform.rotation;
            }

            // --- MUY IMPORTANTE ---
            // Este RPC solo responde al cliente que lo pidió, así que si NO revivimos también en el servidor,
            // el Health del servidor se queda "muerto" y deja de disparar OnDie en muertes posteriores.
            // (Eso rompe el contador de kills/muertes en partidas host+clientes).
            transform.position = spawnPos;
            transform.rotation = spawnRot;
            m_LastPlayerAttacker = null;
            if (m_Health != null) m_Health.Revive();

            // 3. Le responde ÚNICAMENTE al cliente que acaba de pedirlo
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId } }
            };

            RespawnClientRpc(spawnPos, spawnRot, clientRpcParams);
        }

        [ClientRpc]
        void RespawnClientRpc(Vector3 spawnPosition, Quaternion spawnRotation, ClientRpcParams clientRpcParams = default)
        {
            // --- EL CLIENTE RENACE ---

            // 1. Apagamos el motor físico temporalmente para poder teletransportarnos
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // 2. Nos movemos a la coordenada que nos dijo el servidor
            transform.position = spawnPosition;
            transform.rotation = spawnRotation;

            // 3. Volvemos a encender el motor físico
            if (cc != null) cc.enabled = true;

            // 4. Curamos la vida y reactivamos los controles
            if (m_Health != null) m_Health.Revive();
            if (m_CharacterController != null) m_CharacterController.OnRespawn();
        }
    }
}
