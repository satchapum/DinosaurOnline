using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class TimeCount : NetworkBehaviour
{
    [SerializeField] TMP_Text timeText;
    [SerializeField] float currentTime = 0;

    [SerializeField] float timeToChangeSpeed = 5;
    private void Update()
    {
        if (IsHost)
        {
            if (GameManager.Instance.gameStart == true)
            {
                timeText.text = "Time : " + (int)currentTime;
                currentTime += Time.deltaTime;
            }

            if (currentTime >= timeToChangeSpeed)
            {
                GameManager.Instance.gameSpeed += 5;
                timeToChangeSpeed = timeToChangeSpeed + 2;
            }
        }
        
    }

    public void ResetTime()
    {
        currentTime = 0;
    }
}
