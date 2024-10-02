using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialHash
{
    // large primes used for hashing
    const int p1 = 73856093;
    const int p2 = 19349663; // primes from Teschner, Matthias & Heidelberger, Bruno & Müller, Matthias & Pomeranets, Danat & Gross, Markus. (2003). Optimized Spatial Hashing for Collision Detection of Deformable Objects. VMV’03: Proceedings of the Vision, Modeling, Visualization. 3. 
    const int p3 = 83492791;

    public static Vector3Int GetCell(Vector3 position, float radius) //Based off the Discretizing function of Kelager, M. (2006). Lagrangian fluid dynamics using smoothed particle hydrodynamics.
    {
        int x = Mathf.FloorToInt(position.x / radius);
        int y = Mathf.FloorToInt(position.y / radius);
        int z = Mathf.FloorToInt(position.z / radius);
        return new Vector3Int(x, y, z);
    }

    public static uint HashCell(Vector3Int cell, uint tableSize)
    {
        // hash(x,y, z) =( x * p1 xor y * p2 xor z* p3) mod n taken from paper
        return (((uint)cell.x * p1) ^ ((uint)cell.y * p2) ^ ((uint)cell.z * p3)) % tableSize;
    }

    public static bool IsPrime(int number)
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

    // Function to find the next prime number after the given for optimising spatial hashing
    public static int NextPrime(int number)
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