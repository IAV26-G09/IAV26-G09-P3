using UnityEngine;
using UnityEngine.Animations; 
using Unity.FPS.Game;
using Unity.FPS.Gameplay;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(PlayerWeaponsManager))]
    public class WeaponIKSync : MonoBehaviour
    {
        [Tooltip("Arrastra aquí el Target_L de tu Rig")]
        public Transform LeftIKTarget;

        [Tooltip("Arrastra aquí el Target_R de tu Rig")]
        public Transform RightIKTarget;

        PlayerWeaponsManager m_WeaponsManager;

        void Start()
        {
            m_WeaponsManager = GetComponent<PlayerWeaponsManager>();

            // Preparamos los imanes para que puedan usar Constraints
            SetupConstraint(LeftIKTarget);
            SetupConstraint(RightIKTarget);

            if (m_WeaponsManager != null)
            {
                m_WeaponsManager.OnSwitchedToWeapon += HandleWeaponSwitch;
            }
        }

        void OnDestroy()
        {
            if (m_WeaponsManager != null)
            {
                m_WeaponsManager.OnSwitchedToWeapon -= HandleWeaponSwitch;
            }
        }

        // Esta función añade el componente secreto de Unity si no lo tiene
        void SetupConstraint(Transform target)
        {
            if (target == null) return;
            ParentConstraint constraint = target.GetComponent<ParentConstraint>();
            if (constraint == null)
            {
                constraint = target.gameObject.AddComponent<ParentConstraint>();
            }
        }

        void HandleWeaponSwitch(WeaponController newWeapon)
        {
            if (newWeapon == null) return;

            Transform gripLeft = newWeapon.transform.Find("Grip_Left");
            Transform gripRight = newWeapon.transform.Find("Grip_Right");

            LinkGhost(LeftIKTarget, gripLeft);
            LinkGhost(RightIKTarget, gripRight);
        }

        // Aquí ocurre la magia instantánea
        void LinkGhost(Transform target, Transform grip)
        {
            if (target == null || grip == null) return;

            ParentConstraint constraint = target.GetComponent<ParentConstraint>();

            // Limpiamos el enlace del arma anterior
            if (constraint.sourceCount > 0)
            {
                constraint.RemoveSource(0);
            }

            // Creamos el nuevo enlace hacia el arma actual
            ConstraintSource newSource = new ConstraintSource();
            newSource.sourceTransform = grip;
            newSource.weight = 1f;

            constraint.AddSource(newSource);

            // Le decimos que NO haga Offsets raros, que copie la posición y rotación exactas (0,0,0)
            constraint.SetTranslationOffset(0, Vector3.zero);
            constraint.SetRotationOffset(0, Vector3.zero);

            // Encendemos la imantación
            constraint.constraintActive = true;
        }
    }
}