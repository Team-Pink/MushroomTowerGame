using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class CustomCursor
{
    public string cursorName;
    public Sprite cursorSprite;
    public bool displayCost = false;
}

public class CursorManager : MonoBehaviour
{
    public CustomCursor[] cursors;
    [SerializeField] TMP_Text costText;
    [SerializeField] Vector2 costPositionFromSoftwareMousePoint;
    [SerializeField] UnityEngine.UI.Image softwareCursorImage;

    public string currentCursorState
    {
        get;
        private set;
    }


    void Start()
    {
        Cursor.visible = false;
        softwareCursorImage.gameObject.SetActive(true);
        ChangeCursor();
    }

    private void Update()
    {
        PlaceCursorUIOnCursorPosition();
        
        if (IsCursorNotInWindowBounds() && Cursor.visible == false)
            Cursor.visible = true;
        else if (!IsCursorNotInWindowBounds() && Cursor.visible != false)
            Cursor.visible = false;
    }

    public void ChangeCursor(string cursorName)
    {
        CustomCursor desiredCursor = null;

        foreach (CustomCursor cursor in cursors)
        {
            if (cursor.cursorName == cursorName)
            {
                desiredCursor = cursor;
            }
        }

        if (desiredCursor == null || desiredCursor.cursorName == currentCursorState)
            return;
        SetSoftwareCursor(desiredCursor);
    }
    public void ChangeCursor(int cursorIndex)
    {
        CustomCursor desiredCursor = cursors[cursorIndex];

        if (desiredCursor == null || desiredCursor.cursorName == currentCursorState)
            return;
        SetSoftwareCursor(desiredCursor);
    }
    public void ChangeCursor()
    {
        if (cursors[0].cursorName == currentCursorState)
            return;
        SetSoftwareCursor(cursors[0]);
    }

    public void DisplayCost(int cost) => costText.text = cost.ToString();
    public void DisplayCost() => costText.text = "";

    void PlaceCursorUIOnCursorPosition()
    {
        costText.rectTransform.position = Input.mousePosition + new Vector3(costPositionFromSoftwareMousePoint.x, costPositionFromSoftwareMousePoint.y, 0);
        softwareCursorImage.rectTransform.position = Input.mousePosition + new Vector3(softwareCursorImage.rectTransform.rect.width/2, softwareCursorImage.rectTransform.rect.height/-2, 0);
    }

    bool IsCursorNotInWindowBounds()
    {
        Camera camera = transform.parent.GetComponentInChildren<Camera>();
        var view = camera.ScreenToViewportPoint(Input.mousePosition);
        return view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
    }

    void SetSoftwareCursor(CustomCursor cursor)
    {
        softwareCursorImage.sprite = cursor.cursorSprite;
        costText.gameObject.SetActive(cursor.displayCost);
        currentCursorState = cursor.cursorName;
    }

}
