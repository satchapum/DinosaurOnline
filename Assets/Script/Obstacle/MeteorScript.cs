using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MeteorScript : NetworkBehaviour
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
        //transform.position += transform.forward * GameManager.Instance.gameSpeed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (IsHost)
        {
            if (collision.gameObject.tag == "Player")
            {
                Debug.Log(1);
                DestroyObstacle();
            }

            if (collision.gameObject.tag == "Ground")
            {
                Debug.Log(2);
                DestroyObstacle();
            }
        }
        else if (IsClient)
        {
            if (collision.gameObject.tag == "Player")
            {
                Debug.Log(3);
                DestroyObstacleServerRpc();

            }

            if (collision.gameObject.tag == "Ground")
            {
                Debug.Log(4);
                DestroyObstacleServerRpc();
            }
        }

    }
    private void DestroyObstacle()
    {
        ulong networkObjId = GetComponent<NetworkObject>().NetworkObjectId;
        Debug.Log("destroy = : " + networkObjId);
        obstacleSpawn.DestroyMeteorServerRpc(networkObjId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void DestroyObstacleServerRpc()
    {
        ulong networkObjId = GetComponent<NetworkObject>().NetworkObjectId;
        Debug.Log("destroy = : " + networkObjId);
        obstacleSpawn.DestroyMeteorServerRpc(networkObjId);
    }


    private void SpawnEffect()
    {
        GameObject effect = Instantiate(effectFirePrefab, transform.position, Quaternion.identity);
        effect.GetComponent<NetworkObject>().Spawn();
    }
}
