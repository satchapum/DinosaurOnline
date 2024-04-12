using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;
using Unity.Mathematics;
using Newtonsoft.Json.Bson;
using Unity.Netcode.Transports.UTP;


public class LoginManagerScript : NetworkBehaviour
{
    public string ipAddress = "127.0.0.1";
    public TMP_InputField joinCodeInputField;
    public string joinCode;
    UnityTransport transport;

    public List<uint> AlternativePlayerPrefabs;
    public TMP_Dropdown dropdown_TMP;

    public TMP_InputField userNameInputField;
    private bool isApproveConnection = false;

    public GameObject loginPanel;
    public GameObject leaveButton;
    //public GameObject scorePanel;

    [SerializeField] GameObject dinoUI;
    [SerializeField] GameObject godUI;
    [SerializeField] GameObject healthUI;

    public bool isTwoPlayerSpawning = false;

    [Header("SpawnPos")]
    [SerializeField] Transform[] posList;

    [SerializeField] public List<Material> materialList;

    [SerializeField] GameObject[] playerList;
    [SerializeField] LobbyScript lobbyScript;
    [SerializeField] GameObject timeText;

    public bool SetIsApproveConnection()
    {
        isApproveConnection = !isApproveConnection;
        return isApproveConnection;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        SetUIVisible(false);
    }

