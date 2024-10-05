using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    RealtimeHandler rtHandler;

    // Start is called before the first frame update
    void Start()
    {
        rtHandler = Object.FindObjectOfType<RealtimeHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool blockHits = false; 
    void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.root.CompareTag("Player") && !blockHits) // get parent object which has tag
        {
            rtHandler.updateScore();
            blockHits = true; // only allow one collider to cause func
            Destroy(this.gameObject);
        }
    }
}
