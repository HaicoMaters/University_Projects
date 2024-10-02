// Same as Cpu class just converted to be used for compute shader

// large primes used for hashing
#define p1 73856093
#define p2 19349663 // primes from Teschner, Matthias & Heidelberger, Bruno & Müller, Matthias & Pomeranets, Danat & Gross, Markus. (2003). Optimized Spatial Hashing for Collision Detection of Deformable Objects. VMV’03: Proceedings of the Vision, Modeling, Visualization. 3. 
#define p3 83492791

// Function to discretize position into cell coordinates
uint3 GetCell(float3 position, float radius)
{
    int3 cell = int3(floor(position / radius));
    return uint3(cell);
}

// Function to hash cell coordinates
uint HashCell(uint3 cell, uint tableSize)
{
    // hash(x,y, z) =( x * p1 xor y * p2 xor z* p3) mod n taken from paper
    return (((cell.x * p1) ^ (cell.y * p2) ^ (cell.z * p3)) % tableSize);
}