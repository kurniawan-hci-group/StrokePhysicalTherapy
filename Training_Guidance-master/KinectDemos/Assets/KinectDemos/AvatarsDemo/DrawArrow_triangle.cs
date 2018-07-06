using UnityEngine;
using System.Collections;
using UnityEditor;


public class DrawArrow_triangle : MonoBehaviour  // 2-d arrow
{

    public Vector3 start;  // world space
    public Vector3 endpoint;

    private LineRenderer line;
    private Mesh mesh;

    // Use this for initialization
    void Start()
    {
        // line
        line = gameObject.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Particles/Additive"));
        line.SetColors(Color.red, Color.red);
        // triangle
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        mesh = GetComponent<MeshFilter>().mesh;  // default red

    }

    void Update()
    {  
        // start and endpoint are updated in AvatarController.cs

        // draw the line
        line.SetWidth(0.015F, 0.015F);
        line.SetPosition(0, start);
        line.SetPosition(1, endpoint);
        
        // draw the triangle
        mesh.Clear();
        
        Vector3 p1 = endpoint - transform.position;
        Vector3 p2 = ArrowTri(start, endpoint, 0) - transform.position;
        Vector3 p3 = ArrowTri(start, endpoint, 1) - transform.position;
        
        /*
        Vector3 p1 = new Vector3(0, 0, 0) - transform.position;
        Vector3 p2 = new Vector3(0, 1, 0) - transform.position;
        Vector3 p3 = new Vector3(1, 0, 0) - transform.position;
        */
        mesh.vertices = new Vector3[] { p1, p2, p3 };
        mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
        mesh.triangles = new int[] { 0, 1, 2 };
    }


    Vector3 ArrowTri(Vector3 start, Vector3 endpoint, int index)
    {
        if (start.z == endpoint.z)  // in the x-y plane, not sure now....
        {
            Vector3 line = endpoint - start;
            float cos1 = Mathf.Cos(Mathf.PI / 4);  // anti-clockwise, 45 degree
            float sin1 = Mathf.Sin(Mathf.PI / 4);
            float cos2 = Mathf.Cos(-Mathf.PI / 4);  // clockwise, -45 degree
            float sin2 = Mathf.Sin(-Mathf.PI / 4);
            Vector3 lineRotation;
            if (index == 0)
            {
                lineRotation = new Vector3(line.x * cos1 - line.y * sin1, line.x * sin1 + line.y * cos1, line.z);
            }
            else
            {
                lineRotation = new Vector3(line.x * cos2 - line.y * sin2, line.x * sin2 + line.y * cos2, line.z);
            }
            lineRotation.Normalize();

            return endpoint - 0.5f * lineRotation;
        }

        return Vector3.zero;
    }
}
