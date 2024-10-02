using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kernels
{
    float Poly6Factor;
    float Poly6GradientFactor;
    float Poly6LaplacianFactor;
    float SpikyGradientFactor;
    float ViscosityLaplacianFactor;

    public void PrecomputeFactors(float smoothRadius)
    {
        Poly6Factor = 315 / (Mathf.PI * 64 * Mathf.Pow(smoothRadius, 9));
        Poly6GradientFactor = (-945 / (32 * Mathf.PI * Mathf.Pow(smoothRadius, 9)));
        Poly6LaplacianFactor = (-945 / (32 * Mathf.PI * Mathf.Pow(smoothRadius, 9)));
        SpikyGradientFactor = -(45f / (Mathf.PI * Mathf.Pow(smoothRadius, 6)));
        ViscosityLaplacianFactor = (45 / (Mathf.PI * Mathf.Pow(smoothRadius, 6)));
    }

    // Smoothing Kernel currently using cubic spline kernel based off used to calc density and surface tension
    // Another Look At Cubic Spline: from LIU M., LIU G.: Smoothed Particle Hydrodynamics (SPH): an Overview and Recent Developments. Archives of Computational Methods in Engineering 17, 1 (2010), 25–76.
    public float Poly6SmoothingKernel(float smoothRadius, float distance)
    {
        return Poly6Factor * Mathf.Pow((smoothRadius * smoothRadius) - (distance * distance), 3);
    }

    // Used for calculating surface tension force
    public Vector3 Poly6SmoothingKernelGradient(float smoothRadius, Vector3 distance)
    {
        float r = distance.magnitude;
        return Poly6GradientFactor * distance * Mathf.Pow(smoothRadius * smoothRadius - r * r, 2);
    }

    // Used for calculating surface tension force
    public float Poly6SmoothingKernelLaplacian(float smoothRadius, float distance)
    {
        return Poly6LaplacianFactor * (smoothRadius * smoothRadius - distance * distance) * (3 * smoothRadius * smoothRadius - 7 * distance * distance);
    }

    // Used for calculating pressure force
    public Vector3 SpikySmoothingKernelGradient(float smoothRadius, Vector3 distance)
    {
        float r = distance.magnitude;
        return (SpikyGradientFactor * Mathf.Pow((smoothRadius - r), 2)) * distance.normalized;
    }

    public float ViscositySmoothingKernelLaplacian(float smoothRadius, float distance)
    {
        return ViscosityLaplacianFactor * (smoothRadius - distance);
    }
}
