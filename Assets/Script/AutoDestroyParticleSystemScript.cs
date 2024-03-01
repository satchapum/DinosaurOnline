using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AutoDestroyParticleSystemScript : NetworkBehaviour
{
    public float delayBeforeDestroy = 2f;
    private ParticleSystem ps;

    public void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }
    public void Update()
    {
        if (!IsOwner) return;

        if(ps && !ps.IsAlive())
        {
            DestroyObject();
        }
    }
    void DestroyObject()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject, delayBeforeDestroy);
    }
}
