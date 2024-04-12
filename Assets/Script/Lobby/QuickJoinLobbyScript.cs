using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using TMPro;
public class QuickJoinLobbyScript : MonoBehaviour
{
    public Lobby hostLobby;
    public TMP_InputField playerNameInput;
    public GameObject startButton;
    public GameObject lobbyJoinPanel;
    public GameObject roomJoinPanel;
    string lobbyName = "QuickJoinLobby";
    private Lobby joinedLobby;

    public async void CreateOrJoinLobby()
    {
        /*startButton.SetActive(false);
        lobbyJoinPanel.SetActive(false);
        roomJoinPanel.SetActive(true);*/

        //joinedLobby = await QuickJoinLobby();
        joinedLobby = await QuickJoinLobbyNoFinding() ?? await CreateLobby();

        LobbyScript.Instance.hostLobby = joinedLobby;
        LobbyScript.Instance.joinedLobby = LobbyScript.Instance.hostLobby;
        LobbyScript.Instance.PrintPlayers(joinedLobby);
        LobbyScript.Instance.UpdateRoomNameAndJoinCode(joinedLobby);
        if (joinedLobby == null)
        {
           startButton.SetActive(true);
           lobbyJoinPanel.SetActive(true);
           roomJoinPanel.SetActive(false);
        }
    }

    private async Task<Lobby> QuickJoinLobby()
    {
        try
        {
            // Quick-join a random lobby 
            Lobby lobby = await FindRandomLobby();

            if (lobby == null) return null;
            Debug.Log(lobby.Name + " , " + lobby.AvailableSlots);

            // If we found one, grab the relay allocation details
            if (lobby.Data["JoinCodeKey"].Value != null)
            {
                string joinCode = lobby.Data["JoinCodeKey"].Value;
                Debug.Log("joinCode = " + joinCode);
                if (joinCode == null) return null;

                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                // Join the game room as a client

                //NetworkManager.Singleton.StartClient();

                return lobby;
            }

            return null;
        }
        catch (Exception e)
        {
            Debug.Log("No lobbies available via quick join");
            return null;
        }

    }
    
    private async Task<Lobby> QuickJoinLobbyNoFinding()
    {
        try
        {
            // Quick-join a random lobby with a maximum capacity of 10 or more players.
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions()
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerNameInput.text)},
                        {"PlayerCharacterSelect", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "Dino") },
                        {"IsPlayerReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "NotReady") }
                    }
                }
            };

            options.Filter = new List<QueryFilter>()
            {
                new QueryFilter(
                field: QueryFilter.FieldOptions.MaxPlayers,
                op: QueryFilter.OpOptions.GE,
                value: "0")
            };
            var lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            return lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("No lobbies available via quick join");
            return null;
        }

    }
    [ContextMenu("FindRandomLobby")]
    private async Task<Lobby> FindRandomLobby()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"0",QueryFilter.OpOptions.GT)
                }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                return lobby;
            }
            return null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    private async Task<Lobby> CreateLobby()
    {
        try
        {
            const int maxPlayers = 2;

            // Create a relay allocation and generate a join code to share with the lobby
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Create a lobby, adding the relay join code to the lobby data
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,

                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerNameInput.text)},
                        {"PlayerCharacterSelect", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "Dino") },
                        {"IsPlayerReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "NotReady") }
                    }
                },
                Data = new Dictionary<string, DataObject>
                {
                    {"JoinCodeKey", new DataObject(DataObject.VisibilityOptions.Public, "0") }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            hostLobby = lobby;
            Debug.Log("Create Lobby : " + lobby.Name + "," + lobby.MaxPlayers + "," + lobby.Id + "," + lobby.LobbyCode);
            
            // Send a heartbeat every 15 seconds to keep the room alive
            StartCoroutine(HeartBeatLobbyCoroutine(lobby.Id, 15));

            // Set the game room to use the relay allocation
            //RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            //NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Start the room immediately (or can wait for the lobby to fill up)

            //NetworkManager.Singleton.StartHost();
            Debug.Log("Join code = " + lobby.LobbyCode);
            return lobby;
        }
        catch (Exception e)
        {
            Debug.LogFormat("Failed creating a lobby");
            return null;
        }
    }

    private static IEnumerator HeartBeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private void OnDestroy()
    {
        try
        {
            StopAllCoroutines();
            // todo: Add a check to see if you're host
            //if (joinedLobby != null)
            //{
            //    if (joinedLobby.HostId == _playerId) Lobbies.Instance.DeleteLobbyAsync(joinedLobby.Id);
            //    else Lobbies.Instance.RemovePlayerAsync(joinedLobby.Id, _playerId);
            //}
        }
        catch (Exception e)
        {
            Debug.Log($"Error shutting down lobby: {e}");
        }
    }

}
