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
    public enum CursorType
    {
        HardwareCursor,
        SoftwareCursor,
    }

    [SerializeField] CursorType cursorType;

    [Space(30)]

    public CustomCursor[] cursors;
    [SerializeField] TMP_Text costText;
    [SerializeField] Vector2 costPositionFromSoftwareMousePoint;
    [SerializeField] Vector2 costPositionFromHardwareMousePoint;
    [SerializeField] UnityEngine.UI.Image softwareCursorImage;

    public string currentCursorState
    {
        get;
        private set;
    }

    void Start()
    {
        if (cursorType == CursorType.HardwareCursor)
        {
            ChangeCursor();
        }
        else if(cursorType == CursorType.SoftwareCursor)
        {
            Cursor.visible = false;
            softwareCursorImage.gameObject.SetActive(true);
            ChangeCursor();
        }
        else
        {
            Debug.LogError("Cursor Broke, neither Hardware Cursor or Software Cursor has been selected. Exiting Playmode.");
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
            #else
                        Application.Quit();
            #endif
        } //If cursorType is null
    }

    private void Update()
    {
        PlaceCursorUIOnCursorPosition();

        if (cursorType == CursorType.SoftwareCursor)
        {
            if (IsCursorNotInWindowBounds() && Cursor.visible == false)
                Cursor.visible = true;
            else if (!IsCursorNotInWindowBounds() && Cursor.visible != false)
                Cursor.visible = false;

            if (!softwareCursorImage.IsActive())
                softwareCursorImage.gameObject.SetActive(true);
        }
        else if (cursorType == CursorType.HardwareCursor)
        {
            if (softwareCursorImage.IsActive())
                softwareCursorImage.gameObject.SetActive(false);
        }
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

        if(cursorType == CursorType.HardwareCursor)
            SetHardwareCursor(desiredCursor);
        else if(cursorType == CursorType.SoftwareCursor)
            SetSoftwareCursor(desiredCursor);
        else
        {
            Debug.LogError("Cursor Broke, neither Hardware Cursor or Software Cursor has been selected. Exiting Playmode.");
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
        } //If cursorType is null
    }
    public void ChangeCursor(int cursorIndex)
    {
        CustomCursor desiredCursor = cursors[cursorIndex];

        if (desiredCursor == null)
            return;

        if (cursorType == CursorType.HardwareCursor)
            SetHardwareCursor(desiredCursor);
        else if (cursorType == CursorType.SoftwareCursor)
            SetSoftwareCursor(desiredCursor);
        else
        {
            Debug.LogError("Cursor Broke, neither Hardware Cursor or Software Cursor has been selected. Exiting Playmode.");
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
            #endif
        } //If cursorType is null
    }
    public void ChangeCursor()
    {
        if (cursorType == CursorType.HardwareCursor)
            SetHardwareCursor(cursors[0]);
        else if (cursorType == CursorType.SoftwareCursor)
            SetSoftwareCursor(cursors[0]);
        else
        {
            Debug.LogError("Cursor Broke, neither Hardware Cursor or Software Cursor has been selected. Exiting Playmode.");
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
            #endif
        } //If cursorType is null
    }

    public void DisplayCost(int cost) => costText.text = cost.ToString();
    public void DisplayCost() => costText.text = "";

    void PlaceCursorUIOnCursorPosition()
    {
        if (cursorType == CursorType.SoftwareCursor)
            costText.rectTransform.position = Input.mousePosition + new Vector3(costPositionFromSoftwareMousePoint.x, costPositionFromSoftwareMousePoint.y, 0);
        else if (cursorType == CursorType.HardwareCursor)
            costText.rectTransform.position = Input.mousePosition + new Vector3(costPositionFromHardwareMousePoint.x, costPositionFromHardwareMousePoint.y, 0);

        if (cursorType == CursorType.SoftwareCursor)
            softwareCursorImage.rectTransform.position = Input.mousePosition + new Vector3(softwareCursorImage.rectTransform.rect.width/2, softwareCursorImage.rectTransform.rect.height/-2, 0);

    }

    bool IsCursorNotInWindowBounds()
    {
        Camera camera = transform.parent.GetComponentInChildren<Camera>();
        var view = camera.ScreenToViewportPoint(Input.mousePosition);
        return view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
    }

    void SetHardwareCursor(CustomCursor cursor)
    {
        Cursor.SetCursor(cursor.cursorTexture, Vector2.zero, CursorMode.Auto);
        currentCursorState = cursor.cursorName;
        costText.gameObject.SetActive(cursor.displayCost);
    }

    void SetSoftwareCursor(CustomCursor cursor)
    {
        Texture2D tex = cursor.cursorTexture;
        Sprite cursorSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);
        softwareCursorImage.sprite = cursorSprite;
        costText.gameObject.SetActive(cursor.displayCost);
    }

}
