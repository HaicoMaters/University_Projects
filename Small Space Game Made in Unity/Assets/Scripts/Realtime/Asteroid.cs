using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    // Enable / disable asteroid scripts in other scripts based on range from player due to 10000s of asteroids being instantiated

    RealtimePlayer player;
    public Rigidbody rb = null;

    // Rigid body properties to add back when asteroid becomes active again and remove to reduce cpu use
    public Vector3 velocity;

    bool firstInitalisation = true;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<RealtimePlayer>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnEnable()
    {
        // At the start of the game don't change velocity so that RealtimeHandler can add random force
        // before first frame for a random velocity in generateWorld()
        if (!firstInitalisation)
        {
            // revert to velocity from before being disabled
            rb.isKinematic = false;
            rb.velocity = velocity;
        }
        firstInitalisation = false;
    }

    void OnDisable()
    {
        // save the velocity and stop physics
        velocity = rb.velocity;
        rb.isKinematic = true;
    }
}
