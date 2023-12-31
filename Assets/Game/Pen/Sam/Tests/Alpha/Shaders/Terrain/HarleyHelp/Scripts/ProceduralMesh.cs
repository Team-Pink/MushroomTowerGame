using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour
{
#if UNITY_EDITOR

    [ContextMenu("Construct Shape")]
    void ConstructMesh()
    {
        mesh = new Mesh { name = "Procedural Mesh" };
        GetComponent<MeshFilter>().mesh = mesh;
        GenerateMesh();
        vertices = mesh.vertices;
        normals = mesh.normals;
        tangents = mesh.tangents;
    }
   /* [ContextMenu("Save Shape")]
    void SaveShape()
    {
        if (mesh != null)
        {
            string meshName = Path.GetFileNameWithoutExtension(gameObject.name);
            string localPath = Path.Combine("Assets", "Meshes", meshName);
            Directory.CreateDirectory(localPath);

            var filters = gameObject.GetComponentsInChildren<MeshFilter>();
            foreach (var filter in filters)
            {
                var id = filter.sharedMesh.GetInstanceID();
                AssetDatabase.CreateAsset(filter.sharedMesh, Path.Combine(localPath, id + ".asset"));
            }

            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath + ".prefab");

            PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, localPath, InteractionMode.UserAction);
        }
    }*/

#endif

    Vector3[] vertices, normals;
    Vector4[] tangents;

    public bool drawVerts;

    // Array so that the type of mesh being generated can be chosen using the below enum
    static MeshJobScheduleDelegate[] jobs =
    {        
        MeshJob<SharedSquareGrid, SingleStream>.ScheduleParallel,     
    };

    public enum MeshType
    {
        SharedSquareGrid
    };

    [SerializeField]
    MeshType meshType;

    // Clamped for testing periods
    [SerializeField, Range(1, 50)]
    int resolution = 1;

    Mesh mesh;

    void Awake()
    {
        // Make the new mesh to be available for the GenerateMesh func to have one ready to
        // change the values on, attached and ready to go on the Mesh Filter
        mesh = new Mesh { name = "Procedural Mesh" };
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void OnValidate() => enabled = true;

    void Update()
    {
        GenerateMesh();
        // Set the enable to false so that Update stops running until OnValidate
        // re-activates it
        enabled = false;

        vertices = mesh.vertices;
        normals = mesh.normals;
        tangents = mesh.tangents;
        
    }

    private void OnDrawGizmos()
    {
        if(mesh == null || !drawVerts)
        {
            return;
        }
        
        for(int i = 0; i < vertices.Length; i++)
        {
            Vector3 position = vertices[i] + this.gameObject.transform.position;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(position, 0.02f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(position, normals[i] * 0.25f);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(position, tangents[i]*0.25f);
        }
    }

    void GenerateMesh()
    {
        // Allocates a writable MeshData struct for Mesh creation using C# jobs
        // with the size of 1 , meaning it will hold one mesh.
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        // Get the delegate from the enum to send the parameters to the MeshJob's
        // ScheduleJob func
        jobs[(int)meshType](mesh, meshData, resolution, default).Complete();

        // Once the job has finished the compilation of the mesh data apply it to
        // the mesh and then discard the job
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
    }
}