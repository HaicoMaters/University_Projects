using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNight : MonoBehaviour
{
    [SerializeField]
    float dayLength = 1f; // length of day in minutes

    [Range(0f, 1f)]
    float timeOfDay; // % time through day used for changing to appropriate lighting

    float timeScale; // Speed time should pass

    [SerializeField]
    Light sun;
    float sunIntensity = 1f;
    [SerializeField]
    Transform sunRot;
    [SerializeField]
    float baseSunIntensity = 1f;
    [SerializeField]
    Gradient sunColor; // change colour in different points of day e.g sunset


    [SerializeField]
    Light ambientLight;
    [SerializeField]
    Gradient ambientLightColor;
    [SerializeField]
    float ambientIntensity = 0.25f;


    // Start is called before the first frame update
    void Start()
    {
        timeScale = 24 / (dayLength / 60);
        ambientLight.intensity = ambientIntensity;
    }

    // Update is called once per frame
    void Update()
    {
        updateTime();
        rotateSun();
        updateSunIntensity();
        updateSunColor();
        updateAmbientLight();
    }

    void updateTime()
    {
        timeOfDay += Time.deltaTime * (timeScale / 86400); // 86400 seconds in a day
        if (timeOfDay > 1) { timeOfDay--; } // loop to new day if time greater than one
    }

    void rotateSun()
    {
        float sunAngle = timeOfDay * 360;
        sunRot.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, sunAngle));
    }

    void updateSunIntensity()
    {
        sunIntensity = Vector3.Dot(sun.transform.forward, Vector3.down);
        sunIntensity = Mathf.Clamp01(sunIntensity);
        sun.intensity = sunIntensity * baseSunIntensity;
    }

    void updateSunColor()
    {
        Color c = sunColor.Evaluate(sunIntensity);
        sun.color = c;
        //sun.GetComponent<LensFlare>().color = c;
    }

    void updateAmbientLight()
    {
        ambientLight.color = ambientLightColor.Evaluate(sunIntensity);
    }
}