using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    [SerializeField]
    Light worldLight;

    [SerializeField]
    Camera cameraObject;

    [SerializeField]
    float specularPower = 100.0f;    
    
    [SerializeField]
    float pointLightEffectScale = 2.5f;

    [SerializeField]
    Light ambientLight;

    List<Light> pointLights;

    // Start is called before the first frame update
    void Start()
    {
        if (pointLights == null)
        {
            pointLights = new List<Light>();
            pointLights.AddRange(Object.FindObjectsOfType<Light>());

            List<Light> lightsToRemove = new List<Light>();
            foreach (Light light in pointLights)
            {
                if (light.type == LightType.Directional)
                {
                    lightsToRemove.Add(light);
                }
            }
            foreach (Light lightToRemove in lightsToRemove)
            {
                pointLights.Remove(lightToRemove);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        MeshRenderer r = GetComponent<MeshRenderer>();

        r.material.SetVector("camPosition", cameraObject.transform.position);

        r.material.SetVector("lightDirection", -worldLight.transform.forward);
        r.material.SetColor("lightColour", worldLight.color);
        r.material.SetFloat("lightIntensity", worldLight.intensity);
        r.material.SetFloat("specularPower", specularPower);

        r.material.SetFloat("ambientIntensity", ambientLight.intensity);
        r.material.SetColor("ambientColor", ambientLight.color);

        List<Vector4> lightPositions = new List<Vector4>();
        Color[] lightColors = new Color[20];
        float[] lightIntensities = new float[20];
        float[] lightRanges = new float[20];
        for (int i = 0; i < pointLights.Count; i++)
        {
            Vector3 lightPosition = pointLights[i].transform.position;
            lightPositions.Add(new Vector4(lightPosition.x, lightPosition.y, lightPosition.z, 1.0f));
            Color lightColor = pointLights[i].color * pointLightEffectScale; // make the effect more exaggerated on the water surface
            lightColors[i] = lightColor;
            float lightIntensity = pointLights[i].intensity * pointLightEffectScale;
            lightIntensities[i] = lightIntensity;
            float lightRange = pointLights[i].range;
        }

        r.material.SetVectorArray("otherLightPositions[]", lightPositions);
        r.material.SetColorArray("otherLightColours[]", lightColors);
        r.material.SetFloatArray("otherLightIntensities[]", lightIntensities);
        r.material.SetFloatArray("otherLightRanges[]", lightRanges);
        r.material.SetInt("numberOfLights", pointLights.Count);
    }
}
