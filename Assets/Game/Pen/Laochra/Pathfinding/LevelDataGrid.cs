using UnityEngine;
using System.Collections;

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
    private Tile[,] tiles;

    private void Awake()
    {
        StartCoroutine(ReadFromTexture());
    }

    private IEnumerator ReadFromTexture()
    {
        tiles = new Tile[levelTexture.width, levelTexture.height];

        bool muddy;
        Vector2 flowDirection;
        Color color;

        for (int x = 0; x < levelTexture.width; x++)
        {
            for (int z = 0; z < levelTexture.height; z++)
            {
                color = levelTexture.GetPixel(x, z);

                if (color.r > 0.5f)
                    muddy = true;
                else
                    muddy = false;

                flowDirection = new Vector2(color.g, color.b);

                tiles[x, z] = new Tile(flowDirection, muddy);

                yield return null;
            }
        }
    }
}