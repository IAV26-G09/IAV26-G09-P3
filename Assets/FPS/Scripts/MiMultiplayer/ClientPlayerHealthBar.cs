using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

namespace Unity.FPS.UI
{
    public class ClientPlayerHealthBar : NetworkBehaviour
    {
        [Tooltip("Image component dispplaying current health")]
        public Image HealthFillImage;

        Health m_PlayerHealth;

        void Start()
        {
            PlayerCharacterController playerCharacterController =
                GameObject.FindFirstObjectByType<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, PlayerHealthBar>(
                playerCharacterController, this);

            m_PlayerHealth = playerCharacterController.GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, PlayerHealthBar>(m_PlayerHealth, this,
                playerCharacterController.gameObject);
        }

        public override void OnNetworkSpawn()
        {


            base.OnNetworkSpawn();

            if (IsOwner)
            {
                PlayerCharacterController playerCharacterController =
                 GameObject.FindFirstObjectByType<PlayerCharacterController>();
                DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, PlayerHealthBar>(
                    playerCharacterController, this);

                m_PlayerHealth = playerCharacterController.GetComponent<Health>();
                DebugUtility.HandleErrorIfNullGetComponent<Health, PlayerHealthBar>(m_PlayerHealth, this,
                    playerCharacterController.gameObject);

            }


        }



        void Update()
        {
            // update health bar value
            HealthFillImage.fillAmount = m_PlayerHealth.CurrentHealth / m_PlayerHealth.MaxHealth;
        }
    }
}
