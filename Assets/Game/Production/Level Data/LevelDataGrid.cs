using UnityEngine;
using System.Collections;

[System.Serializable]
public struct Tile
{
    public readonly Vector2 flowDirection;
    public readonly bool muddy;

    private float inkLevel;

    public Tile(Vector2 flowDirectionInit, bool muddyInit)
    {
        flowDirection = flowDirectionInit;
        muddy = muddyInit;

        inkLevel = 0;
    }

    public float SpeedMultiplier
    {
        get
        {
            if (muddy)
                return 0.5f;
            else
                return 1.0f - inkLevel;
        }
    }

    public bool GetFlow() => flowDirection != Vector2.zero;
    public bool GetFlow(out Vector2 direction, out float speedMultiplier)
    {
        speedMultiplier = SpeedMultiplier;
        direction = flowDirection;

        return flowDirection != Vector2.zero;
    }

    public void ClearInk() => inkLevel = 0;

    public void SetInkLevel(float newInkLevel) => inkLevel = newInkLevel;
}

public class LevelDataGrid : MonoBehaviour
{
    public Texture2D levelTexture;
    private Tile[,] tiles = new Tile[0,0];

    [SerializeField] float gridWidth;
    [SerializeField] float gridHeight;

    private int tilesWidth;
    private int tilesHeight;

    [Header("Debug")]
    [SerializeField] bool drawGrid;
    [SerializeField] bool drawFlowLines;

    private float TileWidth => gridWidth / tilesWidth;
    private float TileHeight => gridHeight / tilesHeight;

    private void Awake() => StartCoroutine(ReadFromTexture());

    private IEnumerator ReadFromTexture()
    {
        tiles = new Tile[levelTexture.width, levelTexture.height];
        tilesWidth = tiles.GetLength(0);
        tilesHeight = tiles.GetLength(1);

        bool muddy;
        Vector2 flowDirection;
        Color color;
        int currentIndex = 0;
        for (int x = 0; x < levelTexture.width; x++)
        {
            for (int z = 0; z < levelTexture.height; z++)
            {
                color = levelTexture.GetPixel(x, z);

                if (color.r > 0.5f)
                    muddy = true;
                else
                    muddy = false;

                flowDirection = new Vector2((color.g - 0.5f) * 2, (color.b - 0.5f) * 2);

                tiles[x, z] = new Tile(flowDirection, muddy);
                currentIndex++;
            }
        }
        yield return null;
    }

    public Vector2 GetFlowAtPoint(Vector3 position)
    {
        GetTileAtCoords(position, out int xPos, out int zPos);

        int xCoord = Mathf.RoundToInt(xPos + tilesWidth * 0.5f);
        int zCoord = Mathf.RoundToInt(zPos + tilesHeight * 0.5f);
        return tiles[xCoord, zCoord].flowDirection;
    }

    private void GetTileAtCoords(Vector3 position, out int xCoord, out int zCoord)
    {
        xCoord = Mathf.RoundToInt((position.x + tilesWidth * 0.5f) / TileWidth);
        zCoord = Mathf.RoundToInt((-position.z - tilesHeight * 0.5f) / TileHeight);
    }

    public void OnDrawGizmos()
    {
        if (drawGrid)
        {
            // Draw the grid
            for (int xIndex = 0; xIndex < tilesWidth + 1; xIndex++)
            {
                float xCoord = TileWidth * (xIndex - tilesWidth * 0.5f);
                float nextXCoord = xCoord + TileWidth;

                for (int zIndex = 0; zIndex < tilesHeight + 1; zIndex++)
                {
                    float zCoord = TileHeight * (zIndex - tilesHeight * 0.5f);
                    float nextZCoord = zCoord + TileHeight;

                    if (xIndex < tilesWidth)
                        Gizmos.DrawLine(new Vector3(nextXCoord, 0, zCoord), new Vector3(xCoord, 0, zCoord));
                    if (zIndex < tilesHeight)
                        Gizmos.DrawLine(new Vector3(xCoord, 0, zCoord), new Vector3(xCoord, 0, nextZCoord));
                }
            }
        }

        if (drawFlowLines)
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int z = 0; z < tiles.GetLength(1); z++)
                {
                    Tile currentTile = tiles[x, z];

                    if (currentTile.flowDirection != null)
                    {
                        if (currentTile.muddy)
                            Gizmos.color = Color.yellow;
                        else
                            Gizmos.color = Color.magenta;

                        Gizmos.DrawRay(TileToWorldSpace(x, z), new Vector3(currentTile.flowDirection.x/4, 0, currentTile.flowDirection.y/4));
                    }
                }
            }
        }
    }
    public Vector3 TileToWorldSpace(int xIndex, int zIndex)
    {

        return new Vector3((xIndex - tilesWidth * 0.5f) * TileWidth + TileWidth * 0.5f, 0,
            (-zIndex + tilesHeight * 0.5f) * TileHeight - TileWidth * 0.5f);
    }
}