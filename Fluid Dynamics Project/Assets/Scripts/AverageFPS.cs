using UnityEngine;

public class AverageFPS : MonoBehaviour
{
    private float totalTime = 0f;
    private int frameCount = 0;
    private float averageFPS = 0f;
    private float timeStep = 0.003f;
    private float timeStepToStopCountingAt = 10f; // In simulation time to stop counting at
    public float currentTimeStep = 0f;

    private long totalMemoryUsage = 0L;


    private void Update()
    {
        if (currentTimeStep < timeStepToStopCountingAt)
        {
            totalTime += Time.deltaTime;
            frameCount++;
            totalMemoryUsage += System.GC.GetTotalMemory(false);
            currentTimeStep += timeStep;
            if (currentTimeStep >= timeStepToStopCountingAt)
            {
                Debug.Log("Finished Counting Frames");
            }
        }
        if (currentTimeStep >= timeStepToStopCountingAt)
        {
            currentTimeStep += timeStep;
        }
    }
    private void OnApplicationQuit()
    {
        // Calculate average FPS
        if (totalTime > 0)
        {
            averageFPS = frameCount / totalTime;
            Debug.Log("Average FPS: " + averageFPS);
            Debug.Log("Average Memory Usage: " + (((float)totalMemoryUsage / frameCount) /1024)/1024 + " MB");
        }
        else
        {
            Debug.LogWarning("No data collected to calculate average FPS.");
        }
    }
}