    public void SetUIVisible(bool isUserLogin)
    {
        if (isUserLogin)
        {
            healthUI.SetActive(true);
            loginPanel.SetActive(false);
            leaveButton.SetActive(true);
            timeText.SetActive(true);
            //scorePanel.SetActive(true);
        }
        /*else
        {
            healthUI.SetActive(false);
            loginPanel.SetActive(true);
            leaveButton.SetActive(false);
            //scorePanel.SetActive(false);
        }*/
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        Debug.Log("HandleClientDisconnect = " + clientId);
        if (NetworkManager.Singleton.IsHost) { }
        else if (NetworkManager.Singleton.IsClient) { Leave(); }
    }
    public void Leave()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;

        }

        else if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
        }

        dinoUI.SetActive(false);
        godUI.SetActive(false);
        SetUIVisible(false);

    }
    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log("HandleHandleClientConnected = " + clientId);
        if(clientId == NetworkManager.Singleton.LocalClientId)
        {
            SetUIVisible(true);
        }
    }

    private void HandleServerStarted()
    {
        Debug.Log("HandleServerStarted");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) { return; }
        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }

    public async void Host()
    {
        /*if (RelayManagerScript.Instance.IsRelayEnabled)
        {
            await RelayManagerScript.Instance.CreateRelay();
        }*/
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.StartHost();
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // The client identifier to be authenticated
        var clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        var connectionData = request.Payload;

        int byteLength = connectionData.Length;
        bool isApprove = false;
        int characterPrefabIndex = 0;

        //bool nameCheck = false;
        if (byteLength > 0)
        {
            string combinedString = System.Text.Encoding.ASCII.GetString(connectionData,0,byteLength);
            string[] extractedString = HelperScript.ExtractStrings(combinedString);

            //string hostData = lobbyScript.playerName;
            //nameCheck = NameApproveConnection(extractedString[0], hostData);

            for (int i = 0; i < extractedString.Length; i++)
            {
                if (i == 0)
                {
                    /*string clientData = extractedString[i];
                    isApprove = NameApproveConnection(clientData, hostData);
                    if (nameCheck == true)
                    {
                        isApprove = true;
                    }
                    else if (nameCheck == false)
                    {
                        isApprove = false;
                    }*/
                }
                else if (i == 1)
                {
                    characterPrefabIndex = int.Parse(extractedString[i]);
                    isApprove = true;
                }
            }
        }

        else
        {
            //server
            if (NetworkManager.Singleton.IsHost)
            {
                string characterId = setInputSkinData().ToString();
                characterPrefabIndex = int.Parse(characterId);
            }
            else
            {
                string characterId = setInputSkinData().ToString();
                characterPrefabIndex = int.Parse(characterId);
            }
        }
        // Your approval logic determines the following values
        response.Approved = isApprove;
        response.CreatePlayerObject = true;

        // The Prefab hash value of the NetworkPrefab, if null the default NetworkManager player Prefab is used
        //response.PlayerPrefabHash = null;

        response.PlayerPrefabHash = AlternativePlayerPrefabs[characterPrefabIndex];


        // Position to spawn the player object (if null it uses default of Vector3.zero)
        response.Position = Vector3.zero;

        // Rotation to spawn the player object (if null it uses the default of Quaternion.identity)

        response.Rotation = Quaternion.identity;

        SetSpawnLocation(clientId, response, characterPrefabIndex);
        NetworkLog.LogInfoServer("SpanwnPos of " + clientId + " is " + response.Position.ToString());
        NetworkLog.LogInfoServer("SpanwnRot of " + clientId + " is " + response.Rotation.ToString());

        // If response.Approved is false, you can provide a message that explains the reason why via ConnectionApprovalResponse.Reason
        // On the client-side, NetworkManager.DisconnectReason will be populated with this message via DisconnectReasonMessage
        response.Reason = "Some reason for not approving the client";

        // If additional approval steps are needed, set this to true until the additional steps are complete
        // once it transitions from true to false the connection approval response will be processed.
        response.Pending = false;
    }



    private void SetSpawnLocation(ulong clientId, NetworkManager.ConnectionApprovalResponse response, int characterPrefabIndex)
    {
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;
        //server
        if(clientId == NetworkManager.Singleton.LocalClientId)
        {
            int countOfAllPos = posList.Length;
            if (characterPrefabIndex == 0)
            {
                spawnPos = new Vector3(posList[0].position.x, posList[0].position.y, posList[0].position.z);
                spawnRot = Quaternion.Euler(posList[0].eulerAngles.x, posList[0].eulerAngles.y, posList[0].eulerAngles.z);
            }
            else if (characterPrefabIndex == 1)
            {
                spawnPos = new Vector3(posList[1].position.x, posList[1].position.y, posList[1].position.z);
                spawnRot = Quaternion.Euler(posList[1].eulerAngles.x, posList[1].eulerAngles.y, posList[1].eulerAngles.z);
            }
        }
        else
        {
            if (characterPrefabIndex == 0)
            {
                spawnPos = new Vector3(posList[0].position.x, posList[0].position.y, posList[0].position.z);
                spawnRot = Quaternion.Euler(posList[0].eulerAngles.x, posList[0].eulerAngles.y, posList[0].eulerAngles.z);
            }
            else if (characterPrefabIndex == 1)
            {
                spawnPos = new Vector3(posList[1].position.x, posList[1].position.y, posList[1].position.z);
                spawnRot = Quaternion.Euler(posList[1].eulerAngles.x, posList[1].eulerAngles.y, posList[1].eulerAngles.z);
            }
        }
        response.Position = spawnPos;
        response.Rotation = spawnRot;
    }
    public async void Client()
    {
        /*joinCode = joinCodeInputField.GetComponent<TMP_InputField>().text;
        if (RelayManagerScript.Instance.IsRelayEnabled && !string.IsNullOrEmpty(joinCode))
        {
            await RelayManagerScript.Instance.JoinRelay(joinCode);
        }*/
        string username = lobbyScript.playerName;
        string characterId = setInputSkinData().ToString();
        Debug.Log(characterId);
        string[] inputFields = { username, characterId };
        string clientData = HelperScript.CombineStrings(inputFields);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(clientData);
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.StartClient();

        Debug.Log("Start client");
    }

    [ServerRpc]
    public void StarGroundMoveServerRPC()
    {
        isTwoPlayerSpawning = true;
    }

    [ServerRpc]
    public void StopGroundMoveServerRPC()
    {
        isTwoPlayerSpawning = false;
    }

    public bool NameApproveConnection(string clientData, string hostData)
    {
        bool isApprove = System.String.Equals(clientData.Trim(), hostData.Trim()) ? false : true;
        Debug.Log("NameIsApprove = " + isApprove);
        return isApprove;
    }

    public int setInputSkinData()
    {

        if (lobbyScript.playerCharacter == "Dino")
        {
            return 0;
        }
        if (lobbyScript.playerCharacter == "God")
        {
            return 1;
        }
        return 0;
    }
    private void FixedUpdate()
    {
        playerList = GameObject.FindGameObjectsWithTag("Player");
        if (IsHost)
        {
            if (playerList.Length <= 0)
            {
                GameManager.Instance.gameStart = false;
                isTwoPlayerSpawning = false;

                dinoUI.SetActive(false);
                godUI.SetActive(false);
                SetUIVisible(false);

                StopGroundMoveServerRPC();

            }
            else if (playerList.Length == 2)
            {
                StarGroundMoveServerRPC();
            }
        }
    }
}
