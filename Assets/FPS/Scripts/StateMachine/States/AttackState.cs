using UnityEngine;

namespace IAV26.G09.P3
{
    [CreateAssetMenu(menuName = "HSM/States/Attack", fileName = "Attack")]
    public class Attack : State
    {
        [SerializeField] 
        float attackRange = 14f;
        [SerializeField] 
        int minBurstAmmo = 4;
        [SerializeField] 
        float strafeDistance = 2.5f;
        [SerializeField] 
        float strafeMinTime = 0.5f;
        [SerializeField] 
        float strafeMaxTime = 0.75f;
        [SerializeField] 
        float strafeForwardJitter = 0.75f;
        [SerializeField] 
        float strafeAcceleration = 40f;

        bool m_WaitingBurstRecharge;
        int m_StrafeDirection = 1;
        float m_StrafeCountdown;
        float m_StrafeForwardOffset;
        float m_PreviousAcceleration;
        bool m_HadPreviousAcceleration;

        protected override void OnEnter(BotGameplayActions a)
        {
            a.EnableNavMeshAgent();

            m_WaitingBurstRecharge = false;
            m_StrafeDirection = Random.value < 0.5f ? -1 : 1;
            m_StrafeForwardOffset = Random.Range(-strafeForwardJitter, strafeForwardJitter);
            m_StrafeCountdown = Random.Range(strafeMinTime, strafeMaxTime);

            if (a.NavMeshAgent != null)
            {
                m_PreviousAcceleration = a.NavMeshAgent.acceleration;
                m_HadPreviousAcceleration = true;
                a.NavMeshAgent.acceleration = Mathf.Max(m_PreviousAcceleration, strafeAcceleration);
            }
        }

        protected override void OnExit(BotGameplayActions a)
        {
            a.TryFireCurrentWeaponPrimary(false, false, true);
            m_WaitingBurstRecharge = false;

            if (m_HadPreviousAcceleration && a.NavMeshAgent != null)
                a.NavMeshAgent.acceleration = m_PreviousAcceleration;

            m_HadPreviousAcceleration = false;
        }

        protected override State GetTransition(BotGameplayActions a)
        {
            if (a.Health != null && a.Health.IsCritical())
                return FindTransition("Recover");

            if (!a.HasEnemyTarget())
            {
                if (a.HasKnownEnemy())
                    return FindTransition("Engage");

                return null;
            }

            if (!a.CanAttackCurrentEnemy(attackRange))
                return FindTransition("Engage");

            return null;
        }

        protected override void OnUpdate(BotGameplayActions a, float deltaTime)
        {
            if (!a.HasEnemyTarget())
                return;

            UpdateStrafeMovement(a, deltaTime);
            a.TryFaceEnemy();

            var weapon = a.GetActiveWeaponOrNull();
            if (weapon == null)
                return;

            int currentAmmo = weapon.GetCurrentAmmo();
            if (currentAmmo <= 0)
            {
                if (a.TrySwitchToLoadedWeapon(1))
                {
                    m_WaitingBurstRecharge = false;
                    weapon = a.GetActiveWeaponOrNull();
                    if (weapon == null)
                        return;

                    currentAmmo = weapon.GetCurrentAmmo();
                }
                else
                {
                    m_WaitingBurstRecharge = true;
                }
            }

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

        void UpdateStrafeMovement(BotGameplayActions a, float deltaTime)
        {
            Vector3 toEnemy = a.GetCurrentEnemyAimPosition() - a.transform.position;
            toEnemy.y = 0f;
            if (toEnemy.sqrMagnitude < 0.0001f)
                return;

            m_StrafeCountdown -= deltaTime;
            if (m_StrafeCountdown <= 0f)
            {
                m_StrafeDirection *= -1;
                m_StrafeForwardOffset = Random.Range(-strafeForwardJitter, strafeForwardJitter);
                m_StrafeCountdown = Random.Range(strafeMinTime, strafeMaxTime);
            }

            Vector3 forward = toEnemy.normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            float side = strafeDistance * m_StrafeDirection;
            Vector3 target = a.transform.position + right * side + forward * m_StrafeForwardOffset;

            if (!a.TryMoveToWorldPosition(target))
            {
                Vector3 opposite = a.transform.position - right * side + forward * m_StrafeForwardOffset;
                a.TryMoveToWorldPosition(opposite);
            }
        }
    }
}

