using UnityEngine;
using UnityEngine.Animations;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using Unity.Netcode; 

namespace Unity.FPS.Gameplay
{
    // ¡NUEVO! Ahora heredamos de NetworkBehaviour
    public class ThirdPersonWeaponSync : NetworkBehaviour
    {
        [System.Serializable]
        public class WeaponLink
        {
            public string WeaponName;
            public GameObject FakeWeapon;
            public string GripName = "Grip_Left";
        }

        public PlayerWeaponsManager WeaponsManager;

        [Header("Lista de Armas Falsas")]
        public WeaponLink[] ThirdPersonWeapons;

        [Header("IK de Tercera Persona")]
        public Transform LeftHandIKTarget;

        // Esta variable se sincroniza sola por internet
        public NetworkVariable<int> NetworkedWeaponIndex = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner // Solo el dueño del muñeco puede cambiar su arma
        );

        public override void OnNetworkSpawn()
        {
            SetupConstraint(LeftHandIKTarget);

            // Nos suscribimos para escuchar cuando internet nos dice que el jugador cambió de arma
            NetworkedWeaponIndex.OnValueChanged += OnNetworkedWeaponChanged;

            // Si somos el dueño de este jugador, escuchamos al ratón/teclado
            if (IsOwner && WeaponsManager != null)
            {
                WeaponsManager.OnSwitchedToWeapon += HandleLocalWeaponSwitch;
            }

            // Forzamos la actualización visual la primera vez que aparecemos en el mapa
            UpdateVisuals(NetworkedWeaponIndex.Value);
        }

        public override void OnNetworkDespawn()
        {
            NetworkedWeaponIndex.OnValueChanged -= OnNetworkedWeaponChanged;

            if (IsOwner && WeaponsManager != null)
            {
                WeaponsManager.OnSwitchedToWeapon -= HandleLocalWeaponSwitch;
            }
        }

        // Esta función solo se ejecuta en TU ordenador cuando tocas la rueda del ratón
        void HandleLocalWeaponSwitch(WeaponController newWeapon)
        {
            if (newWeapon == null) return;

            // Buscamos qué número de la lista corresponde a tu nueva arma
            for (int i = 0; i < ThirdPersonWeapons.Length; i++)
            {
                if (ThirdPersonWeapons[i].WeaponName == newWeapon.WeaponName)
                {
                    // Al cambiar esta variable, Unity avisa a todos los demás jugadores por internet
                    NetworkedWeaponIndex.Value = i;
                    break;
                }
            }
        }

        // Esta función se ejecuta en TODOS los ordenadores cuando la variable de red cambia
        void OnNetworkedWeaponChanged(int previousValue, int newValue)
        {
            UpdateVisuals(newValue);
        }

        // La función que enciende/apaga los gráficos (igual que la de antes, pero recibe un número)
        void UpdateVisuals(int index)
        {
            // 1. Apagamos todas
            foreach (var link in ThirdPersonWeapons)
            {
                if (link.FakeWeapon != null) link.FakeWeapon.SetActive(false);
            }

            // 2. Encendemos la que toca
            if (index >= 0 && index < ThirdPersonWeapons.Length)
            {
                var activeLink = ThirdPersonWeapons[index];
                if (activeLink.FakeWeapon != null)
                {
                    activeLink.FakeWeapon.SetActive(true);

                    // 3. Pegamos la mano izquierda
                    if (LeftHandIKTarget != null)
                    {
                        Transform gripLeft = activeLink.FakeWeapon.transform.Find(activeLink.GripName);
                        if (gripLeft != null)
                        {
                            LinkGhost(LeftHandIKTarget, gripLeft);
                        }
                        else
                        {
                            ParentConstraint constraint = LeftHandIKTarget.GetComponent<ParentConstraint>();
                            if (constraint != null) constraint.constraintActive = false;
                        }
                    }
                }
            }
        }

        void SetupConstraint(Transform target)
        {
            if (target == null) return;
            ParentConstraint constraint = target.GetComponent<ParentConstraint>();
            if (constraint == null)
            {
                constraint = target.gameObject.AddComponent<ParentConstraint>();
            }
        }

        void LinkGhost(Transform target, Transform grip)
        {
            if (target == null || grip == null) return;
            ParentConstraint constraint = target.GetComponent<ParentConstraint>();

            if (constraint.sourceCount > 0) constraint.RemoveSource(0);

            ConstraintSource newSource = new ConstraintSource();
            newSource.sourceTransform = grip;
            newSource.weight = 1f;

            constraint.AddSource(newSource);
            constraint.SetTranslationOffset(0, Vector3.zero);
            constraint.SetRotationOffset(0, Vector3.zero);
            constraint.constraintActive = true;
        }
    }
}