using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    public enum WeaponShootType
    {
        Manual,
        Automatic,
        Burst,
        Charge,
    }

    [System.Serializable]
    public struct CrosshairData
    {
        [Tooltip("The image that will be used for this weapon's crosshair")]
        public Sprite CrosshairSprite;

        [Tooltip("The size of the crosshair image")]
        public int CrosshairSize;

        [Tooltip("The color of the crosshair image")]
        public Color CrosshairColor;
    }

    [RequireComponent(typeof(AudioSource))]
    public class WeaponController : MonoBehaviour
    {
        public enum MouseButton
        {
            Left,
            Right,
        }
        [Header("Information")] [Tooltip("The name that will be displayed in the UI for this weapon")]
        public string WeaponName;

        [Tooltip("The image that will be displayed in the UI for this weapon")]
        public Sprite WeaponIcon;

        [Tooltip("Default data for the crosshair")]
        public CrosshairData CrosshairDataDefault;

        [Tooltip("Data for the crosshair when targeting an enemy")]
        public CrosshairData CrosshairDataTargetInSight;

        [Header("Internal References")]
        [Tooltip("The root object for the weapon, this is what will be deactivated when the weapon isn't active")]
        public GameObject WeaponRoot;

        [Tooltip("Tip of the weapon, where the projectiles are shot")]
        public Transform WeaponMuzzle;

        [Header("Second Weapon (Dual)")]
        [Tooltip("Enable support for a second weapon (dual setup) handled by this controller")] 
        public bool SecondWeaponEnabled = false;

        [Tooltip("Root object for the second weapon")] 
        public GameObject SecondWeaponRoot;

        [Tooltip("Tip of the second weapon, where projectiles are shot")] 
        public Transform SecondWeaponMuzzle;

        [Header("Shoot Parameters")] [Tooltip("The type of weapon wil affect how it shoots")]
        public WeaponShootType ShootType;

        [Tooltip("The projectile prefab")] public ProjectileBase ProjectilePrefab;
        [Tooltip("If set, spawns this prefab once while fire is held, parented to the muzzle (e.g., flamethrower emitter)")]
        public ProjectileBase HeldEmitterPrefab;
        [Tooltip("Use a single emitter while holding fire instead of spawning projectiles each shot")] 
        public bool UseHeldEmitter = false;

        [Tooltip("Minimum duration between two shots")]
        public float DelayBetweenShots = 0.5f;

        [Tooltip("Number of shots per burst (Burst mode only)")]
        public int BurstCount = 3;

        [Tooltip("Delay between shots in a burst (Burst mode only)")]
        public float BurstShotInterval = 0.08f;

        [Tooltip("Angle for the cone in which the bullets will be shot randomly (0 means no spread at all)")]
        public float BulletSpreadAngle = 0f;

        [Tooltip("Amount of bullets per shot")]
        public int BulletsPerShot = 1;

        [Tooltip("Force that will push back the weapon after each shot")] [Range(0f, 2f)]
        public float RecoilForce = 1;

        [Tooltip("Ratio of the default FOV that this weapon applies while aiming")] [Range(0f, 1f)]
        public float AimZoomRatio = 1f;

        [Tooltip("Translation to apply to weapon arm when aiming with this weapon")]
        public Vector3 AimOffset;

        [Header("Ammo Parameters")]
        [Tooltip("Should the player manually reload")]
        public bool AutomaticReload = true;
        [Tooltip("Has physical clip on the weapon and ammo shells are ejected when firing")]
        public bool HasPhysicalBullets = false;
        [Tooltip("Number of bullets in a clip")]
        public int ClipSize = 30;
        [Tooltip("Bullet Shell Casing")]
        public GameObject ShellCasing;
        [Tooltip("Weapon Ejection Port for physical ammo")]
        public Transform EjectionPort;
        [Tooltip("Force applied on the shell")]
        [Range(0.0f, 5.0f)] public float ShellCasingEjectionForce = 2.0f;
        [Tooltip("Maximum number of shell that can be spawned before reuse")]
        [Range(1, 30)] public int ShellPoolSize = 1;
        [Tooltip("Amount of ammo reloaded per second")]
        public float AmmoReloadRate = 1f;

        [Tooltip("Delay after the last shot before starting to reload")]
        public float AmmoReloadDelay = 2f;

        [Tooltip("Maximum amount of ammo in the gun")]
        public int MaxAmmo = 8;

        [Header("Charging parameters (charging weapons only)")]
        [Tooltip("Trigger a shot when maximum charge is reached")]
        public bool AutomaticReleaseOnCharged;

        [Tooltip("Duration to reach maximum charge")]
        public float MaxChargeDuration = 2f;

        [Tooltip("Initial ammo used when starting to charge")]
        public float AmmoUsedOnStartCharge = 1f;

        [Tooltip("Additional ammo used when charge reaches its maximum")]
        public float AmmoUsageRateWhileCharging = 1f;

        [Header("Audio & Visual")] 
        [Tooltip("Optional weapon animator for OnShoot animations")]
        public Animator WeaponAnimator;

        [Tooltip("Prefab of the muzzle flash")]
        public GameObject MuzzleFlashPrefab;

        [Tooltip("Unparent the muzzle flash instance on spawn")]
        public bool UnparentMuzzleFlash;

        [Tooltip("sound played when shooting")]
        public AudioClip ShootSfx;

        [Tooltip("Sound played when changing to this weapon")]
        public AudioClip ChangeWeaponSfx;

        [Tooltip("Continuous Shooting Sound")] public bool UseContinuousShootSound = false;
        public AudioClip ContinuousShootStartSfx;
        public AudioClip ContinuousShootLoopSfx;
        public AudioClip ContinuousShootEndSfx;
        AudioSource m_ContinuousShootAudioSource = null;
        bool m_WantsToShoot = false;
        ProjectileBase m_ActiveHeldEmitter = null;

        [Header("Input Overrides")]
        [Tooltip("Enable per-weapon custom mouse button for firing (default is Left click)")]
        public bool UseCustomFireButtonOverride = false;

        [Tooltip("Mouse button to use when the override is enabled")]
        public MouseButton FireMouseButton = MouseButton.Left;

        [Tooltip("Enable custom mouse button for firing the second weapon")] 
        public bool UseCustomFireButtonOverrideSecondary = false;

        [Tooltip("Mouse button used to fire the second weapon when override is enabled")] 
        public MouseButton FireMouseButtonSecondary = MouseButton.Right;

        [Tooltip("Disable aiming down sights for this weapon")] 
        public bool DisableADSForThisWeapon = false;

        Transform m_CurrentMuzzleOverride;

        public UnityAction OnShoot;
        public event Action OnShootProcessed;

        int m_CarriedPhysicalBullets;
        float m_CurrentAmmo;
        float m_LastTimeShot = Mathf.NegativeInfinity; // keeps global last shot (for cooling/reload)
        float m_LastTimeShotPrimary = Mathf.NegativeInfinity;
        float m_LastTimeShotSecondary = Mathf.NegativeInfinity;
        public float LastChargeTriggerTimestamp { get; private set; }
        Vector3 m_LastMuzzlePosition;

        public GameObject Owner { get; set; }
        public GameObject SourcePrefab { get; set; }
        public bool IsCharging { get; private set; }
        public float CurrentAmmoRatio { get; private set; }
        public bool IsWeaponActive { get; private set; }
        public bool IsCooling { get; private set; }
        public float CurrentCharge { get; private set; }
        public Vector3 MuzzleWorldVelocity { get; private set; }

        public float GetAmmoNeededToShoot() =>
            (ShootType != WeaponShootType.Charge ? 1f : Mathf.Max(1f, AmmoUsedOnStartCharge)) /
            (MaxAmmo * BulletsPerShot);

        public int GetCarriedPhysicalBullets() => m_CarriedPhysicalBullets;
        public int GetCurrentAmmo() => Mathf.FloorToInt(m_CurrentAmmo);

        AudioSource m_ShootAudioSource;
        Renderer[] m_WeaponRenderers;
        Dictionary<Renderer, bool> m_InitialRendererEnabled;

        public bool IsReloading { get; private set; }

        const string k_AnimAttackParameter = "Attack";

        private Queue<Rigidbody> m_PhysicalAmmoPool;

        void Awake()
        {
            m_CurrentAmmo = MaxAmmo;
            m_CarriedPhysicalBullets = HasPhysicalBullets ? ClipSize : 0;
            m_LastMuzzlePosition = WeaponMuzzle.position;

            m_ShootAudioSource = GetComponent<AudioSource>();
            DebugUtility.HandleErrorIfNullGetComponent<AudioSource, WeaponController>(m_ShootAudioSource, this,
                gameObject);

            // Cache all renderers under this weapon instance and remember their initial enabled state
            m_WeaponRenderers = GetComponentsInChildren<Renderer>(true);
            m_InitialRendererEnabled = new Dictionary<Renderer, bool>(m_WeaponRenderers != null ? m_WeaponRenderers.Length : 0);
            if (m_WeaponRenderers != null)
            {
                for (int i = 0; i < m_WeaponRenderers.Length; i++)
                {
                    var r = m_WeaponRenderers[i];
                    if (r != null && !m_InitialRendererEnabled.ContainsKey(r))
                    {
                        m_InitialRendererEnabled.Add(r, r.enabled);
                    }
                }
            }

            if (UseContinuousShootSound)
            {
                m_ContinuousShootAudioSource = gameObject.AddComponent<AudioSource>();
                m_ContinuousShootAudioSource.playOnAwake = false;
                m_ContinuousShootAudioSource.clip = ContinuousShootLoopSfx;
                m_ContinuousShootAudioSource.outputAudioMixerGroup =
                    AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponShoot);
                m_ContinuousShootAudioSource.loop = true;
            }

            if (HasPhysicalBullets)
            {
                m_PhysicalAmmoPool = new Queue<Rigidbody>(ShellPoolSize);

                for (int i = 0; i < ShellPoolSize; i++)
                {
                    GameObject shell = Instantiate(ShellCasing, transform);
                    shell.SetActive(false);
                    m_PhysicalAmmoPool.Enqueue(shell.GetComponent<Rigidbody>());
                }
            }
        }

        public void AddCarriablePhysicalBullets(int count) => m_CarriedPhysicalBullets = Mathf.Max(m_CarriedPhysicalBullets + count, MaxAmmo);

        void ShootShell()
        {
            Rigidbody nextShell = m_PhysicalAmmoPool.Dequeue();

            nextShell.transform.position = EjectionPort.transform.position;
            nextShell.transform.rotation = EjectionPort.transform.rotation;
            nextShell.gameObject.SetActive(true);
            nextShell.transform.SetParent(null);
            nextShell.collisionDetectionMode = CollisionDetectionMode.Continuous;
            nextShell.AddForce(nextShell.transform.up * ShellCasingEjectionForce, ForceMode.Impulse);

            m_PhysicalAmmoPool.Enqueue(nextShell);
        }

        void PlaySFX(AudioClip sfx) => AudioUtility.CreateSFX(sfx, transform.position, AudioUtility.AudioGroups.WeaponShoot, 0.0f);


        void Reload()
        {
            if (m_CarriedPhysicalBullets > 0)
            {
                m_CurrentAmmo = Mathf.Min(m_CarriedPhysicalBullets, ClipSize);
            }

            IsReloading = false;
        }

        public void StartReloadAnimation()
        {
            if (m_CurrentAmmo < m_CarriedPhysicalBullets)
            {
                GetComponent<Animator>().SetTrigger("Reload");
                IsReloading = true;
            }
        }

        void Update()
        {
            UpdateAmmo();
            UpdateCharge();
            UpdateContinuousShootSound();

            // Burst logic: process pending burst shots
            if (ShootType == WeaponShootType.Burst && m_BurstActive && m_BurstShotsRemaining > 0 && Time.time >= m_NextBurstShotTime)
            {
                if (TryShoot())
                {
                    m_BurstShotsRemaining--;
                    m_NextBurstShotTime = Time.time + BurstShotInterval;
                }
                else
                {
                    m_BurstShotsRemaining = 0;
                }
                if (m_BurstShotsRemaining <= 0)
                {
                    m_BurstActive = false;
                }
            }

            if (Time.deltaTime > 0)
            {
                MuzzleWorldVelocity = (WeaponMuzzle.position - m_LastMuzzlePosition) / Time.deltaTime;
                m_LastMuzzlePosition = WeaponMuzzle.position;
            }
        }

        void UpdateAmmo()
        {
            if (AutomaticReload && m_LastTimeShot + AmmoReloadDelay < Time.time && m_CurrentAmmo < MaxAmmo && !IsCharging)
            {
                // reloads weapon over time
                m_CurrentAmmo += AmmoReloadRate * Time.deltaTime;

                // limits ammo to max value
                m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo, 0, MaxAmmo);

                IsCooling = true;
            }
            else
            {
                IsCooling = false;
            }

            if (MaxAmmo == Mathf.Infinity)
            {
                CurrentAmmoRatio = 1f;
            }
            else
            {
                CurrentAmmoRatio = m_CurrentAmmo / MaxAmmo;
            }
        }

        void UpdateCharge()
        {
            if (IsCharging)
            {
                if (CurrentCharge < 1f)
                {
                    float chargeLeft = 1f - CurrentCharge;

                    // Calculate how much charge ratio to add this frame
                    float chargeAdded = 0f;
                    if (MaxChargeDuration <= 0f)
                    {
                        chargeAdded = chargeLeft;
                    }
                    else
                    {
                        chargeAdded = (1f / MaxChargeDuration) * Time.deltaTime;
                    }

                    chargeAdded = Mathf.Clamp(chargeAdded, 0f, chargeLeft);

                    // See if we can actually add this charge
                    float ammoThisChargeWouldRequire = chargeAdded * AmmoUsageRateWhileCharging;
                    if (ammoThisChargeWouldRequire <= m_CurrentAmmo)
                    {
                        // Use ammo based on charge added
                        UseAmmo(ammoThisChargeWouldRequire);

                        // set current charge ratio
                        CurrentCharge = Mathf.Clamp01(CurrentCharge + chargeAdded);
                    }
                }
            }
        }

        void UpdateContinuousShootSound()
        {
            if (UseContinuousShootSound)
            {
                if (m_WantsToShoot && m_CurrentAmmo >= 1f)
                {
                    if (!m_ContinuousShootAudioSource.isPlaying)
                    {
                        m_ShootAudioSource.PlayOneShot(ShootSfx);
                        m_ShootAudioSource.PlayOneShot(ContinuousShootStartSfx);
                        m_ContinuousShootAudioSource.Play();
                    }
                }
                else if (m_ContinuousShootAudioSource.isPlaying)
                {
                    m_ShootAudioSource.PlayOneShot(ContinuousShootEndSfx);
                    m_ContinuousShootAudioSource.Stop();
                }
            }
        }

        public void ShowWeapon(bool show)
        {
            WeaponRoot.SetActive(show);
            if (SecondWeaponEnabled && SecondWeaponRoot != null)
            {
                SecondWeaponRoot.SetActive(show);
            }

            // Toggle only the weapon's renderers. On show, restore initial states; on hide, disable.
            if (m_WeaponRenderers != null)
            {
                if (show)
                {
                    for (int i = 0; i < m_WeaponRenderers.Length; i++)
                    {
                        var r = m_WeaponRenderers[i];
                        if (r != null && m_InitialRendererEnabled != null && m_InitialRendererEnabled.TryGetValue(r, out bool initial))
                        {
                            r.enabled = initial;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < m_WeaponRenderers.Length; i++)
                    {
                        var r = m_WeaponRenderers[i];
                        if (r != null)
                        {
                            r.enabled = false;
                        }
                    }
                }
            }

            if (show && ChangeWeaponSfx)
            {
                m_ShootAudioSource.PlayOneShot(ChangeWeaponSfx);
            }

            IsWeaponActive = show;
        }

        public void UseAmmo(float amount)
        {
            m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo - amount, 0f, MaxAmmo);
            m_CarriedPhysicalBullets -= Mathf.RoundToInt(amount);
            m_CarriedPhysicalBullets = Mathf.Clamp(m_CarriedPhysicalBullets, 0, MaxAmmo);
            m_LastTimeShot = Time.time;
        }

        int m_BurstShotsRemaining = 0;
        float m_NextBurstShotTime = 0f;
        bool m_BurstActive = false;

        public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
        {
            m_WantsToShoot = inputDown || inputHeld;
            switch (ShootType)
            {
                case WeaponShootType.Manual:
                    if (inputDown)
                    {
                        return TryShoot();
                    }
                    return false;

                case WeaponShootType.Automatic:
                    if (UseHeldEmitter)
                    {
                        // ...existing code...
                        if (inputHeld && m_ActiveHeldEmitter == null && m_LastTimeShot + DelayBetweenShots < Time.time)
                        {
                            if (HeldEmitterPrefab)
                            {
                                var emitter = Instantiate(HeldEmitterPrefab, WeaponMuzzle.position, WeaponMuzzle.rotation, WeaponMuzzle);
                                emitter.Shoot(this);
                                m_ActiveHeldEmitter = emitter;
                                m_LastTimeShot = Time.time;
                            }
                        }

                        if (inputUp && m_ActiveHeldEmitter)
                        {
                            Destroy(m_ActiveHeldEmitter.gameObject);
                            m_ActiveHeldEmitter = null;
                        }

                        return false;
                    }
                    else
                    {
                        if (inputHeld)
                        {
                            return TryShoot();
                        }
                        return false;
                    }

                case WeaponShootType.Burst:
                    // Start burst on inputDown if not already bursting
                    if (inputDown && !m_BurstActive && m_CurrentAmmo >= 1f && GetLastShotTimeForCurrentMuzzle() + DelayBetweenShots < Time.time)
                    {
                        m_BurstShotsRemaining = BurstCount;
                        m_NextBurstShotTime = Time.time;
                        m_BurstActive = true;
                        // Fire first shot immediately
                        if (TryShoot())
                        {
                            m_BurstShotsRemaining--;
                            m_NextBurstShotTime = Time.time + BurstShotInterval;
                        }
                        else
                        {
                            m_BurstShotsRemaining = 0;
                            m_BurstActive = false;
                        }
                        return true;
                    }
                    // Return true if a burst is in progress
                    return m_BurstActive;

                case WeaponShootType.Charge:
                    if (inputHeld)
                    {
                        TryBeginCharge();
                    }

                    // Check if we released charge or if the weapon shoot autmatically when it's fully charged
                    if (inputUp || (AutomaticReleaseOnCharged && CurrentCharge >= 1f))
                    {
                        return TryReleaseCharge();
                    }

                    return false;

                default:
                    return false;
            }
        }

        // Allow driving shooting logic using a specific muzzle (primary or secondary)
        public bool HandleShootInputsForMuzzle(Transform muzzle, bool inputDown, bool inputHeld, bool inputUp)
        {
            var previous = m_CurrentMuzzleOverride;
            m_CurrentMuzzleOverride = muzzle;
            bool result = HandleShootInputs(inputDown, inputHeld, inputUp);
            m_CurrentMuzzleOverride = previous;
            return result;
        }

        float GetLastShotTimeForCurrentMuzzle()
        {
            var muzzle = m_CurrentMuzzleOverride;
            if (muzzle == null || muzzle == WeaponMuzzle)
                return m_LastTimeShotPrimary;
            if (SecondWeaponEnabled && muzzle == SecondWeaponMuzzle)
                return m_LastTimeShotSecondary;
            return m_LastTimeShotPrimary;
        }

        void SetLastShotTimeForCurrentMuzzle(float time)
        {
            var muzzle = m_CurrentMuzzleOverride;
            if (muzzle == null || muzzle == WeaponMuzzle)
                m_LastTimeShotPrimary = time;
            else if (SecondWeaponEnabled && muzzle == SecondWeaponMuzzle)
                m_LastTimeShotSecondary = time;
        }

        bool TryShoot()
        {
            float lastShotForMuzzle = GetLastShotTimeForCurrentMuzzle();
            if (m_CurrentAmmo >= 1f
                && lastShotForMuzzle + DelayBetweenShots < Time.time)
            {
                HandleShoot();
                m_CurrentAmmo -= 1f;

                return true;
            }

            return false;
        }

        bool TryBeginCharge()
        {
            if (!IsCharging
                && m_CurrentAmmo >= AmmoUsedOnStartCharge
                && Mathf.FloorToInt((m_CurrentAmmo - AmmoUsedOnStartCharge) * BulletsPerShot) > 0
                && m_LastTimeShot + DelayBetweenShots < Time.time)
            {
                UseAmmo(AmmoUsedOnStartCharge);

                LastChargeTriggerTimestamp = Time.time;
                IsCharging = true;

                return true;
            }

            return false;
        }

        bool TryReleaseCharge()
        {
            if (IsCharging)
            {
                HandleShoot();

                CurrentCharge = 0f;
                IsCharging = false;

                return true;
            }

            return false;
        }

        void HandleShoot()
        {
            int bulletsPerShotFinal = ShootType == WeaponShootType.Charge
                ? Mathf.CeilToInt(CurrentCharge * BulletsPerShot)
                : BulletsPerShot;

            // spawn all bullets with random direction
            for (int i = 0; i < bulletsPerShotFinal; i++)
            {
                Transform muzzle = m_CurrentMuzzleOverride != null ? m_CurrentMuzzleOverride : WeaponMuzzle;
                Vector3 shotDirection = GetShotDirectionWithinSpread(muzzle);
                ProjectileBase newProjectile = Instantiate(ProjectilePrefab, muzzle.position,
                    Quaternion.LookRotation(shotDirection));
                newProjectile.Shoot(this);
            }

            // muzzle flash
            if (MuzzleFlashPrefab != null)
            {
                Transform muzzle = m_CurrentMuzzleOverride != null ? m_CurrentMuzzleOverride : WeaponMuzzle;
                GameObject muzzleFlashInstance = Instantiate(MuzzleFlashPrefab, muzzle.position,
                    muzzle.rotation, muzzle.transform);
                // Unparent the muzzleFlashInstance
                if (UnparentMuzzleFlash)
                {
                    muzzleFlashInstance.transform.SetParent(null);
                }

                Destroy(muzzleFlashInstance, 2f);
            }

            if (HasPhysicalBullets)
            {
                ShootShell();
                m_CarriedPhysicalBullets--;
            }

            // update per-muzzle last shot timestamp
            SetLastShotTimeForCurrentMuzzle(Time.time);
            // also update global last shot for cooling/reload logic
            m_LastTimeShot = Time.time;

            // play shoot SFX
            if (ShootSfx && !UseContinuousShootSound)
            {
                m_ShootAudioSource.PlayOneShot(ShootSfx);
            }

            // Trigger attack animation if there is any
            if (WeaponAnimator)
            {
                WeaponAnimator.SetTrigger(k_AnimAttackParameter);
            }

            OnShoot?.Invoke();
            OnShootProcessed?.Invoke();
        }

        public Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
        {
            float spreadAngleRatio = BulletSpreadAngle / 180f;
            Vector3 spreadWorldDirection = Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere,
                spreadAngleRatio);

            return spreadWorldDirection;
        }
    }
}