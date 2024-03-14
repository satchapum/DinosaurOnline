using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using UnityEngine.UI;


public class ObstacleSpawn : NetworkBehaviour
{
    [Header("ObstaclePrefab")]
    public GameObject meteorPrefab;
    public GameObject birdPrefab;
    public GameObject stonePrefab;
    public GameObject cactusPrefab;
    public GameObject pondPrefab;

    [Header("ListOfItem")]
    private List<GameObject> spawnedBird = new List<GameObject>();
    private List<GameObject> spawnedStone = new List<GameObject>();
    private List<GameObject> spawnedCactus = new List<GameObject>();
    private List<GameObject> spawnedPond = new List<GameObject>();
    private List<GameObject> spawnedMeteor = new List<GameObject>();

    public Button skill_1;
    public Button skill_2;
    public Button skill_3;
    public Button skill_4;
    public Button skill_5;

    private void FixedUpdate()
    {
        FindObject();
    }
    
    void Start()
    {
        FindObject();

        skill_1.onClick.AddListener(SpawnMeteoServerRpc);
        skill_2.onClick.AddListener(SpawnPondServerRpc);
        skill_3.onClick.AddListener(SpawnStoneServerRpc);
        skill_4.onClick.AddListener(SpawnCactusServerRpc);
        skill_5.onClick.AddListener(SpawnBirdServerRpc);
    }

    void FindObject()
    {
        skill_1 = Resources.FindObjectsOfTypeAll<Button>().FirstOrDefault(g => g.CompareTag("Button_1"));
        skill_2 = Resources.FindObjectsOfTypeAll<Button>().FirstOrDefault(g => g.CompareTag("Button_2"));
        skill_3 = Resources.FindObjectsOfTypeAll<Button>().FirstOrDefault(g => g.CompareTag("Button_3"));
        skill_4 = Resources.FindObjectsOfTypeAll<Button>().FirstOrDefault(g => g.CompareTag("Button_4"));
        skill_5 = Resources.FindObjectsOfTypeAll<Button>().FirstOrDefault(g => g.CompareTag("Button_5"));
    }

    [ServerRpc]
    public void SpawnMeteoServerRpc()
    {
        Vector3 spawnPos = new Vector3(transform.position.x - 4, transform.position.y, transform.position.z);
        Quaternion spawnRot = transform.rotation;
        GameObject meteor = Instantiate(meteorPrefab, spawnPos, spawnRot);
        meteor.GetComponent<NetworkObject>().Spawn();
        spawnedMeteor.Add(meteor);
        meteor.GetComponent<MeteorScript>().obstacleSpawn = this;
        
    }

    [ServerRpc]
    public void SpawnStoneServerRpc()
    {
        Vector3 spawnPos = new Vector3(transform.position.x - 4, transform.position.y, transform.position.z);
        Quaternion spawnRot = transform.rotation;
        GameObject stone = Instantiate(stonePrefab, spawnPos, spawnRot);
        stone.GetComponent<NetworkObject>().Spawn();
        spawnedStone.Add(stone);
        stone.GetComponent<StoneScript>().obstacleSpawn = this;

    }

    [ServerRpc]
    public void SpawnPondServerRpc()
    {
        Vector3 spawnPos = new Vector3(transform.position.x - 4, transform.position.y, transform.position.z);
        Quaternion spawnRot = transform.rotation;
        GameObject pond = Instantiate(pondPrefab, spawnPos, spawnRot);
        pond.GetComponent<NetworkObject>().Spawn();
        spawnedPond.Add(pond);
        pond.GetComponent<PondScript>().obstacleSpawn = this;

    }

    [ServerRpc]
    public void SpawnCactusServerRpc()
    {
        Vector3 spawnPos = new Vector3(transform.position.x - 4, transform.position.y, transform.position.z);
        Quaternion spawnRot = transform.rotation;
        GameObject cactus = Instantiate(cactusPrefab, spawnPos, spawnRot);
        cactus.GetComponent<NetworkObject>().Spawn();
        spawnedCactus.Add(cactus);
        cactus.GetComponent<CactusScript>().obstacleSpawn = this;

    }

    [ServerRpc]
    public void SpawnBirdServerRpc()
    {
        Vector3 spawnPos = new Vector3(transform.position.x - 4, transform.position.y, transform.position.z);
        Quaternion spawnRot = transform.rotation;
        GameObject bird = Instantiate(birdPrefab, spawnPos, spawnRot);
        bird.GetComponent<NetworkObject>().Spawn();
        spawnedBird.Add(bird);
        bird.GetComponent<BirdScript>().obstacleSpawn = this;

    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyMeteorServerRpc(ulong networkObjId)
    {
        GameObject obj = findSpawnerMeteor(networkObjId);
        if (obj == null) return;
        obj.GetComponent<NetworkObject>().Despawn();
        spawnedMeteor.Remove(obj);
        Destroy(obj);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyStoneServerRpc(ulong networkObjId)
    {
        GameObject obj = findSpawnerStone(networkObjId);
        if (obj == null) return;
        obj.GetComponent<NetworkObject>().Despawn();
        spawnedStone.Remove(obj);
        Destroy(obj);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyPondServerRpc(ulong networkObjId)
    {
        GameObject obj = findSpawnerPond(networkObjId);
        if (obj == null) return;
        obj.GetComponent<NetworkObject>().Despawn();
        spawnedPond.Remove(obj);
        Destroy(obj);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyCactusServerRpc(ulong networkObjId)
    {
        GameObject obj = findSpawnerCactus(networkObjId);
        if (obj == null) return;
        obj.GetComponent<NetworkObject>().Despawn();
        spawnedCactus.Remove(obj);
        Destroy(obj);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyBirdServerRpc(ulong networkObjId)
    {
        GameObject obj = findSpawnerBird(networkObjId);
        if (obj == null) return;
        obj.GetComponent<NetworkObject>().Despawn();
        spawnedBird.Remove(obj);
        Destroy(obj);
    }

    private GameObject findSpawnerMeteor(ulong netWorkObjId)
    {
        foreach (GameObject arrow in spawnedMeteor)
        {
            ulong arrowId = arrow.GetComponent<NetworkObject>().NetworkObjectId;
            if (arrowId == netWorkObjId) { return arrow; }
        }
        return null;
    }

    private GameObject findSpawnerStone(ulong netWorkObjId)
    {
        foreach (GameObject stone in spawnedStone)
        {
            ulong stoneId = stone.GetComponent<NetworkObject>().NetworkObjectId;
            if (stoneId == netWorkObjId) { return stone; }
        }
        return null;
    }

    private GameObject findSpawnerCactus(ulong netWorkObjId)
    {
        foreach (GameObject cactus in spawnedCactus)
        {
            ulong cactusId = cactus.GetComponent<NetworkObject>().NetworkObjectId;
            if (cactusId == netWorkObjId) { return cactus; }
        }
        return null;
    }

    private GameObject findSpawnerBird(ulong netWorkObjId)
    {
        foreach (GameObject bird in spawnedBird)
        {
            ulong birdId = bird.GetComponent<NetworkObject>().NetworkObjectId;
            if (birdId == netWorkObjId) { return bird; }
        }
        return null;
    }

    private GameObject findSpawnerPond(ulong netWorkObjId)
    {
        foreach (GameObject pond in spawnedPond)
        {
            ulong pondId = pond.GetComponent<NetworkObject>().NetworkObjectId;
            if (pondId == netWorkObjId) { return pond; }
        }
        return null;
    }
}
