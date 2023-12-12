using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class CustomCursor
{
    public string cursorName;
    public Sprite cursorSprite;
    public bool displayCost = false;
    public Color costTextColour;
}

public class CursorManager : MonoBehaviour
{
    private CurrencyManager currencyManager;

    public CustomCursor[] cursors;
    [SerializeField] TMP_Text costText;
    [SerializeField] Vector2 costPositionFromSoftwareMousePoint;
    [SerializeField] UnityEngine.UI.Image softwareCursorImage;

    [SerializeField] Color canAffordColour = new Color(1, 1, 1);
    [SerializeField] Color cantAffordColor = new Color(1, 0, 0);

    public string currentCursorState
    {
        get;
        private set;
    }

    CanvasScaler canvasScaler;

    void Start()
    {
        Cursor.visible = false;
        softwareCursorImage.gameObject.SetActive(true);
        ChangeCursor();
        canvasScaler = GameObject.Find("Canvas").GetComponent<CanvasScaler>();

        currencyManager = gameObject.GetComponent<CurrencyManager>();
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

    public void DisplayCost(int cost)
    {
        costText.text = cost.ToString();
        costText.color = cost < currencyManager.GetCurrencyTotal() ? canAffordColour : cantAffordColor;
    }
    public void DisplayCost()
    {
        costText.text = "";
    }

    void PlaceCursorUIOnCursorPosition()
    {
        Vector3 mousePos = Input.mousePosition;        
        float refRatio = Screen.height / canvasScaler.referenceResolution.y;
        Vector3 costPushPosition = costPositionFromSoftwareMousePoint;

        costText.rectTransform.position = costText.transform.parent.position + costPushPosition * refRatio;
        softwareCursorImage.rectTransform.position = mousePos;
    }

    bool IsCursorNotInWindowBounds()
    {
        Camera camera = transform.parent.GetComponentInChildren<Camera>();
        Vector3 view = camera.ScreenToViewportPoint(Input.mousePosition);
        return view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
    }

    void SetSoftwareCursor(CustomCursor cursor)
    {
        softwareCursorImage.sprite = cursor.cursorSprite;
        costText.gameObject.SetActive(cursor.displayCost);
        currentCursorState = cursor.cursorName;
    }
}
