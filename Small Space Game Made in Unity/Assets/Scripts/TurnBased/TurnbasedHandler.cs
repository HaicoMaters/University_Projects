using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AI;
using MinMaxLibrary.utils;


public class TurnbasedHandler : MonoBehaviour
{
    [SerializeField]
    public TurnbasedUI ui;

    [SerializeField]
    public GameObject playerPrefab;

    public enum TurnState
    {
        PLAYER, ENEMY, WAITING
    }

    public TurnState turnState;

    public PlayerUnit[] playerUnits;
    public EnemyUnit[] enemyUnits;

    public bool playerTurn;
    int playerRemainingUnits;
    int enemyRemainingUnits;
    public int phaseActionsRemaining; // At start of phase equals player remaining units or
                                      // enemy remaining units depending on phase. turn ends when = 0

    public bool finished = false;

    // Start is called before the first frame update
    void Start()
    {
        SpawnPlayerUnits();
        turnState = TurnState.PLAYER;
        playerTurn = true;
        phaseActionsRemaining = playerRemainingUnits;
    }

    // Update is called once per frame
    void Update()
    {
        playerUnits = Object.FindObjectsOfType<PlayerUnit>();
        playerRemainingUnits = playerUnits.Length;
        enemyUnits = Object.FindObjectsOfType<EnemyUnit>();
        enemyRemainingUnits = enemyUnits.Length;
        if (phaseActionsRemaining == 0 && turnState != TurnState.WAITING)
        {
            ChangeTurn();
        }
        if (playerRemainingUnits == 0 || enemyRemainingUnits == 0)
        {
            FinishGame();
        }
    }

    void ChangeTurn()
    {
        ui.AttackText.SetActive(false);
        if (playerTurn)
        {
            playerTurn = false;
            phaseActionsRemaining = enemyRemainingUnits;
            if (enemyUnits.Length > 0)
            {
                foreach (EnemyUnit enemy in enemyUnits)
                {
                    enemy.lastPosition = enemy.transform.position;
                    enemy.usedAction = false;
                }
            }
            StartCoroutine(waitForTurnChange(TurnState.ENEMY));
            ui.turnText.text = "Enemy Turn";
        }
        else
        {
            playerTurn = true;
            phaseActionsRemaining = playerRemainingUnits;
            foreach (PlayerUnit playerUnit in playerUnits)
            {
                playerUnit.lastPosition = playerUnit.transform.position;
                playerUnit.usedAction = false;
            }
            StartCoroutine(waitForTurnChange(TurnState.PLAYER)); // add small delay so player can see it has returned back to their turn if no active enemies
            ui.turnText.text = "Player Turn";
        }
    }

    void SpawnPlayerUnits()
    {
        for (int i = 0; i < 3; i++) // 3 player units
        {
            bool posFound = false;
            while (!posFound)
            {
                NavMeshHit hit;
                Vector3 position = new Vector3(transform.position.x + Random.Range(-2.0f, 2.0f), 0, transform.position.z + Random.Range(-2.0f, 3.0f));
                if (NavMesh.SamplePosition(position, out hit, 0.1f, NavMesh.AllAreas))
                {
                    posFound = true;
                    PlayerUnit p = Instantiate(playerPrefab, hit.position, Quaternion.identity).GetComponent<PlayerUnit>();
                    p.SetupUnit();
                    p.weapon.randomWeapon(); ;
                    p.inventory.generateRandomInventory();
                }
            }
        }
        playerUnits = Object.FindObjectsOfType<PlayerUnit>();
        playerRemainingUnits = playerUnits.Length;
    }

    public void Combat(PlayerUnit player, EnemyUnit enemy, bool playerAttacked)
    {
        if (playerAttacked)
        {
            player.transform.rotation = Quaternion.LookRotation(enemy.transform.position - player.transform.position, Vector3.up);
            StartCoroutine(player.playAttackAnimation());
            if (player.attack + player.weapon.addedAttack > enemy.defence) // player attacks enemy
            {
                enemy.currentHP = enemy.currentHP - (player.attack + player.weapon.addedAttack - enemy.defence);
            }
        }
        else
        {
            enemy.transform.rotation = Quaternion.LookRotation(player.transform.position - enemy.transform.position, Vector3.up);
            StartCoroutine(enemy.playAttackAnimation());
            if (enemy.attack + enemy.weapon.addedAttack > player.defence) // enemy attacks player
            {
                player.currentHP = player.currentHP - (enemy.attack + enemy.weapon.addedAttack - player.defence);
            }
        }
        if (player.currentHP <= 0)
        {
            player.die();

        }
        if (enemy.currentHP <= 0)
        {
            player.exp += 15 + (enemy.level + 5 - player.level) * 5; // player gains exp from kills enemy does not
            enemy.die();
            while (player.exp >= 100)
            {
                player.levelUp();
            }
        }
    }

    public void DamageDoor(Door door, Unit unit)
    {
        unit.transform.rotation = Quaternion.LookRotation(door.transform.position - unit.transform.position, Vector3.up);
        StartCoroutine(unit.playAttackAnimation());
        if (unit.attack + unit.weapon.addedAttack > door.defence)
        {
            door.currentHP = door.currentHP - (unit.attack + unit.weapon.addedAttack - door.defence);
        }
        if (door.currentHP <= 0)
        {
            door.Break();
        }
    }

    void FinishGame()
    {
        finished = true;
        if (enemyUnits.Length == 0)
        {
            ui.winScreen.SetActive(true);
        }
        else
        {
            ui.loseScreen.SetActive(true);
        }
    }

    IEnumerator waitForTurnChange(TurnState ts)
    {
        turnState = TurnState.WAITING;
        yield return new WaitForSeconds(1.5f);
        turnState = ts;
    }
}
