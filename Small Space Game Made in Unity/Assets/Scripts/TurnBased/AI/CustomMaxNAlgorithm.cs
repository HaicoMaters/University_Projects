using System.Collections;
using System.Collections.Generic;
using MinMaxLibrary.utils;
using MinMaxLibrary.algorithms;
using UnityEngine;
using System;

/*
 *  Custom Version of the MaxNAlgorithm from MinMaxLibrary which has been adapted to suit my needs. This version is more specific to my project
 *  with how actions are represented and more dynamic to changes in environment taking into account vision and distance and Inventory items etc.
 *  however this version is very costly
 */

namespace AI
{

    public class CustomMaxNAlgorithm<GameConfiguration, PlayerConfiguration> where PlayerConfiguration : PlayerConf
    {

        // used to represent a unit for min max
        public class UnitRepresentation
        {
            Unit representedUnit;
            public int maxHP;
            public int currentHP;
            public int attack;
            public int defence;
            public Inventory inventory;
            public float attackRange;
            public float availableMoveDistance;
            public Vector3 position;
            public bool usedAction;
            public bool playerUnit; // is player unit or enemy unit
            public PlayerConfiguration life;
            public bool isAntagonistAndMax;

            // used to initalise a unit to become an existing unit
            public UnitRepresentation(Unit u, PlayerConfiguration life, bool playerTurn)
            {
                this.maxHP = u.maxHP;
                this.currentHP = u.currentHP;
                this.attack = u.attack + u.weapon.addedAttack;
                this.position = u.transform.position;
                this.usedAction = false;
                this.playerUnit = u.playerUnit;
                this.attackRange = u.weapon.attackRange;
                this.availableMoveDistance = u.availableMoveDistance;
                this.inventory = new Inventory();
                foreach (Item i in u.inventory.itemList)
                {
                    this.inventory.addToInventory(i);
                }
                this.life = life;
                if (u.playerUnit == playerTurn) // if player unit on player turn or enemy unit on enemy turn not antagonist else is antagonist
                {
                    this.isAntagonistAndMax = false;
                }
                else
                {
                    this.isAntagonistAndMax = true;
                }
            }

            public UnitRepresentation(int maxHP, int currentHP, int attack, int defence, Inventory inventory, float attackRange, float availableMoveDistance, Vector3 position, bool usedAction,
                bool playerUnit, PlayerConfiguration life, bool isAntagonist)
            {
                this.maxHP = maxHP;
                this.currentHP = currentHP;
                this.attack = attack;
                this.defence = defence;
                this.inventory = inventory;
                this.attackRange = attackRange;
                this.availableMoveDistance = availableMoveDistance;
                this.position = position;
                this.usedAction = usedAction;
                this.playerUnit = playerUnit;
                this.life = life;
                this.isAntagonistAndMax = isAntagonist;
            }

            public UnitRepresentation clone()
            {
                Inventory inventory = new Inventory();
                foreach (Item i in this.inventory.itemList)
                {
                    inventory.addToInventory(i);
                }
                return new UnitRepresentation(maxHP, currentHP, attack, defence, inventory, attackRange, availableMoveDistance, position, usedAction, playerUnit, (PlayerConfiguration)life.clone(), isAntagonistAndMax);
            }
        }

        public class GameState
        {
            public List<UnitRepresentation> Units { get; private set; }
            public List<DoorRepresentation> Doors { get; private set; }
            // alternate between each state where a unit has just moved and a unit has just made an action when using min max

            public bool wasMove; // last gamestate change / last minmax tree branch was moving or action 
            public int movedUnitIndex; // if last gamestate change was move make unit action on this index
            public Action bestAction;
            public Action parentAction;
            public GameConfiguration gameConfiguration;
            public List<double> bestUtilityVector;
            public bool playerTurn;

