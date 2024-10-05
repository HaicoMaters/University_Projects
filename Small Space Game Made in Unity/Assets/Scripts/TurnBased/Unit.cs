using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Unit : Selectable
{
    public TurnbasedHandler tbHandler;

    // Class to handle common behaviour/traits between all player units and enemy agents in turn based combat
    // Unit able to open door with key or deal damage to door to destroy (taking 1 turn vs multiple)
    public int maxHP = 15;
    public int currentHP = 15;
    public int attack = 5; // attack before adding weapon
    public int defence = 3;

    public Inventory inventory;
    public Weapon weapon;
    public float availableMoveDistance = 7.5f; // Can move distance from last position before stopping (if path exceeds distance away prevents further movement)
                                               // calculate movement based on distance travelled not direct distance from begining each adjacent tile costs 1 (tiles may cost aditional movement)
    public Vector3 lastPosition;
    public Vector3 destination;

    public bool usedAction = false;

    public int level = 0; // On level up increase stats by random (around 1-4 per stat per level) for enemy add level 1 + additional random level ups
    public int exp = 100; // exp formula when killing enemy = 15 + (enemy level + 5 - unit level) * 5 (if enemy level < unit level - 5 replace with just gain 10) at 100xp level up

    public bool playerUnit; // used for minmax


    public void UnlockDoor(Door door)
    {
        if (inventory.itemList.Exists(Item => Item.type == Item.ItemType.key))
        {
            door.Open();
            int i = inventory.itemList.FindIndex(Item => Item.type == Item.ItemType.key);
            inventory.removeFromInventory(inventory.itemList[i]);
            StartCoroutine(playItemAnimation());
            UseUpAction();
        }
    }

    public void UseItem(Item item)
    {
        if (inventory.itemList.Exists(Item => Item.type == item.type))
        {
            if (currentHP + item.healAmount > maxHP)
            {
                currentHP = maxHP;
            }
            else
            {
                currentHP += item.healAmount;
            }
            int i = inventory.itemList.FindIndex(Item => Item.type == item.type);
            inventory.removeFromInventory(inventory.itemList[i]);
            StartCoroutine(playItemAnimation());
            UseUpAction();
        }
    }

    public void Wait()
    {
        UseUpAction();
    }

    public abstract void Attack(Selectable target); // attack door or unit
    public abstract void UseUpAction();

    protected Animator anim;


    public bool canTravelTo(NavMeshPath path)
    {
        float length = 0.0f;

        // Make sure that path is valid
        if (path.status == NavMeshPathStatus.PathComplete)
        {
            for (int i = 1; i < path.corners.Length; ++i)
            {
                length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
        }
        if (availableMoveDistance >= length && length > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void moveToLocation(Vector3 location)
    {
        destination = location;
        GetComponent<NavMeshAgent>().destination = destination;
        StartCoroutine(fixGettingStuck()); // just incase to prevent infinite movement
    }


    public void displayInfo() // show stats and items
    {
        tbHandler.ui.SelectableUI.SetActive(true);
        tbHandler.ui.InventoryUI.SetActive(true);
        tbHandler.ui.selectObject(this);
        tbHandler.ui.updateSelectableText();
        tbHandler.ui.updateInventoryText();
    }
    
    public void levelUp()
    {
        level += 1;
        exp -= 100;
        //Randomly gain stats up to certain number
        attack += Random.Range(1, 4);
        defence += Random.Range(1, 3);
        int increaseHP = Random.Range(2, 6);
        maxHP += increaseHP;
        currentHP += increaseHP;
    }

    public Door closestDoorToUnit()
    {
        //Returns the closest door to unit which is still closed
        Door[] doors = Object.FindObjectsOfType<Door>();
        Door closestDoor = doors[0];
        float closest = Mathf.Infinity;
        for (int i = 0; i < doors.Length - 1; i++)
        {
            float distance = Vector3.Distance(doors[i].gameObject.transform.position, transform.position);
            if (distance < closest && !doors[i].openDoor)
            {
                closestDoor = doors[i];
                closest = distance;
            }
        }
        return closestDoor;
    }

    public void SetupUnit() // setup unit after being spawned
    {
        tbHandler = Object.FindObjectOfType<TurnbasedHandler>();
        while (exp >= 100)
        {
            levelUp();
        }
        lastPosition = transform.position;
        inventory = new Inventory();
        weapon = new Weapon();
        anim = GetComponent<Animator>();
        anim.SetFloat("Speed", 0f);
    }


    public void die()
    {
        anim.SetBool("Dead", true);
        StartCoroutine(waitForDeathAnimation());
    }

    IEnumerator playItemAnimation()
    {
        anim.SetBool("Use", true);
        yield return new WaitForSeconds(1.5f);
        anim.SetBool("Use", false);
        anim.SetBool("StopUse", true);
    }

    IEnumerator waitForDeathAnimation()
    {
        tbHandler.turnState = TurnbasedHandler.TurnState.WAITING;
        yield return new WaitForSeconds(4);
        if (playerUnit)
        {
            tbHandler.turnState = TurnbasedHandler.TurnState.ENEMY; // to turnstate of who killed the unit
        }
        else
        {
            tbHandler.turnState = TurnbasedHandler.TurnState.PLAYER;
        }
        Destroy(this.gameObject);
    }

    public IEnumerator playAttackAnimation()
    {
        anim.SetBool("Aiming", true);
        anim.SetBool("Shoot", true);
        yield return new WaitForSeconds(2f);
        anim.SetBool("Shoot", false);
        anim.SetBool("Aiming", false);
        anim.SetBool("Reloading", true);
        yield return new WaitForSeconds(1f);
        anim.SetBool("Reloading", false);
    }

    public IEnumerator fixGettingStuck()
    {
        yield return new WaitForSeconds(12f);
        destination = transform.position;
        GetComponent<NavMeshAgent>().destination = transform.position;
    }
}
