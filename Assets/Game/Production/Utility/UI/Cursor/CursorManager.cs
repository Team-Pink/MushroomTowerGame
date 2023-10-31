using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomCursor
{
    public string cursorName;
    public Texture2D cursorTexture;
}

public class CursorManager : MonoBehaviour
{
    public CustomCursor[] cursors;
    public string currentCursorState
    {
        get;
        private set;
    }

    void Start()
    {
        ChangeCursor();
    }

    public void ChangeCursor(string cursorName)
    {
        bool found = false;
        CustomCursor desiredCursor = null;

        foreach (CustomCursor cursor in cursors)
        {
            if (cursor.cursorName == cursorName)
            {
                found = true;
                desiredCursor = cursor;
            }
        }

        if (!found)
            return;

        Cursor.SetCursor(desiredCursor.cursorTexture, Vector2.zero, CursorMode.Auto);
        currentCursorState = desiredCursor.cursorName;
    }
    public void ChangeCursor(int cursorIndex)
    {
        CustomCursor desiredCursor = cursors[cursorIndex];

        if (desiredCursor == null)
            return;

        Cursor.SetCursor(desiredCursor.cursorTexture, Vector2.zero, CursorMode.Auto);
        currentCursorState = desiredCursor.cursorName;
    }
    public void ChangeCursor()
    {
        Cursor.SetCursor(cursors[0].cursorTexture, Vector2.zero, CursorMode.Auto);
        currentCursorState = cursors[0].cursorName;
    }

}
