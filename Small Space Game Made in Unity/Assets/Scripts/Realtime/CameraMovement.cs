using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Model a spring to change camera distance from player
    float minDistY = 2f;
    float maxDistY = 6f;
    float minDistZ = -15f;
    float maxDistZ = -25f;
    float inverseMaxVelocity = 0f;

    [SerializeField]
    RealtimePlayer player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (inverseMaxVelocity == 0 && player.maxVelocity > 0)
        {
            inverseMaxVelocity = 1/player.maxVelocity;
        }
        changePosition();
    }

    void changePosition()
    {
        float playerVelocity = player.rb.velocity.magnitude;
        float posX = 0;
        // Calculate extension of spring and use to find the postion along the axis
        float posY = minDistY + (inverseMaxVelocity * playerVelocity * (maxDistY - minDistY));
        float posZ = minDistZ + (inverseMaxVelocity * playerVelocity * (maxDistZ - minDistZ));

        transform.localPosition = new Vector3(posX, posY, posZ); // local position to be able to follow the player
    }
}
