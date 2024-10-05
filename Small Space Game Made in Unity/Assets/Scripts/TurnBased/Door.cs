using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Door : Selectable
{
    [SerializeField]
    TurnbasedHandler tbHandler;

    private Animator anim;
    public bool openDoor = false;

    public int maxHP = 30;
    public int currentHP;
    public int defence = 5;

    // Start is called before the first frame update
    void Start()
    {
        currentHP = maxHP;
        tbHandler = Object.FindObjectOfType<TurnbasedHandler>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !anim.IsInTransition(0) && openDoor)
        {
            anim.Play("door_2_opened");
        }
            SelectCheck();
        if (selected)
        {
            displayInfo();
        }
    }

    public void displayInfo() // show hp and defence
    {
        tbHandler.ui.SelectableUI.SetActive(true);
        tbHandler.ui.selectObject(this);
        tbHandler.ui.updateSelectableText();
    }

    public void Open()
    {
        anim.Play("door_2_open");
        openDoor = true;
        Destroy(GetComponent<BoxCollider>());
        Destroy(GetComponent<NavMeshObstacle>());
        Destroy(this);
    }

    public void Break()
    {
        Destroy(this.gameObject);
    }
}
