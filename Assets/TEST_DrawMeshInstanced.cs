using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST_DrawMeshInstanced : MonoBehaviour
{

    public Mesh mesh;

    public Material material;
    

    void Update()
    {
        Graphics.DrawMeshInstanced(mesh, 0, material, new Matrix4x4[] {transform.localToWorldMatrix});
    }
}
