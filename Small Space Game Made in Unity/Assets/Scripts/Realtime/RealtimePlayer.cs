using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealtimePlayer : MonoBehaviour
{
    [SerializeField]
    public float thrustForce = 30f;

    [SerializeField]
    public float turnspeed = 1f;

    [SerializeField]
    RealtimeHandler rtHandler;

    public int hp = 5;

    public float maxVelocity = 0f; // used to calculate camera hookes law

    
    public Rigidbody rb = null;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(enableAsteroids()); // start code to enable and disable asteroids based on their distance from player
        rb = GetComponent<Rigidbody>();
        maxVelocity = ((thrustForce / rb.drag) - Time.fixedDeltaTime * thrustForce) / rb.mass; // calculate max velocity with some error
                                                                                              // due to increase from planet gravity
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        // Add a thrust type force to unit while key is press
        if (Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(transform.forward * thrustForce);
        }

        // Change direction ship faces
        if (Input.GetKey(KeyCode.W))
        {
            transform.Rotate(turnspeed, 0.0f, 0.0f, Space.Self);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Rotate(-turnspeed, 0.0f, 0.0f, Space.Self);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0.0f, -turnspeed, 0.0f, Space.Self);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0.0f, turnspeed, 0.0f, Space.Self);
        }
    }

    bool hitCooldown = false;

    void OnCollisionEnter(Collision collision)
    {
        if (!hitCooldown) // only allow for 1 collider of ship to reduce hp at time
        {
            hp--;
            StartCoroutine(collisionCooldown());
        }
    }

    IEnumerator collisionCooldown()
    {
        hitCooldown = true;
        yield return new WaitForSeconds(2);
        hitCooldown = false;
    }

    bool inViewOfCamera(GameObject g)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        if (GeometryUtility.TestPlanesAABB(planes, g.GetComponent<Collider>().bounds))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    IEnumerator enableAsteroids()
    {
        yield return new WaitForSeconds(0.3f);
        // periodically enable nearby asteroid scripts every few seconds
        foreach (GameObject asteroid in rtHandler.asteroids)
        {
            if (inViewOfCamera(asteroid) || (asteroid.transform.position - transform.position).magnitude < 100f)
            {
                asteroid.GetComponent<Asteroid>().enabled = true;
            }
            else
            {
                asteroid.GetComponent<Asteroid>().enabled = false;
            }
        }
        StartCoroutine(enableAsteroids()); // start new instance of coroutine
    }
}
