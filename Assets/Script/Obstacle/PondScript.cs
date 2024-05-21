using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PondScript : NetworkBehaviour
{
    
    public ObstacleSpawn obstacleSpawn;
    public GameObject effectFirePrefab;

    [SerializeField] float slowTime = 2;

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
        }

    }
    private void DestroyObstacle()
    {
        ulong networkObjId = GetComponent<NetworkObject>().NetworkObjectId;
        Debug.Log("destroy = : " + networkObjId);
        obstacleSpawn.DestroyPondServerRpc(networkObjId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void DestroyObstacleServerRpc()
    {
        ulong networkObjId = GetComponent<NetworkObject>().NetworkObjectId;
        Debug.Log("destroy = : " + networkObjId);
        obstacleSpawn.DestroyPondServerRpc(networkObjId);
    }
    private void SpawnEffect()
    {
        GameObject effect = Instantiate(effectFirePrefab, transform.position, Quaternion.identity);
        effect.GetComponent<NetworkObject>().Spawn();
    }
}
