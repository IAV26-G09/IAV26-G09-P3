using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.FPS.Gameplay
{
    public class LocalVisibility : NetworkBehaviour
    {
        [Tooltip("Si está desactivado, solo verás tu sombra. Si está activado, verás tu cuerpo entero.")]
        public bool ShowBodyToOwner = false;

        [Tooltip("Arrastra aquí el/los 'Skinned Mesh Renderer' de tu personaje de Mixamo")]
        public SkinnedMeshRenderer[] BodyRenderers;

        [Tooltip("Arrastra aquí los brazos sueltos")]
        public GameObject ArmsRenderers;

        [Tooltip("Arrastra aquí el contenedor de armas en tercera persona")]
        public GameObject ThirdPersonWeapons;

        // Se ejecuta en el momento en que el jugador aparece en la red
        public override void OnNetworkSpawn()
        {
            UpdateVisibility();
        }

        // OnValidate nos permite que, si haces clic en el botón en el Inspector mientras juegas, se actualice al instante
        void OnValidate()
        {
            if (IsSpawned)
            {
                UpdateVisibility();
            }
        }

        void UpdateVisibility()
        {
            // 1. Gestionar el cuerpo de Mixamo
            foreach (var renderer in BodyRenderers)
            {
                if (renderer != null)
                {
                    if (IsOwner)
                    {
                        // Si somos el dueño, decidimos si lo vemos o solo vemos la sombra
                        renderer.shadowCastingMode = ShowBodyToOwner ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly;
                    }
                    else
                    {
                        // Para el resto de jugadores en la red, nuestro cuerpo SIEMPRE es visible
                        renderer.shadowCastingMode = ShadowCastingMode.On;
                    }
                }
            }

            // 2. Gestionar las armas de tercera persona (Falsas)
            if (ThirdPersonWeapons != null)
            {
                // Obtenemos todos los renderers de las armas, incluso de las que están apagadas internamente (true)
                Renderer[] tpWeaponRenderers = ThirdPersonWeapons.GetComponentsInChildren<Renderer>(true);

                foreach (var renderer in tpWeaponRenderers)
                {
                    if (IsOwner)
                    {
                        // Para ti (el dueño): Las armas físicas siguen ahí y funcionan, pero solo ves su sombra
                        renderer.shadowCastingMode = ShowBodyToOwner ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly;
                    }
                    else
                    {
                        // Para los demás: Las ven perfectamente
                        renderer.shadowCastingMode = ShadowCastingMode.On;
                    }
                }
            }

            // 3. Gestionar los brazos flotantes de primera persona
            if (ArmsRenderers != null)
            {
                if (IsOwner)
                {
                    // Solo tú ves tus brazos flotantes
                    ArmsRenderers.SetActive(true);
                }
                else
                {
                    // Los demás no ven tus brazos flotantes
                    ArmsRenderers.SetActive(false);
                }
            }
        }
    }
}