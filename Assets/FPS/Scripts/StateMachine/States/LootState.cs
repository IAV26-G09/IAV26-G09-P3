using UnityEngine;

namespace HFSM
{
    [CreateAssetMenu(menuName = "HSM/States/Loot", fileName = "Loot")]
    public class Loot : State
    {
        bool lootingWeapon;

        protected override void OnEnter(BotGameplayActions a)
        {
            Debug.Log("ENTRANDO A LOOT");
            a.ClearLootRequest();

            lootingWeapon = a.SeesWeapon && a.WeaponTransform != null;
        }

        protected override State GetTransition(BotGameplayActions a)
        {
            State patrol = FindTransition("Patrol");

            if (a.HasEnemyTarget())
            {
                State engage = FindTransition("Engage");
                return engage != null ? engage : patrol;
            }

            if (lootingWeapon)
            {
                if (a.ResetWeaponPickedUp() || !a.SeesWeapon || a.WeaponTransform == null)
                {
                    a.ClearWeaponTarget();
                    return patrol;
                }
            }
            else
            {
                if (!a.SeesHealth || a.HealthTransform == null)
                    return patrol;
            }

            return null;
        }

        protected override void OnUpdate(BotGameplayActions a, float deltaTime)
        {
            if (lootingWeapon && a.WeaponTransform != null)
            {
                a.FaceCurrentWeapon();
                a.TryMoveToWorldPosition(a.WeaponTransform.position);
            }
            else if (!lootingWeapon && a.HealthTransform != null)
            {
                a.FaceCurrentHealth();
                a.TryMoveToWorldPosition(a.HealthTransform.position);
            }
        }
    }
}