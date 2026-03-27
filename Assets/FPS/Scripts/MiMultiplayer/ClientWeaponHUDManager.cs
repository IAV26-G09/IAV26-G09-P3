using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using Unity.Netcode;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class ClientWeaponHUDManager : MonoBehaviour
    {
        [Tooltip("UI panel containing the layoutGroup for displaying weapon ammo")]
        public RectTransform AmmoPanel;

        [Tooltip("Prefab for displaying weapon ammo")]
        public GameObject AmmoCounterPrefab;

        private PlayerWeaponsManager m_PlayerWeaponsManager;
        private List<AmmoCounter> m_AmmoCounters = new List<AmmoCounter>();

        void Update()
        {
            // 1. Si ya hemos encontrado al jugador y nos hemos suscrito, salimos del Update inmediatamente.
            // ¡Esto salva el rendimiento del juego!
            if (m_PlayerWeaponsManager != null) return;

            // 2. Si no lo tenemos, buscamos a los jugadores que haya en la escena.
            PlayerWeaponsManager[] players = FindObjectsByType<PlayerWeaponsManager>(FindObjectsSortMode.None);

            foreach (var player in players)
            {
                // 3. Comprobamos si el jugador encontrado es el NUESTRO (IsOwner).
                NetworkObject netObj = player.GetComponent<NetworkObject>();
                if (netObj != null && netObj.IsOwner)
                {
                    // ¡Encontramos a nuestro jugador local! Lo inicializamos.
                    InitializeHUD(player);
                    break;
                }
            }
        }

        void InitializeHUD(PlayerWeaponsManager localPlayer)
        {
            m_PlayerWeaponsManager = localPlayer;

            // Dibujamos el arma con la que nacemos
            WeaponController activeWeapon = m_PlayerWeaponsManager.GetActiveWeapon();
            if (activeWeapon)
            {
                AddWeapon(activeWeapon, m_PlayerWeaponsManager.ActiveWeaponIndex);
                ChangeWeapon(activeWeapon);
            }

            // ¡NOS SUSCRIBIMOS UNA SOLA VEZ!
            m_PlayerWeaponsManager.OnAddedWeapon += AddWeapon;
            m_PlayerWeaponsManager.OnRemovedWeapon += RemoveWeapon;
            m_PlayerWeaponsManager.OnSwitchedToWeapon += ChangeWeapon;
        }

        void OnDestroy()
        {
            // Limpieza vital: Cuando el mapa se cierra, nos desuscribimos para no dejar "fantasmas" en la memoria.
            if (m_PlayerWeaponsManager != null)
            {
                m_PlayerWeaponsManager.OnAddedWeapon -= AddWeapon;
                m_PlayerWeaponsManager.OnRemovedWeapon -= RemoveWeapon;
                m_PlayerWeaponsManager.OnSwitchedToWeapon -= ChangeWeapon;
            }
        }

        void AddWeapon(WeaponController newWeapon, int weaponIndex)
        {
            // Barrera de seguridad extra: Si la barra ya existe, no la dibujamos.
            for (int i = 0; i < m_AmmoCounters.Count; i++)
            {
                if (m_AmmoCounters[i] != null && m_AmmoCounters[i].WeaponCounterIndex == weaponIndex)
                {
                    return;
                }
            }

            GameObject ammoCounterInstance = Instantiate(AmmoCounterPrefab, AmmoPanel);
            AmmoCounter newAmmoCounter = ammoCounterInstance.GetComponent<AmmoCounter>();

            newAmmoCounter.Initialize(newWeapon, weaponIndex);
            m_AmmoCounters.Add(newAmmoCounter);
        }

        void RemoveWeapon(WeaponController newWeapon, int weaponIndex)
        {
            int foundCounterIndex = -1;
            for (int i = 0; i < m_AmmoCounters.Count; i++)
            {
                if (m_AmmoCounters[i].WeaponCounterIndex == weaponIndex)
                {
                    foundCounterIndex = i;
                    Destroy(m_AmmoCounters[i].gameObject);
                }
            }

            if (foundCounterIndex >= 0)
            {
                m_AmmoCounters.RemoveAt(foundCounterIndex);
            }
        }

        void ChangeWeapon(WeaponController weapon)
        {
            if (AmmoPanel != null)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(AmmoPanel);
            }
        }
    }
}