using UnityEngine;


[CreateAssetMenu(fileName = "Weapon", menuName = "ShooterGame/New Weapon", order = 0)]
public class Weapon : ScriptableObject
{   
    public  GameObject WeaponPrefab = null;
    public int HeadDamage = 100;
    public int bodyDamage = 40;
    public int LegDamage = 17;
    public int HandDamage = 20;
    public int WeaponMagSize = 30;
    public int WeaponAmmoLeft = 90;
    public float WeaponReloadTimer = 2;
    public float FireRatePerSec = 2;
    public GameObject HitVfx;
    public GameObject Bullet;
    public GameObject BulletHoles;
    public bool NonAuto = false;
    public GameObject DeadVfx = null;
    public AudioClip BulletSound = null;
    public GameObject FireVfx = null;

}
