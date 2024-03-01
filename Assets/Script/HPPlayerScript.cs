using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Netcode.Components;
using UnityEngine.SocialPlatforms.Impl;

public class HPPlayerScript : NetworkBehaviour
{
    private OwnerNetworkAnimationScript ownerNetworkAnimationScript;

    TMP_Text p1Text;
    TMP_Text p2Text;
    MainPlayerScript mainPlayer;
    public NetworkVariable<int> hpP1 = new NetworkVariable<int>(5,
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> hpP2 = new NetworkVariable<int>(5,
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // Start is called before the first frame update
    void Start()
    {
        p1Text = GameObject.Find("player_1Text").GetComponent<TMP_Text>();
        p2Text = GameObject.Find("player_2Text").GetComponent<TMP_Text>();
        ownerNetworkAnimationScript = GetComponent<OwnerNetworkAnimationScript>();
        mainPlayer = GetComponent<MainPlayerScript>();
    }

    private void UpdatePlayerNameAndScore()
    {
        if (IsOwnedByServer)
        {
            p1Text.text = $"{mainPlayer.playerNameA.Value} : {hpP1.Value}";
        }
        else
        {
            p2Text.text = $"{mainPlayer.playerNameB.Value} : {hpP2.Value}";
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerNameAndScore();

    }

    public void UpdateScore()
    {
        if (hpP1.Value == 0 && IsOwnedByServer)
        {
            ownerNetworkAnimationScript.SetTrigger("Die");
            hpP1.Value = 5;
            gameObject.GetComponent<PlayerSpawnerScript>().Respawn();

        }
        else if (hpP2.Value == 0 && IsClient)
        {
            ownerNetworkAnimationScript.SetTrigger("Die");
            hpP2.Value = 5;
            gameObject.GetComponent<PlayerSpawnerScript>().Respawn();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsLocalPlayer) return;

        if (collision.gameObject.tag == "DeathZone")
        {
            if (IsOwnedByServer)
            {
                hpP1.Value--;
            }
            else
            {
                hpP2.Value--;
            }
            gameObject.GetComponent<PlayerSpawnerScript>().Respawn();
        }
        if (collision.gameObject.tag == "Bomb")
        {
            if (IsOwnedByServer)
            {
                hpP1.Value--;
            }
            else
            {
                hpP2.Value--;
            }
        }
        if (collision.gameObject.tag == "Bullet")
        {
            if (IsOwnedByServer)
            {
                hpP1.Value--;
            }
            else
            {
                hpP2.Value--;
            }
        }
        UpdateScore();

    }
}
