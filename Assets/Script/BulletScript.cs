using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BulletScript : NetworkBehaviour
{
    public BulletSpawnScript bulletSpawner;
    public GameObject effectFirePrefab;
    public float speed = 10f;
    public float destroyDelay = 3f;

    private void Start()
    {
        if (!IsOwner) return;
        SpawnEffect();
        StartCoroutine(DestroyBulletDelay());
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;
        if (collision.gameObject.tag == "Player")
        {
            StopCoroutine(DestroyBulletDelay());
            ulong networkObjId = GetComponent<NetworkObject>().NetworkObjectId;
            bulletSpawner.DestroyServerRpc(networkObjId);

        }
    }
    private void SpawnEffect()
    {
        GameObject effect = Instantiate(effectFirePrefab, transform.position, Quaternion.identity);
        effect.GetComponent<NetworkObject>().Spawn();
    }

    IEnumerator DestroyBulletDelay()
    {
        ulong networkObjId = GetComponent<NetworkObject>().NetworkObjectId;
        yield return new WaitForSeconds(destroyDelay);
        bulletSpawner.DestroyServerRpc(networkObjId);
    }
}
