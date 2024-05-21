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
        obstacleSpawn = GameObject.FindAnyObjectByType<ObstacleSpawn>();
        //SpawnEffect();
    }

    private void Update()
    {
        transform.position += transform.forward * GameManager.Instance.gameSpeed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (IsHost)
        {
            if (collision.gameObject.tag == "Player")
            {
                DestroyObstacle();
            }

            if (collision.gameObject.tag == "DeleteZone")
            {
                DestroyObstacle();
            }

            if (collision.gameObject.tag == "Cactus")
            {
                DestroyObstacle();
            }
        }
        if (IsClient)
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

    }
    private void DestroyObstacle()
    {
        ulong networkObjId = GetComponent<NetworkObject>().NetworkObjectId;
        Debug.Log("destroy = : " + networkObjId);
        obstacleSpawn.DestroyCactusServerRpc(networkObjId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void DestroyObstacleServerRpc()
    {
        ulong networkObjId = GetComponent<NetworkObject>().NetworkObjectId;
        Debug.Log("destroy = : " + networkObjId);
        obstacleSpawn.DestroyCactusServerRpc(networkObjId);
    }

    private void SpawnEffect()
    {
        GameObject effect = Instantiate(effectFirePrefab, transform.position, Quaternion.identity);
        effect.GetComponent<NetworkObject>().Spawn();
    }
}
