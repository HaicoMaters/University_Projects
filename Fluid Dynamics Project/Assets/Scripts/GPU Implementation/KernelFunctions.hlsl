// The kernel functions without the factors infront (which are based on smoothing radius)
// these factors will be precomputed in c# and then passed to the compute shader directly
// to avoid redundant calculations

float Poly6SmoothingKernel(float smoothRadius, float distance) 
{
    return pow((smoothRadius * smoothRadius) - (distance * distance), 3);
}

float3 Poly6SmoothingKernelGradient(float smoothRadius, float3 distance)
{
    float r = length(distance);
    return distance * pow(smoothRadius * smoothRadius - r * r, 2);
}

float Poly6SmoothingKernelLaplacian(float smoothRadius, float distance)
{
    return (smoothRadius * smoothRadius - distance * distance) * (3 * smoothRadius * smoothRadius - 7 * distance * distance);
}

float3 SpikySmoothingKernelGradient(float smoothRadius, float3 distance)
{
    float r = length(distance);
    return pow((smoothRadius - r), 2) * normalize(distance);
}

float ViscositySmoothingKernelLaplacian(float smoothRadius, float distance)
{
    return (smoothRadius - distance);
}