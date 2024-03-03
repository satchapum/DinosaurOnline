using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class OpenUI : NetworkBehaviour
{
    [SerializeField] GameObject dinoUI;
    [SerializeField] GameObject godUI;
    [SerializeField] GameObject healthUI;

    [SerializeField] int characterNumber;

    private void Update()
    {
        dinoUI = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.CompareTag("dinoUI"));
        godUI = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.CompareTag("godUI"));
        healthUI = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.CompareTag("healthUI"));

    }

    private void FixedUpdate()
    {
        if (IsOwner && healthUI != null && godUI != null && dinoUI != null)
        {
            if (characterNumber == 0)
            {
                dinoUI.gameObject.SetActive(true);
                godUI.gameObject.SetActive(false);
                healthUI.gameObject.SetActive(true);
            }
            else
            {
                dinoUI.gameObject.SetActive(false);
                godUI.gameObject.SetActive(true);
                healthUI.gameObject.SetActive(false);
            }
        }
    }
}
