using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class ShooterController : NetworkBehaviour
{   
    [SerializeField] private LayerMask aimCollider = new LayerMask();
    [SerializeField] Transform bulletSpawn;
    private float lastTimeFired = 0f;
    [SerializeField]Transform fps;
    [SerializeField] private LayerMask BulletHoles;
    public Weapon weapon;
    bool isReloading = false;
    public int LoadedAmmo = 30;
    public int AmmoLeft = 90;
    public NetworkVariable<bool> GunSlotEmpty = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] Transform GunDropPoint;
    [SerializeField] Transform GunSpawnPoint;
    public AudioSource BulletSound;
    public NetworkVariable<int> KILLS = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> IsDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> DEATH = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<FixedString32Bytes> username = new NetworkVariable<FixedString32Bytes>("username", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
     {
       if(IsServer)
       { 
         username.Value = "Player"+NetworkObject.OwnerClientId;
       }
       
     }

    private void Start() {
        BulletSound = GetComponent<AudioSource>();
    }
    private void Update()

    {   

        if (!IsOwner) return;
        if (IsDead.Value == true) return;
        if (weapon == null) return;
        DropGun();
        if (isReloading) return;
        Reload();
        Shooting();
    }

    private void DropGun()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            UnEquipWeapon();
        }
    }

    private void Shooting()
    {   
        if(weapon == null)return;
        if (weapon.NonAuto)
        {
            if (Input.GetMouseButtonDown(0) && Time.time >= lastTimeFired + (1 / weapon.FireRatePerSec))
            {
                PerformShooting();
            }
        }
        else
        {
            if (Input.GetMouseButton(0) && Time.time >= lastTimeFired + (1 / weapon.FireRatePerSec))
            {
                PerformShooting();
            }
        }
    }
   
    public void EquipWeapon(Weapon ItemWeapon,int ammoloaded , int ammoleft )
    {   
        weapon = ItemWeapon;
        LoadedAmmo = ammoloaded;
        AmmoLeft = ammoleft;
        if(!IsOwner)return;
        GunSlotEmpty.Value = false;

    }
    
    [ClientRpc]
    public void SpawnWeaponClientRpc()
    {
        Instantiate(weapon.WeaponPrefab,GunSpawnPoint);
    }

    void UnEquipWeapon()
    {
        SpawnPickItemServerRpc(AmmoLeft,LoadedAmmo);
        LoadedAmmo = 0;
        AmmoLeft = 0;
        GunSlotEmpty.Value = true;
        isReloading = false;
        StopCoroutine(DelayedReload(0.0f));

    }

 

    private void PerformShooting()
    {   
    
        if(LoadedAmmo == 0)return;
        
        if (Time.time >= lastTimeFired + (1 / weapon.FireRatePerSec))
        {
            lastTimeFired = Time.time;
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if (Physics.Raycast(fps.position,fps.forward, out RaycastHit raycastHit, 999f,aimCollider))
            {
                OnHitObject(raycastHit);
                LoadedAmmo--;
                NewMethodServerRpc();

            }



        }   
    }
    [ServerRpc]
    private void NewMethodServerRpc()
    {
        PlaySoundClientRpc();
    }

    [ClientRpc]
    void PlaySoundClientRpc()
    {
        BulletSound.Play();
    }
    private void OnHitObject(RaycastHit hit)
    {   
           
        if (BulletHoles == (BulletHoles | (1 << hit.transform.gameObject.layer)))
        {
            Vector3 bulletHolePosition = hit.point + (hit.normal * .01f);
            Quaternion bulletHoleRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            SpawnBulletHoleServerRpc(bulletHolePosition, bulletHoleRotation);
        }
        else if (hit.transform.tag == ("Player")) 
        {   
            
            ShooterController TargetShooterController = hit.transform.GetComponentInParent<ShooterController>();
            
            if (TargetShooterController != null)
            {   
                 
                if(TargetShooterController.username.Value != username.Value)
                {   
                    if(TargetShooterController.IsDead.Value)return;
                        BodyPaths bodyPaths = hit.transform.GetComponent<BodyPaths>();
                        {   
                            if(bodyPaths!= null)
                            {
                            
                                bodyPaths.BodyDamage(weapon.LegDamage, weapon.bodyDamage, weapon.HeadDamage, weapon.HandDamage,this);
                                Vector3 hitEffectPosition = hit.point + (hit.normal * .01f);
                                Quaternion hitEffectRotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);
                                Instantiate(weapon.HitVfx, hitEffectPosition, hitEffectRotation);
                                
                            }
                            
                        }
                    

                }
                
            }
            
          
        }
        
    }
    [ServerRpc(RequireOwnership = false)]
    public void SpawnBulletHoleServerRpc(Vector3 position, Quaternion rotation)
    {  
        GameObject BulletHole = Instantiate(weapon.BulletHoles, position, rotation);
        BulletHole.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    public void SpawnPickItemServerRpc(int _AmmoLeft,int _LoadedAmmo)
    {   
       
        GameObject PickItem = Instantiate(weapon.WeaponPrefab, GunDropPoint.position,GunDropPoint.rotation);
        PickItem.GetComponent<NetworkObject>().Spawn();
        PickItem.GetComponent<PickUpItem>().AmmoLeft.Value = _AmmoLeft;
        PickItem.GetComponent<PickUpItem>().LoadedAmmo.Value = _LoadedAmmo;
        PickItem.GetComponent<Rigidbody>().AddForce(GunDropPoint.forward * 10,ForceMode.VelocityChange);
        PickItem.GetComponent<Rigidbody>().AddForce(GunDropPoint.up * 10,ForceMode.Impulse);
        weapon = null;
        SetWeaponNullClientRpc();
        
    }

    [ClientRpc]
    void SetWeaponNullClientRpc()
    {
        weapon = null;
        DestroyChildObjects(GunSpawnPoint);

    }
    private void DestroyChildObjects(Transform parent)
    {
        foreach (Transform child in parent)
        {

            GameObject.Destroy(child.gameObject);
        }
    }
    private void Reload()
    {    
        if(weapon == null)return;
        
        if (Input.GetKeyDown(KeyCode.R) || LoadedAmmo == 0 && LoadedAmmo != weapon.WeaponMagSize )
        {   
            
            if (AmmoLeft == 0) return;
            isReloading = true;
            StartCoroutine(DelayedReload(weapon.WeaponReloadTimer));
            
            
           
        }   
    }

    IEnumerator DelayedReload(float delay)
    {    
        yield return new WaitForSeconds(delay);
        int bulletsToReload = weapon.WeaponMagSize - LoadedAmmo;
        if (AmmoLeft < bulletsToReload)
            bulletsToReload = AmmoLeft;
        LoadedAmmo += bulletsToReload;
        AmmoLeft -= bulletsToReload;
        isReloading = false;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
    
   

}
