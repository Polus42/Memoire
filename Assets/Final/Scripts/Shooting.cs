using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour {
    public GameObject bulletPrefab;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetMouseButtonDown(0))
        {
            InvokeRepeating("fire", 0, 0.2f);
        }
        if (Input.GetMouseButtonUp(0))
        {
            CancelInvoke();
        }
    }
    void fire()
    {
        Vector3 p = new Vector3();
        Camera c = Camera.main;
        Vector2 mousePos = new Vector2();

        // Get the mouse position from Event.
        // Note that the y position from Event is inverted.
        mousePos.x = Input.mousePosition.x;
        mousePos.y = /*c.pixelHeight -*/ Input.mousePosition.y;

        p = c.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10000));
        GameObject g = Instantiate(bulletPrefab);
        g.GetComponent<Rigidbody>().velocity = Vector3.zero;
        g.transform.position = transform.position + Camera.main.transform.forward * 2;
        g.GetComponent<Rigidbody>().AddForce((p - transform.position) / 100, ForceMode.VelocityChange);
        Debug.DrawLine(transform.position, p, Color.red, 5);
    }
}