            public GameState(Action parent, GameState x)
            {
                Units = new List<UnitRepresentation>();
                foreach (var y in x.Units)
                    Units.Add(y.clone());
                Doors = new List<DoorRepresentation>();
                foreach (var y in x.Doors)
                    Doors.Add(y.clone());
                this.wasMove = x.wasMove;
                this.movedUnitIndex = x.movedUnitIndex;
                this.parentAction = parent;
                this.bestUtilityVector = x.bestUtilityVector;
                this.gameConfiguration = x.gameConfiguration;
                this.playerTurn = x.playerTurn;
            }

            public GameState(GameState x)
            {
                this.gameConfiguration = x.gameConfiguration;
                this.wasMove = x.wasMove;
                Units = new List<UnitRepresentation>();
                foreach (var y in x.Units)
                    Units.Add(y.clone());
                Doors = new List<DoorRepresentation>();
                foreach (var y in x.Doors)
                    Doors.Add(y.clone());
                this.bestUtilityVector = x.bestUtilityVector;
                this.movedUnitIndex = x.movedUnitIndex;
                this.parentAction = x.parentAction;
                this.playerTurn = x.playerTurn;
            }

            public GameState()
            {
                this.gameConfiguration = default(GameConfiguration);
                this.wasMove = false;
                this.bestAction = default(Action);
                this.parentAction = default(Action);
                bestUtilityVector = new List<double>();
                this.movedUnitIndex = 0;
                Doors = new List<DoorRepresentation>();
                Units = new List<UnitRepresentation>();
                this.playerTurn = true;
            }

            // used to get the inital gamestate with necessary information from turn based handler
            public GameState(bool playerTurn, bool wasMove, int movedUnitIndex, List<UnitRepresentation> units, List<DoorRepresentation> doors, GameConfiguration gc)
            {
                this.gameConfiguration = gc;
                this.wasMove = wasMove;
                this.bestAction = default(Action);
                this.parentAction = default(Action);
                bestUtilityVector = new List<double>();
                this.movedUnitIndex = movedUnitIndex;
                Doors = doors;
                Units = units;
                this.playerTurn = playerTurn;
            }

            public GameState(GameConfiguration gc, List<UnitRepresentation> units, int movedUnitIndex)
            {
                this.gameConfiguration = gc;
                Units = new List<UnitRepresentation>();
                foreach (UnitRepresentation u in units) Units.Add(u.clone());
                this.movedUnitIndex = movedUnitIndex;
            }

            public double getEnemyUtilityScore()
            {
                double total = 0.0;
                foreach (var u in Units)
                    total += (u.isAntagonistAndMax ? -1.0 : 1.0) * u.life.getScore();
                return total;
            }

            internal bool haveAllEnemiesLost()
            {
                return Units.FindAll(x => x.isAntagonistAndMax).TrueForAll(x => x.life.hasPlayerLost());
            }

            internal bool haveAllHelpersLost()
            {
                return Units.FindAll(x => !x.isAntagonistAndMax).TrueForAll(x => x.life.hasPlayerLost());
            }

            internal bool hasOneEnemyWon()
            {
                return Units.FindAll(x => x.isAntagonistAndMax).Exists(x => x.life.hasPlayerWon());
            }

            internal bool hasOneHelperWon()
            {
                return Units.FindAll(x => !x.isAntagonistAndMax).Exists(x => x.life.hasPlayerWon());
            }

            public double getLocalRewardForTransition(GameState nextStep)
            {
                return nextStep.getEnemyUtilityScore() - getEnemyUtilityScore();
            }

            public Winner whoWins()
            {
                Func<bool, bool, bool> impl = (prem, cons) => (!prem) || cons;
                var oppLost = haveAllEnemiesLost();
                var plLost = haveAllHelpersLost();

                if (oppLost)
                {
                    // All opponents have lost, player wins
                    return Winner.PLAYER_WINS;
                }
                else if (plLost)
                {
                    // All helpers have lost, opponent wins
                    return Winner.OPPONENT_WINS;
                }
                else
                {
                    // Game is still running
                    return Winner.TIE_OR_GAME_RUNNING;
                }
            }

