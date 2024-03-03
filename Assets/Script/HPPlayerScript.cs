using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Netcode.Components;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class HPPlayerScript : NetworkBehaviour
{
    private OwnerNetworkAnimationScript ownerNetworkAnimationScript;

    MainPlayerScript mainPlayer;
    public NetworkVariable<int> hpDino = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private Image health_1;
    private Image health_2;
    private Image health_3;

    void Start()
    {
        hpDino.Value = GameManager.Instance.dinoHealth;

        ownerNetworkAnimationScript = GetComponent<OwnerNetworkAnimationScript>();
        mainPlayer = GetComponent<MainPlayerScript>();

        health_1 = GameObject.Find("Health_1").GetComponent<Image>();
        health_2 = GameObject.Find("Health_2").GetComponent<Image>();
        health_3 = GameObject.Find("Health_3").GetComponent<Image>();
       
    }

    private void UpdatePlayerNameAndScore()
    {
        if (IsOwnedByServer)
        {
            if (hpDino.Value == 3)
            {
                ChangeColor(Color.red, Color.red, Color.red);
            }
            else if (hpDino.Value == 2)
            {
                ChangeColor(Color.red, Color.red, Color.white);
            }
            else if (hpDino.Value == 1)
            {
                ChangeColor(Color.red, Color.white, Color.white);
            }
            else if (hpDino.Value == 0)
            {
                ChangeColor(Color.white, Color.white, Color.white);
            }
        }
    }

    private void ChangeColor(Color color_1, Color color_2, Color color_3)
    {
        health_1.color = color_1;
        health_2.color = color_2;
        health_3.color = color_3;
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerNameAndScore();

    }

    public void UpdateScore()
    {
        if (hpDino.Value == 0 && IsOwnedByServer)
        {
            ownerNetworkAnimationScript.SetTrigger("Die");
            //hpP1.Value = 5;
            //gameObject.GetComponent<PlayerSpawnerScript>().Respawn();

        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsLocalPlayer) return;

        if (collision.gameObject.tag == "Bomb")
        {
            if (IsOwnedByServer)
            {
                hpDino.Value--;
            }
        }
        if (collision.gameObject.tag == "Bullet")
        {
            if (IsOwnedByServer)
            {
                hpDino.Value--;
            }
        }
        UpdateScore();

    }
}
