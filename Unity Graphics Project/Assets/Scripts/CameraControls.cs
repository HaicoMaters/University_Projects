using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    [SerializeField]
    float speed = 20f;

    [SerializeField]
    float turnSpeedX = 3f;    
    [SerializeField]
    float turnSpeedY = 1.5f;

    [SerializeField]
    Transform snowFall;

    float rotX;
    float rotY;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // get mouse movement
        rotX += Input.GetAxis("Mouse X") * turnSpeedX;
        rotY += Input.GetAxis("Mouse Y") * turnSpeedY;

        // clamp vertical rotation to keep consistent controls
        rotY = Mathf.Clamp(rotY, -90, 90);

        // rotate camera
        transform.rotation = Quaternion.Euler(-rotY, rotX, 0);

        // handle snowfall following player
        snowFall.transform.rotation = Quaternion.Euler(0, -rotX, 0);
        snowFall.transform.position = new Vector3(transform.position.x, transform.position.y + 65, transform.position.z);

        // directional movement
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += -transform.right * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += -transform.forward * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * speed * Time.deltaTime;
        }    
    }
}