            internal void setActionFromParent(Action action)
            {
                parentAction = action;
            }

            public List<double> getUtilityVector()
            {
                List<double> vs = new List<double>(Units.Count);
                for (int i = 0, N = Units.Count; i < N; i++)
                {
                    vs.Add(Units[i].life.getScore());
                }
                return vs;
            }

            /*
             * Below are functions which are used each GameState to get the available actions and movements
             * Depending on whether it is turn to make an action or a move
             */

            // updates gamestate where an action has been taken should make a new gameState before computing the new action
            // next action in the tree to make sure that the units are cloned for the new gamestate before computing anything
            // to avoid messing up the Max N algortithm this function computes the new gamestate which will be a node on the tree
            public GameState computeAction(Action action)
            {
                UnitRepresentation actor = Units[action.actorIndex];
                UnitRepresentation target = Units[action.targetUnitIndex];
                DoorRepresentation door = action.targetDoor;
                switch (action.Type)
                {
                    // carry out movement action
                    case Action.ActionType.Move:
                        actor.position = action.Destination;
                        wasMove = true;
                        break;
                    // carry out attack unit action
                    case Action.ActionType.AttackUnit:
                        target.currentHP -= Mathf.Clamp(actor.attack - target.defence, 0, target.currentHP); // clamp to avoid negative score values
                        wasMove = false;
                        break;
                    // carry out attack door action
                    case Action.ActionType.AttackDoor:
                        door.hp -= Mathf.Clamp(actor.attack - door.defence, 0, door.hp);
                        if (door.hp == 0)
                        {
                            Doors.Remove(door);
                        }
                        wasMove = false;
                        break;
                    // carry out unlock door action
                    case Action.ActionType.UnlockDoor:
                        actor.inventory.removeFromInventory(action.UsedItem);
                        Doors.Remove(door);
                        wasMove = false;
                        break;
                    // carry out use item action
                    case Action.ActionType.UseItem:
                        actor.currentHP += Mathf.Clamp(action.UsedItem.healAmount, actor.currentHP, actor.maxHP - actor.currentHP);
                        actor.inventory.removeFromInventory(action.UsedItem);
                        wasMove = false;
                        break;
                    // carry ouy wait action
                    case Action.ActionType.Wait:
                        wasMove = false;
                        break;
                }
                // if was move then next thing to happen will be action carried out by same unit
                if (!wasMove)
                {
                    actor.usedAction = true;
                    // get the new action index if unit which has not used action
                    if (playerTurn)
                    {
                        // itterate through unit list to see if unit with action remaining
                        for (int i = action.actorIndex; i < Units.Count; i++)
                        {
                            if (Units[i].playerUnit && !Units[i].usedAction)
                            {
                                movedUnitIndex = i;
                                return this;
                            }
                        }
                        // if no more actions swap turn refresh actions and get index of first enemy Unit
                        playerTurn = false;
                        int currIndex = -1;

                        for (int i = 1; i < Units.Count; i++)
                        {
                            if (!Units[i].playerUnit)
                            {
                                if (currIndex == -1)
                                {
                                    movedUnitIndex = i;
                                    currIndex = i;
                                }
                                Units[i].usedAction = false;
                                // make player units antagonist and ai units not antagonist for Max N algorithm
                                Units[i].isAntagonistAndMax = false;
                            }
                            else
                            {
                                Units[i].isAntagonistAndMax = true;
                            }
                        }
                    }
                    else
                    { // reverse for when enemy turn
                        for (int i = action.actorIndex; i < Units.Count; i++)
                        {
                            if (!Units[i].playerUnit && !Units[i].usedAction)
                            {
                                movedUnitIndex = i;
                                return this;
                            }
                        }
                        playerTurn = true;
                        int currIndex = -1;
                        for (int i = 0; i < Units.Count; i++)
                        {
                            if (Units[i].playerUnit)
                            {
                                if (currIndex == -1)
                                {
                                    movedUnitIndex = i;
                                    currIndex = i;
                                }
                                Units[i].usedAction = false;
                                Units[i].isAntagonistAndMax = false;
                            }
                            else
                            {
                                Units[i].isAntagonistAndMax = true;
                            }
                        }
                    }
                }
                return this;
            }

