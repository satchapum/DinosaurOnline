using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] public float gameSpeed;
    [SerializeField] public int dinoHealth;

    [SerializeField] LoginManagerScript loginScript;

    [SerializeField] public bool gameStart = false;
    [SerializeField] TimeCount timeCount;
    public float character;
    private void Update()
    {
        if (loginScript.isTwoPlayerSpawning == true && timeCount.currentTime >= 0)
        {
            gameStart = true;
        }
        else if(timeCount.currentTime <= 0)
        {
            gameStart = false;
            gameSpeed = 0;
        }
    }
}
