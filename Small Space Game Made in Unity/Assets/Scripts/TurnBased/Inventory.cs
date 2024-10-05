using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Inventory
{
    public List<Item> itemList;
    public Inventory()
    {
        itemList = new List<Item>();
    } 

    public void addToInventory(Item item)
    {
        if (itemList.Exists(Item => Item.type == item.type))
        {
            int i = itemList.FindIndex(Item => Item.type == item.type);
            itemList[i].quantity++;
        }
        else
        {
            itemList.Add(item);
        }
    }

    public void removeFromInventory(Item item)
    {
        int i = itemList.FindIndex(Item => Item.type == item.type);
        if (itemList[i].quantity > 1)
        {
            itemList[i].quantity--;
        }
        else
        {
            itemList.Remove(itemList[i]);
        }
    }

    public void generateRandomInventory()
    {
        // Random number of random items
        int i = Random.Range(1, 101);
        int numberOfItems = 0;  
        if (i <= 80)
        {
            numberOfItems = 1;
        }
        else
        {
            numberOfItems = 2;
        }
        for (int j = 0; j < numberOfItems; j++)
        {
            Item item = new Item();
            item.randomItem();
            addToInventory(item);
        }
    }
}
