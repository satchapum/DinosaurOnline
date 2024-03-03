using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class TimeCount : NetworkBehaviour
{
    [SerializeField] TMP_Text timeText;
    [SerializeField] int currentTime = 0;
    private void Update()
    {
        if (IsOwnedByServer)
        {
            if (GameManager.Instance.gameStart == true)
            {
                timeText.text = "Time : " + currentTime;
                currentTime = currentTime + (int)Time.deltaTime;
            }
        }
        
    }

    private void ResetTime()
    {
        currentTime = 0;
    }
}
