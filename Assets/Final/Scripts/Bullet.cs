using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Bullet that can interact with swarms
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class Bullet : MonoBehaviour {
    /// <summary>
    /// List of all the bullets
    /// </summary>
    public static List<GameObject> bullets = new List<GameObject>();
    public static int maxBulletNumber = 10;
    private static int numberSpawned = 0;

	void Start () {
        if(bullets.Count!=maxBulletNumber)
        {
            bullets.Add(gameObject);
        }
        else
        {
            Destroy(bullets[numberSpawned]);
            bullets[numberSpawned] = gameObject;
        }
        numberSpawned++;
        numberSpawned %= maxBulletNumber;
	}
    public static Vector3[] getPositionsArray()
    {
        Vector3[] positions = new Vector3[maxBulletNumber];
        for (int i = 0; i < bullets.Count; i++)
        {
            positions[i] = bullets[i].transform.position;
        }
        return positions;
    }
}
