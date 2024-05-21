using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MeteorScript : NetworkBehaviour
{
    public ObstacleSpawn obstacleSpawn;
    public GameObject effectFirePrefab;
    public float destroyDelay = 3f;

    private void Start()
    {
        if (!IsOwner) return;
        SpawnEffect();
        StartCoroutine(DestroyBulletDelay());
    }

    private void Update()
    {
        //transform.position += transform.forward * GameManager.Instance.gameSpeed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;
        if (collision.gameObject.tag == "Player")
        {
            DestroyObstacleServerRpc();

        }

        if (collision.gameObject.tag == "Ground")
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
    IEnumerator DestroyBulletDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        DestroyObstacleServerRpc();
    }
}
