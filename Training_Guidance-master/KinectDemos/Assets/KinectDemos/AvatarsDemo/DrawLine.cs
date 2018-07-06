using UnityEngine;
using System.Collections;

public class DrawLine : MonoBehaviour {

    public Vector3 start;
    public Vector3 endpoint;
    public float width;
    private LineRenderer line;

	// Use this for initialization
	void Start () {

        line = gameObject.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Particles/Additive"));
        line.SetColors(Color.green, Color.green);
	}
	
	void Update () {  // start and endpoint are updated in AvatarController.cs

        // show the right position
        line.SetWidth(width, width);
        line.SetPosition(0, start);
        line.SetPosition(1, endpoint);

	}
}

