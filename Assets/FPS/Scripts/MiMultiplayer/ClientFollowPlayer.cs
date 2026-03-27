using Unity.FPS.Game;
using UnityEngine;
using Unity.Netcode;
using Unity.FPS.Gameplay;
using Unity.FPS.UI;


namespace Unity.FPS.AI
{
    public class ClientFollowPlayer : NetworkBehaviour
    {


        Transform m_PlayerTransform;
        Vector3 m_OriginalOffset;

        void Start()
        {
            ActorsManager actorsManager = FindAnyObjectByType<ActorsManager>();
            if (actorsManager != null)
                m_PlayerTransform = actorsManager.Player.transform;
            else
            {
                enabled = false;
                return;
            }

            m_OriginalOffset = transform.position - m_PlayerTransform.position;
        }

        public override void OnNetworkSpawn()
        {


            base.OnNetworkSpawn();

            if (IsOwner)
            {
                ActorsManager actorsManager = FindAnyObjectByType<ActorsManager>();
                if (actorsManager != null)
                    m_PlayerTransform = actorsManager.Player.transform;
                else
                {
                    enabled = false;
                    return;
                }

                m_OriginalOffset = transform.position - m_PlayerTransform.position;

            }


        }



        void LateUpdate()
        {
            transform.position = m_PlayerTransform.position + m_OriginalOffset;
        }
    }
}
