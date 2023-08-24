using UnityEngine;

// Will be working on this 24/08. Working on applying next best target and will work in directions after that.

public class Tile
{
    public Tile bestNextTile;
    public float distanceFromCentreTile = float.PositiveInfinity;
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

    public Tile(bool muddyInit = false)
    {
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
        // Populate Grid Muddiness here
        //PopulateGridDistances();
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
    }

    [ContextMenu("PopulateGridDistances")]
    private void PopulateGridDistances()
    {
        int xMidpoint = xSubdivs / 2;
        int zMidpoint = zSubdivs / 2;

        tiles[xMidpoint, zMidpoint].distanceFromCentreTile = 0;

        // Dijkstra's Algorithm
        for (int iterationCount = 1; iterationCount <= Mathf.Max(xMidpoint, zMidpoint); iterationCount++)
        {
            for (int xIndex = 0; xIndex < iterationCount + 2; xIndex++)
            {
                int xOffset = xIndex - iterationCount;
                bool atXBound = Mathf.Abs(xOffset) == iterationCount;

                for (int zIndex = 0; zIndex < iterationCount + 2; zIndex++)
                {
                    int zOffset = zIndex - iterationCount;
                    bool atZBound = Mathf.Abs(zOffset) == iterationCount;

                    if (!atXBound && !atZBound)
                    {
                        //Debug.Log("Tile[" + xIndex + ", " + zIndex + "] has already been set");
                        continue;
                    }


                    int actualXIndex = xMidpoint + xOffset;
                    int actualZIndex = zMidpoint + zOffset;

                    Vector2Int[] targetIndicesToCheck;
                    if (atXBound && atZBound)
                    {
                        int xOffsetDirection = (int)Mathf.Sign(xOffset);
                        int previousXIndex = xMidpoint + (xOffset - 1 * xOffsetDirection);
                        int zOffsetDirection = (int)Mathf.Sign(zOffset);
                        int previousZIndex = zMidpoint + (zOffset - 1 * zOffsetDirection);

                        targetIndicesToCheck = new Vector2Int[]
                        {
                            new Vector2Int(previousXIndex, actualZIndex),
                            new Vector2Int(previousXIndex, previousZIndex),
                            new Vector2Int(actualXIndex, previousZIndex)
                        };

                    }
                    else if (atXBound)
                    {
                        if (zIndex > zMidpoint)
                            continue;

                        int xOffsetDirection = (int)Mathf.Sign(xOffset);
                        int previousXIndex = xMidpoint + (xOffset - 1 * xOffsetDirection);


                        targetIndicesToCheck = new Vector2Int[]
                        {
                            new Vector2Int(previousXIndex, actualZIndex - 1),
                            new Vector2Int(previousXIndex, actualZIndex    ),
                            new Vector2Int(previousXIndex, actualZIndex + 1)
                        };
                    }
                    else
                    {
                        if (xIndex > xMidpoint)
                            continue;

                        int zOffsetDirection = (int)Mathf.Sign(zOffset);
                        int previousZIndex = zMidpoint + (zOffset - 1 * zOffsetDirection);

                        targetIndicesToCheck = new Vector2Int[]
                        {
                            new Vector2Int(actualXIndex - 1, previousZIndex),
                            new Vector2Int(actualXIndex,     previousZIndex),
                            new Vector2Int(actualXIndex + 1, previousZIndex)
                        };
                    }

                    float currentBestDistance = float.PositiveInfinity;
                    Vector2Int currentBestTileIndex = new Vector2Int();
                    for (int tileIndex = 0; tileIndex < targetIndicesToCheck.Length; tileIndex++)
                    {
                        float potentialBestDistance = CalculateTileDistanceToCentre(new Vector2Int(actualXIndex, actualZIndex), targetIndicesToCheck[tileIndex]);
                        if (potentialBestDistance < currentBestDistance)
                        {
                            currentBestTileIndex = targetIndicesToCheck[tileIndex];
                            currentBestDistance = potentialBestDistance;
                        }
                    }
                    tiles[actualXIndex, actualZIndex].distanceFromCentreTile = currentBestDistance;
                    tiles[actualXIndex, actualZIndex].bestNextTile = tiles[currentBestTileIndex.x, currentBestTileIndex.y];

                    Debug.Log("Tile[" + actualXIndex + ", " + actualZIndex + "]'s best next tile will be Tile[" +
                        currentBestTileIndex.x + ", " + currentBestTileIndex.y + "]. Distance From Centre: " + currentBestDistance);
                }
            }
        }
    }

    private float CalculateTileDistanceToCentre(Vector2Int tileIndex, Vector2Int targetTileIndex)
    {
        float distanceToTarget = Mathf.Abs((targetTileIndex - tileIndex).magnitude);
        if (tiles[targetTileIndex.x, targetTileIndex.y].muddy)
            distanceToTarget *= 2;

        return distanceToTarget + tiles[targetTileIndex.x, targetTileIndex.y].distanceFromCentreTile;
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
        return Vector2.zero;
        //return tiles[xCoord, zCoord].FlowDirection;
    }
}
