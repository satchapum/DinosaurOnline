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
using System.Threading.Tasks;

public class LobbyScript : Singleton<LobbyScript>
{
    public Lobby hostLobby;
    public Lobby joinedLobby;
    public GameObject parent;
    private string playerName;
    private float lobbyUpdateTimer;
    private float currentLobbyCount;
    private float oldLobbyCount;
    public float timeCount;

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
    [SerializeField] GameObject lobbyPrefab;

    private void Start()
    {
        timeCount = 0;
        oldLobbyCount = 0;
        currentLobbyCount = 0;
        //var callbacks = new LobbyEventCallbacks();
        //callbacks.LobbyChanged += OnLobbyChanged;
    }
    private void Update()
    {
        timeCount += Time.deltaTime;
        if (joinedLobby != null)
        {
            if (joinedLobby.Players.Count == 2)
            {
                updatePlayerListName(joinedLobby);
            }
            else if (joinedLobby.Players.Count == 1)
            {
                updatePlayerListName(joinedLobby);
            }
        }

        if (oldLobbyCount == currentLobbyCount && timeCount >= 5 && joinedLobby == null)
        {
            UpdateLobbyCount();
            timeCount = 0;
        }
        else if(timeCount >= 5 && joinedLobby == null)
        {
            oldLobbyCount = currentLobbyCount;
            timeCount = 0;
            FindAllLobby();
        }

        HandleLobbyPollForUpdate();
    }
    private async void UpdateLobbyCount()
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
            currentLobbyCount = queryResponse.Results.Count;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void FindAllLobby()
    {
        try
        {
            int numberOflistObject = 0;
            //List<GameObject> allListLobbyRoomGameObject = new List<GameObject>();
            foreach (Transform tr in parent.GetComponentsInChildren<Transform>())
            {
                if (numberOflistObject == 0)
                {
                    numberOflistObject++;
                }
                else
                {
                    Destroy(tr.gameObject);
                }

            }

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

            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);

            foreach (Lobby lobby in lobbies.Results)
            {
                Debug.Log("lobby id is : " + lobby.Id);
                GameObject createLobbyPrefab = Instantiate(lobbyPrefab, lobbyPrefab.transform.parent);
                createLobbyPrefab.GetComponentsInChildren<TMP_Text>()[0].text = lobby.Name;
                createLobbyPrefab.GetComponentsInChildren<TMP_Text>()[1].text = lobby.Players.Count + "/" + lobby.Players.Capacity;
                JoinButtonScript setJoinButtonRoomId  = createLobbyPrefab.GetComponentsInChildren<JoinButtonScript>()[0];
                setJoinButtonRoomId.SetRoomId(lobby.Id);
                createLobbyPrefab.SetActive(true);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobbyById(GameObject joinButtonScriptGameObject)
    {
        try
        {
            string roomId = joinButtonScriptGameObject.GetComponent<JoinButtonScript>().RoomId;
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
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
            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(roomId, options);
            hostLobby = lobby;
            joinedLobby = hostLobby;
            Debug.Log("Joined by lobby code : " + joinCodeIdInput.text);
            UpdateRoomNameAndJoinCode(joinedLobby);
            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }


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
            hostLobby = lobby;
            joinedLobby = hostLobby;
            Debug.Log("Joined by lobby code : " + joinCodeIdInput.text);
            UpdateRoomNameAndJoinCode(joinedLobby);
            PrintPlayers(joinedLobby);
        }catch (LobbyServiceException e)
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
    private void OnLobbyChanged(ILobbyChanges changes)
    {
        changes.ApplyToLobby(joinedLobby);
        if (changes.LobbyDeleted)
        {
            Debug.Log("Somethingchange");
            LeaveRoom();
        }
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
            if (player.Data != null)
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
