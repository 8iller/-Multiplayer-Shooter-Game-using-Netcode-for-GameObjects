using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class count : NetworkBehaviour
{
    [SerializeField] private ShooterController shooterController;
    private TextMeshProUGUI TM;

    public override void OnNetworkSpawn()
    {
        TM = GetComponent<TextMeshProUGUI>();
        shooterController = GetComponentInParent<ShooterController>();
    }

    void Update()
    {
       
        if (IsLocalPlayer)
        {
            if (shooterController != null )
                    
                TM.text = shooterController.LoadedAmmo.ToString() + " | " + shooterController.AmmoLeft.ToString();
                
            else 
            {

                TM.text = "";

            }    
        }

        
    }   


}
