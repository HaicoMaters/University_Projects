using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public enum ItemType
    {
        medkit,
        medkitLarge,
        key
    }

    public ItemType type;
    public bool droppable; // Allow for some enemies to drop certain items from their inventory (always drop keys for example) if item is dropable show player by highlighting 
                           // in inventory and chance of droping will drop when enemy dies (Randomly generate with unique percentage per item if will be highlighted)
                           // for player items droppable always false

    public bool consumable;
    public int healAmount;
    public int quantity;
    public string name;


    public void SetupItem()
    {
        switch (type) // initalise values for items
        {
            case ItemType.medkit:
                consumable = true;
                healAmount = 10; // just use same variable for attack of weapon as amount healed
                quantity = 1;
                name = "Medkit";
                break;
            case ItemType.medkitLarge:
                consumable = true;
                healAmount = 25;
                quantity = 1;
                name = "Large Medkit";
                break;
            case ItemType.key:
                consumable = false;
                quantity = 1;
                name = "Key";
                break;
        }
    }

    public string itemToString()
    {
        string s = "";
        switch (type)
        {
            case ItemType.medkit:
                s = s + "Medkit - Heals 10 HP when used lost upon use";
                break;
            case ItemType.medkitLarge:
                s = s + "Large Medkit - Heals 25 HP when used lost upon use";
                break;
            case ItemType.key:
                s = s + "Key - Can be used to unlock a door lost upon use";
                break;
        }
        return s + " - Number Left: " + quantity;
    }

    public void randomItem()
    {
        int i = Random.Range(1, 101);
        if (i <= 50)
        {
            type = ItemType.medkit;
        }
        else if (i <= 65)
        {
            type = ItemType.medkitLarge;
        }
        else
        {
            type = ItemType.key;
        }
        i = Random.Range(1, 101);
        if (i < 21)
        {
           droppable = true;
        }
        SetupItem();
    }
}
