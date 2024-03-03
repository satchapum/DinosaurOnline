using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] public float gameSpeed;
    [SerializeField] public int dinoHealth;

    [SerializeField] LoginManagerScript loginScript;

    [SerializeField] public bool gameStart = false;
    private void Update()
    {
        if (loginScript.isTwoPlayerSpawning == true && gameStart == false)
        {
            gameStart = true;
        }
        else
        {
            gameStart = false;
        }
    }
}
