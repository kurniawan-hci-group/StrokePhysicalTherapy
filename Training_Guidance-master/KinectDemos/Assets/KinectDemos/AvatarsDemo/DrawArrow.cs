using UnityEngine;
using System.Collections;
using UnityEditor;

/* Design the initial arrow
 * In Blender put the line end at (0,0,0), otherwise the position here is not accurate
 * Line end represents the position of the arrow
 * Initial direction is along the y-axis (0,0,1)
 */

public class DrawArrow : MonoBehaviour
{

    public Vector3 start;  // world space coordinate
    public Vector3 endpoint;
    //LineRenderer line;


    // Use this for initialization
    void Start()
    {
        transform.position = new Vector3(0, 0, -1);   // invisible
        /*
        line = gameObject.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Particles/Additive"));
        line.SetColors(Color.green, Color.green);
         * */
    }

    void Update()
    {
        /*
        line.SetWidth(0.02F, 0.02F);
        line.SetPosition(0, start);
        line.SetPosition(1, endpoint);
        */

        // position of the line end
        transform.position = start;

        // rotation of the arrow
        Vector3 unit = (endpoint - start).normalized;  // unit vector of the arrow direction
        Vector3 basic = new Vector3(0, 1, 0);  // stand for the axis(rotation) for the initial cone
        float alpha = Vector3.Angle(basic, unit);
        Vector3 axis = Vector3.Cross(basic, unit).normalized;  // rotate around the axis
        transform.rotation = Quaternion.identity;
        transform.Rotate(axis, alpha, Space.World);

        // scale of the arrow
        //original length = 0.75
        float length = (endpoint - start).magnitude;
        transform.localScale = new Vector3(2*length / 0.75f, length / 0.75f, 2*length / 0.75f);
    }

}
