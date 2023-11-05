using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class CustomCursor
{
    public string cursorName;
    public Texture2D cursorTexture;
    public bool displayCost = false;
}

public class CursorManager : MonoBehaviour
{
    public CustomCursor[] cursors;
    [SerializeField] TMP_Text costText;
    [SerializeField] Vector2 positionFromMousePoint;
    public string currentCursorState
    {
        get;
        private set;
    }

    void Start()
    {
        ChangeCursor();
    }

    private void Update()
    {
        PutTextOnCursorPosition();
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
        costText.gameObject.SetActive(desiredCursor.displayCost);
    }
    public void ChangeCursor(int cursorIndex)
    {
        CustomCursor desiredCursor = cursors[cursorIndex];

        if (desiredCursor == null)
            return;

        Cursor.SetCursor(desiredCursor.cursorTexture, Vector2.zero, CursorMode.Auto);
        currentCursorState = desiredCursor.cursorName;
        costText.gameObject.SetActive(desiredCursor.displayCost);
    }
    public void ChangeCursor()
    {
        Cursor.SetCursor(cursors[0].cursorTexture, Vector2.zero, CursorMode.Auto);
        currentCursorState = cursors[0].cursorName;
        costText.gameObject.SetActive(cursors[0].displayCost);
    }

    public void DisplayCost(int cost) => costText.text = cost.ToString();

    void PutTextOnCursorPosition()
    {
        costText.rectTransform.position = Input.mousePosition + new Vector3(positionFromMousePoint.x, positionFromMousePoint.y, 0);
    }

}
