using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

[Serializable]
public class ConteoVoto
{
    public ulong idVotante;
    public int totalVotos;
}

[Serializable]
public class PlayerDataExport
{
    public ulong clientId;
    public string playerName;
    public List<ConteoVoto> votosHumano;
    public List<ConteoVoto> votosRobot;
}

[Serializable]
public class MatchDataWrapperExport
{
    public List<PlayerDataExport> resultadosPartida = new List<PlayerDataExport>();
}

public class PlayerData
{
    public ulong clientId;
    public string playerName;
    public List<ulong> votosHumanoRecibidos = new List<ulong>();
    public List<ulong> votosRobotRecibidos = new List<ulong>();
}

public class MatchDataManager : MonoBehaviour
{
    public static MatchDataManager Instance { get; private set; }

    private Dictionary<ulong, PlayerData> playersData = new Dictionary<ulong, PlayerData>();

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer) 
        {
            NetworkManager.Singleton.OnServerStarted += RegistrarHost;

            if (NetworkManager.Singleton.IsListening)
            {
                RegistrarHost();
            } 
        }
    }

    private void RegistrarHost()
    {
        ulong hostId = NetworkManager.Singleton.LocalClientId;

        string hostName = PlayerPrefs.GetString("PlayerName", "Host_Desconocido");

        RegisterPlayer(hostId, hostName);
    }

    public void RegisterPlayer(ulong clientId, string playerName)
    {
        if (!playersData.ContainsKey(clientId))
        {
            playersData[clientId] = new PlayerData { clientId = clientId, playerName = playerName };
        }
        else
        {
            // Si el jugador ya existía (quizás por el registro de emergencia), actualizamos su nombre real
            if (playersData[clientId].playerName.StartsWith("VirtualPlayer_"))
            {
                playersData[clientId].playerName = playerName;
            }
        }
    }

    public void RegistrarVoto(ulong tiradorId, ulong objetivoId, bool esVotoHumano)
    {
        // Registro de emergencia para el objetivo
        if (!playersData.ContainsKey(objetivoId))
        {
            RegisterPlayer(objetivoId, "VirtualPlayer_" + objetivoId);
        }

        // Registro de emergencia para el tirador
        if (!playersData.ContainsKey(tiradorId))
        {
            RegisterPlayer(tiradorId, "VirtualPlayer_" + tiradorId);
        }

        PlayerData datosObjetivo = playersData[objetivoId];

        if (esVotoHumano)
        {
            datosObjetivo.votosHumanoRecibidos.Add(tiradorId);
        }
        else
        {
            datosObjetivo.votosRobotRecibidos.Add(tiradorId);
        }

    }

    public void ExportarDatosAJson()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer) return;
        if (playersData.Count == 0) return;

        MatchDataWrapperExport wrapperExport = new MatchDataWrapperExport();

        foreach (var data in playersData.Values)
        {
            List<ConteoVoto> humanosAgrupados = data.votosHumanoRecibidos
                .GroupBy(voto => voto)
                .Select(grupo => new ConteoVoto { idVotante = grupo.Key, totalVotos = grupo.Count() })
                .ToList();

            List<ConteoVoto> robotsAgrupados = data.votosRobotRecibidos
                .GroupBy(voto => voto)
                .Select(grupo => new ConteoVoto { idVotante = grupo.Key, totalVotos = grupo.Count() })
                .ToList();

            PlayerDataExport exportData = new PlayerDataExport
            {
                clientId = data.clientId,
                playerName = data.playerName,
                votosHumano = humanosAgrupados,
                votosRobot = robotsAgrupados
            };

            wrapperExport.resultadosPartida.Add(exportData);
        }

        string json = JsonUtility.ToJson(wrapperExport, true);
        string timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filePath = Path.Combine(Application.dataPath, $"ResultadosPartida_{timeStamp}.json");

        File.WriteAllText(filePath, json);
        Debug.Log($"<color=green>¡Partida guardada! JSON creado en: {filePath}</color>");
    }

    public void OnDestroy()
    {
        ExportarDatosAJson();
    }
}