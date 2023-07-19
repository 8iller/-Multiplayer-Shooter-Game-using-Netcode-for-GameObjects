using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class Destroy : NetworkBehaviour 
    

{

    [SerializeField] float DestoryTime = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
       
       if (!IsOwner)return;
        DespawnServerRpc();
        
        
    }

    [ServerRpc]
    public void DespawnServerRpc()
    {

        StartCoroutine(Remove(DestoryTime));

    }


     IEnumerator Remove( float delay)
    {   
        yield return new WaitForSeconds(delay);
        gameObject.GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
        
    }


}
