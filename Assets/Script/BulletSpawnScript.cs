using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using UnityEngine.UI;


public class BulletSpawnScript : NetworkBehaviour
{
    public GameObject bulletPrefab;
    private List<GameObject> spawnedBullet = new List<GameObject>();
    public Button skill_1;

    private void FixedUpdate()
    {
        skill_1 = Resources.FindObjectsOfTypeAll<Button>().FirstOrDefault(g => g.CompareTag("Button_1"));
       
    }
    
    void Start()
    {
        skill_1 = Resources.FindObjectsOfTypeAll<Button>().FirstOrDefault(g => g.CompareTag("Button_1"));
        skill_1.onClick.AddListener(SpawnBulletServerRpc);
    }

    [ServerRpc]
    public void SpawnBulletServerRpc()
    {
        Vector3 spawnPos = new Vector3(transform.position.x-1, transform.position.y, transform.position.z);
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