            public List<Action> getAvailableActions(UnitRepresentation actor)
            {
                HashSet<Action> actions = new HashSet<Action>();
                // get the index of the actor by finding the unit with the same position in units
                int actorIndex = movedUnitIndex;
                for (int i = 0; i < Units.Count; i++)
                {
                    if (Units[i].position == actor.position)
                    {
                        actorIndex = i;
                        break;
                    }
                }
                // add all available attack unit actions
                for (int i = 0; i < this.Units.Count; i++)
                {
                    if (canAttackUnit(Units[i], actor))
                    {
                        // get the index of the unit i
                        actions.Add(new Action(actorIndex, i)); // attack unit constructor

                    }
                }
                // get all available attack door actions
                /*foreach (DoorRepresentation d in this.Doors)
                {
                    // don't allow enemy units to open or attack doors 
                    if (actor.playerUnit && canAttackDoor(d, actor))
                    {
                        actions.Add(new Action(actorIndex, d)); // attack door constructor
                    }
                }*/
                // get all useable item actions
                if (actor.inventory.itemList.Count > 0)
                {
                    foreach (Item item in actor.inventory.itemList)
                    {
                        // heal items
                        if (item.type != Item.ItemType.key)
                        {
                            if (actor.currentHP != actor.maxHP) // will never be the best option if at full hp wait will always be better so can reduce number of nodes
                            {
                                actions.Add(new Action(actorIndex, item)); // use item constructor
                            }
                        }
                        /* // keys
                         else if (item.type == Item.ItemType.key)
                         {
                             if (this.Doors.Count < 0)
                             {
                                 DoorRepresentation closestDoor = this.Doors[0];
                                 for (int i = 1; i < this.Doors.Count; i++)
                                 {
                                     if (Vector3.Distance(actor.position, this.Doors[i].position) < Vector3.Distance(actor.position, closestDoor.position))
                                     {
                                         closestDoor = this.Doors[i];
                                     }
                                 }
                                 if (Vector3.Distance(actor.position, closestDoor.position) < 3f)
                                 {
                                     actions.Add(new Action(actorIndex, closestDoor, item)); // unlock door constructor
                                 }
                             }
                         }*/
                    }
                }
                // got rid of all door actions to reduce number of nodes as they are equally desireable as a wait action and prevents from more enemies being
                // activated by enemy units which would increase total number of nodes unecessary for gameplay just effects performance of algorithm

                if (actions.Count == 0)
                {
                    actions.Add(new Action(actorIndex)); // wait constructor if no available actions wait will be only option
                }

                List<Action> ls = new List<Action>();
                ls.AddRange(actions);
                return ls;
            }

