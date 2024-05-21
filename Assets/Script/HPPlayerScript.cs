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
    public PlayerControllerScript playerControllerScript;
    public float characterNumber;

    [SerializeField] LoginManagerScript loginManagerScript;

    public bool IsImmune = false;
    public float immuneTime = 1;
    

    void Start()
    {
        characterNumber = gameObject.GetComponent<OpenUI>().characterNumber;
        playerControllerScript = gameObject.GetComponent<PlayerControllerScript>();
        

        loginManagerScript = Resources.FindObjectsOfTypeAll<LoginManagerScript>().FirstOrDefault(g => g.CompareTag("LoginManager"));

        ownerNetworkAnimationScript = GetComponent<OwnerNetworkAnimationScript>();
        mainPlayer = GetComponent<MainPlayerScript>();

        health_1 = Resources.FindObjectsOfTypeAll<Image>().FirstOrDefault(g => g.CompareTag("Health_1"));
        health_2 = Resources.FindObjectsOfTypeAll<Image>().FirstOrDefault(g => g.CompareTag("Health_2"));
        health_3 = Resources.FindObjectsOfTypeAll<Image>().FirstOrDefault(g => g.CompareTag("Health_3"));

        if (!IsOwner) return;
        hpDino.Value = GameManager.Instance.dinoHealth;
    }

    private void UpdatePlayerNameAndScore()
    {
        
        if (IsOwner)
        {
            if (characterNumber == 0)
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
            else if (characterNumber == 1)
            {
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

        if (characterNumber == 0)
        {
            if (other.gameObject.tag == "Pond")
            {
                playerControllerScript.playerGetDelay();

            }
        }
        if (characterNumber == 1)
        {
            if (other.gameObject.tag == "Pond")
            {
                playerControllerScript.playerGetDelayClientRpc();

            }
        }

    }
    private void OnTriggerExit(Collider other)
    {
        Debug.Log(other.gameObject.tag);
        if (characterNumber == 0)
        {
            if (other.gameObject.tag == "Pond")
            {
                playerControllerScript.playerDontGetDelay();

            }
        }
        if (characterNumber == 1)
        {
            if (other.gameObject.tag == "Pond")
            {
                playerControllerScript.playerDontGetDelayClientRpc();

            }
        }

    }
    private void OnCollisionEnter(Collision collision)
    {
        //if (!IsLocalPlayer) return;
        if (characterNumber == 0)
        {
            if (collision.gameObject.tag == "Cactus")
            {
                hpDino.Value--;

            }
            if (collision.gameObject.tag == "Bullet")
            {
                hpDino.Value--;

            }
        }
        if (characterNumber == 1)
        {
            if (collision.gameObject.tag == "Cactus")
            {
                hpDino.Value--;

            }
            if (collision.gameObject.tag == "Bullet")
            {
                hpDino.Value--;

            }
        }
        
        

        UpdateScoreServerRpc();

    }
}
