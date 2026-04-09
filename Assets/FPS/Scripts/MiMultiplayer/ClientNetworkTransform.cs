using UnityEngine;
using Unity.Netcode.Components;
using static UnityEngine.UI.GridLayoutGroup;

public class ClientNetworkTransform : NetworkTransform
{

    protected override bool OnIsServerAuthoritative()
    {
        // Jugadores humanos: authority del owner (cliente) para movimiento local.
        // Bots (FSM/NavMeshAgent): authority del servidor, porque el movimiento se simula en el servidor
        // y debe replicarse a todos los clientes de forma consistente.
        return GetComponent<FSM>() != null;
    }
}
