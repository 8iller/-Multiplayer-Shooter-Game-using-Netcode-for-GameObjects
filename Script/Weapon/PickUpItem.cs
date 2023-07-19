using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PickUpItem : NetworkBehaviour
{   
    public Weapon weapon;
    public NetworkVariable<int> LoadedAmmo = new NetworkVariable<int>(20, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> AmmoLeft = new NetworkVariable<int>(300, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

   

    private void OnTriggerEnter(Collider other)
    {    
        if(!IsServer)return;
        ShooterController shooterController = other.GetComponentInParent<ShooterController>();
        if(shooterController != null)
        {   

            if(!shooterController.GunSlotEmpty.Value)return;
            shooterController.weapon = weapon;
            SetWeaponDataClientRpc(shooterController.GetNetworkObject());
            shooterController.SpawnWeaponClientRpc();
            try{
                gameObject.GetComponent<NetworkObject>().Despawn();

            }
            catch { 

               
            }
        }


    }

    [ClientRpc]
    void SetWeaponDataClientRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        ShooterController shooterController = networkObject.GetComponent<ShooterController>();
        shooterController.EquipWeapon(weapon,LoadedAmmo.Value,AmmoLeft.Value);
        shooterController.BulletSound.clip = weapon.BulletSound;

    }







}