            public List<Action> getAvailableMoveActions(UnitRepresentation actor)
            {
                List<Action> actions = new List<Action>();
                int actorIndex = movedUnitIndex;

                for (int i = 0; i < Units.Count; i++)
                {
                    if (Units[i].position == actor.position)
                    {
                        actorIndex = i;
                        break;
                    }
                }

                int maxSamples = 16;
                float actorX = actor.position.x;
                float actorY = actor.position.y;
                float actorZ = actor.position.z;
                int angleIncrease = 0; // used to try to make the positions of the positions chosen not too similar e.g. if sample 0 is chosen sample 1 not likely to be chosen

                for (int i = 0; i < maxSamples; i++)
                {
                    float angle = (i + angleIncrease) * (2 * Mathf.PI) / maxSamples;
                    // Add a bit of randomness so that it chooses that are somewhat spread out while having placements that prefer using more move distance
                    float randomRadiusFactor = UnityEngine.Random.Range(0.65f, 1f); // try to use most of movement
                    float randomAngleFactor = UnityEngine.Random.Range(0.8f, 1.1f);

                    float x = Mathf.Cos(angle * randomAngleFactor) * actor.availableMoveDistance * randomRadiusFactor + actorX;
                    float z = Mathf.Sin(angle * randomAngleFactor) * actor.availableMoveDistance * randomRadiusFactor + actorZ;

                    Vector3 pointInCircle = new Vector3(x, actorY, z);

                    if (canMoveToLocation(actor, pointInCircle))
                    {
                        // try to make the positions not all in the same area to hopefully get better action variety while being able to loop around and
                        // return back to old position if other areas don't have valid available movements
                        angleIncrease += (int)(maxSamples * 0.2);
                        actions.Add(new Action(actorIndex, pointInCircle));

                        // If the number of desired actions is reached, exit the loop
                        if (actions.Count >= 4)
                        {
                            break;
                        }
                    }
                }

                if (actions.Count == 0)
                {
                    // add the center point always able to just wait
                    actions.Add(new Action(actorIndex, actor.position));
                }

                return actions;
            }


