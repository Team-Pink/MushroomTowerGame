using UnityEngine;

// Will be working on this 24/08. Working on applying next best target and will work in directions after that.

public class Tile
{
    public Tile bestNextTile;
    public float distanceFromCentreTile;
    //public Vector2 FlowDirection { get; private set; }
    public float InkThickness { get; private set; }
    private readonly bool muddy;
    public float SpeedMultiplier
    {
        get
        {
            if (muddy)
                return 0.5f * (1 - InkThickness);
            else
                return 1 - InkThickness;
        }
    }

    public Tile(float distanceFromCentreTileInit = 0, bool muddyInit = false)
    {
        distanceFromCentreTile = distanceFromCentreTileInit;
        muddy = muddyInit;
    } // Constructor

    /*public void ApplyDirection(Vector2 flowDirectionInit)
    {
        FlowDirection = flowDirectionInit;
    }*/

    public void ClearInk()
    {
        InkThickness = 0;
    }

    public void SetInkThickness(float inkThicknessInit)
    {
        InkThickness = inkThicknessInit;
    }
}

[ExecuteInEditMode]
public class FlowGrid : MonoBehaviour
{
    [Header("Grid Tiles")]
    [SerializeField] int xSubdivs = 200;
    [SerializeField] int zSubdivs = 200;

    private Tile[,] tiles;

    [Header("Grid Scale")]
    [SerializeField] float gridWidth = 500;
    [SerializeField] float gridHeight = 500;
    private float TileWidth()
    {
        return gridWidth / xSubdivs;
    }
    private float TileHeight()
    {
        return gridHeight / zSubdivs;
    }

    private float xMin;
    private float xMax;
    private float zMin;
    private float zMax;

    public void Awake()
    {
        Initialise();

        CreateGrid();
        // populate the grid with values somehow...?
    }

    private void OnValidate()
    {
        Initialise();
        CreateGrid();
    }

    private void Initialise()
    {
        xMin = -gridWidth * 0.5f;
        xMax = gridWidth * 0.5f;

        zMin = -gridHeight * 0.5f;
        zMax = gridHeight * 0.5f;
    }

    private void CreateGrid()
    {
        tiles = new Tile[xSubdivs, zSubdivs];

        for (int xIndex = 0; xIndex < xSubdivs; xIndex++)
        {
            for (int zIndex = 0; zIndex < zSubdivs; zIndex++)
            {
                tiles[xIndex, zIndex] = new Tile();
            }
        }

        /*
        int iterationCount = 1;

        int xMidpoint = xSubdivs / 2;
        int zMidpoint = zSubdivs / 2;

        for (int xIndex = 0; xIndex < iterationCount; xIndex++)
        {
            int xFromCentre = xIndex - xMidpoint;
            for (int zIndex = 0; zIndex < iterationCount; zIndex++)
            {
                int zFromCentre = zIndex - zMidpoint;
                

            }
        }*/
    }

    private Vector2 GetTilePos(float xCoord, float zCoord)
    {
        return new Vector2(xCoord, zCoord);
    }

    public Vector3 SwizzleXZY(float xCoord, float zCoord, float yCoord = 0)
    {
        return new Vector3(xCoord, yCoord, zCoord);
    }

    public void OnDrawGizmos()
    {
        // Draw the grid
        for (int xIndex = 0; xIndex < xSubdivs+1; xIndex++)
        {
            float xCoord = TileWidth() * (xIndex - xSubdivs * 0.5f);
            float nextXCoord = xCoord + TileWidth();

            for(int zIndex = 0; zIndex < zSubdivs+1; zIndex++)
            {
                float zCoord = TileHeight() * (zIndex - zSubdivs * 0.5f);
                float nextZCoord = zCoord + TileHeight();

                if (xIndex < xSubdivs)
                    Gizmos.DrawLine(SwizzleXZY(nextXCoord, zCoord), SwizzleXZY(xCoord, zCoord));
                if (zIndex < zSubdivs)
                    Gizmos.DrawLine(SwizzleXZY(xCoord, zCoord), SwizzleXZY(xCoord, nextZCoord));
            }
        }

        // Gizmos.DrawCube() Draw a cube in the tile that the mouse is over. Make use of GetTileCoords
    }

    public void GetTileCoords(Vector3 position, out float xCoord, out float zCoord)
    {
        xCoord = position.x / TileWidth();
        zCoord = position.z / TileHeight();
    }


    private Vector2 GetFlowAtPoint(Vector3 position)
    {
        GetTileCoords(position, out float xPos, out float zPos);

        int xCoord = Mathf.RoundToInt(xPos + xSubdivs * 0.5f);
        int zCoord = Mathf.RoundToInt(zPos + zSubdivs * 0.5f);

        return tiles[xCoord, zCoord].FlowDirection;
    }
}
