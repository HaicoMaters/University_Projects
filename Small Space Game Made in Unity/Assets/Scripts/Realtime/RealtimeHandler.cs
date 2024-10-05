using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealtimeHandler : MonoBehaviour
{
    /*
     *  Boost performance (Maybe Look at planet get rid of if statement and store player and do seperatly)
     *  If have time maybe add more physics e.g.:
     *  More physics based player movement
     *  Do more stuff after a collision
     *  WHATEVER OTHER IDEAS I HAVE
     */

    [SerializeField]
    GameObject[] asteroidPrefabs;

    [SerializeField]
    GameObject planetPrefab;

    [SerializeField]
    public RealtimePlayer player;

    [SerializeField]
    GameObject collectablePrefab;

    // values used to handle world generation
    public int n_planets = 3; // number of planets to spawn
    public float planetDistance = 800f; // minimum distance between planets
    public int maxPlanetDistanceChecks = 50; // times to check before allowing planets to be closer than planetDistance away from each other
    public float asteroidAvgSpeed = 5f;
    public float avgDistAsteroid = 70f;
    public int xAsteroids = 15; // number of astroids to spawn along each axis x, y, z in a box shape
    public int yAsteroids = 15;
    public int zAsteroids = 15;
    public int requiredCollect = 20; // number of collectables for player to get to win the game
    public int score = 0;

    public List<GameObject> asteroids; // all asteroids in scene

    public bool paused = false;

    // Start is called before the first frame update
    void Start()
    {
        generateWorld();
        PauseGame(); // pause game before instructions are used
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PauseGame()
    {
        paused = true;
        Time.timeScale = 0;
    }
    public void ResumeGame()
    {
        paused = false;
        Time.timeScale = 1;
    }

    public void updateScore()
    {
        score++;
    }

    void generateWorld()
    {
        // varying mass of objects leads to better collisions to showcase conservation of momentum
        // circle vol is 4/3 * pi * r^3 this is an aproximation for 4/3
        // model original asteroid and planet prefabs as sphere with radius 1
        float volumeCoefficient = 1.333333333333f; 
        float inverseOriginalVol = 1 / (volumeCoefficient * Mathf.PI);
        
        // Centre around (0, 0, 0) and spawn asteroid prefabs
        Vector3 startPos = new Vector3(-xAsteroids * avgDistAsteroid * 0.5f, -yAsteroids * avgDistAsteroid * 0.5f, -zAsteroids * avgDistAsteroid * 0.5f);
        
        // First spawn planet prefabs
        //
        Vector3 lastPlanetLocation = new Vector3(0f, 0f, 0f);
        for (int i = 0; i < n_planets; i++)
        {
            // avoid spawning too close to player
            Vector3 planetLocation = new Vector3(Random.Range(0, xAsteroids + 1), Random.Range(0, yAsteroids + 1), Random.Range(0, zAsteroids + 1))
                * avgDistAsteroid + startPos;
            int numberOfChecks = 0;
            // try to make planets somewhat spreadout
            while (((planetLocation - lastPlanetLocation).magnitude < planetDistance && numberOfChecks < maxPlanetDistanceChecks)
                || Physics.OverlapSphere(planetLocation, 2f).Length != 0)
            {
                planetLocation = new Vector3(Random.Range(0, xAsteroids + 1), Random.Range(0, yAsteroids + 1), Random.Range(0, zAsteroids + 1)) 
                    * avgDistAsteroid + startPos;
                numberOfChecks++;
            }
            lastPlanetLocation = planetLocation;
            if (Vector3.Distance(planetLocation, new Vector3(0,0,0)) > 300f)
            {
                GameObject g = Instantiate(planetPrefab, planetLocation, Quaternion.identity);
                float scale = 10f; // vary size of planet

                Rigidbody rb = g.GetComponent<Rigidbody>();
                float density = rb.mass * inverseOriginalVol; // aproximate new mass of scaled object by modelling as a
                                                              // sphere with radius 1 and calculating density
                                                              // then scale up the radius when calculating final volume and mass
                rb.mass = density * (volumeCoefficient * Mathf.PI * scale * scale * scale); // mass = density * volume
                g.transform.localScale = Vector3.one * scale;
            }
            else
            {
                i--; // if spawn on player retry spawning planet
            }
 
        }


        // Spawn asteroids
        asteroids = new List<GameObject>();
        float maxOffset = avgDistAsteroid * 0.75f;
        for (int i = 0; i < xAsteroids; i++)
        {
            for (int j = 0; j < yAsteroids; j++)
            {
                for (int k = 0; k < zAsteroids; k++)
                {
                    // instantiate asteroids with random offsets and rotations to make more random
                    Vector3 randomOffset = new Vector3(Random.Range(-maxOffset, maxOffset), Random.Range(-maxOffset, maxOffset), Random.Range(-maxOffset, maxOffset));
                    Vector3 spawnLocation = new Vector3(i * avgDistAsteroid, j * avgDistAsteroid, k * avgDistAsteroid)
                        + startPos - randomOffset;
                    if(Vector3.Distance(Vector3.zero, spawnLocation) > 5f && Physics.OverlapSphere(spawnLocation, 2f).Length == 0) // no instance if where player spawns or overlaping objects
                    {
                        int prefab = Random.Range(0, 4);
                        GameObject g = Instantiate(asteroidPrefabs[prefab], spawnLocation, Random.rotation);
                        float randomScale = Random.Range(0.5f, 6f); // vary size of asteroids

                        Rigidbody rb = g.GetComponent<Rigidbody>();
                        float density = rb.mass * inverseOriginalVol; // model asteroids as a sphere
                        rb.mass = density * (volumeCoefficient * Mathf.PI * randomScale * randomScale * randomScale); // mass = density * volume
                        g.transform.localScale = Vector3.one * randomScale;
                        rb.AddForce(Random.onUnitSphere * (Random.Range(asteroidAvgSpeed * 0.5f, asteroidAvgSpeed * 2f)), ForceMode.VelocityChange); // add a random force to asteroids to make it move in space
                        // add to list of all asteroids
                        asteroids.Add(g);
                    }
                }
            }
        }

        // Spawn Collectables needed to win the game
        int collectablesToSpawn = (int) (requiredCollect * 1.75); // more spawned than required to beat the game to make easier to find 
        Vector3 spawnPos = new Vector3(Random.Range(0, xAsteroids + 1), Random.Range(0, yAsteroids + 1), Random.Range(0, zAsteroids + 1))
            * avgDistAsteroid + startPos;
        for (int i = 0; i < collectablesToSpawn; ++i)
        {
            while (Physics.OverlapSphere(spawnPos, 2f).Length != 0) {
                spawnPos = new Vector3(Random.Range(0, xAsteroids + 1), Random.Range(0, yAsteroids + 1), Random.Range(0, zAsteroids + 1))
                    * avgDistAsteroid + startPos;
            }
            Instantiate(collectablePrefab, spawnPos, Quaternion.identity);
        }
    }
}