            public bool canMoveToLocation(UnitRepresentation actor, Vector3 location)
            {
                float length = 0.0f;
                UnityEngine.AI.NavMeshHit hit;
                if (!UnityEngine.AI.NavMesh.SamplePosition(location, out hit, 0.1f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    // If position is not on the NavMesh do not consider position further 
                    return false;
                }
                UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
                UnityEngine.AI.NavMesh.CalculatePath(actor.position, location, UnityEngine.AI.NavMesh.AllAreas, path);
                // Make sure that path is valid
                if (path.status != UnityEngine.AI.NavMeshPathStatus.PathInvalid)
                {
                    for (int i = 1; i < path.corners.Length; ++i)
                    {
                        length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
                    }
                }
                if (actor.availableMoveDistance >= length)
                {
                    if (path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                    {
                        return true;
                    }
                    else // PathPartial
                    {
                        Door d = getDoorObstacle(path);
                        if (d == null) { return false; }
                        foreach (DoorRepresentation door in Doors)
                        {
                            if (door.representedDoor.gameObject == d.gameObject)
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
                return false;
            }

            private Door getDoorObstacle(UnityEngine.AI.NavMeshPath path)
            {

                for (int i = 0; i < path.corners.Length; i++)
                {
                    Vector3 corner = path.corners[i];

                    Collider[] colliders = Physics.OverlapSphere(corner, 2f, LayerMask.GetMask("Door"));

                    foreach (var collider in colliders)
                    {
                        Door door = collider.GetComponent<Door>();
                        if (door != null)
                        {
                            return door;
                        }
                    }
                }
                return null;
            }




            public bool canAttackUnit(UnitRepresentation target, UnitRepresentation attacker)
            {
                // not more calculation if same team. Don't consider where do no damage wait will always be the same outcome in that case so no need
                if (target.playerUnit == attacker.playerUnit || attacker.attack < target.defence ||
                    Vector3.Distance(target.position, attacker.position) > attacker.attackRange) // don't consider if too far
                {
                    return false;
                }

                // raycast ignore doors that have been opened in current gamestate if nothing at location currently
                // or ignore hit units unit return true
                RaycastHit hit;
                LayerMask layerMask = LayerMask.GetMask("Default", "Door");

                if (Physics.Raycast(attacker.position, target.position - attacker.position, out hit, attacker.attackRange, layerMask))
                {
                    /*  foreach (DoorRepresentation d in this.Doors)
                      {
                          if (d.representedDoor.gameObject == hit.collider.gameObject)
                          {
                              // Door is open, so check if there's anything else at the location
                              layerMask = LayerMask.GetMask("Default");
                              return !Physics.Raycast(attacker.position, target.position - attacker.position, out hit, attacker.attackRange, layerMask); // if nothing true if something false

                          }
                      }*/
                    if (hit.collider.gameObject.CompareTag("PlayerUnit") || hit.collider.gameObject.CompareTag("EnemyUnit"))
                    {
                        return true;
                    }
                }
                else
                {
                    return true; // No obstruction, can attack
                }
                return false;
            }

            public bool canAttackDoor(DoorRepresentation target, UnitRepresentation attacker)
            {
                // if further than attack range or no damage no more calculation needed
                if (attacker.attack < target.defence || Vector3.Distance(target.position, attacker.position) > attacker.attackRange)
                {
                    return false;
                }

                // raycast ignore doors that have been opened in current gamestate if nothing at location currently
                // or ignore hit units unit return true
                RaycastHit hit;
                LayerMask layerMask = LayerMask.GetMask("Default", "Door");

                if (Physics.Raycast(attacker.position, target.position - attacker.position, out hit, attacker.attackRange, layerMask))
                {

                    foreach (DoorRepresentation d in this.Doors)
                    {
                        if (d == target)
                        {
                            return true;
                        }
                        if (d.representedDoor.gameObject == hit.collider.gameObject)
                        {
                            // Door is open, so check if there's anything else at the location
                            layerMask = LayerMask.GetMask("Default");
                            if (Physics.Raycast(attacker.position, target.position - attacker.position, out hit, attacker.attackRange, layerMask))
                            {
                                if (hit.transform.position == target.position)
                                {
                                    return true;
                                }
                                return false; // Something is in the way
                            }
                        }
                    }
                }
                return false;
            }
        }

        class UpdateNextState
        {
            private Func<GameState, Action, Optional<GameState>> updateState;
            private List<Action> currentActions;
            private GameState currState;

            public List<Action> getNextActions(GameState gs)
            {
                if (gs.wasMove)
                {
                    return gs.getAvailableActions(gs.Units[gs.movedUnitIndex]);
                }
                else
                {
                    return gs.getAvailableMoveActions(gs.Units[gs.movedUnitIndex]);
                }
            }

            public UpdateNextState(Func<GameState, Action, Optional<GameState>> x)
            {
                updateState = x;
                currState = null;
            }

            public GameState getCurrentState()
            {
                return currState;
            }

            public UpdateNextState setCurrentState(GameState x)
            {
                currState = x;
                currentActions = getNextActions(x);
                return this;
            }

            public Action getAction(int i)
            {
                return currentActions[i];
            }

            public Optional<Tuple<GameState, double>> applyFunction(int actionId)
            {
                if ((actionId < 0) || (actionId >= currentActions.Count))
                    return new Optional<Tuple<GameState, double>>();
                else
                {
                    var opt = updateState(currState, currentActions[actionId]);
                    if (opt.HasValue)
                    {
                        opt.Value.setActionFromParent(currentActions[actionId]);
                        return new Optional<Tuple<GameState, double>>(new Tuple<GameState, double>(opt.Value, currState.getLocalRewardForTransition(opt.Value)));
                    }
                    else
                        return new Optional<Tuple<GameState, double>>();
                }
            }

            public Optional<GameState> applyFunctionNoScore(int actionId)
            {
                if ((actionId < 0) || (actionId >= currentActions.Count))
                {
                    return new Optional<GameState>();
                }
                else
                {
                    var nextGameState = updateState(currState, currentActions[actionId]);
                    if (nextGameState.HasValue)
                    {
                        nextGameState.Value.setActionFromParent(currentActions[actionId]);
                    }
                    return nextGameState;
                }
            }

            public GameState getCurrentGameState()
            {
                return currState;
            }
        }

        UpdateNextState ugs;

        public CustomMaxNAlgorithm(Func<GameState, Action, Optional<GameState>> updateState)
        {
            ugs = new UpdateNextState(updateState);
        }

        public List<Action> retrieveReversedBestActivity(NTree<GameState> tree)
        {
            if (tree.getChildrenSize() == 0)
            {
                var ls = new List<Action>();
                ls.Add(tree.data.bestAction);
                return ls;
            }
            else
            {
                for (int i = 0, N = tree.getChildrenSize(); i < N; i++)
                {
                    var child = tree.GetChild(i);
                    if (child.data.parentAction.Equals(tree.data.bestAction))
                    {
                        var ls = retrieveReversedBestActivity(child);
                        ls.Add(tree.data.bestAction);
                        return ls;
                    }
                }
                return new List<Action>();
            }
        }

        public NTree<GameState> fitModel(GameState gs, int maxDepth)
        {
            NTree<GameState> retVal = null;
            Stack<CustomRecursionParameters<NTree<GameState>>> stack = new Stack<CustomRecursionParameters<NTree<GameState>>>();
            stack.Push(new CustomRecursionParameters<NTree<GameState>>(new NTree<GameState>(gs)));
            while (stack.Count > 0)
            {
                CustomRecursionParameters<NTree<GameState>> currentSnapshot = stack.Pop();

                ugs.setCurrentState(currentSnapshot.input.data);
                Optional<GameState> candidateChild = ugs.applyFunctionNoScore(currentSnapshot.iterativeStep);
                bool someoneHasWon = (currentSnapshot.input.data.whoWins() == Winner.PLAYER_WINS) ||
                                         (currentSnapshot.input.data.movedUnitIndex == -1);
                bool reachedLeafNode = (someoneHasWon || (!candidateChild.HasValue));


                if (currentSnapshot.iterativeStep == 0)
                {
                    currentSnapshot.input.data.bestAction = null;
                    if (reachedLeafNode)
                    {
                        currentSnapshot.input.data.bestUtilityVector = currentSnapshot.input.data.getUtilityVector();
                    }
                    else
                    {
                        currentSnapshot.input.data.bestUtilityVector = null;
                    }
                }
                else
                {
                    if (currentSnapshot.input.data.bestUtilityVector == null)
                    {
                        currentSnapshot.input.data.bestAction = retVal.data.parentAction;
                        currentSnapshot.input.data.bestUtilityVector = retVal.data.bestUtilityVector;
                    }
                    else
                    {
                        
                        // get the best vector where the enemy scores are as high as possible and player scores are as low as possible
                        double oldScore = 0;
                        double newScore = 0;

                        List<double> oldVector = currentSnapshot.input.data.getUtilityVector();
                        List<double> newVector = retVal.data.getUtilityVector();
                        for (int i = 0; i < oldVector.Count; i++)
                        {
                            if (currentSnapshot.input.data.Units[i].playerUnit)
                            {
                                oldScore -= oldVector[i] * 2.5; // multiplier to make the AI prefer to be more aggresive
                                newScore -= newVector[i] * 2.5;
                            }
                            else
                            {
                                oldScore += oldVector[i];
                                newScore += newVector[i];
                            }
                        }
                        if (oldScore < newScore)
                        {
                            currentSnapshot.input.data.bestAction = retVal.data.parentAction;
                            currentSnapshot.input.data.bestUtilityVector = retVal.data.bestUtilityVector;
                        }
                    }
                }
                if (reachedLeafNode || currentSnapshot.depth > maxDepth)
                {
                    retVal = currentSnapshot.input;
                }
                else
                {
                    NTree<GameState> childTree = currentSnapshot.input.AddChild(candidateChild.Value);
                    childTree.data.setActionFromParent(ugs.getAction(currentSnapshot.iterativeStep));
                    currentSnapshot.iterativeStep++;
                    stack.Push(currentSnapshot);
                    var child = new CustomRecursionParameters<NTree<GameState>>(childTree);
                    child.depth = currentSnapshot.depth + 1;
                    stack.Push(child);
                }
            }
            return retVal;
        }
    }
}
