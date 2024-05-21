using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Netcode.Components;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using System.Linq;

public class HPPlayerScript : NetworkBehaviour
{
    private OwnerNetworkAnimationScript ownerNetworkAnimationScript;

    MainPlayerScript mainPlayer;
    public NetworkVariable<int> hpDino = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private Image health_1;
    private Image health_2;
    private Image health_3;
    private PlayerControllerScript playerControllerScript;
    public float characterNumber;

    [SerializeField] LoginManagerScript loginManagerScript;
    

    void Start()
    {
        if (!IsOwner) return;
        characterNumber = gameObject.GetComponent<OpenUI>().characterNumber;
        playerControllerScript = gameObject.GetComponent<PlayerControllerScript>();
        hpDino.Value = GameManager.Instance.dinoHealth;

        loginManagerScript = Resources.FindObjectsOfTypeAll<LoginManagerScript>().FirstOrDefault(g => g.CompareTag("LoginManager"));

        ownerNetworkAnimationScript = GetComponent<OwnerNetworkAnimationScript>();
        mainPlayer = GetComponent<MainPlayerScript>();

        health_1 = Resources.FindObjectsOfTypeAll<Image>().FirstOrDefault(g => g.CompareTag("Health_1"));
        health_2 = Resources.FindObjectsOfTypeAll<Image>().FirstOrDefault(g => g.CompareTag("Health_2"));
        health_3 = Resources.FindObjectsOfTypeAll<Image>().FirstOrDefault(g => g.CompareTag("Health_3"));
    }

    private void UpdatePlayerNameAndScore()
    {
        //Debug.Log("Update");
        if (IsOwner)
        {
            if (characterNumber == 0)
            {
                Debug.Log("changeColorhost");
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
            else if (characterNumber == 1)
            {
                Debug.Log("changeColorclient");
                if (hpDino.Value == 3)
                {
                    ChangeColorClientRpc(Color.red, Color.red, Color.red);
                }
                else if (hpDino.Value == 2)
                {
                    ChangeColorClientRpc(Color.red, Color.red, Color.white);
                }
                else if (hpDino.Value == 1)
                {
                    ChangeColorClientRpc(Color.red, Color.white, Color.white);
                }
                else if (hpDino.Value == 0)
                {
                    ChangeColorClientRpc(Color.white, Color.white, Color.white);
                }
            }
        }
    }
    private void ChangeColor(Color color_1, Color color_2, Color color_3)
    {
        health_1.color = color_1;
        health_2.color = color_2;
        health_3.color = color_3;
    }
    [ClientRpc]
    private void ChangeColorClientRpc(Color color_1, Color color_2, Color color_3)
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

    [ServerRpc]
    public void UpdateScoreServerRpc()
    {
        /*if (hpDino.Value == 0 && IsOwnedByServer)
        {
            ownerNetworkAnimationScript.SetTrigger("Die");
            loginManagerScript.Leave();
            //hpP1.Value = 5;
            //gameObject.GetComponent<PlayerSpawnerScript>().Respawn();
        }*/
        if (hpDino.Value == 0)
        {
            //ownerNetworkAnimationScript.SetTrigger("Die");
            loginManagerScript.Leave();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.tag);
        if (other.gameObject.tag == "Pond")
        {
            if (IsOwnedByServer)
            {
                playerControllerScript.playerGetDelayServerRpc();
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        Debug.Log(other.gameObject.tag);
        if (other.gameObject.tag == "Pond")
        {
            if (IsOwnedByServer)
            {
                playerControllerScript.playerDontGetDelayServerRpc();
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        //if (!IsLocalPlayer) return;
        Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.tag == "Cactus")
        {
            hpDino.Value--;
        }
        if (collision.gameObject.tag == "Bullet")
        {
            hpDino.Value--;
        }

        UpdateScoreServerRpc();

    }
}
