using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro; 

public class PlayerNameTag : NetworkBehaviour
{
    [Tooltip("Arrastra aquí el componente TextMeshPro que está sobre la cabeza")]
    public TextMeshProUGUI NameText;

    // Esta es la variable mágica de red que guarda el nombre de hasta 32 letras
    public NetworkVariable<FixedString32Bytes> NetworkedName = new NetworkVariable<FixedString32Bytes>(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    void Start()
    {
        // Nos suscribimos para escuchar cuando internet cambie el nombre
        NetworkedName.OnValueChanged += OnNameChanged;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Si somos el dueño, leemos nuestro nombre guardado y lo subimos a la red
            string savedName = PlayerPrefs.GetString("PlayerName", "Player");
            NetworkedName.Value = new FixedString32Bytes(savedName);
            NameText.gameObject.SetActive(false);
        }
        else
        {
            // Si es un enemigo que ya estaba en la partida, forzamos que se muestre su nombre
            UpdateNameTag(NetworkedName.Value.ToString());
        }
    }

    public override void OnNetworkDespawn()
    {
        NetworkedName.OnValueChanged -= OnNameChanged;
    }

    void OnNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        UpdateNameTag(newValue.ToString());
    }

    void UpdateNameTag(string newName)
    {
        if (NameText != null)
        {
            NameText.text = newName;
        }
    }

    // --- EFECTO BILLBOARD (Mirar a la cámara) ---
    void LateUpdate()
    {
        // Si hay un texto asignado y hay una cámara principal en la escena
        if (NameText != null && Camera.main != null)
        {
            // Hacemos que el cartel mire exactamente hacia la misma dirección que la cámara principal
            NameText.transform.rotation = Camera.main.transform.rotation;
        }
    }
}