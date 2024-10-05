using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI;
using System;
using MinMaxLibrary.utils;
using MinMaxLibrary.algorithms;

public class AIHandler : MonoBehaviour
{
    List<AI.Action> actionBuffer; // to hold all of the actions to perform in order for 1 enemy turn
    TurnbasedHandler tbHandler;

    List<Unit> unitsToConsider;
    List<EnemyUnit> activeEnemyUnits;
    int lastPlayerCount;

    // Start is called before the first frame update
    void Start()
    {
        tbHandler = UnityEngine.Object.FindObjectOfType<TurnbasedHandler>();
        actionBuffer = new List<AI.Action>();
    }

    // Update is called once per frame
    void Update()
    {
        if (tbHandler.turnState == TurnbasedHandler.TurnState.ENEMY && !tbHandler.finished)
        {
            if (actionBuffer == null || actionBuffer.Count == 0)
            {
                actionBuffer = getBestActions(2);
            }
            if (actionBuffer.Count > 0)
            {
                AssignAIAction();
            }
        }
    }

    public CustomMaxNAlgorithm<TreeBasedGameConfiguration<AI.Action>, TreeBasedUnitConf>.UnitRepresentation initalisedUnitWithScore(Unit u, bool playerTurn)
    {
        return new CustomMaxNAlgorithm<TreeBasedGameConfiguration<AI.Action>, TreeBasedUnitConf>.UnitRepresentation(u, new TreeBasedUnitConf(u.currentHP / u.maxHP, true, false), playerTurn);
    }

    // function to get a list of UnitRepresentation of all units in the scene to use for max N algorithm
    public List<CustomMaxNAlgorithm<TreeBasedGameConfiguration<AI.Action>, TreeBasedUnitConf>.UnitRepresentation> initaliseAllUnitsWithScore()
    {
        unitsToConsider = new List<Unit>();
        unitsToConsider.AddRange(((Unit[])tbHandler.playerUnits));
        unitsToConsider.AddRange((activeEnemyUnits));

        List<CustomMaxNAlgorithm<TreeBasedGameConfiguration<AI.Action>, TreeBasedUnitConf>.UnitRepresentation> rUnits = new List<CustomMaxNAlgorithm<TreeBasedGameConfiguration<AI.Action>, TreeBasedUnitConf>.UnitRepresentation>();

        foreach (Unit u in unitsToConsider)
        {
            rUnits.Add(initalisedUnitWithScore(u, tbHandler.playerTurn));
        }
        lastPlayerCount = tbHandler.playerUnits.Length;
        return rUnits;
    }

    // function to get the inital GameState
    public CustomMaxNAlgorithm<TreeBasedGameConfiguration<AI.Action>, TreeBasedUnitConf>.GameState initalGameState(bool playerTurn, bool wasMove, int movedUnitIndex,
        List<CustomMaxNAlgorithm<TreeBasedGameConfiguration<AI.Action>, TreeBasedUnitConf>.UnitRepresentation> units, List<DoorRepresentation> doors)
    {
        return new CustomMaxNAlgorithm<TreeBasedGameConfiguration<AI.Action>, TreeBasedUnitConf>.GameState(playerTurn, wasMove, movedUnitIndex, units, doors, new TreeBasedGameConfiguration<AI.Action>());
    }

    // function to update the individual score of a unit 
    public void updateUnitScore(CustomMaxNAlgorithm<TreeBasedGameConfiguration<AI.Action>, TreeBasedUnitConf>.UnitRepresentation unit)
    {
        unit.life = new TreeBasedUnitConf((double)unit.currentHP / unit.maxHP, true, false);
    }

