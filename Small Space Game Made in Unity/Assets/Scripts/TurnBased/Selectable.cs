using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Selectable : MonoBehaviour
{
    // Class for handling how objects Units and doors can be selected with mouse in turn based mode
    public bool selected = false;

    public void SelectCheck()
    {
        if (Input.GetMouseButtonDown(0)) // Only 1st frame when mouse is down
        {
            selected = false; // Deselect any current selected objects
                              // (When using buttons e.g. attack/item/move have to manually reset selectable status to true if was true before)
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo = new RaycastHit();
            if (GetComponent<Collider>().Raycast(mouseRay, out hitInfo, 100.0f))
            { // If click on selectable selected = true
                selected = true;
            }
        }
        if (Input.GetMouseButtonDown(1)) // Allow to deselect a unit by right clicking
        {
            selected = false;
        }
    }
}
