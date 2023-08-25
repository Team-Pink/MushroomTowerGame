using System.Collections.Generic;
using UnityEngine;

public enum TileStatus
{
    Unreached,
    Pending,
    Closed
}

public class DijkstraTile
{
    public int x { get; private set; }
    public int z { get; private set; }

    public DijkstraTile currentBestNextTile;
    public float distanceFromCentreTile { get; private set; }

    public DijkstraTile(int xIndex, int zIndex)
    {
        x = xIndex;
        z = zIndex;

        currentBestNextTile = null;
        distanceFromCentreTile = float.PositiveInfinity;
    }

    public void SetDistance(float distance)
    {
        distanceFromCentreTile = distance;
    }

    public void SetNextTile(DijkstraTile tile)
    {
        currentBestNextTile = tile;
    }

    public List<DijkstraTile> GetConnectedTiles(Tile[,] indexingReference)
    {
        int xMax = indexingReference.GetLength(0) - 1;
        int zMax = indexingReference.GetLength(1) - 1;

        bool canGetAbove = z != 0;
        bool canGetBelow = z != zMax;
        bool canGetLeft = x != 0;
        bool canGetRight = x != xMax;

        List<DijkstraTile> connectedTiles = new();

        if (canGetAbove)
        {
            connectedTiles.Add(new(x, z - 1));
            if (canGetLeft)
                connectedTiles.Add(new(x - 1, z - 1));
            if (canGetRight)
                connectedTiles.Add(new(x + 1, z - 1));
        }
        if (canGetBelow)
        {
            connectedTiles.Add(new(x, z + 1));
            if (canGetLeft)
                connectedTiles.Add(new(x - 1, z + 1));
            if (canGetRight)
                connectedTiles.Add(new(x + 1, z + 1));
        }
        if (canGetLeft)
            connectedTiles.Add(new(x - 1, z));
        if (canGetRight)
            connectedTiles.Add(new(x + 1, z));

        foreach (DijkstraTile tile in connectedTiles)
        {
            Vector2 offset = new ( tile.x - x, tile.z - z); //Make this either 1 or 1.414 - Finn
            float distanceToTile = offset.magnitude;
            tile.SetDistance(distanceToTile + distanceFromCentreTile);
            tile.SetNextTile(this);
        }
        return connectedTiles;
    }
}

public class Tile
{
    public TileStatus tileStatus = TileStatus.Unreached;
    //public Tile bestNextTile;
    //public float distanceFromCentreTile = float.PositiveInfinity;
    //public Vector2 FlowDirection { get; private set; }
    public float InkThickness { get; private set; }
    public readonly bool muddy;
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
public class WorldDataGrid : MonoBehaviour
{
    [Header("Grid Tiles")]
    [SerializeField] int xSubdivs = 200;
    [SerializeField] int zSubdivs = 200;

    private Tile[,] tiles;

    List<DijkstraTile> pendingList = new();

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

    private int amountClosed = 0;

    [Header("Debug")]
    [SerializeField] bool drawGrid;

    public void Awake()
    {
        Initialise();

        CreateGrid();
        // Populate Grid Muddiness here
        //PopulateGridDistances();
    }

    private void OnValidate()
    {
        Initialise();
        CreateGrid();
    }

    [ContextMenu("Clear Tiles")]
    private void Clear()
    {
        foreach (Tile tile in tiles)
        {
            tile.tileStatus = TileStatus.Unreached;
        }
        amountClosed = 0;
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
    }

    [ContextMenu("PopulateGridDistances")]
    private void PopulateGridDistances()
    {
        int xMidpoint = xSubdivs / 2;
        int zMidpoint = zSubdivs / 2;

        // Dijkstra's Algorithm
        DijkstraTile centreTile = new(xMidpoint, zMidpoint);
        pendingList.Add(centreTile);
        centreTile.SetDistance(0);
        tiles[xMidpoint, zMidpoint].tileStatus = TileStatus.Pending;
        
        while (pendingList.Count > 0)
        {
            DijkstraTile currentCheapestTile = null;

            foreach(DijkstraTile tile in pendingList)
            {
                if (currentCheapestTile == null)
                    currentCheapestTile = tile;
                else
                {
                    if (tile.distanceFromCentreTile < currentCheapestTile.distanceFromCentreTile)
                        currentCheapestTile = tile;
                }
            }

            AddToPendingList(currentCheapestTile.GetConnectedTiles(tiles));
            Close(currentCheapestTile);
        }
        Debug.Log("Successfully set " + amountClosed.ToString() + " tiles");
    }

    private void Close(DijkstraTile tileToSet)
    {
        //Debug.Log("Setting tile " + tileToSet.x + ", " + tileToSet.z + " to closed.");
        tiles[tileToSet.x, tileToSet.z].tileStatus = TileStatus.Closed;
        pendingList.Remove(tileToSet);
        amountClosed++;
    }

    private void SetPending(DijkstraTile tileToSet)
    {
        //Debug.Log("Setting tile " + tileToSet.x + ", " + tileToSet.z + " to pending.");
        if (tiles[tileToSet.x, tileToSet.z].tileStatus == TileStatus.Closed) return;
        if (tiles[tileToSet.x, tileToSet.z].tileStatus == TileStatus.Unreached)
        {
            pendingList.Add(tileToSet);
            tiles[tileToSet.x, tileToSet.z].tileStatus = TileStatus.Pending;
            return;
        }
        foreach (DijkstraTile tile in pendingList)
        {
            if (tile.x == tileToSet.x && tile.z == tileToSet.z)
            {
                if (tileToSet.distanceFromCentreTile < tile.distanceFromCentreTile)
                {
                    tile.SetDistance(tileToSet.distanceFromCentreTile);
                    tile.SetNextTile(tileToSet.currentBestNextTile);
                }
                return;
            }
        }

    }
    private void AddToPendingList(List<DijkstraTile> listToAssign)
    {
        foreach (DijkstraTile tile in listToAssign)
        {
            SetPending(tile);
        }
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
        if (drawGrid)
        {
            // Draw the grid
            for (int xIndex = 0; xIndex < xSubdivs + 1; xIndex++)
            {
                float xCoord = TileWidth() * (xIndex - xSubdivs * 0.5f);
                float nextXCoord = xCoord + TileWidth();

                for (int zIndex = 0; zIndex < zSubdivs + 1; zIndex++)
                {
                    float zCoord = TileHeight() * (zIndex - zSubdivs * 0.5f);
                    float nextZCoord = zCoord + TileHeight();

                    if (xIndex < xSubdivs)
                        Gizmos.DrawLine(SwizzleXZY(nextXCoord, zCoord), SwizzleXZY(xCoord, zCoord));
                    if (zIndex < zSubdivs)
                        Gizmos.DrawLine(SwizzleXZY(xCoord, zCoord), SwizzleXZY(xCoord, nextZCoord));
                }
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
        return Vector2.zero;
        //return tiles[xCoord, zCoord].FlowDirection;
    }
}
