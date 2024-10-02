using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    [Header("Simulation Visuals")]
    public float particleRadius = 0.1f;
    public Color particleColour = Color.blue;
    public Material particleMaterial;
    Mesh particleMesh;

    [Header("Fluid Settings")]
    public int numberOfParticles = 100;
    public Vector3 gravity;
    public float smoothingRadius = 0.4f;
    public float mass = 1;
    public float referenceDensity = 1000f;
    public float gasStiffness = 1f; // Known as Tait's parameter or Bulk modulus
    public float viscosityCoefficient = 0.005f;
    public float tensionCoefficient = 0.00728f;
    public float surfaceTensionThreshold = 8;
    public float XSPHsmoothingConstant = 0.05f;
    public float buoyancy = 0f;
    public Vector3 initalVelocity = Vector3.zero;
    public float timeScale = 0.01f;
    public float timeStep = 0.02f;
    int numThreads;
    uint tableSize;
    int numStages;

    [Header("Compute Shader")]
    public ComputeShader compute;
    public ComputeBuffer positionBuffer;
    public ComputeBuffer velocityBuffer;
    ComputeBuffer densityBuffer;
    ComputeBuffer pressureBuffer;
    ComputeBuffer forceBuffer;
    ComputeBuffer factorsBuffer;
    ComputeBuffer argsBuffer;

    // Spatial Hashing
    ComputeBuffer spatialIndicesBuffer;
    ComputeBuffer particleIndicesBuffer;

    [Header("Fluid bounds")]
    public Vector3 minBounds;
    public Vector3 maxBounds;
    public float boundBounceDampening = 0.8f;

    // Kernel Ids
    const int densityKernel = 0;
    const int pressureKernel = 1;
    const int internalForceKernel = 2;
    const int externalForceKernel = 3;
    const int integrationKernel = 4;
    const int collisionKernel = 5;
    const int spatialIndicesKernel = 6;
    const int particleIndicesKernel = 7;
    const int sortKernel = 8;

    void Start()
    {
        CreateLowPolySphereMesh(2);
        numThreads = Mathf.CeilToInt((float)numberOfParticles / 256f); // make sure that the divison float cooresponds to numthreads in the compute shader

        tableSize = (uint)NextPrime(numberOfParticles * 2);
        // Create compute bufferss
        positionBuffer = new ComputeBuffer(numberOfParticles, sizeof(float) * 3);
        velocityBuffer = new ComputeBuffer(numberOfParticles, sizeof(float) * 3);
        forceBuffer = new ComputeBuffer(numberOfParticles, sizeof(float) * 3);
        densityBuffer = new ComputeBuffer(numberOfParticles, sizeof(float));
        pressureBuffer = new ComputeBuffer(numberOfParticles, sizeof(float));
        factorsBuffer = new ComputeBuffer(5, sizeof(float));
        particleIndicesBuffer = new ComputeBuffer((int)tableSize, sizeof(uint));
        spatialIndicesBuffer = new ComputeBuffer(numberOfParticles, sizeof(uint) * 2); //For sorting needs to be power of 2


        int nextPowerOf2 = 1;
        while (nextPowerOf2 < numberOfParticles)
        {
            nextPowerOf2 *= 2;
        }
        numStages = (int)Mathf.Log(nextPowerOf2, 2);

        // Set original Values
        SetInitialBufferData(getParticleSpawnPositions()); // need to have an array of original positions
        compute.SetInt("numParticles", numberOfParticles);
        compute.SetInt("tableSize", (int)tableSize);

        // Bind buffers to kernel functions
        compute.SetBuffer(densityKernel, "positions", positionBuffer);
        compute.SetBuffer(densityKernel, "densities", densityBuffer);
        compute.SetBuffer(densityKernel, "spatialIndices", spatialIndicesBuffer);
        compute.SetBuffer(densityKernel, "particleIndices", particleIndicesBuffer);
        compute.SetBuffer(densityKernel, "kernelFactors", factorsBuffer);

        compute.SetBuffer(pressureKernel, "densities", densityBuffer);
        compute.SetBuffer(pressureKernel, "pressures", pressureBuffer);

        compute.SetBuffer(internalForceKernel, "positions", positionBuffer);
        compute.SetBuffer(internalForceKernel, "densities", densityBuffer);
        compute.SetBuffer(internalForceKernel, "pressures", pressureBuffer);
        compute.SetBuffer(internalForceKernel, "spatialIndices", spatialIndicesBuffer);
        compute.SetBuffer(internalForceKernel, "particleIndices", particleIndicesBuffer);
        compute.SetBuffer(internalForceKernel, "velocities", velocityBuffer);
        compute.SetBuffer(internalForceKernel, "resultantForces", forceBuffer);
        compute.SetBuffer(internalForceKernel, "kernelFactors", factorsBuffer);

        compute.SetBuffer(externalForceKernel, "positions", positionBuffer);
        compute.SetBuffer(externalForceKernel, "densities", densityBuffer);
        compute.SetBuffer(externalForceKernel, "resultantForces", forceBuffer);

        compute.SetBuffer(integrationKernel, "positions", positionBuffer);
        compute.SetBuffer(integrationKernel, "velocities", velocityBuffer);
        compute.SetBuffer(integrationKernel, "resultantForces", forceBuffer);
        compute.SetBuffer(integrationKernel, "densities", densityBuffer);
        compute.SetBuffer(integrationKernel, "spatialIndices", spatialIndicesBuffer);
        compute.SetBuffer(integrationKernel, "particleIndices", particleIndicesBuffer);
        compute.SetBuffer(integrationKernel, "kernelFactors", factorsBuffer);


        compute.SetBuffer(collisionKernel, "positions", positionBuffer);
        compute.SetBuffer(collisionKernel, "velocities", velocityBuffer);

        compute.SetBuffer(spatialIndicesKernel, "spatialIndices", spatialIndicesBuffer);
        compute.SetBuffer(spatialIndicesKernel, "particleIndices", particleIndicesBuffer);
        compute.SetBuffer(spatialIndicesKernel, "positions", positionBuffer);

        compute.SetBuffer(sortKernel, "spatialIndices", spatialIndicesBuffer);

        compute.SetBuffer(particleIndicesKernel, "spatialIndices", spatialIndicesBuffer);
        compute.SetBuffer(particleIndicesKernel, "particleIndices", particleIndicesBuffer);

        particleMaterial.SetBuffer("positions", positionBuffer);

        uint[] args =
        {
            particleMesh.GetIndexCount(0), (uint)numberOfParticles, particleMesh.GetIndexStart(0), particleMesh.GetBaseVertex(0), 0
        };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
    }

    void Update()
    {

        UpdateSettings();
        PrecomputeFactors(); // Update Smoothing Kernel Factors In The Case Of Smoothing Radius Changing
        compute.Dispatch(spatialIndicesKernel, numThreads, 1, 1);
        GPUBitonicSort();
        compute.Dispatch(particleIndicesKernel, numThreads, 1, 1);
        compute.Dispatch(densityKernel, numThreads, 1, 1);
        compute.Dispatch(pressureKernel, numThreads, 1, 1);
        compute.Dispatch(internalForceKernel, numThreads, 1, 1);
        compute.Dispatch(externalForceKernel, numThreads, 1, 1);
        compute.Dispatch(integrationKernel, numThreads, 1, 1);
        compute.Dispatch(collisionKernel, numThreads, 1, 1);
        Graphics.DrawMeshInstancedIndirect(particleMesh, 0, particleMaterial, new Bounds(minBounds, maxBounds), argsBuffer);
    }

    void GPUBitonicSort()
    {
        for (int i = 0; i < numStages; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                int groupWidth = 1 << (i - j);
                int groupHeight = 2 * groupWidth - 1;
                compute.SetInt("groupWidth", groupWidth);
                compute.SetInt("groupHeight", groupHeight);
                compute.SetInt("stepIndex", j);
                compute.Dispatch(sortKernel, numThreads, 1, 1);
            }
        }
    }

    void UpdateSettings()
    {
        compute.SetFloat("timeStep", timeStep);
        compute.SetVector("gravity", gravity);
        compute.SetFloat("mass", mass);
        compute.SetFloat("smoothingRadius", smoothingRadius);
        compute.SetFloat("referenceDensity", referenceDensity);
        compute.SetFloat("gasStiffness", gasStiffness);
        compute.SetFloat("viscosityCoefficient", viscosityCoefficient);
        compute.SetFloat("tensionCoefficient", tensionCoefficient);
        compute.SetFloat("surfaceTensionThreshold", surfaceTensionThreshold);
        compute.SetFloat("XSPHsmoothingConstant", XSPHsmoothingConstant);
        compute.SetFloat("buoyancy", buoyancy);
        compute.SetVector("minBounds", minBounds);
        compute.SetVector("maxBounds", maxBounds);
        compute.SetFloat("boundBounceDampening", boundBounceDampening);
        compute.SetFloat("particleRadius", particleRadius);
        compute.SetFloat("sqrRadius", smoothingRadius * smoothingRadius);
        particleMaterial.SetFloat("_Radius", particleRadius);
        particleMaterial.SetColor("_Color", particleColour);
    }

    Vector3[] getParticleSpawnPositions()
    {
        Vector3[] positions = new Vector3[numberOfParticles];
        int particlesCount = 0; // Variable to track the total number of particles added
        int maxParticlesPerAxis = Mathf.CeilToInt(Mathf.Pow(numberOfParticles, 1f / 3f));

        // Calculate the spacing between particles along each axis
        float spacing = particleRadius + (smoothingRadius * 0.2f);
        float jitterMagnitude = 0.2f * particleRadius; // Adjust this value to control the amount of jitter

        // Calculate the starting position of the cuboid based on the minBounds
        Vector3 start = minBounds;

        // Spawn particles within the cuboid volume
        for (int x = 0; x < maxParticlesPerAxis && particlesCount < numberOfParticles; x++)
        {
            for (int y = 0; y < maxParticlesPerAxis && particlesCount < numberOfParticles; y++)
            {
                for (int z = 0; z < maxParticlesPerAxis && particlesCount < numberOfParticles; z++)
                {
                    // Calculate the position of the current particle within the cuboid
                    Vector3 particlePosition = start +
                                                new Vector3(x * spacing, y * spacing, z * spacing);

                    // Add random jitter to the particle position
                    particlePosition -= new Vector3(Random.Range(-jitterMagnitude, jitterMagnitude),
                                                    Random.Range(-jitterMagnitude, jitterMagnitude),
                                                    Random.Range(-jitterMagnitude, jitterMagnitude));
                    positions[particlesCount] = particlePosition;
                    particlesCount++;
                }
            }
        }
        return positions;
    }


    void SetInitialBufferData(Vector3[] originalParticlePositions)
    {
        // Set position buffer data
        positionBuffer.SetData(originalParticlePositions);

        // Initialize velocity buffer data
        Vector3[] velocities = new Vector3[numberOfParticles];
        for (int i = 0; i < numberOfParticles; i++)
        {
            velocities[i] = initalVelocity;
        }
        velocityBuffer.SetData(velocities);

        // Initialize force buffer data
        Vector3[] forces = new Vector3[numberOfParticles];
        forceBuffer.SetData(forces);

        // Initialize density buffer data
        float[] densities = new float[numberOfParticles];
        densityBuffer.SetData(densities);

        // Initialize pressure buffer data
        float[] pressures = new float[numberOfParticles];
        pressureBuffer.SetData(pressures);


        // Initialize factors buffer data
        PrecomputeFactors();

        Vector2Int[] spatialIndices = new Vector2Int[numberOfParticles];
        spatialIndicesBuffer.SetData(spatialIndices);
        // Initalize Spatial Hash Data
        uint[] particleIndices = new uint[tableSize];
        for (int i = 0; i < tableSize; i++)
        {
            particleIndices[i] = tableSize + 1;
        }
        particleIndicesBuffer.SetData(particleIndices);
    }

    void PrecomputeFactors()
    {
        if (smoothingRadius != 0)
        {
            float[] factorsArray = new float[5];
            factorsArray[0] = 315 / (Mathf.PI * 64 * Mathf.Pow(smoothingRadius, 9)); // poly6 factor
            factorsArray[1] = (-945 / (32 * Mathf.PI * Mathf.Pow(smoothingRadius, 9))); // poly 6 gradient factor
            factorsArray[2] = (-945 / (32 * Mathf.PI * Mathf.Pow(smoothingRadius, 9))); // poly 6 laplacian factor
            factorsArray[3] = -(45 / (Mathf.PI * Mathf.Pow(smoothingRadius, 6))); // Spiky gradient factor
            factorsArray[4] = (45 / (Mathf.PI * Mathf.Pow(smoothingRadius, 6))); // Viscosity Laplacian Factor
            factorsBuffer.SetData(factorsArray);
        }
    }

    void ReleaseComputeBuffers()
    {
        // Release each compute buffer
        ReleaseBuffer(positionBuffer);
        ReleaseBuffer(velocityBuffer);
        ReleaseBuffer(forceBuffer);
        ReleaseBuffer(densityBuffer);
        ReleaseBuffer(pressureBuffer);
        ReleaseBuffer(factorsBuffer);
        ReleaseBuffer(spatialIndicesBuffer);
        ReleaseBuffer(particleIndicesBuffer);
        ReleaseBuffer(argsBuffer);
    }

    // Helper method to release a compute buffer if it's not null
    void ReleaseBuffer(ComputeBuffer buffer)
    {
        if (buffer != null)
        {
            buffer.Release();
            buffer = null;
        }
    }

    // OnDestroy is called when the MonoBehaviour is being destroyed
    void OnDestroy()
    {
        ReleaseComputeBuffers();
        Destroy(particleMesh);
    }

    // Draws the AABB shape of the bounds as lines for better visualisation
    void DrawBounds()
    {
        Gizmos.color = Color.white;
        // Each vertex
        Vector3 v0 = new Vector3(minBounds.x, minBounds.y, minBounds.z);
        Vector3 v1 = new Vector3(maxBounds.x, minBounds.y, minBounds.z);
        Vector3 v2 = new Vector3(maxBounds.x, maxBounds.y, minBounds.z);
        Vector3 v3 = new Vector3(minBounds.x, maxBounds.y, minBounds.z);
        Vector3 v4 = new Vector3(minBounds.x, minBounds.y, maxBounds.z);
        Vector3 v5 = new Vector3(maxBounds.x, minBounds.y, maxBounds.z);
        Vector3 v6 = new Vector3(maxBounds.x, maxBounds.y, maxBounds.z);
        Vector3 v7 = new Vector3(minBounds.x, maxBounds.y, maxBounds.z);

        Gizmos.DrawLine(v0, v1);
        Gizmos.DrawLine(v1, v2);
        Gizmos.DrawLine(v2, v3);
        Gizmos.DrawLine(v3, v0);
        Gizmos.DrawLine(v4, v5);
        Gizmos.DrawLine(v5, v6);
        Gizmos.DrawLine(v6, v7);
        Gizmos.DrawLine(v7, v4);
        Gizmos.DrawLine(v0, v4);
        Gizmos.DrawLine(v1, v5);
        Gizmos.DrawLine(v2, v6);
        Gizmos.DrawLine(v3, v7);
    }

    void CreateLowPolySphereMesh(int subdivisions)
    {
        if (particleMesh != null)
        {
            Destroy(particleMesh);
        }
        particleMesh = new Mesh();
        int numVertices = 6 * subdivisions * (subdivisions + 1);
        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numVertices * 2];

        int index = 0;
        for (int lat = 0; lat <= subdivisions; lat++)
        {
            float theta = lat * Mathf.PI / subdivisions;
            float sinTheta = Mathf.Sin(theta);
            float cosTheta = Mathf.Cos(theta);

            for (int lon = 0; lon < subdivisions * 2; lon++)
            {
                float phi = lon * Mathf.PI / subdivisions;
                float sinPhi = Mathf.Sin(phi);
                float cosPhi = Mathf.Cos(phi);

                float x = cosPhi * sinTheta;
                float y = cosTheta;
                float z = sinPhi * sinTheta;

                vertices[index++] = new Vector3(x, y, z);
            }
        }

        index = 0;
        for (int lat = 0; lat < subdivisions; lat++)
        {
            for (int lon = 0; lon < subdivisions * 2; lon++)
            {
                int current = lat * (subdivisions * 2) + lon;
                int next = current + subdivisions * 2;

                triangles[index++] = current;
                triangles[index++] = next + 1;
                triangles[index++] = current + 1;

                triangles[index++] = current;
                triangles[index++] = next;
                triangles[index++] = next + 1;
            }
        }

        particleMesh.Clear();
        particleMesh.vertices = vertices;
        particleMesh.triangles = triangles;

        particleMesh.RecalculateNormals();
        particleMesh.RecalculateBounds();
    }


    void OnDrawGizmos()
    {
        DrawBounds();
    }

    // Function to check if a number is prime
    bool IsPrime(int number)
    {
        if (number <= 1)
            return false;

        for (int i = 2; i <= Mathf.Sqrt(number); i++)
        {
            if (number % i == 0)
                return false;
        }

        return true;
    }

    // Function to find the next prime number after the given number
    int NextPrime(int number)
    {
        number++;
        while (true)
        {
            if (IsPrime(number))
                return number;
            number++;
        }
    }

}
