using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using Unity.Collections;

public class MainPlayerScript : NetworkBehaviour
{
    public Rigidbody rb;
    public TMP_Text namePrefab;
    private TMP_Text nameLabel;

    private LoginManagerScript loginManagerScript;
    private LobbyScript lobbyScript;

    private NetworkVariable<int> posX = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Button changeStatusButton;
    public NetworkVariable<bool> isRedMat = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    //ใช้ตอนเปลี่ยนสีโดนยิง
    public void ChangeMatColor()
    {
        if (IsOwner)
        {
            isRedMat.Value = !isRedMat.Value;
        }
    }

    public struct NetworkString : INetworkSerializable
    {
        public FixedString32Bytes info;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref info);
        }
        public override string ToString()
        {
            return info.ToString();
        }
        public static implicit operator NetworkString(string v) =>
            new NetworkString() { info = new FixedString32Bytes(v)};
    }

    public NetworkVariable<NetworkString> playerNameA = new NetworkVariable<NetworkString>(
        new NetworkString { info = "player" },
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<NetworkString> playerNameB = new NetworkVariable<NetworkString>(
        new NetworkString { info = "player" },
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    public override void OnNetworkSpawn()
    {
        GameObject canvas = GameObject.FindWithTag("MainCanvas");
        nameLabel = Instantiate(namePrefab, Vector3.zero, Quaternion.identity) as TMP_Text;

        //colorToChange.Add(colorToAdd[0]);
        //colorToChange.Add(colorToAdd[1]);

        nameLabel.transform.SetParent(canvas.transform);
        posX.OnValueChanged += (int previousValue, int newValue) =>
        {
            Debug.Log("Owner ID = " + OwnerClientId + " : pos X = " + posX.Value);
        };

        if (IsOwner)
        {
            loginManagerScript = GameObject.FindAnyObjectByType<LoginManagerScript>();
            lobbyScript = GameObject.FindObjectOfType<LobbyScript>();
            Debug.Log(lobbyScript.playerName);
            if (lobbyScript != null)
            {
                Debug.Log(lobbyScript.playerName);
                string name = lobbyScript.playerName;
                if (IsOwnedByServer) { playerNameA.Value = name; }
                else { playerNameB.Value = name; }
            }
        }
    }

    private void Update()
    {
        Vector3 nameLabelPos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 3.5f, 0));
        nameLabel.text = gameObject.name;
        nameLabel.transform.position = nameLabelPos;
        if (IsOwner)
        {
            posX.Value = (int)System.Math.Ceiling(transform.position.x);
            if (Input.GetKeyDown(KeyCode.K))
            {
                TestServerRpc("hello", new ServerRpcParams());
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                ClientRpcSendParams clientRpcSendParams = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 1} };
                ClientRpcParams clientRpcParams = new ClientRpcParams { Send = clientRpcSendParams };
                TestClientRpc("Hi , this is server", clientRpcParams);
            }
        }
        UpdatePlayerInfo();
        UpdatePlayerStatus();
    }

    [ClientRpc]
    private void TestClientRpc(string msg, ClientRpcParams clientRpcParams)
    {
        Debug.Log("testServer rpc from server = " + msg);
    }

    [ServerRpc]
    private void TestServerRpc(string msg, ServerRpcParams serverRpcParams)
    {
        Debug.Log("testServer rpc from client = " + OwnerClientId);
    }

    private void UpdatePlayerInfo()
    {
        if (IsOwnedByServer) { nameLabel.text = playerNameA.Value.ToString(); }
        else { nameLabel.text = playerNameB.Value.ToString(); }
    }

    private void UpdatePlayerStatus()
    {
        loginManagerScript = GameObject.FindAnyObjectByType<LoginManagerScript>();
        if (IsOwnedByServer) 
        {
            if (OwnerClientId == 0)
            {
                if (isRedMat.Value)
                {
                    gameObject.GetComponentInChildren<Renderer>().material = loginManagerScript.materialList[1];
                }
                else
                {
                    gameObject.GetComponentInChildren<Renderer>().material = loginManagerScript.materialList[0];
                }
            }
        }
        else 
        {
            if (isRedMat.Value)
            {
                gameObject.GetComponentInChildren<Renderer>().material = loginManagerScript.materialList[1];
            }
            else
            {
                gameObject.GetComponentInChildren<Renderer>().material = loginManagerScript.materialList[0];
            }
        }
    }

    private void OnDestroy()
    {
        if (nameLabel != null) Destroy(nameLabel.gameObject);
        base.OnDestroy();
    }

    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (nameLabel != null) 
        {
            nameLabel.enabled = true;
        }
    }

    private void OnDisable()
    {
        if (nameLabel != null)
        {
            nameLabel.enabled = false;
        }
    }
}
