using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ProjectileFlameEmitter : ProjectileBase
    {
        [Header("Damage")]
        [Tooltip("Damage per second applied to targets in range")] public float DamagePerSecond = 25f;
        [Tooltip("Layers this flame can hit")] public LayerMask HittableLayers = -1;

        [Header("Shape")] 
        [Tooltip("Effective range of the flame from the muzzle")] public float Range = 6f;
        [Tooltip("Radius of the flame cone (spherecast radius)")] public float Radius = 0.5f;
        [Tooltip("Number of spherecasts across the range per frame (higher = fuller cone)")] [Range(1,8)] public int Segments = 3;

        [Header("VFX/SFX (optional)")]
        [Tooltip("Optional VFX on hit point")] public GameObject ImpactVfx;
        [Tooltip("Lifetime of spawned impact VFX")] public float ImpactVfxLifetime = 0.5f;

        [Header("Debug")] public Color GizmoColor = new Color(1f, 0.5f, 0f, 0.2f);

        List<Collider> m_IgnoredColliders;

        void OnEnable()
        {
            OnShoot += HandleShoot;
        }

        void OnDisable()
        {
            OnShoot -= HandleShoot;
        }

        void HandleShoot()
        {
            // Ignore owner colliders
            Collider[] ownerColliders = Owner ? Owner.GetComponentsInChildren<Collider>() : null;
            m_IgnoredColliders = new List<Collider>();
            if (ownerColliders != null) m_IgnoredColliders.AddRange(ownerColliders);

            // If parented by WeaponController, ensure local zero so it sits at muzzle
            if (transform.parent)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }

        void Update()
        {
            // Apply cone damage forward from current position
            if (!Owner) return;

            float step = Range / Mathf.Max(1, Segments);
            Vector3 origin = transform.position;
            Vector3 dir = transform.forward;

            for (int i = 1; i <= Segments; i++)
            {
                float dist = step * i;
                RaycastHit[] hits = Physics.SphereCastAll(origin, Radius, dir, dist, HittableLayers, QueryTriggerInteraction.Collide);
                foreach (var hit in hits)
                {
                    if (!IsHitValid(hit)) continue;

                    Damageable dmg = hit.collider.GetComponent<Damageable>();
                    if (dmg)
                    {
                        dmg.InflictDamage(DamagePerSecond * Time.deltaTime, false, Owner);
                    }

                    if (ImpactVfx)
                    {
                        var vfx = Object.Instantiate(ImpactVfx, hit.point, Quaternion.LookRotation(hit.normal));
                        if (ImpactVfxLifetime > 0)
                            Object.Destroy(vfx, ImpactVfxLifetime);
                    }
                }
            }
        }

        bool IsHitValid(RaycastHit hit)
        {
            if (hit.collider.GetComponent<IgnoreHitDetection>()) return false;
            if (hit.collider.isTrigger && hit.collider.GetComponent<Damageable>() == null) return false;
            if (m_IgnoredColliders != null && m_IgnoredColliders.Contains(hit.collider)) return false;
            return true;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = GizmoColor;
            Vector3 origin = transform.position;
            Vector3 dir = transform.forward;
            float step = Range / Mathf.Max(1, Segments);
            for (int i = 1; i <= Segments; i++)
            {
                float d = step * i;
                Gizmos.DrawWireSphere(origin + dir * d, Radius);
            }
        }
    }
}
