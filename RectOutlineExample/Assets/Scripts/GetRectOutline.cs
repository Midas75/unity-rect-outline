using System.Collections;
using UnityEngine;

public class GetRectOutline : MonoBehaviour
{
    public enum BorderType
    {
        RENDER_BOUNDS,
        MESH_BOUNDS,
        MESH_VERTICES,
        MESH_VERTICES_SAMPLED,
    }
    public BorderType borderType;
    public string targetTag = "ObjectDetection";
    private static GameObject[] targets;
    private Vector3[][] cacheMeshBounds;
    public bool drawUI = true;
    public Camera cam;
    private Mesh cacheMesh;
    public uint sampleInterval = 5;
    public int sampleMoreThan = 8;
    // Start is called before the first frame update
    void Start()
    {
        targets = GameObject.FindGameObjectsWithTag(targetTag);
        cacheMeshBounds = new Vector3[targets.Length][];
        cam = GetComponent<Camera>();
        cacheMesh = new();
        StartCoroutine(UpdateMeshBounds());
    }
    Vector3[] SampleVertices(Vector3[] vertices, uint sampleInterval)
    {
        var new_vertices = new Vector3[Mathf.CeilToInt(1.0f * vertices.Length / sampleInterval)];
        for (uint i = 0; i * sampleInterval < vertices.Length; i++)
        {
            new_vertices[i] = vertices[i * sampleInterval];
        }
        return new_vertices;
    }
    Vector3[] GetLocalVertices(GameObject target, uint sampleInterval = 1)
    {
        if (target.GetComponent<MeshFilter>())
        {
            var vertices = target.GetComponent<MeshFilter>().sharedMesh.vertices;
            if (sampleInterval == 1 || vertices.Length <= sampleMoreThan)
            {

            }
            else
            {
                uint newLength = (uint)Mathf.CeilToInt(1.0f * vertices.Length / sampleInterval);
                if (newLength <= 8)
                {
                    vertices = BoundsTo8Points(target.GetComponent<Renderer>().localBounds);
                }
                else
                {
                    vertices = SampleVertices(vertices, sampleInterval);
                }

            }
            return vertices;
        }
        if (target.GetComponent<MeshRenderer>())
        {
            return BoundsTo8Points(target.GetComponent<MeshRenderer>().localBounds);
        }
        else if (target.GetComponent<SkinnedMeshRenderer>())
        {

            var smr = target.GetComponent<SkinnedMeshRenderer>();
            smr.BakeMesh(cacheMesh, true);
            var vertices = cacheMesh.vertices;
            if (sampleInterval == 1 || cacheMesh.vertices.Length <= sampleMoreThan)
            {

            }
            else
            {
                uint newLength = (uint)Mathf.CeilToInt(1.0f * vertices.Length / sampleInterval);
                if (newLength <= 8)
                {
                    vertices = BoundsTo8Points(target.GetComponent<Renderer>().localBounds);
                }
                else
                {
                    vertices = SampleVertices(vertices, sampleInterval);
                }
            }
            return vertices;
        }
        else
        {
            return null;
        }
    }
    IEnumerator UpdateMeshBounds()
    {
        while (true)
        {
            if (borderType != BorderType.MESH_BOUNDS)
            {

            }
            else
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i].GetComponent<MeshRenderer>())
                    {
                        cacheMeshBounds[i] = BoundsTo8Points(targets[i].GetComponent<MeshRenderer>().localBounds);
                    }
                    else if(targets[i].GetComponent<SkinnedMeshRenderer>())
                    {
                        targets[i].GetComponent<SkinnedMeshRenderer>().BakeMesh(cacheMesh, true);
                        cacheMeshBounds[i] = BoundsTo8Points(cacheMesh.bounds);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
    Vector3[] BoundsTo8Points(Bounds bounds)
    {
        return new Vector3[8]{
            new(bounds.min.x,bounds.min.y,bounds.min.z),
            new(bounds.max.x,bounds.min.y,bounds.min.z),
            new(bounds.min.x,bounds.max.y,bounds.min.z),
            new(bounds.min.x,bounds.min.y,bounds.max.z),
            new(bounds.max.x,bounds.max.y,bounds.min.z),
            new(bounds.max.x,bounds.min.y,bounds.max.z),
            new(bounds.min.x,bounds.max.y,bounds.max.z),
            new(bounds.max.x,bounds.max.y,bounds.max.z)
        };

    }
    private void OnGUI()
    {
        Vector3[][] testPoints = new Vector3[targets.Length][];
        switch (borderType)
        {
            case BorderType.RENDER_BOUNDS:
                for (int i = 0; i < targets.Length; i++)
                {
                    var bounds = targets[i].GetComponent<Renderer>().localBounds;
                    testPoints[i] = BoundsTo8Points(bounds);
                }
                break;
            case BorderType.MESH_BOUNDS:
                for (int i = 0; i < targets.Length; i++)
                {
                    testPoints[i] = cacheMeshBounds[i];
                }
                break;
            case BorderType.MESH_VERTICES:

                for (int i = 0; i < targets.Length; i++)
                {
                    testPoints[i] = GetLocalVertices(targets[i]);
                }
                break;
            case BorderType.MESH_VERTICES_SAMPLED:
                for (int i = 0; i < targets.Length; i++)
                {
                    testPoints[i] = GetLocalVertices(targets[i], sampleInterval);
                }
                break;
        }
        Vector3 mmin = new(0f, 0f, cam.nearClipPlane), mmax = new(Screen.width, Screen.height, cam.farClipPlane);
        for (int i = 0; i < targets.Length; i++)
        {
            if (testPoints[i] == null) continue;
            Vector3 max = new(float.MinValue, float.MinValue, float.MinValue), min = new(float.MaxValue, float.MaxValue, float.MaxValue);
            foreach (var point in testPoints[i])
            {
                var worldPoint = targets[i].transform.TransformPoint(point);
                var screenPoint = cam.WorldToScreenPoint(worldPoint);
                if (screenPoint.z < cam.nearClipPlane)
                {
                    continue;
                }
                min = Vector3.Min(min, screenPoint);
                max = Vector3.Max(max, screenPoint);
            }
            min = Vector3.Max(mmin, min);
            max = Vector3.Min(mmax, max);
            if (drawUI) DrawScreenRect(new(min.x, min.y), new(max.x, max.y), new(1, 0, 0, 0.5f));
        }
    }
    void DrawScreenRect(Vector3 topLeft, Vector3 bottomRight, Color color)
    {
        // 计算矩形宽高
        float width = bottomRight.x - topLeft.x;
        float height = bottomRight.y - topLeft.y;
        // 保存当前的GUI颜色
        Color oldColor = GUI.color;

        // 设置新的GUI颜色
        GUI.color = color;

        // 绘制矩形
        GUI.DrawTexture(new Rect(topLeft.x, Screen.height - topLeft.y - height, width, height), Texture2D.whiteTexture);

        // 恢复原来的GUI颜色
        GUI.color = oldColor;
    }
}
