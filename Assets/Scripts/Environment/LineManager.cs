using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour
{
    public static LineManager inst;
    public GameObject lineObject;

    public float lineSize;

    public List<LineRenderer> oldRenderers;
    private int newRenderCount;

    private void Awake()
    {
        inst = this;
        oldRenderers = new List<LineRenderer>();
        newRenderCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (oldRenderers.Count >= newRenderCount)
        {
            for (int i = 0; i < oldRenderers.Count - newRenderCount; ++i)
            {
                LineRenderer lr = oldRenderers[oldRenderers.Count - 1];
                oldRenderers.Remove(lr);
                Destroy(lr);
            }
        }
        newRenderCount = 0;
    }

    public void DrawLine(Vector3 start, Vector3 end, Color color)
    {

        newRenderCount++;
        LineRenderer lr = null;
        // reuse old objects
        if (oldRenderers.Count >= newRenderCount)
        {
            lr = oldRenderers[newRenderCount - 1];
            lr.startWidth = lineSize;
            lr.endWidth = lineSize;
        }
        else
        {
            GameObject newLine = new GameObject();
            newLine.transform.parent = transform;
            lr = newLine.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.startWidth = lineSize;
            lr.endWidth = lineSize;
            lr.useWorldSpace = false;
            lr.receiveShadows = false;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            oldRenderers.Add(lr);
        }
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startColor = color;
        lr.endColor = color;

    }
}
