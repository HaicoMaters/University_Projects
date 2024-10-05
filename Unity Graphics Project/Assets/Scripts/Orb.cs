using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    float bigLoopRadius;
    float smallLoopRadius;
    float bigLoopAngle;
    float smallLoopAngle = 0f;
    float bigLoopSpeed = 4f;
    float smallLoopSpeed = 12f;
    float inverseBigLoopRadius;
    float invereseSmallLoopRadius;

    Light orbLight;

    // Start is called before the first frame update
    void Start()
    {
        orbLight = this.gameObject.GetComponent<Light>();
        // small orb vs large orb have different scales
        if (transform.localScale.x == 4)
        {
            this.orbLight.range *= 4; // sphere with all axis the same
        }
        else
        {
            this.orbLight.range *= 2;

        }
        bigLoopRadius = new Vector2(transform.localPosition.x, transform.localPosition.z).magnitude;
        smallLoopRadius = bigLoopRadius * Random.Range(0.5f, 0.8f);

        // calculate the original angle
        bigLoopAngle = Mathf.Atan2(transform.localPosition.z, transform.localPosition.x) * Mathf.Rad2Deg;

        invereseSmallLoopRadius = 1 / (smallLoopRadius);
        inverseBigLoopRadius = 1 / (bigLoopRadius);

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = NewRandomColour();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update angles
        bigLoopAngle += Time.deltaTime * bigLoopSpeed * inverseBigLoopRadius;
        smallLoopAngle += Time.deltaTime * smallLoopSpeed * invereseSmallLoopRadius;

        // Calculate positions for both loops
        float bigLoopX = Mathf.Cos(bigLoopAngle) * bigLoopRadius;
        float bigLoopZ = Mathf.Sin(bigLoopAngle) * bigLoopRadius;

        float smallLoopX = Mathf.Cos(smallLoopAngle) * smallLoopRadius;
        float smallLoopZ = Mathf.Sin(smallLoopAngle) * smallLoopRadius;

        // Set the orb's position based on both loops
        transform.localPosition = new Vector3(bigLoopX + smallLoopX, transform.localPosition.y, bigLoopZ + smallLoopZ);
    }

    Material NewRandomColour()
    {
        Material material = new Material(Shader.Find("Custom/OrbShader"));

        // Set up orb material colour
        Color randomColor = new Color(Random.value, Random.value, Random.value);
        Color randomGlow = new Color(Random.value, Random.value, Random.value);
        material.SetColor("_Color", randomColor);

        material.SetColor("_GlowColor", randomGlow);

        // Set up the light colour
        orbLight.color = randomGlow * 1.5f;

        return material;
    }
}
