using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControlsTurnbased : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // Directional Movement
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += -transform.up * 0.08f;
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.up * 0.08f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * 0.08f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += -transform.right * 0.08f;
        }
        // Rotational Movement
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(0f, 0f, 0.5f, Space.Self);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(0f, 0f, -0.5f, Space.Self);
        }
    }

    public void viewMovingEnemyUnit(Vector3 unitPos)
    {
        transform.position = new Vector3(unitPos.x, transform.position.y, unitPos.z);
    }
}
