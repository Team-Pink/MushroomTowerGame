using UnityEngine;

[ExecuteInEditMode]
public class FlowGrid : MonoBehaviour
{
    [Header("Grid Tiles")]
    [SerializeField] private int xSubdivs = 50;
    [SerializeField] private int zSubdivs = 50;

    private Vector2[,] tiles;

    [Header("Grid Scale")]
    [SerializeField] private float width = 20;
    [SerializeField] private float height = 20;

    private float xMin;
    private float xMax;
    private float zMin;
    private float zMax;

    public void Awake()
    {
        Initialise();

        CreateGrid();
        //populate the grid with values somehow...?
    }

    private void OnValidate()
    {
        Initialise();
        CreateGrid();
    }

    private void Initialise()
    {
        xMin = -width / 2;
        xMax = width / 2;

        zMin = -height / 2;
        zMax = height / 2;
    }

    private void CreateGrid()
    {
        tiles = new Vector2[xSubdivs, zSubdivs];

        for (int x = 0; x < xSubdivs; x++)
        {
            float xProgress = ((float)x) / xSubdivs;

            for (int z = 0; z < zSubdivs; z++)
            {
                float zProgress = ((float)z) / zSubdivs;

                float xPos = Mathf.Lerp(xMin, xMax, xProgress);
                float zPos = Mathf.Lerp(zMin, zMax, zProgress);

                tiles[x, z] = new Vector2(xPos, zPos);
            }
        }
    }

    public Vector3 SwizzleXZ(Vector2 vector2)
    {
        return new Vector3(vector2.x, 0, vector2.y);
    }

    public void OnDrawGizmos()
    {
        //Gizmos.DrawLine() Draw the grid
        for (int x = 0; x < xSubdivs; x++)
        {
            for(int z = 0; z < zSubdivs; z++)
            {
                if (x < xSubdivs - 1)
                {
                    Gizmos.DrawLine(SwizzleXZ(tiles[x, z]), SwizzleXZ(tiles[x + 1, z]));
                }
                else
                {
                    Gizmos.DrawLine(SwizzleXZ(tiles[x, z]), SwizzleXZ(new Vector2(xMax, tiles[x, z].y)));
                }

                if (z < zSubdivs - 1)
                {
                    Gizmos.DrawLine(SwizzleXZ(tiles[x, z]), SwizzleXZ(tiles[x, z + 1]));
                }
                else
                {
                    Gizmos.DrawLine(SwizzleXZ(tiles[x, z]), SwizzleXZ(new Vector2(tiles[x, z].x, zMax)));
                }
            }
        }

        //Gizmos.DrawCube() Draw a cube in the tile that the mouse is over. Make use of GetTileCoords
    }

    private void GetTileCoords(Vector3 position, out int xCoord, out int zCoord)
    {
        //TODO
        xCoord = 0;
        zCoord = 0;
    }


    private Vector2 GetFlowAtPoint(Vector3 position)
    {
        int xPos, zPos;
        GetTileCoords(position, out xPos, out zPos);
        return tiles[xPos, zPos];
    }
}
