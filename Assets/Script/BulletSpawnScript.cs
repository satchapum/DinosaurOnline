using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BulletSpawnScript : NetworkBehaviour
{
    public GameObject bulletPrefab;
    private List<GameObject> spawnedBullet = new List<GameObject>();

    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            SpawnBulletServerRpc();
        }
    }

    [ServerRpc]
    void SpawnBulletServerRpc()
    {
        Vector3 spawnPos = transform.position + (transform.forward * 1.5f) + (transform.up * 1.5f);
        Quaternion spawnRot = transform.rotation;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, spawnRot);
        bullet.GetComponent<NetworkObject>().Spawn();
        spawnedBullet.Add(bullet);
        bullet.GetComponent<BulletScript>().bulletSpawner = this;
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyServerRpc(ulong networkObjId)
    {
        GameObject obj = findSpawnerBullet(networkObjId);
        if (obj == null) return;
        obj.GetComponent<NetworkObject>().Despawn();
        spawnedBullet.Remove(obj);
        Destroy(obj);
    }

    private GameObject findSpawnerBullet(ulong netWorkObjId)
    {
        foreach (GameObject bullet in spawnedBullet)
        {
            ulong bulletId = bullet.GetComponent<NetworkObject>().NetworkObjectId;
            if (bulletId == netWorkObjId) { return bullet; }
        }
        return null;
    }
}
