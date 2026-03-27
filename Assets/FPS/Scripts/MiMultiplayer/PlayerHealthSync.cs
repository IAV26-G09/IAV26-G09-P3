using Unity.Netcode;
using UnityEngine;
using Unity.FPS.Game;

public class PlayerHealthSync : NetworkBehaviour
{
    Health m_Health;

    void Awake()
    {
        // Cogemos el script de vida original
        m_Health = GetComponent<Health>();
    }

    // Esta función es llamada por Damageable.cs usando SendMessage
    void OnNetworkDamageRequested(float damage)
    {
        // El ordenador que ha disparado la bala le pide al servidor que aplique el daño
        if (IsServer)
        {
            ApplyDamageClientRpc(damage);
        }
        else
        {
            RequestDamageServerRpc(damage);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestDamageServerRpc(float damage)
    {
        ApplyDamageClientRpc(damage);
    }

    // El servidor da la orden y esto se ejecuta en las pantallas de TODOS los jugadores
    [ClientRpc]
    void ApplyDamageClientRpc(float damage)
    {
        if (m_Health != null)
        {
            // Ahora sí, ejecutamos el daño real en el script original de cada ordenador a la vez
            m_Health.TakeDamage(damage, null);
        }
    }
}