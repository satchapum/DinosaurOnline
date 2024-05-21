using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PondScript : NetworkBehaviour
{
    
    public ObstacleSpawn obstacleSpawn;
    public GameObject effectFirePrefab;
    public PlayerControllerScript playerControllerScript;
    public float characterNumber;
    [SerializeField] float slowTime = 2;

    private void Start()
    {
        playerControllerScript = gameObject.GetComponent<PlayerControllerScript>();
        if (!IsOwner) return;
        SpawnEffect();
    }

    private void Update()
    {
        transform.position += transform.forward * GameManager.Instance.gameSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!IsOwner) return;
        if (collision.gameObject.tag == "DeleteZone")
        {
            DestroyObstacleServerRpc();
        }
    }
    [ServerRpc]
    private void DestroyObstacleServerRpc()
    {
        if (!IsOwner) return;
        ulong networkObjId = GetComponent<NetworkObject>().NetworkObjectId;
        obstacleSpawn.DestroyCactusServerRpc(networkObjId);
    }
    private void SpawnEffect()
    {
        GameObject effect = Instantiate(effectFirePrefab, transform.position, Quaternion.identity);
        effect.GetComponent<NetworkObject>().Spawn();
    }
}
