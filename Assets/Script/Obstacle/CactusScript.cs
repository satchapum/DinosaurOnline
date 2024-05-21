using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CactusScript : NetworkBehaviour
{
    public ObstacleSpawn obstacleSpawn;
    public GameObject effectFirePrefab;

    private void Start()
    {
        if (!IsOwner) return;
        SpawnEffect();
    }

    private void Update()
    {
        transform.position += transform.forward * GameManager.Instance.gameSpeed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            DestroyObstacleServerRpc();
        }

        if (collision.gameObject.tag == "DeleteZone")
        {
            DestroyObstacleServerRpc();
        }

        if (collision.gameObject.tag == "Cactus")
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
