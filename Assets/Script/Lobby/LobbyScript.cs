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
using System.Linq;
using TMPro;

public class LobbyScript : Singleton<LobbyScript>
{
    public Lobby hostLobby;
    public Lobby joinedLobby;
    private string playerName;
    private float lobbyUpdateTimer;

    [Header("Get Gameobject")]
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] TMP_Text joinCodeText;
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] TMP_Text player_1_NameText;
    [SerializeField] TMP_Text player_2_NameText;
    [SerializeField] TMP_InputField joinCodeIdInput;
    [SerializeField] TMP_InputField lobbyNameInput;
    [SerializeField] TMP_InputField playerNameInput;
    [SerializeField] TMP_InputField playerNameChangeInput;
    [SerializeField] GameObject lobbyJoinPanel;
    [SerializeField] GameObject roomJoinPanel;
    [SerializeField] TMP_Dropdown characterSelect;

    private void Start()
    {
        //var callbacks = new LobbyEventCallbacks();
        //callbacks.LobbyChanged += OnLobbyChanged;
    }
    private void Update()
    {

        if (joinedLobby != null)
        {
            if (joinedLobby.Players.Count == 2)
            {
                updatePlayerListName(joinedLobby);
            }
        }

        HandleLobbyPollForUpdate();
    }
    private async void HandleLobbyPollForUpdate()
    {
        
        try
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
        catch
        {
            lobbyJoinPanel.SetActive(true);
            roomJoinPanel.SetActive(false);
            joinedLobby = null;
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
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerNameInput.text)},
                        {"PlayerCharacterSelect", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "Dino") }
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
    public async void JoinByLobbyCode()
    {
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerNameInput.text)},
                        {"PlayerCharacterSelect", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "Dino") }
                    }
                }
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(joinCodeIdInput.text, options);
            joinedLobby = lobby;
            Debug.Log("Joined by lobby code : " + joinCodeIdInput.text);
            UpdateRoomNameAndJoinCode(joinedLobby);
            PrintPlayers(joinedLobby);
        }catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    /*private async void QuickJoinLobby()
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
    }*/
    private static IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
    private void OnLobbyChanged(ILobbyChanges changes)
    {
        changes.ApplyToLobby(joinedLobby);
        if (changes.LobbyDeleted)
        {
            Debug.Log("Somethingchange");
            LeaveRoom();
        }
        // Refresh the UI in some way
    }
    public async void KickPlayer()
    {
        try
        {
            for (int i = 1; i < joinedLobby.Players.Count; i++)
            {
                string playerId = joinedLobby.Players[i].Id;
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);

            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void LeaveRoom()
    {
        try
        {
            //Ensure you sign-in before calling Authentication Instance
            //See IAuthenticationService interface
            string playerId = AuthenticationService.Instance.PlayerId;
            if (playerId == hostLobby.Players[0].Id)
            {
                lobbyJoinPanel.SetActive(true);
                roomJoinPanel.SetActive(false);
                
                if (hostLobby.Players.Count == 2)
                {
                    KickPlayer();
                }
                DeleteLobby();
                /*if (joinedLobby.Players.Count == 2)
                {
                    foreach (Player player in hostLobby.Players)
                    {
                        Debug.Log(player.Data["PlayerName"].Value);
                    }
                    MigrateLobbyHost();
                }
                else
                {
                    lobbyJoinPanel.SetActive(true);
                    roomJoinPanel.SetActive(false);
                    DeleteLobby();
                    return;
                }
                foreach (Player player in hostLobby.Players)
                {
                    if (player.Id == AuthenticationService.Instance.PlayerId)
                    {
                        Debug.Log("Player Leave : " + player.Data["PlayerName"].Value);
                    }
                }

                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);*/

            }
            else
            {
                foreach (Player player in hostLobby.Players)
                {
                    if (player.Id == AuthenticationService.Instance.PlayerId)
                    {
                        Debug.Log("Player Leave : " + player.Data["PlayerName"].Value);
                    }
                }
                lobbyJoinPanel.SetActive(true);
                roomJoinPanel.SetActive(false);
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
                joinedLobby = null;
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
            joinedLobby = null;
            Debug.Log("Delete lobby");
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void MigrateLobbyHost()
    {
        try
        {

            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = hostLobby.Players[0].Id
            });
            joinedLobby = null;
            PrintPlayers(hostLobby);
            Debug.Log("Change host");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
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

    /*private async void JoinLobby()
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
    }*/

    public void PrintPlayers(Lobby lobby)
    {
        //Debug.Log("Lobby : " + lobby.Name + " / " + lobby.Data["JoinCodeKey"].Value);
        foreach(Player player in lobby.Players)
        {
            Debug.Log(player.Id + " : " + player.Data["PlayerName"].Value);
        }
    }

    public void UpdateRoomNameAndJoinCode(Lobby lobby)
    {
        lobbyJoinPanel.SetActive(false);
        roomJoinPanel.SetActive(true);

        foreach (Player player in lobby.Players)
        {
            if (player.Id == AuthenticationService.Instance.PlayerId)
            {
                playerNameText.text = "Player name : " + playerNameInput.text;
            }
        }
        updatePlayerListName(lobby);
        roomNameText.text = lobby.Name;
        joinCodeText.text = "Join code : " + lobby.LobbyCode;
    }

    public async void UpdatePlayerName()
    {
        try
        {
            playerName = playerNameInput.text;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id,
                AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerNameChangeInput.text) },
                }
            });
            PrintPlayers(joinedLobby);
            UpdatePlayerName(joinedLobby);
            updatePlayerListName(joinedLobby);
        }
        catch ( LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void UpdatePlayerName(Lobby lobby)
    {
        foreach (Player player in lobby.Players)
        {
            if (player.Id == AuthenticationService.Instance.PlayerId)
            {
                playerNameText.text = "Player name : " + player.Data["PlayerName"].Value;
            }
        }
    }
    public void updatePlayerListName(Lobby lobby)
    {
        int numberOfPlayer = 0; 
        foreach (Player player in lobby.Players)
        {
            if (numberOfPlayer == 0)
            {
                player_1_NameText.text = player.Data["PlayerName"].Value;
                numberOfPlayer++;
            }
            else
            {
                player_2_NameText.text = player.Data["PlayerName"].Value;
                numberOfPlayer++;
            }
        }
        if (lobby.Players.Count == 0)
        {
            player_1_NameText.text = "Empty Slot";
            player_2_NameText.text = "Empty Slot";
        }
        else if (lobby.Players.Count == 1)
        {
            player_2_NameText.text = "Empty Slot";
        }
    }
}
