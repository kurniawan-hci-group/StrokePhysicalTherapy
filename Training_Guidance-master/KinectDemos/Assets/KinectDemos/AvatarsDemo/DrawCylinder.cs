using UnityEngine;
using System.Collections;

public class DrawCylinder : MonoBehaviour {

    public Vector3 start;
    public Vector3 endpoint;
    public float width;

	// Use this for initialization
	void Start () {
        transform.position = new Vector3(0, 0, -1);
	}
	
	void Update () {  // start and endpoint are updated in AvatarController.cs

        // position of the line end
        transform.position = start;

        // rotation of the cylinder
        Vector3 unit = (endpoint - start).normalized;  // unit vector of the cylinder direction
        Vector3 basic = new Vector3(0, 1, 0);  // stand for the axis(rotation) for the initial cylinder
        float alpha = Vector3.Angle(basic, unit);
        Vector3 axis = Vector3.Cross(basic, unit).normalized;  // rotate around the axis
        transform.rotation = Quaternion.identity;
        transform.Rotate(axis, alpha, Space.World);

        // scale of the cyclinder
		// y: length, xz: cross section
		// original diameter 0.2
		float length = (endpoint - start).magnitude;  //original length = 2

		transform.localScale = new Vector3(5.0f * width, 0.5f * length, 5.0f * width);
    

	}
}

