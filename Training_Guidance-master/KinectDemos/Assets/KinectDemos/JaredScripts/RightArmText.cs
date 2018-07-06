using UnityEngine;
using System.Collections;

public class RightArmText : MonoBehaviour
{

    public Vector3 position;
    public Vector3 offset;
    public string guidance;
    LineRenderer line;

    // Use this for initialization
    void Start()
    {
        transform.position = new Vector3(0, 0, -1);
        line = gameObject.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Particles/Additive"));
        line.SetColors(Color.white, Color.white);
    }

    // Update is called once per frame
    void Update()
    {
        // show guidance message
        transform.position = position + offset;
        guidance = ShortGuidance(guidance, 30);
        GetComponent<TextMesh>().text = guidance;
        GetComponent<TextMesh>().characterSize = 0.01f;
        GetComponent<TextMesh>().fontSize = 90;

        // add a line
        line.SetWidth(0.01F, 0.01F);
        line.SetPosition(0, position);
        if (offset.x < 0)  // move to the left
        {
            line.SetPosition(1, position + offset + new Vector3(1.1f, -0.05f, 0f));
        }
        else
        {
            line.SetPosition(1, position + offset + new Vector3(0f, -0.05f, 0f));
        }

    }

    string ShortGuidance(string guidance, int n)  // at most n letters a line
    {
        if (guidance.Length == 0)   // important
            return guidance;

        string guidance_new = "";

        char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
        string[] words = guidance.Split(delimiterChars);

        int thisline = 0;   // current length in this line
        foreach (string s in words)
        {
            if (thisline + s.Length > n)  // cannot be the first word // a new line
            {
                guidance_new = guidance_new + "\n" + s + " ";
                thisline = s.Length + 1;
            }
            else
            {
                guidance_new = guidance_new + s + " ";
                thisline = thisline + s.Length + 1;
            }
        }

        return guidance_new;
    }
}
