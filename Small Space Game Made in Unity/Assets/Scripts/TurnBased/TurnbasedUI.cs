using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TurnbasedUI : MonoBehaviour
{
    [SerializeField]
    public GameObject SelectableUI;
    [SerializeField]
    TMP_Text SelectableText;

    [SerializeField]
    public GameObject InventoryUI;
    [SerializeField]
    TMP_Text InventoryText;

    [SerializeField]
    public GameObject ActionUI;

    [SerializeField]
    public GameObject AttackText;

    [SerializeField]
    public GameObject ItemUI;
    [SerializeField]
    public TMP_Dropdown ChosenItem;

    Selectable selectedObject;

    [SerializeField]
    GameObject instructions;

    [SerializeField]
    public TMP_Text turnText;

    [SerializeField]
    public GameObject winScreen;

    [SerializeField]
    public GameObject loseScreen;

    [SerializeField]
    GameObject exitGame;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (instructions.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                instructions.SetActive(false);
                turnText.gameObject.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (winScreen.activeSelf || loseScreen.activeSelf || exitGame)
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (winScreen.activeSelf || loseScreen.activeSelf)
            {
                SceneManager.LoadScene("TurnBasedScene");
            }
            if (exitGame.activeSelf)
            {
                exitGame.SetActive(false);
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            exitGame.SetActive(true);
        }
    }

    public void useItemButtonPress()
    {
        string selectedOption = ChosenItem.options[ChosenItem.value].text;
        PlayerUnit player = selectedObject.GetComponent<PlayerUnit>();
        switch (selectedOption)
        {
            case "Key":
                Door door = player.closestDoorToUnit();
                player.UnlockDoor(player.closestDoorToUnit());
                break;
            case "Medkit":
            case "Large Medkit":
                int i = player.inventory.itemList.FindIndex(Item => Item.name.Equals(selectedOption));
                player.UseItem(player.inventory.itemList[i]);
                break;
            default:
                break;
        }
    }

    public void itemButtonPress()
    {
        ItemUI.SetActive(true);
        ChosenItem.ClearOptions();
        PlayerUnit player = selectedObject.GetComponent<PlayerUnit>();
        int i = player.getDifferentItems();
        List<string> options = new List<string>();
        for (int j = 0; j < i; j++)
        {
            string itemToAdd = player.inventory.itemList[j].name;
            switch (itemToAdd)
            {
                case "Key": // if item is a key check distance from door to be able to use
                    if (Vector3.Distance(player.transform.position, player.closestDoorToUnit().gameObject.transform.position) < 3f)
                    {
                        options.Add(player.inventory.itemList[j].name);
                    }
                    break;
                default:
                    options.Add(player.inventory.itemList[j].name);
                    break;
            }
        }
        if (options.Count == 0)
        {
            options.Add("None");
        }
        ChosenItem.AddOptions(options);
    }

    public void waitButtonPress()
    {
        selectedObject.GetComponent<PlayerUnit>().Wait();
    }

    public void attackButtonPress()
    {
        selectedObject.GetComponent<PlayerUnit>().selectingAttackTarget = true;
    }

    public void toggleSelectableUi()
    {
        SelectableUI.SetActive(!SelectableUI.activeSelf);
    }

    public void toggleActionUi()
    {
        ActionUI.SetActive(!ActionUI.activeSelf);
    }

    public void selectObject(Selectable s)
    {
        selectedObject = s;
    }

    public void updateSelectableText()
    {
        if (selectedObject.GetComponent<PlayerUnit>() != null)
        {
            PlayerUnit unit = selectedObject.GetComponent<PlayerUnit>();
            SelectableText.text = "Your Unit \n Level: " + unit.level + "\n Exp" + unit.exp + "\n HP: " + unit.currentHP + "/" + unit.maxHP + "\n Attack: " +
            unit.attack + " + " + unit.weapon.addedAttack + "\n Defence: " + unit.defence;
        }
        else if (selectedObject.GetComponent<EnemyUnit>() != null)
        {
            EnemyUnit unit = selectedObject.GetComponent<EnemyUnit>();
            SelectableText.text = "Enemy Unit \n Level: " + unit.level + "\n Exp" + unit.exp + "\n HP: " + unit.currentHP + "/" + unit.maxHP + "\n Attack: " +
            unit.attack + " + " + unit.weapon.addedAttack + "\n Defence: " + unit.defence;
        }
        else if (selectedObject.GetComponent<Door>() != null)
        {
            Door door = selectedObject.GetComponent<Door>();
            SelectableText.text = "Door \n HP: " + door.currentHP + "/" + door.maxHP + "\n Defence: " + door.defence;
        }
    }

    public void updateInventoryText()
    {
        if (selectedObject.GetComponent<PlayerUnit>() != null)
        {
            PlayerUnit unit = selectedObject.GetComponent<PlayerUnit>();
            string text = "Weapon : " + unit.weapon.weaponToString();
            for (int i = 0; i < unit.inventory.itemList.Count; i++)
            {
                text = text + "\n Item" + (i + 1) + " : " + unit.inventory.itemList[i].itemToString() + ".  ";
                if (unit.inventory.itemList[i].droppable)
                {
                    text = text + "Dropped by unit upon death.";
                }
            }
            InventoryText.text = text;
        }
        else if (selectedObject.GetComponent<EnemyUnit>() != null)
        {
            EnemyUnit unit = selectedObject.GetComponent<EnemyUnit>();
            string text = "Weapon : " + unit.weapon.weaponToString();
            for (int i = 0; i < unit.inventory.itemList.Count; i++)
            {
                text = text + "\n Item" + (i + 1) + " : " + unit.inventory.itemList[i].itemToString() + ".  ";
                if (unit.inventory.itemList[i].droppable)
                {
                    text = text + "Dropped by unit upon death.";
                }
            }
            InventoryText.text = text;
        }
    }
}
