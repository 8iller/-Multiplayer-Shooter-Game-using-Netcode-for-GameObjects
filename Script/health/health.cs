using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Spawner;
using Unity.Collections;

public class health : NetworkBehaviour
{       

    [SerializeField] TextMeshProUGUI healthUi;
    [SerializeField]RandomSpawner randomSpawner;
    [SerializeField]TMP_Text killMessageText;
    [SerializeField] TMP_Text  killFeedText;
    [SerializeField] GameObject killFeedBoard;
    public int ygn = 04;
    public static bool OnDisplay = false;
    ShooterController shooterController ;
    [SerializeField] NetworkVariable<int> Health = new NetworkVariable<int>(100,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    
   
    public override  void OnNetworkSpawn()
    {

        
        shooterController = GetComponent<ShooterController>();
        if(!IsOwner)return;
        randomSpawner.SpawnRandomObject(gameObject);
        


    }
    private void Start() {
        killFeedBoard = GameObject.FindGameObjectWithTag("killFeed");
        
        OnDisplay = false;
    }

    void Update()
    {   
        if(!IsOwner)return;
        
        if(Health.Value <= 0)
        {
            healthUi.text = "0";
        }
        else {
            healthUi.text = Health.Value.ToString();
        }
       
    }

 
   

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage,NetworkBehaviourReference networkBehaviourReference)
    {   
        Health.Value -= damage;
        if(Health.Value <= 0)
        {   
            if(shooterController.IsDead.Value == true)return;
            shooterController.IsDead.Value = true;
            shooterController.DEATH.Value += 1;
            networkBehaviourReference.TryGet(out ShooterController TargetShooterController);
            TargetShooterController.KILLS.Value += 1;
            StartCoroutine(OnDead());
            DisplayKillFeedClientRpc(TargetShooterController.username.Value,shooterController.username.Value);
            ShowKillMessageClientRpc(TargetShooterController.username.Value,new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { NetworkObject.OwnerClientId } } });
           
        }

    }

    [ClientRpc]
   void DisplayKillFeedClientRpc(FixedString32Bytes KilledUsername , FixedString32Bytes DeadUsername)
   {
        TMP_Text killMessage = Instantiate(killFeedText, killFeedBoard.transform);
        killMessage.text = KilledUsername+" KILLED "+DeadUsername;
   }




    IEnumerator OnDead()
    {   
        yield return new WaitForSeconds (2f);
        shooterController.IsDead.Value = false;
        Health.Value = 100;

    }
    [ClientRpc]
    public void ShowKillMessageClientRpc(FixedString32Bytes username,ClientRpcParams clientRpcParams)
    {
        if(!IsOwner)return;
        randomSpawner.SpawnRandomObject(gameObject);
        StartCoroutine(DisplayKillMessage(username));
        
    } 
    
    
    IEnumerator DisplayKillMessage(FixedString32Bytes username)
    {   
        killMessageText.gameObject.SetActive(true);
        killMessageText.text = "KILLED BY "+username;
        yield return new WaitForSeconds (2f);
        killMessageText.gameObject.SetActive(false);

    }
}

