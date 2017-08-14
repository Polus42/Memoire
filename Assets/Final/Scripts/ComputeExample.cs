using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeExample : MonoBehaviour {
    public ComputeShader computeShader;
    ComputeBuffer positionBuffer;
    int CSMainIndex;

	void Start () {
        // Allocation d'un buffer de 100 élément d'une taille de 4*3 bytes
        positionBuffer = new ComputeBuffer(100, 4*3); // 4 byte par float * 3 (float3)
        // On remplit le buffer avec des données initiales
        Vector3[] pos = new Vector3[100];
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = Random.onUnitSphere;
        }
        positionBuffer.SetData(pos);
        // On retrouve l'indice du kernel
        CSMainIndex = computeShader.FindKernel("CSMain");
        // On attribue le buffer au compute shader
        computeShader.SetBuffer(CSMainIndex, "Position", positionBuffer);
	}
	
	void Update () {
        // On lance le calcul sur CSMain
        computeShader.Dispatch(CSMainIndex, 100, 1, 1);
	}
}
