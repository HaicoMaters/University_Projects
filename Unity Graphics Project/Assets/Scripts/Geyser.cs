using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geyser : MonoBehaviour
{
    [SerializeField]
    Light worldLight;

    [SerializeField]
    Light ambientLight;

    ParticleSystem system;
    ParticleSystemRenderer particleRend;

    [SerializeField]
    Camera cam;

    CameraControls cc; 

    [SerializeField]
    float eruptionFrequency = 300f; // how often the geyser erupts in seconds
    float timeScale;
    // Start is called before the first frame update
    void Start()
    {
        particleRend = GetComponent<ParticleSystemRenderer>();
        system = GetComponent<ParticleSystem>();
        system.Stop();
        cc = cam.GetComponent<CameraControls>();
        StartCoroutine(Erupt());
    }

    // Update is called once per frame
    void Update()
    {
        updateMaterial();
    }

    void updateMaterial()
    {
        particleRend.material.SetVector("lightDirection", -worldLight.transform.forward);
        particleRend.material.SetColor("lightColour", worldLight.color);
        particleRend.material.SetFloat("lightIntensity", worldLight.intensity);
        particleRend.material.SetFloat("ambientIntensity", ambientLight.intensity);
        particleRend.material.SetColor("ambientColor", ambientLight.color);
        particleRend.material.SetFloat("ParticleRandom", Random.Range(0f, 1f));
    }



    // handles the eruption behaviour of the system
    IEnumerator Erupt()
    {
        yield return new WaitForSeconds(eruptionFrequency - system.main.duration);
        system.Play();
        yield return new WaitForSeconds(system.main.duration);
        system.Stop();
        StartCoroutine(Erupt());
    }
}