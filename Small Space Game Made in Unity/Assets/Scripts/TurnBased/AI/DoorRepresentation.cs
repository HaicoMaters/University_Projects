using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class DoorRepresentation
    {
        public int hp;
        public int defence;
        public Vector3 position;
        public Door representedDoor;

        // for initalisation
        public DoorRepresentation(Door d)
        {
            this.hp = d.currentHP;
            this.defence = d.defence;
            this.position = d.transform.position;
            this.representedDoor = d;
        }

        public DoorRepresentation(int hp, int defence, Vector3 position, Door representedDoor)
        {
            this.hp = hp;
            this.defence = defence;
            this.position = position;
            this.representedDoor = representedDoor;
        }

        public DoorRepresentation clone()
        {
            return new DoorRepresentation(hp, defence, position, representedDoor);
        }
    }
}
