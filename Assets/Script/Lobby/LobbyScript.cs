using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Net.Http.Headers;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using TMPro;

public class LobbyScript : Singleton<LobbyScript>
{
    Lobby hostLobby;
    private Lobby joinedLobby;

    string playerName;
    private float lobbyUpdateTimer;

    [SerializeField] TMP_Text roomNameText;
    [SerializeField] TMP_Text joinCodeText;
    [SerializeField] TMP_InputField lobbyNameInput;
    [SerializeField] GameObject lobbyJoinPanel;
    [SerializeField] GameObject roomJoinPanel;

    private void Start()
    {
        playerName = "Myname" + Random.Range(1, 9999);
        Debug.Log("Player name = " + playerName);
    }
    private void Update()
    {
        HandleLobbyPollForUpdate();
    }

    private async void HandleLobbyPollForUpdate()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
            }
        }
    }

    public async void CreateLobby()
    {

        try
        {
            string lobbyName = lobbyNameInput.text;
            int maxPlayers = 2;
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,

                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
                    }
                },
                Data = new Dictionary<string, DataObject>
                {
                    {"JoinCodeKey", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            hostLobby = lobby;
            joinedLobby = hostLobby;
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));
            Debug.Log("Create Lobby : " + lobby.Name + "," + lobby.MaxPlayers  + "," + lobby.Id + "," +  lobby.LobbyCode);
            PrintPlayers(hostLobby);
            UpdateRoomNameAndJoinCode(hostLobby);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode)}
                }
            });
            joinedLobby = hostLobby;
            PrintPlayers(hostLobby);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }
    public async void JoinByLobbyCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
                    }
                }
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            joinedLobby = lobby;
            Debug.Log("Joined  by lobby code : " + lobbyCode);
            PrintPlayers(joinedLobby);
        }catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void QuickJoinLobby()
    {
        try
        {
            Lobby lobby = await Lobbies.Instance.QuickJoinLobbyAsync();
            Debug.Log(lobby.Name + "," + lobby.AvailableSlots);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private static IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder>()
            {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync();
            Debug.Log("Lobbies found : " + lobbies.Results.Count);
            foreach(Lobby lobby in lobbies.Results)
            {
                Debug.Log(lobby.Name + " , " + lobby.MaxPlayers + " , " + lobby.Data["GameMode"].Value);
            }
        }catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    private async void JoinLobby()
    {
        try
        {
            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync();
            await Lobbies.Instance.JoinLobbyByIdAsync(lobbies.Results[0].Id);
            Debug.Log(lobbies.Results[0].Name + " , " + lobbies.Results[0].AvailableSlots);
        }catch(LobbyServiceException e)
        {
            Debug.Log(e);   
        }
    }

    public void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Lobby : " + lobby.Name + " / " + lobby.Data["JoinCodeKey"].Value);
        foreach(Player player in lobby.Players)
        {
            Debug.Log(player.Id + " : " + player.Data["PlayerName"].Value);
        }
    }

    public void UpdateRoomNameAndJoinCode(Lobby lobby)
    {
        lobbyJoinPanel.SetActive(false);
        roomJoinPanel.SetActive(true);
        roomNameText.text = lobby.Name;
        joinCodeText.text = "Join code : " + lobby.Data["JoinCodeKey"].Value;
    }

}
