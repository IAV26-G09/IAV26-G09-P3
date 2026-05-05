using UnityEngine;

namespace HFSM
{
    [CreateAssetMenu(menuName = "HSM/States/Attack", fileName = "Attack")]
    public class Attack : State
    {
        [SerializeField] float attackRange = 14f;
        [SerializeField] int minBurstAmmo = 4;

        bool m_WaitingBurstRecharge;

        protected override void OnEnter(BotGameplayActions a)
        {
            Debug.Log("ENTER ATTACK");
            a.StopNavigation();
            m_WaitingBurstRecharge = false;
        }

        protected override void OnExit(BotGameplayActions a)
        {
            // release fire when we leave attack
            a.TryFireCurrentWeaponPrimary(false, false, true);
            m_WaitingBurstRecharge = false;
        }

        protected override State GetTransition(BotGameplayActions a)
        {
            if (a.Health != null && a.Health.IsCritical())
                return FindTransition("Recover");

            if (!a.HasEnemyTarget())
                return FindTransition("Patrol");

            if (!a.CanAttackCurrentEnemy(attackRange))
                return FindTransition("Pursue");

            return null;
        }

        protected override void OnUpdate(BotGameplayActions a, float deltaTime)
        {
            if (!a.HasEnemyTarget())
                return;

            a.StopNavigation();
            a.FaceCurrentEnemy();

            var weapon = a.GetActiveWeaponOrNull();
            if (weapon == null)
                return;

            int currentAmmo = weapon.GetCurrentAmmo();
            //int burstAmmoNeeded = Mathf.Clamp(minBurstAmmo, 1, Mathf.Max(1, weapon.MaxAmmo));

            if (currentAmmo <= 0)
                m_WaitingBurstRecharge = true;

            if (m_WaitingBurstRecharge)
            {
                a.TryReloadActiveWeapon();

                if (currentAmmo < minBurstAmmo)
                {
                    a.TryFireCurrentWeaponPrimary(false, false, true);
                    return;
                }

                m_WaitingBurstRecharge = false;
            }

            bool fired = a.TryFireCurrentWeaponPrimary(true, true, false);
            if (!fired)
                a.TryReloadActiveWeapon();
        }
    }
}

