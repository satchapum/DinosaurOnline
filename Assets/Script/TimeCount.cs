using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class TimeCount : NetworkBehaviour
{
    [SerializeField] TMP_Text timeText;
    [SerializeField] float timeToCount = 120;
    [SerializeField] public float currentTime = 120;
    [SerializeField] float speedToChange = 5;

    [SerializeField] float timeToChangeSpeed = 90;
    [SerializeField] LoginManagerScript loginManager;

    private void Start()
    {
        currentTime = timeToCount;
    }

    private void Update()
    {
        if (IsHost)
        {
            if (currentTime <= 0)
            {
                GameManager.Instance.gameStart = false;
                loginManager.Leave();
            }

            if (GameManager.Instance.gameStart == true)
            {
                timeText.text = "Time : " + (int)currentTime;
                currentTime -= Time.deltaTime;
            }

            if (currentTime <= timeToChangeSpeed)
            {
                GameManager.Instance.gameSpeed += speedToChange;
                timeToChangeSpeed = timeToChangeSpeed - 15;
            }
        }
        
    }

    public void ResetTime()
    {
        currentTime = timeToCount;
        timeToChangeSpeed = 90;
        timeText.text = "Time : " + (int)currentTime;

    }
}
