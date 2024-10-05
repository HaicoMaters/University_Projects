using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Profiling;

public class ShowPerformance : MonoBehaviour
{
    [SerializeField]
    TMP_Text fpsText;

    [SerializeField]
    TMP_Text memoryText;

    private float fps = 0;

    public float period = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(periodicUpdate()); // use coroutine with time period to be better to update at a rate which is easier to read/view
    }

    // Update is called once per frame
    void Update()
    {

    }

    void updateFpsText()
    {
        fps = (1f / Time.deltaTime);
        fpsText.text = "FPS: " + (int)fps;
    }

    void updateMemoryText()
    {
        memoryText.text = "Memory Used: " + Profiler.GetTotalAllocatedMemoryLong() / 1048576 + " MB";
    }

    IEnumerator periodicUpdate()
    {
        updateFpsText();
        updateMemoryText();
        yield return new WaitForSeconds(period);
        StartCoroutine(periodicUpdate());
    }
}
