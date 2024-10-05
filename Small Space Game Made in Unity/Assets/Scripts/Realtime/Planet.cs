using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public Rigidbody planet;
    float G = 6.67384e-11f; // Gravitational constant G
    [SerializeField]
    public long gravityScale = 80000000; // used to scale the force to create a more exagerated
                                       // gravity effect due to limitations of rigidbody maximum mass
    List<GameObject> objects;

    RealtimeHandler rtHandler;

    // Start is called before the first frame update
    void Start()
    {
        rtHandler = Object.FindObjectOfType<RealtimeHandler>();
        planet = GetComponent<Rigidbody>();
        StartCoroutine(findPhysicsObjects());
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        if (objects.Count > 0)
        {
            foreach (GameObject obj in objects)
            {
                GravityPull(obj);
            }
        }
    }

    public void GravityPull(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>(); // asteroid or player rigidbody

        Vector3 dir = planet.position - rb.position;
        float dist = dir.magnitude;
        if (dist != 0) // prevent from dividing by zero
        {
            //Newton's law of universal gravitation : F = ((Mass1 - Mass2)/distance^2) * G
            float forceMagnitude = ((planet.mass - rb.mass) / (dist * dist)) * G * gravityScale;

            if (obj.CompareTag("Player"))
            {
                forceMagnitude = forceMagnitude * 0.0008f; // reduce the magnitude player is affected compared to
                                                           // asteroids for gameplay purposes while keeping asteroid orbits
            }
            Vector3 force = dir.normalized * forceMagnitude;

            rb.AddForce(force);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // when colliding with object prevent object getting stuck to planet
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>(); 
        if (rb != null)
        {
            Vector3 dir = planet.position - rb.position;
            rb.AddForce((-dir.normalized) * 20, ForceMode.Impulse);
            Vector3 normal = collision.contacts[0].normal;
            Vector3.Reflect(dir, normal);
        }
    }

    // Gets only the physics objects that are active at any given time and reduce load by not doing every frame
    // Player unable to tell objects are unaffected by gravity just by looking
    IEnumerator findPhysicsObjects()
    {
        objects = new List<GameObject>();
        objects.Add(GameObject.FindGameObjectWithTag("Player"));
        foreach (GameObject asteroid in rtHandler.asteroids)
        {
            if(asteroid.GetComponent<Rigidbody>().isKinematic == true)
            {
                objects.Add(asteroid);
            }
        }
        yield return new WaitForSeconds(0.3f);
        StartCoroutine(findPhysicsObjects());
    }
}
