using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using System;
using TMPro;

public class StatisticsTab : NetworkBehaviour
{
    [SerializeField] GameObject tab;
    [SerializeField] GameObject PlayerData;
    [SerializeField] Transform container;
    ShooterController[] TargetShooterControllers;


  
    void Update()
    {   
    
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            tab.SetActive(true);
            GetStatics();
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            tab.SetActive(false);
            DestroyChildObjects(container);
            Array.Clear(TargetShooterControllers, 0, TargetShooterControllers.Length);
        }
    }

    private void GetStatics()
    {
        TargetShooterControllers = FindObjectsOfType<ShooterController>();
        foreach (ShooterController TargetShooterController in TargetShooterControllers)
        {   
            GameObject playerInfo = Instantiate(PlayerData, container);
            FixedString32Bytes username = TargetShooterController.username.Value;
            int KILLS = TargetShooterController.KILLS.Value;
            int DEATH = TargetShooterController.DEATH.Value;
            TMPro.TextMeshProUGUI[] usernameTexts = playerInfo.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            foreach (TMPro.TextMeshProUGUI usernameText in usernameTexts)
            {
                if (usernameText.gameObject.name == "username")
                {
                    usernameText.text = username.Value;
                }
                else if (usernameText.gameObject.name == "DeadText")
                {
                    usernameText.text = DEATH.ToString();

                }
                else if (usernameText.gameObject.name == "KillText")
                {

                    usernameText.text = KILLS.ToString();
                }

            }

           


            
        }
    }

    private void DestroyChildObjects(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name == "scoreboardNameitems")
            {
                continue; // Skip destroying the "scoreboard Nameitems" object
            }

            GameObject.Destroy(child.gameObject);
        }
    }
    
}   