    // need to fix the changes in enviroment e.g. when door does not exist anymore remove it from doors fix them not wanting to attack try and figure out scoring etc
    public List<AI.Action> getBestActions(int turnsToConsider)
    {
        List<AI.Action> bestActions = new List<AI.Action>(); // List of actions to be filled with 1 move action and 1 other action per enemy unit to carry out
        activeEnemyUnits = new List<EnemyUnit>();
        foreach (EnemyUnit u in tbHandler.enemyUnits)
        {
            if (u.activeAI && u.currentHP > 0)
            {
                activeEnemyUnits.Add(u);
            }
            else if (u.activationDoor == null || u.activationDoor.openDoor) // activate the unit for future actions
            {
                u.activeAI = true;
                activeEnemyUnits.Add(u);
            }
        }
        /* 
         * Standard situation is total of 9 units at the begining 3 player units and 6 enemy units to prevent this leading to large trees I have introduced
         * A way for the Max^N algorithm to only consider a few units at a time by not considering enemy units based on whether or not their trigger door has been opened or not
         * By doing this i can reduce the number of units from 9 to 5 after only the first door has been opened and therefore if turns to consider is set to 2 the max depth of the tree will be
         * 10 instead of of 18 making the complexity significantly lower. I have also prevented enemy units destroying to reduce the number of nodes however situations where the player
         * destroys doors will still be considered and keep behaviour that seems like enemy ship (can still potentially use a key). Increases scalability of game without effecting performance much
         * 
         * Least number of Nodes for 5 units and 1 turns // only 1 move action which is current location and only 1 other action which is wait same for all units = each node has one child
         * so 10 child nodes // extremely unlikely
         * 
         * Highest possible number of nodes for 5 units and 1 turn // 4 move actions each turn, all units are damaged so can use item and each have 2 items so 2 item use actions
         * 2 units can attack 3 units and 3 units can attack 2 units, no units can kill after all actions, unit can wait, avg alternate between 4 move actions and 5 other actions
         * and making the tree have a maximum depth which is number of turns * 2 * (unitcount - 1) = 8 
         * Total nodes =  370,525  //  unlikely (calculated using a recursive function)
         * 
         * By not considering the final unit's actions the number of nodes has been reduced from where there is a maximum depth of 10
         * Total nodes = 7,410,525 (20x less nodes / 95% decrease huge performance increase and memory save) 
         * While still considering all but one unit's actions for 1 turns not massively significant final decision change
         * Also Likely to lead to more agressive AI which is preferred as unit not considering damage taken from last player unit
         * 
         * Due to a lot of nodes only leading to 1 action being able to wait there are expected to be much lower number of nodes here many times just 1 child
         * leading the calculated maximum number of nodes to be highly improbable in actual gameplay so can safely increase number of turns to consider
         * and a large number of nodes with early exit due to win/lose
         * 
         * Final Decision was to consider all enemy turns twice and player turns once for 5 units
         * When testing when lots of decisions up to 30000 transition function calls and not lots of decisions around 1500
         * During fighting generally ranged mostly between 12000 - 20000 in most situations
         */

        //Transition Function
        Func<CustomMaxNAlgorithm<TreeBasedGameConfiguration<AI.Action>, TreeBasedUnitConf>.GameState, AI.Action,
            Optional<CustomMaxNAlgorithm<TreeBasedGameConfiguration<AI.Action>, TreeBasedUnitConf>.GameState>> f = (conff, act) =>
            {
                //Debug.Log("Func");
                int Nact = conff.gameConfiguration.actionsPerformed.Count;
                if (Nact < unitsToConsider.Count * turnsToConsider * 2 - 1) // times for all units to do both of their actions once to complete a full turn action 1 which is a move action and action 2 which is a different type
                {
                    var result = new CustomMaxNAlgorithm<TreeBasedGameConfiguration<AI.Action>, TreeBasedUnitConf>.GameState(act, conff);
                    result = result.computeAction(act); // function which updates the gamestate based on an action scores etc
                    result.gameConfiguration = conff.gameConfiguration.appendAction(act);

                    /*List<double> vec = conff.getUtilityVector();
                    Debug.Log($"Considered actions : Is player {conff.playerTurn} WasMove {conff.wasMove} Type: {act.Type}" + // useful for debugging very slow though so only use with small turns to consider
                    $"Actor: {act.actorIndex}, " +
                    $"Destination: {act.Destination}, " +
                    $"UsedItem: {act.UsedItem}, " +
                    $"TargetUnit: {act.targetUnitIndex}, " +
                    $"TargetDoor: {act.targetDoor}  UnitHP: {conff.Units[act.actorIndex].currentHP} / {conff.Units[act.actorIndex].maxHP} Parent: {conff.parentAction}" +
                  //  $"Utility Vector {vec[0]} {vec[1]} {vec[2]} {vec[3]} {vec[4]}" +
                    $" Act Number = {Nact}  Unit Index{conff.movedUnitIndex} number of parents: {conff.gameConfiguration.actionsPerformed.Count + 1}");
                   /* vec = result.getUtilityVector();
                    Debug.Log($"Considered results : Is player {result.playerTurn} WasMove {result.wasMove} " +
                  //  $"Utility Vector {vec[0]} {vec[1]} {vec[2]} {vec[3]} {vec[4]}" +
                    $" Act Number = {Nact}  UnitIndex{result.movedUnitIndex} number of parents: {result.gameConfiguration.actionsPerformed.Count + 1}" +
                    $" Parent {result.parentAction.Type} {result.parentAction.actorIndex} {result.parentAction.Destination} {result.parentAction.targetUnitIndex}");
                    */

                    foreach (var u in result.Units)
                    {
                        updateUnitScore(u);
                    }
                    return result;
                }
                else if (Nact == unitsToConsider.Count * turnsToConsider * 2 - 1)
                {
                    var result = new CustomMaxNAlgorithm<TreeBasedGameConfiguration<AI.Action>, TreeBasedUnitConf>.GameState(act, conff);
                    result.gameConfiguration = conff.gameConfiguration.appendAction(act);

                    return result;
                }
                return new Optional<CustomMaxNAlgorithm<TreeBasedGameConfiguration<AI.Action>, TreeBasedUnitConf>.GameState>();
            };

        Door[] doors = UnityEngine.Object.FindObjectsOfType<Door>();
        List<DoorRepresentation> rDoors = new List<DoorRepresentation>();
        foreach (Door d in doors)
        {
            if (!d.openDoor)
            {
                DoorRepresentation rd = new DoorRepresentation(d);
                rDoors.Add(rd);
            }
        }

        if (activeEnemyUnits.Count > 0)
        {
            var cgs = new CustomMaxNAlgorithm<TreeBasedGameConfiguration<AI.Action>, TreeBasedUnitConf>(f);
            // new state starting off with first enemy Unit index and was not move last
            CustomMaxNAlgorithm<TreeBasedGameConfiguration<AI.Action>, TreeBasedUnitConf>.GameState initalState = initalGameState(tbHandler.playerTurn, false, tbHandler.playerUnits.Length, initaliseAllUnitsWithScore(), rDoors);
            var tree = cgs.fitModel(initalState, turnsToConsider * 2 * (unitsToConsider.Count - tbHandler.playerUnits.Length));

            List<AI.Action> actions = cgs.retrieveReversedBestActivity(tree);
            int i = 0;
            foreach (AI.Action action in actions) // for debugging
            {
                if (action != null)
                {

                   /* Debug.Log($" Best Action?: Index: {i} Old Position {initalState.Units[action.actorIndex].position} Type: {action.Type}, " +
                        $"Actor: {action.actorIndex}, " +
                        $"Destination: {action.Destination}, " +
                        $"UsedItem: {action.UsedItem}, " +
                        $"TargetUnit: {action.targetUnitIndex}, " +
                        $"TargetDoor: {action.targetDoor}");
                   */
                }
                i++;
            }
            int numberOfActionsToGet = activeEnemyUnits.Count * 2;
            for (int j = 0; j < numberOfActionsToGet; j++)
            {
                if (j < actions.Count)
                {
                    bestActions.Add(actions[actions.Count - (j + 1)]); // so the list is in order of which action to make
                }
            }
        }
        foreach (EnemyUnit e in tbHandler.enemyUnits)
        {
            if (e.activeAI == false)
            {
                e.Wait();
            }
        }
        return bestActions;
    }

    public void AssignAIAction()
    {
        AI.Action action = actionBuffer[0];
        if (unitsToConsider.Count >  action.actorIndex)
        {
            if (activeEnemyUnits[action.actorIndex - tbHandler.playerUnits.Length] != null)
            {
                activeEnemyUnits[action.actorIndex - tbHandler.playerUnits.Length].currentAction = action;
            }
            else // handle the case where a player unit dies mid turn
            {
                for (int i = 0; i < activeEnemyUnits.Count; i++)
                {
                    if (activeEnemyUnits[action.actorIndex + i - tbHandler.playerUnits.Length] != null)
                    {
                        activeEnemyUnits[action.actorIndex + i - tbHandler.playerUnits.Length].currentAction = action;
                        break;
                    }
                }
            }
            tbHandler.turnState = TurnbasedHandler.TurnState.WAITING;
        }
        actionBuffer.Remove(action);
    }
}
