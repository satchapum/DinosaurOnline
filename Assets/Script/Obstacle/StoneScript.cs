using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class StoneScript : NetworkBehaviour
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
        if (!IsOwner) return;
        if (collision.gameObject.tag == "Player")
        {
            ulong networkObjId = GetComponent<NetworkObject>().NetworkObjectId;
            obstacleSpawn.DestroyStoneServerRpc(networkObjId);
        }

        if (collision.gameObject.tag == "DeleteZone")
        {
            ulong networkObjId = GetComponent<NetworkObject>().NetworkObjectId;
            obstacleSpawn.DestroyStoneServerRpc(networkObjId);
        }
    }
    private void SpawnEffect()
    {
        GameObject effect = Instantiate(effectFirePrefab, transform.position, Quaternion.identity);
        effect.GetComponent<NetworkObject>().Spawn();
    }
}
