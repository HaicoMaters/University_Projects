using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weapon
{
    public enum WeaponType
    {
        rifle,
        shotgun,
        sniper
    }

    // Each unit will be given a reandom weapon when spawned
    public int addedAttack;
    public float attackRange;
    public WeaponType weaponType;

    public Weapon()
    {

    }

    public void SetupWeapon()
    {
        switch (weaponType)
        {
            case WeaponType.rifle:
                addedAttack = 5;
                attackRange = 7f;
                break;
            case WeaponType.shotgun:
                addedAttack = 7;
                attackRange = 5f;
                break;
            case WeaponType.sniper:
                addedAttack = 4;
                attackRange = 9f;
                break;
        }
    }

    public string weaponToString()
    {
        string s = "";
        switch (weaponType)
        {
            case WeaponType.rifle:
                s = s + "Rifle - Atk = 5, Range = Medium";
                break;
            case WeaponType.shotgun:
                s = s + "Shotgun - Atk = 7, Range = Short";
                break;
            case WeaponType.sniper:
                s = s + "Sniper - Atk = 4, Range = Large";
                break;
        }
        return s;
    }

    public void randomWeapon()
    {
        int i = Random.Range(0, 3);
        switch (i)
        {
            case 0:
                weaponType = WeaponType.rifle;
                break;
            case 1:
                weaponType = WeaponType.shotgun;
                break;
            case 2:
                weaponType = WeaponType.sniper;
                break;
        }
        SetupWeapon();
    }
}
