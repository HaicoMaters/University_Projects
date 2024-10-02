using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using static ParticleHandler;

[System.Serializable]
public class ParticleHandler : MonoBehaviour
{
    public struct Particle
    {
        public int id;
        public List<int> neighbours;
        public Vector3 position;
        public Vector3 resultantForce;
        public Vector3 velocity;
    }

    [Header("Simulation Visuals")]
    public float particleRadius = 0.1f;
    public Color particleColour = Color.blue;
    // Compute Buffer interaction with the sole purpose of GPU rendering for better comparison of performance
    public Material particleMaterial;
    Mesh particleMesh;
    ComputeBuffer argsBuffer;
    ComputeBuffer positions;

    Vector3[] particlePositions;

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
    Kernels kernels;

    Particle[] particles;
    float[] densities;
    float[] pressures;

    [Header("Fluid bounds")]
    public Vector3 minBounds;
    public Vector3 maxBounds;
    public float boundBounceDampening = 0.8f;


    Vector3 halfBounds;

    public Dictionary<uint, List<Particle>> spatialIndicies; // particles stored with their hash
    int hashTableSize;

    // Start is called before the first frame update
    void Start()
    {
        // Actual CPU implementation setup
        Time.timeScale = timeScale;
        kernels = new Kernels();
        InitParticles();
        hashTableSize = SpatialHash.NextPrime(numberOfParticles * 2);
        halfBounds = (maxBounds - minBounds) * 0.5f;

        // GPU Rendering Setup
        CreateLowPolySphereMesh(2);
        uint[] args =
        {
            particleMesh.GetIndexCount(0), (uint)numberOfParticles, particleMesh.GetIndexStart(0), particleMesh.GetBaseVertex(0), 0
        };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
        positions = new ComputeBuffer(numberOfParticles, sizeof(float) * 3);
        particlePositions = new Vector3[numberOfParticles];
        for (int i = 0; i < numberOfParticles; i++)
            particlePositions[i] = particles[i].position;
        positions.SetData(particlePositions);
        particleMaterial.SetBuffer("positions", positions);
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = timeScale;
        kernels.PrecomputeFactors(smoothingRadius);
        UpdateSpatialIndicies();
        UpdateParticles();
        // Render particles on the GPU
        particleMaterial.SetFloat("_Radius", particleRadius);
        particleMaterial.SetColor("_Color", particleColour);
        positions.SetData(particlePositions);
        Graphics.DrawMeshInstancedIndirect(particleMesh, 0, particleMaterial, new Bounds(minBounds, maxBounds), argsBuffer);
    }

    void UpdateSpatialIndicies()
    {
        spatialIndicies = new Dictionary<uint, List<Particle>>();
        // Calculate the cells of all particles and put in dict
        for (int i = 0; i < particles.Length; i++)
        {
            Vector3Int cell = SpatialHash.GetCell(particles[i].position, smoothingRadius);
            uint hash = SpatialHash.HashCell(cell, (uint)hashTableSize);
            if (!spatialIndicies.ContainsKey(hash))
            {
                spatialIndicies[hash] = new List<Particle>();
            }
            spatialIndicies[hash].Add(particles[i]);
        }
    }

