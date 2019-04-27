using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassicSpawner : MonoBehaviour
{

    public GameObject prefab;

    public int numX;

    public int numY;

    public float gap;

    void Start()
    {
        for (int x = 0; x < numX; x++)
        {
            for (int y = 0; y < numY; y++)
            {
                Instantiate(prefab, new Vector3(transform.position.x + gap * x, transform.position.y, transform.position.z + gap * y),
                    Quaternion.AngleAxis(Random.value * 360f, Vector3.up));
            }
        }
    }
}
