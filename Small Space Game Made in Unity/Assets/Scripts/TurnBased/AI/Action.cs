using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class Action
    {
        public enum ActionType
        {
            Move,
            AttackUnit,
            AttackDoor,
            UnlockDoor,
            UseItem,
            Wait
        }

        public ActionType Type;
        public int actorIndex;
        public Vector3 Destination;
        public Item UsedItem;
        public int targetUnitIndex;
        public DoorRepresentation targetDoor;

        // Constructor for Move Action
        public Action(int actor, Vector3 destination)
        {
            Type = ActionType.Move;
            this.actorIndex = actor;
            this.Destination = destination;
        }

        // Constructor for Attack Unit Action
        public Action(int actor, int target)
        {
            Type = ActionType.AttackUnit;
            this.actorIndex = actor;
            this.targetUnitIndex = target;
        }

        // Constructor for Attack Door Action
        public Action(int actor, DoorRepresentation target)
        {
            Type = ActionType.AttackDoor;
            this.actorIndex = actor;
            this.targetDoor = target;
        }

        // Constructor for Unlock Door Action
        public Action(int actor, DoorRepresentation target, Item item)
        {
            Type = ActionType.UnlockDoor;
            this.actorIndex = actor;
            this.targetDoor = target;
            this.UsedItem = item;
        }

        // Constructor for Use Item Action
        public Action(int actor, Item item)
        {
            Type = ActionType.UseItem;
            this.actorIndex = actor;
            this.UsedItem = item;
        }

        // Constructor for Wait Action
        public Action(int actor)
        {
            Type = ActionType.Wait;
            this.actorIndex = actor;
        }
    }
}