    List<int> FindNeighbours(Vector3 pos, float radius)
    {
        List<int> neighbours = new List<int>();
        Vector3Int newKey;
        Vector3Int cell = SpatialHash.GetCell(pos, radius);
        float sqrRadius = radius * radius;
        // Check 27 cells arround current cell
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    // Get the hash of all cells within radius of particles for more explanation look at the paper above especially the appendix
                    newKey = new Vector3Int(i, j, k);
                    uint testKey = SpatialHash.HashCell(cell + newKey, (uint)hashTableSize);
                    if (spatialIndicies.ContainsKey(testKey))
                    {
                        var cellParticles = spatialIndicies[testKey];
                        foreach (Particle neighbour in cellParticles)
                        {
                            float sqrDistance = (pos - neighbour.position).sqrMagnitude;
                            if (sqrDistance < sqrRadius && pos != neighbour.position) // only add particles within the smoothing radius
                            {
                                bool inList = false;
                                for (int q = 0; q < neighbours.Count; q++)
                                {
                                    if (neighbour.id == neighbours[q])
                                    {
                                        inList = true;
                                        break;
                                    }
                                }
                                if (!inList)
                                {
                                    neighbours.Add(neighbour.id);
                                }
                            }
                        }
                    }
                }
            }
        }
        return neighbours;
    }

    void InitParticles()
    {
        particles = new Particle[numberOfParticles];
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

                    // Set the position of the particle
                    Particle particle = new Particle();
                    particle.position = particlePosition;
                    particle.velocity = initalVelocity;
                    particle.id = particlesCount;
                    //particle.colour = particleColour;
                    particle.resultantForce = Vector3.zero;
                    particles[particlesCount] = particle;
                    particlesCount++;
                }
            }
        }
    }


    // Each timestep update the particle positions and velocities
    void UpdateParticles()
    {
        densities = new float[numberOfParticles];
        pressures = new float[numberOfParticles];
        Parallel.For(0, numberOfParticles, i =>
        {
            Particle particle = particles[i];
            particle.neighbours = FindNeighbours(particle.position, smoothingRadius); // find all neighbours
            particles[i] = particle;
            densities[i] = CalculateDensity(particle);  // Calculate each particle density 
            pressures[i] = CalculatePressure(particle);
        });
        Parallel.For(0, numberOfParticles, i =>
        {
            Particle particle = particles[i];
            particle.resultantForce = Vector3.zero;
            if (particle.neighbours.Count > 0)
            {
                particle.resultantForce = CalculateInternalForces(particle);
                /*if (particle.id == 80)
                {
                     Debug.Log("Pressure Force: " + pressureForce + "Viscosity Force: " + viscosityForce +
                    "Surface Tension Force" + surfaceTensionForce + "Resultant Force: " + particle.resultantForce + "Particle Density: " + densities[i]);
                }*/
            }
            particle.resultantForce += buoyancy * (densities[i] - referenceDensity) * gravity;
            particle.resultantForce += gravity * densities[i]; // force from gravity  
            /* if (particle.id == 80)
                 Debug.Log("Force including gravity: " + particle.resultantForce);
             */
            particles[i] = particle;
        });
        Parallel.For(0, numberOfParticles, i =>
        {
            Particle particle = particles[i];
            Vector3 acceleration = Vector3.zero;
            if (densities[i] != 0)
            {
                acceleration = particle.resultantForce / densities[i];
            }
            /* if (particle.id == 80)
                Debug.Log("Accel " + acceleration + "Resultant Force: " + particle.resultantForce);
            */
            // Semi-Implicit Euler 
            particle.velocity += timeStep * acceleration;
            ComputeXSPHCorrection(particle);
            particle.position += timeStep * particle.velocity;

            particle = ResolveBoundsCollision(particle);

            particlePositions[i] = particle.position;
            //particle.colour = particleColour;
            particles[i] = particle;
        });
    }

    Particle ResolveBoundsCollision(Particle particle)
    {
        // Calculates localPosition within bounds and then uses that to see if inside outside of box and resolves issues related
        Vector3 localPos = new Vector3(Mathf.InverseLerp(minBounds.x, maxBounds.x, particle.position.x),
            Mathf.InverseLerp(minBounds.y, maxBounds.y, particle.position.y), Mathf.InverseLerp(minBounds.z, maxBounds.z, particle.position.z));
        // local sizes for particle radius for comparision 
        Vector3 localParticleRadiusSize = new Vector3(particleRadius / (maxBounds.x - minBounds.x), particleRadius / (maxBounds.y - minBounds.y),
            particleRadius / (maxBounds.z - minBounds.z));
        // The bouncing off the normal will have to be tested later when setup has more forces
        if (localPos.x <= 0f + localParticleRadiusSize.x || localPos.x >= 1f - localParticleRadiusSize.x)
        {
            particle.position.x = (localPos.x > 0.5f) ? maxBounds.x - (particleRadius + 0.00001f) : minBounds.x + (particleRadius + 0.00001f);
            particle.velocity.x *= -boundBounceDampening;
        }
        if (localPos.y <= 0f + localParticleRadiusSize.y || localPos.y >= 1f - localParticleRadiusSize.y)
        {
            particle.position.y = (localPos.y > 0.5f) ? maxBounds.y - (particleRadius + 0.00001f) : minBounds.y + (particleRadius + 0.00001f);
            particle.velocity.y *= -boundBounceDampening;
        }
        if (localPos.z <= 0f + localParticleRadiusSize.z || localPos.z >= 1f - localParticleRadiusSize.z)
        {
            particle.position.z = (localPos.z > 0.5f) ? maxBounds.z - (particleRadius + 0.00001f) : minBounds.z + (particleRadius + 0.00001f);
            particle.velocity.z *= -boundBounceDampening;
        }
        return particle;
    }

    float CalculateDensity(Particle particle)
    {
        float density = kernels.Poly6SmoothingKernel(smoothingRadius, 0); // start including self

        for (int i = 0; i < particle.neighbours.Count; i++)
        {
            float distance = Vector3.Distance(particle.position, particles[particle.neighbours[i]].position);
            float influence = kernels.Poly6SmoothingKernel(smoothingRadius, distance);
            density += influence;
        }
        return density * mass;
    }

    float CalculatePressure(Particle particle)
    {
        return gasStiffness * (Mathf.Pow(densities[particle.id] / referenceDensity, 7) - 1); // Tait equation expressed as P = k(density - reference density) ^ n where n = 1 to acheive computational efficiency
    }

    /* Vector3 CalculatePressureForce(Particle particle)
     {
         Vector3 pressureForce = Vector3.zero;

         float particlePressureDensity = pressures[particle.id] / (densities[particle.id] * densities[particle.id]);

         for (int j = 0; j < particle.neighbours.Count; j++)
         {
             Particle neighbour = particle.neighbours[j];
             Vector3 direction = particle.position - neighbour.position;
             float sqrDistance = direction.sqrMagnitude;
             if (sqrDistance == 0)
                 continue;

             float neighbourPressureDensity = pressures[neighbour.id] / (densities[neighbour.id] * densities[neighbour.id]);

             Vector3 kernelGradient = kernels.SpikySmoothingKernelGradient(smoothingRadius, direction);
             float pressureGradient = particlePressureDensity + neighbourPressureDensity;
             // if (particle.id == 0)
             //   {
             //       Debug.Log("Particle Pressure Density: " + particlePressureDensity + "Neighbour Pressure Density: " + neighbourPressureDensity + " Neighbour id: " + neighbour.id + "Kernel Gradient: " + kernelGradient + " Pressure Gradient" + pressureGradient + "Distance: " + distance + "Individual Force: " + (pressureGradient * kernelGradient * -densities[particle.id]));
             //  }
             pressureForce += pressureGradient * kernelGradient;

         }
         pressureForce *= -densities[particle.id];

         return pressureForce;
     }

     Vector3 CalculateViscosityForce(Particle particle)
     {
         Vector3 viscosityForce = Vector3.zero;
         for (int i = 0; i < particle.neighbours.Count; i++)
         {
             Particle neighbour = particle.neighbours[i];
             float distance = Vector3.Distance(particle.position, neighbour.position);
             if (distance == 0)
                 continue;
             float kernelLaplacian = kernels.ViscositySmoothingKernelLaplacian(smoothingRadius, distance);
             viscosityForce += (mass / densities[neighbour.id])
                 * (neighbour.velocity - particle.velocity) * kernelLaplacian; // Viscocity equation from same paper as pressure equation
                                                                               //   if (particle.id == 0)
                                                                               //   {
                                                                               //    Debug.Log("Individual viscosity Foree: " + ((mass / densities[neighbour.id]) * (neighbour.velocity - particle.velocity) * kernelLaplacian) * viscosityCoefficient + "Kernel Laplacian: " + kernelLaplacian + "Neighbour Velocity " + neighbour.velocity + "Particle Velocity " + particle.velocity + "neighbour Density: " + densities[neighbour.id]);
                                                                               // }
         }
         return viscosityForce * viscosityCoefficient;
     }

     Vector3 CalculateSurfaceTensionForce(Particle particle)
     {
         Vector3 normal = Vector3.zero;
         float colourFieldLaplacian = 0;
         for (int i = 0; i < particle.neighbours.Count; i++)
         {
             Particle neighbour = particle.neighbours[i];
             Vector3 direction = particle.position - neighbour.position;
             float distance = direction.magnitude;
             if (distance == 0)
                 continue;
             Vector3 kernelGradient = kernels.Poly6SmoothingKernelGradient(smoothingRadius, direction);
             float kernelLaplcian = kernels.Poly6SmoothingKernelLaplacian(smoothingRadius, distance);
             normal += kernelGradient / densities[neighbour.id];
             colourFieldLaplacian += kernelLaplcian / densities[neighbour.id];
         }
         if (normal.magnitude > surfaceTensionThreshold)
         {
             return Vector3.zero; // For stability of simulation
         }
         normal.Normalize();
         return -tensionCoefficient * normal * colourFieldLaplacian * mass;
     }*/

    Vector3 CalculateInternalForces(Particle particle)
    {
        Vector3 pressureForce = Vector3.zero;
        Vector3 viscosityForce = Vector3.zero;
        Vector3 normal = Vector3.zero;
        float colourFieldLaplacian = 0;
        float particleDensity = densities[particle.id];
        float particlePressureDensity = pressures[particle.id] / (particleDensity * particleDensity);
        for (int i = 0; i < particle.neighbours.Count; i++)
        {
            Particle neighbour = particles[particle.neighbours[i]];
            Vector3 direction = particle.position - neighbour.position;
            float distance = direction.magnitude;
            if (distance == 0)
                continue;

            // Calculate pressure
            float neighbourDensity = densities[neighbour.id];
            float neighbourPressureDensity = pressures[neighbour.id] / (neighbourDensity * neighbourDensity);
            Vector3 pressureKernelGradient = kernels.SpikySmoothingKernelGradient(smoothingRadius, direction);
            pressureForce += (particlePressureDensity + neighbourPressureDensity) * pressureKernelGradient;

            // Calculate viscosity
            float viscosityKernelLaplacian = kernels.ViscositySmoothingKernelLaplacian(smoothingRadius, distance);
            viscosityForce += (1 / neighbourDensity) * (neighbour.velocity - particle.velocity) * viscosityKernelLaplacian;

            // Surface Tension
            Vector3 tensionKernelGradient = kernels.Poly6SmoothingKernelGradient(smoothingRadius, direction);
            float tensionKernelLaplcian = kernels.Poly6SmoothingKernelLaplacian(smoothingRadius, distance);
            normal += tensionKernelGradient / neighbourDensity;
            colourFieldLaplacian += tensionKernelLaplcian / neighbourDensity;
        }
        pressureForce *= -particleDensity * mass;
        viscosityForce *= viscosityCoefficient * mass;
        if (normal.magnitude > surfaceTensionThreshold)
        {
            colourFieldLaplacian = 0; // For stability of simulation
        }
        normal.Normalize();
        return pressureForce + viscosityForce + (-tensionCoefficient * normal * colourFieldLaplacian * mass);
    }


    void ComputeXSPHCorrection(Particle p)
    {
        Vector3 XSPHvelocity = Vector3.zero;

        for (int i = 0; i < p.neighbours.Count; i++)
        {
            Particle n = particles[p.neighbours[i]];
            XSPHvelocity += ((n.velocity - p.velocity) * kernels.Poly6SmoothingKernel(smoothingRadius, Vector3.Distance(p.position, n.position)))
                / (densities[n.id] + densities[p.id]);
        }
        p.velocity += XSPHvelocity * XSPHsmoothingConstant * mass * mass;
        particles[p.id] = p;
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

    void OnDrawGizmos()
    {
        DrawBounds();
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

    // OnDestroy is called when the MonoBehaviour is being destroyed
    void OnDestroy()
    {
        if (argsBuffer != null)
        {
            argsBuffer.Release();
            argsBuffer = null;
        }
        if (positions != null)
        {
            positions.Release();
            positions = null;
        }
        Destroy(particleMesh);
    }
}
