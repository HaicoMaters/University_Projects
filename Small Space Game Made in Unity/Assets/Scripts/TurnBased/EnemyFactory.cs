using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFactory : MonoBehaviour
{
    // 1 Enemy factory per room excluding starting room
    // Enemy Unit prefab has slightly lowered original stats compare to PlayerUnit
    [SerializeField]
    GameObject enemyPrefab;

    [SerializeField]
    Door activationTriggerDoor; // for reducing the depth of the Max^N algorithm have 

    // Start is called before the first frame update
    void Start()
    {
        Spawn();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Spawn() {
        int amount = 2; // 2 enemy units per room
        for (int i = 0; i < amount; i++)
        {
            bool posFound = false;
            while (!posFound)
            {
                NavMeshHit hit;
                Vector3 position = new Vector3(transform.position.x + Random.Range(-3.0f, 5.0f), 0, transform.position.z + Random.Range(-3.0f, 5.0f));
                if (NavMesh.SamplePosition(position, out hit, 0.1f, NavMesh.AllAreas))
                {
                    posFound = true;
                    EnemyUnit e = Instantiate(enemyPrefab, hit.position, Quaternion.identity).GetComponent<EnemyUnit>();
                    e.SetupUnit();
                    e.weapon.randomWeapon();
                    e.inventory.generateRandomInventory();
                    e.activationDoor = activationTriggerDoor;
                }
            }
        }
    }
}
