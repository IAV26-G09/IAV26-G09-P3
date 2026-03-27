using Unity.Netcode;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class WeaponSpawner : NetworkBehaviour
    {
        [Header("Configuración del Spawner")]
        [Tooltip("Lista de los prefabs de los Pickups de armas (Deben tener NetworkObject)")]
        public GameObject[] WeaponPickups;

        [Tooltip("Tiempo en segundos entre cada aparición")]
        public float SpawnInterval = 15f;

        // Referencia al arma que está actualmente flotando
        private NetworkObject currentSpawnedWeapon;
        private float timer;

        public override void OnNetworkSpawn()
        {
            // Solo el servidor controla cuándo aparecen las armas
            if (IsServer)
            {
                timer = SpawnInterval; // Empezamos a contar
            }
        }

        void Update()
        {
            // Si no somos el servidor, no hacemos nada
            if (!IsServer) return;

            // Si ya hay un arma flotando en este spawner, pausamos el temporizador
            if (currentSpawnedWeapon != null && currentSpawnedWeapon.IsSpawned)
                return;

            // Restamos tiempo
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                SpawnRandomWeapon();
                timer = SpawnInterval; // Reiniciamos el contador
            }
        }

        void SpawnRandomWeapon()
        {
            if (WeaponPickups.Length == 0) return;

            // 1. Elegimos un arma al azar
            int randomIndex = Random.Range(0, WeaponPickups.Length);
            GameObject weaponPrefab = WeaponPickups[randomIndex];

            // 2. La instanciamos físicamente en la posición del spawner
            GameObject spawnedObject = Instantiate(weaponPrefab, transform.position, transform.rotation);

            // 3. Le decimos al servidor que la haga aparecer en todas las pantallas
            currentSpawnedWeapon = spawnedObject.GetComponent<NetworkObject>();
            if (currentSpawnedWeapon != null)
            {
                currentSpawnedWeapon.Spawn();
            }
            else
            {
                Debug.LogError("¡El arma " + weaponPrefab.name + " no tiene el componente NetworkObject!");
            }
        }
    }
}