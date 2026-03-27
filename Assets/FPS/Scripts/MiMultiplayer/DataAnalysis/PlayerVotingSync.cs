using Unity.Netcode;
using UnityEngine;

public class PlayerVotingSync : NetworkBehaviour
{
    public void SubmitVoteHumano(GameObject targetGameObject)
    {
        ProcesarVoto(targetGameObject, true);
    }

    public void SubmitVoteRobot(GameObject targetGameObject)
    {
        ProcesarVoto(targetGameObject, false);
    }

    private void ProcesarVoto(GameObject targetGameObject, bool esVotoHumano)
    {
        if (!IsOwner) return;

        NetworkObject targetNetObj = targetGameObject.GetComponentInParent<NetworkObject>();

        if (targetNetObj != null && targetNetObj.IsPlayerObject)
        {
            SendVoteServerRpc(OwnerClientId, targetNetObj.OwnerClientId, esVotoHumano);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SendVoteServerRpc(ulong tiradorId, ulong objetivoId, bool esVotoHumano, ServerRpcParams rpcParams = default)
    {
        if (MatchDataManager.Instance != null)
        {
            MatchDataManager.Instance.RegistrarVoto(tiradorId, objetivoId, esVotoHumano);
        }
    }
}