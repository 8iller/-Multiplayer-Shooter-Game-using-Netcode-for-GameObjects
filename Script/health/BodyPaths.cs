using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class BodyPaths : NetworkBehaviour
{
    enum BodyType { Body, Head, Leg, Hand }

    public int tess = 1;
    [SerializeField] BodyType bodyType;
    public health health;

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            
            gameObject.layer = 6;
        }
    }

    public void BodyDamage(int LegDamage, int BodyDamage, int HeadDamage, int HandDamage,NetworkBehaviourReference networkBehaviourReference)
    {   
        
        if (bodyType == BodyType.Body)
        {
            health.TakeDamageServerRpc(BodyDamage,networkBehaviourReference);
        }
        else if (bodyType == BodyType.Head)
        {
            health.TakeDamageServerRpc(HeadDamage,networkBehaviourReference);
        }
        else if (bodyType == BodyType.Leg)
        {
            health.TakeDamageServerRpc(LegDamage,networkBehaviourReference);
        }
        else if (bodyType == BodyType.Hand)
        {
            health.TakeDamageServerRpc(HandDamage,networkBehaviourReference);
        }
    }

}
