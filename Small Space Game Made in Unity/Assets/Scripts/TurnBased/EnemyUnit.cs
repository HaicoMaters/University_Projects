using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AI;

public class EnemyUnit : Unit
{

    public AI.Action currentAction = null; // make null upon finishing action assigned to unit using min max in tb handler
    bool moving = false;
    bool carryingOut = false;
    public bool activeAI = false; // for reducing the computation

    public Door activationDoor; // door to open associated with activating the AI of the unit

    CameraControlsTurnbased cameraControls;

    // Start is called before the first frame update
    void Start()
    {
        playerUnit = false;
        cameraControls = Object.FindObjectOfType<CameraControlsTurnbased>();
    }

    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            cameraControls.viewMovingEnemyUnit(transform.position); // follow the movement of the unit
            anim.SetFloat("Speed", 0.2f);
        }

        // complete the move action
        if (Vector3.Distance(destination, transform.position) < 0.5f && moving)
        {
            moving = false;
            tbHandler.turnState = TurnbasedHandler.TurnState.ENEMY;
            anim.SetFloat("Speed", 0f);
        }

        if (currentAction != null && !carryingOut)
        {
            carryingOut = true;
            carryOutAction();
        }

        SelectCheck();
        if (selected)
        {
            displayInfo();
        }
    }

    public override void Attack(Selectable target)
    {
        if (target.GetComponent<PlayerUnit>() != null)
        {
            tbHandler.Combat(target.GetComponent<PlayerUnit>(), this, false);
            UseUpAction();
        }
        else if (target.GetComponent<Door>() != null)
        {
            tbHandler.DamageDoor(target.GetComponent<Door>(), this);
            UseUpAction();
        }
    }

    public void carryOutAction()
    {
        cameraControls.viewMovingEnemyUnit(transform.position);
        switch (currentAction.Type)
        {
            case AI.Action.ActionType.Move:
                moveToLocation(currentAction.Destination);
                moving = true;
                break;
            case AI.Action.ActionType.AttackUnit:
                Attack(tbHandler.playerUnits[currentAction.targetUnitIndex]);
                break;
            case AI.Action.ActionType.AttackDoor:
                Attack(currentAction.targetDoor.representedDoor);
                break;
            case AI.Action.ActionType.UnlockDoor:
                UnlockDoor(currentAction.targetDoor.representedDoor);
                break;
            case AI.Action.ActionType.UseItem:
                UseItem(currentAction.UsedItem);
                break;
            case AI.Action.ActionType.Wait:
                Wait();
                break;
        }
        if (currentAction.Type != AI.Action.ActionType.Move)
        {
            StartCoroutine(giveTimeForAction());
        }
        currentAction = null;
        carryingOut = false;
    }

    public override void UseUpAction()
    { // Reset things to original state
        usedAction = true;
        tbHandler.phaseActionsRemaining -= 1;
    }

    IEnumerator giveTimeForAction()
    {
        tbHandler.turnState = TurnbasedHandler.TurnState.WAITING;
        yield return new WaitForSeconds(2f);
        tbHandler.turnState = TurnbasedHandler.TurnState.ENEMY;
    }    
}
