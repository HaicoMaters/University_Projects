using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerUnit : Unit
// For player units controlled in turn based mode                                        
{
    bool pathSelected = false;
    public bool selectingAttackTarget = false;

    LineRenderer line;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        playerUnit = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (tbHandler.turnState == TurnbasedHandler.TurnState.PLAYER)
        {
            if (Input.GetMouseButtonDown(1) && pathSelected == true)
            // Allow to deselect unit while path has been selected
            {
                pathSelected = false;
                resetLocation();
                tbHandler.ui.ActionUI.SetActive(false);
                tbHandler.ui.ItemUI.SetActive(false);
            }
            if (selected)
            {
                displayInfo();
                if (!usedAction)
                {
                    if (!pathSelected)
                    {
                        // Allow player to select space to move to
                        if (validPathToCursor())
                        {
                            if (Input.GetMouseButtonDown(0))
                            {
                                Vector3 mousePosition;
                                RaycastHit hit;
                                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                                if (Physics.Raycast(ray, out hit))
                                {
                                    mousePosition = hit.point;
                                    mousePosition.y = 0;
                                    moveToLocation(mousePosition);
                                    pathSelected = true;
                                    tbHandler.turnState = TurnbasedHandler.TurnState.WAITING;
                                }
                            }
                        }
                    }
                    else
                    {
                        line.positionCount = 0;
                        makeAction();
                    }
                }
            }
        }
        if (selectingAttackTarget)
        {
            showAttackRange();
            tbHandler.ui.AttackText.SetActive(true);
            Selectable target = chooseAttackTarget();
            if (target != null)
            {
                Attack(target);
            }
            if(Input.GetMouseButtonDown(1))
            {
                selectingAttackTarget = false;
                tbHandler.ui.AttackText.SetActive(false);
            }

        }
        if (!selected || usedAction)
        {
            line.positionCount = 0;
        }
        if (!selectingAttackTarget)
        {
            SelectCheck(); // Check to select and deselect at the end 
            if (pathSelected == true) // reselect before the next frame if player clicked to choose a path
            {
                selected = true;
            }
        }
        if (tbHandler.turnState == TurnbasedHandler.TurnState.WAITING && Vector3.Distance(destination, transform.position) < 0.5f && pathSelected && !usedAction)
        {
            anim.SetFloat("Speed", 0f);
            tbHandler.turnState = TurnbasedHandler.TurnState.PLAYER; // re enable player action after movement occurs
        }
        if (tbHandler.turnState == TurnbasedHandler.TurnState.WAITING && Vector3.Distance(destination, transform.position) > 0.5f && pathSelected && !usedAction)
        {
            anim.SetFloat("Speed", 0.2f);
        }
    }

    public bool validPathToCursor()
    {
        // Find Where mouse is pointing to in the world
        Vector3 mousePosition;
        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            mousePosition = hit.point;
            // Found y coordinates to be 0.08f higher when on y cord of 0
            mousePosition.y = 0;
            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(lastPosition, mousePosition, NavMesh.AllAreas, path);
            if (canTravelTo(path))
            {
                // Display path in green
                line.positionCount = path.corners.Length;
                line.startColor = Color.green;
                line.endColor = Color.green;
                for (int i = 0; i < path.corners.Length; i++)
                {
                    line.SetPosition(i, path.corners[i]);
                }
                return true;
            }
            else
            {
                // Display path in red
                line.positionCount = path.corners.Length;
                line.startColor = Color.red;
                line.endColor = Color.red;
                for (int i = 0; i < path.corners.Length; i++)
                {
                    line.SetPosition(i, path.corners[i]);
                }
                return false;
            }
        }
        return false;
    }

    public void makeAction()
    {
        tbHandler.ui.selectObject(this);
        tbHandler.ui.ActionUI.SetActive(true);
    }

    public void resetLocation() // Allow player to return to original position if clicking off unit before making action
    {
        transform.position = lastPosition;
        moveToLocation(lastPosition);
    }

    public override void Attack(Selectable target)
    {
        selectingAttackTarget = false;
        if (target.GetComponent<EnemyUnit>() != null)
        {
            tbHandler.ui.AttackText.SetActive(false);
            tbHandler.Combat(this, target.GetComponent<EnemyUnit>(), true);
            UseUpAction();
        }
        else if (target.GetComponent<Door>() != null)
        {
            tbHandler.ui.AttackText.SetActive(false);
            tbHandler.DamageDoor(target.GetComponent<Door>(), this);
            UseUpAction();
        }        
    }

    public override void UseUpAction()
    { // Reset things to original state
        usedAction = true;
        pathSelected = false;
        selected = false;
        selectingAttackTarget = false;
        tbHandler.phaseActionsRemaining -= 1;
        tbHandler.ui.ActionUI.SetActive(false);
        tbHandler.ui.SelectableUI.SetActive(false);
        tbHandler.ui.InventoryUI.SetActive(false);
        tbHandler.ui.AttackText.SetActive(false);
        tbHandler.ui.ItemUI.SetActive(false);
    }

    public Selectable chooseAttackTarget()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = new RaycastHit();
            RaycastHit hit2 = new RaycastHit();
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.GetComponent<EnemyUnit>())
                {
                    if (Physics.Raycast(transform.position, hit.point - transform.position, out hit2, weapon.attackRange))
                    {
                        if (hit2.collider.GetComponent<EnemyUnit>() && hit.collider == hit2.collider)
                        {
                            return hit2.collider.GetComponent<EnemyUnit>();
                        }
                    }
                }
                if (hit.collider.GetComponent<Door>())
                {
                    if (Physics.Raycast(transform.position, hit.point - transform.position, out hit2, weapon.attackRange))
                    {
                        if (hit2.collider.GetComponent<Door>() && hit.collider == hit2.collider)
                        {
                            return hit2.collider.GetComponent<Door>();
                        }
                    }
                }
            }
        }
        return null;
    }
    
    public void showAttackRange()
    {
        // Use Line Renderer to draw circle with radius of weapon's attackrange
        float x;
        float z;
        float r = weapon.attackRange;
        int segments = 180;
        line.positionCount = segments + 1;
        line.startColor = Color.white;
        line.endColor = Color.white;
        RaycastHit hit = new RaycastHit();
        float angle = 0f;
        for (int i = 0; i < segments + 1; i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * r;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * r;

            // Use the same raycasting logic as chooseAttackTarget to determine visibility
            var ray = new Ray(transform.position, new Vector3(x, 0, z));
            if (Physics.Raycast(ray, out hit, weapon.attackRange))
            {
                line.SetPosition(i, new Vector3(hit.transform.position.x, 0.8f, hit.transform.position.z));
            }
            else
            {
                line.SetPosition(i, new Vector3(transform.position.x + x, 0.8f, transform.position.z + z));
            }
            angle += (360f / segments); 
        }
    }

    public int getDifferentItems()
    {
        int items = 0;
        foreach (Item item in inventory.itemList)
        {
            items++;
        }
        return items;
    }
}
