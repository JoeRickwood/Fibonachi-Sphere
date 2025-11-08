using MIConvexHull;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


public class Vertex3D : IVertex
{
    public double[] Position { get; private set; }
    public int Index { get; private set; }

    public Vertex3D(double x, double y, double z, int index = -1)
    {
        Position = new double[] { x, y, z };
        Index = index;
    }
}

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FibonachiDome : MonoBehaviour
{
    public AnimationCurve simulationCurve;

    public int maxPoints;
    private int i = 220;
    public float simulationSpeed;

    public Material mainMaterial;

    public Slider slider;

    public Transform cameraRig;

    public float radius;

    float t = 0;

    float rotX = 0f;
    float rotY = 0f;

    private void Awake()
    {
        i = maxPoints;
    }

    private void Update()
    {
        SetMaxPoints(Mathf.FloorToInt(slider.value));

        t += Time.deltaTime * simulationSpeed;

        float curT = ((Mathf.Sin(t) + 1.0f) / 2.0f) * (Mathf.PI * 2.0f);

        i = Mathf.FloorToInt(Mathf.Lerp(4, maxPoints, simulationCurve.Evaluate(curT / (Mathf.PI * 2.0f))));


        if(Input.GetMouseButton(1))
        {
            rotX -= Input.GetAxis("Mouse Y");
            rotX = Mathf.Clamp(rotX, -90.0f, 90.0f);

            rotY += Input.GetAxis("Mouse X");
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            ResetSimulation();
        }
        
        cameraRig.rotation = Quaternion.Slerp(cameraRig.rotation, Quaternion.Euler(rotX, rotY, 0f), Time.deltaTime * 5.0f);

        GenerateMesh(simulationCurve.Evaluate(curT / (Mathf.PI * 2.0f)));
    }

    public void ResetSimulation()
    {
        t = 0;
    }


    void GenerateMesh(float _t)
    {
        if(_t <= 0.05f)
        {
            GetComponent<MeshFilter>().mesh = null;

            return;
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>(); 
        List<Vector3> normals = new List<Vector3>();

        int count = Mathf.Max(1, i);
        float phi = Mathf.PI * (3f - Mathf.Sqrt(5f)); 

        for (int curN = 0; curN < count; curN++)
        {
            float y = 1f - (curN / (float)(count - 1)) * 2f; 
            float radius = Mathf.Sqrt(1f - y * y) * _t;           
            float theta = phi * curN;

            float x = Mathf.Cos(theta) * radius;
            float z = Mathf.Sin(theta) * radius;

            Vector3 diff = new Vector3(x, y * _t, z).normalized;

            normals.Add(new Vector3(diff.x, diff.y, diff.z));

            vertices.Add(new Vector3(x, y * _t, z));
        }

        List<Vertex3D> points3D = new List<Vertex3D>();
        for (int curN = 0; curN < vertices.Count; curN++)
        {
            var v = vertices[curN];
            points3D.Add(new Vertex3D(v.x, v.y, v.z, curN));
        }

        var hull = ConvexHull.Create(points3D, 1e-5);

        foreach (var face in hull.Result.Faces)
        {
            triangles.Add(face.Vertices[0].Index);
            triangles.Add(face.Vertices[1].Index);
            triangles.Add(face.Vertices[2].Index);

            double[] norm = face.Normal;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetNormals(normals);

        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = mainMaterial;
    }

    public void SetMaxPoints(int _value)
    {
        maxPoints = _value;
    }

    public void Quit()
    {
        Application.Quit();
    }

}
