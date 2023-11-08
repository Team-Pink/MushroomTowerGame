using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System.Collections.Generic;

public enum InteractionState
{
    None,

    BuildingInteraction,
    Selling,
    SellingHover,

    PlacingFromMeteor,
    PlacingFromNode,
    ShroomSelection
}

public class InteractionManager : MonoBehaviour
{
    [Header("Objects")]
    #region Object Variables
    [SerializeField] GameObject targetPlane;
    [SerializeField] GameObject nodePrefab;
    [SerializeField, NonReorderable] private List<GameObject> shroomPrefabs = new(5);
    private Camera mainCamera;
    private const int shroomPrefabAmount = 5;
    #endregion

    [Header("Building Selection")]
    #region Building Selection Variables
    [SerializeField] LayerMask buildingLayers;
    private Building targetBuilding;
    [SerializeField] GameObject shroomTooltip;
    [SerializeField] TMP_Text shroomName;
    [SerializeField] string[] shroomNames;
    [SerializeField] TMP_Text shroomDescription;
    [SerializeField, TextArea] string[] shroomDescriptions;
    [SerializeField] GameObject radiusPreviewPrefab;
    private GameObject shroomRadiusPreview;
    #endregion

    [Header("Placement")]
    #region Placement Variables
    [SerializeField] bool placeOnPaths;
    LevelDataGrid levelDataGrid;
    [SerializeField] LayerMask placementBlockers;
    private LayerMask nodeLayer;
    private LayerMask placableLayers;
    private LayerMask budLayer;

    private GameObject activeBud;
    //private Vector3 dragStartPosition;

    [SerializeField] float placementExclusionSize = 1;
    [SerializeField] float maxDistanceFromNode = 10;
    private const float capsuleCheckBound = 5;
    #endregion

    [Header("Currency")]
    #region Cost Of Placement
    private int nodeMultiplier = 1;
    private int placementCost = 0;
    private CurrencyManager currencyManager;
    private Node refNode;

    enum RadialType
    {
        Node,
        Residual,
        Shroom,
        ShroomSelection,
    }
    #endregion

    [Header("UI")]
    #region UI Variables
    [SerializeField] private Color buttonBaseColour;
    [SerializeField] private Color buttonHoverColour;

    [SerializeField, Space()] Image sellButton;
    [SerializeField] Sprite sellButtonHighlight;
    [SerializeField] Sprite sellButtonActive;
    private Sprite sellButtonDefault;
    private RectTransform sellButtonTransform;

    [SerializeField, Space()] Image selectionIndicator;
    [SerializeField] private float radialExclusionZone = 10.0f;
    private Vector2 startingMousePosition;

    [SerializeField, Space()] GameObject shroomSelectionMenu;
    [SerializeField, NonReorderable] Image[] shroomSelectionMenuButtons;
    [SerializeField] Sprite lockedShroomSprite;
    private readonly Sprite[] shroomIconSprites = new Sprite[5];
    private int unlockedShrooms = 0;
    private readonly int maxShroomsUnlockable = 5;
    [SerializeField] bool unlockAllShrooms = false;
    [SerializeField] TMP_Text shroomSelectionCostText;

    [SerializeField, Space()] private Color canPurchaseColour;
    [SerializeField] private Color canNotPurchaseColour;
    [SerializeField] private Color sellColour;

    [SerializeField, Space(10)] private CursorManager cursorManager;
    #endregion

    [Header("Interaction")]
    #region Interaction Variables
    public static bool gamePaused = false;
    public static bool tutorialMode = false;
    public KeyCode interactKey = KeyCode.Mouse0;
    [SerializeField] KeyCode cancelKey = KeyCode.Mouse1;
    [SerializeField, Space()] float interactHoldRequirement = 0.25f;
    private bool interactKeyHeld = false;
    private float timeHeld = 0.0f;
    private Vector3 mouseScreenPosition;
    private Vector3 mouseWorldPosition;

    public InteractionState currentInteraction = InteractionState.None;
    private InteractionState previousInteraction = InteractionState.None;
    private InteractionState CurrentInteraction
    {
        get => currentInteraction;
        set
        {
            if (logInteractionChange)
                Debug.Log(value);

            previousInteraction = currentInteraction;
            currentInteraction = value;
        }
    }
    private RaycastHit currentHit;
    private bool TargetIsPlane
    {
        get => currentHit.collider.gameObject == targetPlane;
    }
    #endregion

    [Header("Debug")]
    #region Debug Variables
    [SerializeField] bool showMouseDirection;
    [SerializeField] bool showCameraProjection;
    private float screenWidth;
    private float screenHeight;
    [SerializeField] bool logInteractionChange;
    #endregion

    private TutorialManager tutorial;
    private CanvasScaler canvasScaler;

    private void Awake()
    {
        ResetInteraction();

        mainCamera = Camera.main;
        levelDataGrid = GetComponent<LevelDataGrid>();

        placableLayers = LayerMask.GetMask("Ground");
        nodeLayer = LayerMask.GetMask("Node");
        budLayer = LayerMask.GetMask("Bud");

        sellButtonDefault = sellButton.sprite;
        sellButtonTransform = sellButton.GetComponent<RectTransform>();

        currencyManager = gameObject.GetComponent<CurrencyManager>();
        cursorManager = gameObject.GetComponent<CursorManager>();

        for (int i = 0; i < shroomSelectionMenuButtons.Length; i++)
        {
            shroomIconSprites[i] = shroomSelectionMenuButtons[i].sprite;

            if (i >= unlockedShrooms)
            {
                shroomSelectionMenuButtons[i].sprite = lockedShroomSprite;
            }
        }

        if (unlockAllShrooms)
        {
            for (int i = unlockedShrooms - 1; i < maxShroomsUnlockable; i++)
            {
                UnlockShroom(i);
            }
        }

        tutorial = GetComponent<TutorialManager>();
        canvasScaler = GameObject.Find("Canvas").GetComponent<CanvasScaler>();
    }

    private void OnValidate()
    {
        if (shroomPrefabs.Count == shroomPrefabAmount)
            return;

        Debug.LogWarning("Stop that, the list shroomPrefabs should be exactly " + shroomPrefabAmount + " elements!", this);

        while (shroomPrefabs.Count < shroomPrefabAmount)
        {
            shroomPrefabs.Add(null);
        }
        while (shroomPrefabs.Count > shroomPrefabAmount)
        {
            shroomPrefabs.RemoveAt(shroomPrefabs.Count - 1);
        }
    }

    private void Update()
    {
        if (tutorialMode == false && gamePaused) return;

        mouseScreenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.1f);
        mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        switch(CurrentInteraction)
        {
            case InteractionState.None:
                DefaultState();
                break;

            case InteractionState.BuildingInteraction:
                BuildingInteractionState();
                break;
            case InteractionState.Selling:
                SellingState();
                break;
            case InteractionState.SellingHover:
                SellingHoverState();
                break;

            case InteractionState.PlacingFromMeteor:
                PlacingFromMeteorState();
                break;
            case InteractionState.PlacingFromNode:
                PlacingFromNodeState();
                break;
            case InteractionState.ShroomSelection:
                ShroomSelectionState();
                break;
        }

#if UNITY_EDITOR
        DEBUG();
#endif
    }

    private void DefaultState()
    {
        currentHit = GetRayHit(budLayer);

        if (cursorManager.currentCursorState != "Default")
            cursorManager.ChangeCursor("Default");

        if (tutorialMode && !tutorial.CorrectTutorialPlacement(mouseScreenPosition))
        {
            ResetInteraction();
            return;
        }

        if (currentHit.collider is null)
        {
            currentHit = GetRayHit(buildingLayers);

            if (currentHit.collider is null)
            {
                if ((new Vector2(0, Screen.height) +
                    (sellButtonTransform.anchoredPosition * Screen.height / canvasScaler.referenceResolution.y) -
                    new Vector2(mouseScreenPosition.x, mouseScreenPosition.y)).magnitude < 60 * sellButtonTransform.localScale.x)
                {
                    sellButton.sprite = sellButtonHighlight;
                    if (Input.GetKeyDown(interactKey))
                    {
                        sellButton.sprite = sellButtonActive;
                        CurrentInteraction = InteractionState.Selling;
                    }
                }
                else
                {
                    sellButton.sprite = sellButtonDefault;

                    if (targetBuilding is not null)
                    {
                        ResetInteraction();
                    }
                }
            }
            else
            {
                targetBuilding = currentHit.collider.gameObject.GetComponent<Building>();

                CurrentInteraction = InteractionState.BuildingInteraction;
                return;
            }
        }
        else
        {
            targetBuilding = currentHit.collider.transform.parent.GetComponent<Building>();

            CurrentInteraction = InteractionState.BuildingInteraction;
            return;
        }
    }

    private void BuildingInteractionState()
    {
        DisplayBuildingHealth(out MeshRenderer healthDisplay);

        if (targetBuilding is not Shroom)
        {
            if (targetBuilding is Node && ((targetBuilding as Node).isResidual || !targetBuilding.Active))
            {
                targetBuilding.ShowDeactivateLines();
            }
            else targetBuilding.ShowDefaultLines();
        }

        if (!interactKeyHeld)
        {
            currentHit = GetRayHit(budLayer);
            if (currentHit.collider is null) currentHit = GetRayHit(buildingLayers);

            if (currentHit.collider is null)
            {
                if (healthDisplay != null) healthDisplay.enabled = false;
                ResetInteraction();
                return;
            }
        }

        DisplayBuildingRadius(out GameObject radiusDisplay);

        if (interactKeyHeld)
        {
            if (timeHeld > interactHoldRequirement)
            {
                if (targetBuilding is Meteor)
                {
                    radiusDisplay.SetActive(false);
                    if (healthDisplay != null) healthDisplay.enabled = false;
                    CurrentInteraction = InteractionState.PlacingFromMeteor;
                    return;
                }
                else if (targetBuilding is Node)
                {
                    if ((targetBuilding as Node).isResidual == false && targetBuilding.Active)
                    {
                        radiusDisplay.SetActive(false);
                        if (healthDisplay != null) healthDisplay.enabled = false;
                        CurrentInteraction = InteractionState.PlacingFromNode;
                    }
                    return;
                }
            }
            else
            {
                timeHeld += Time.deltaTime;
            }

            if (Input.GetKeyUp(interactKey))
            {
                radiusDisplay.SetActive(false);
                if (healthDisplay != null) healthDisplay.enabled = false;

                if (targetBuilding is Node && (targetBuilding as Node).isResidual == false && targetBuilding.Active)
                {
                    CurrentInteraction = InteractionState.PlacingFromNode;
                }
                else if (targetBuilding is Meteor)
                {
                    CurrentInteraction = InteractionState.PlacingFromMeteor;
                }
                else
                {
                    ResetInteraction(new GameObject[] { radiusDisplay });
                    DefaultState();
                }
            }
        }
        else if (Input.GetKeyDown(interactKey))
        {
            if (targetBuilding is Meteor ||
                (targetBuilding is Node && targetBuilding.Active && (targetBuilding as Node).isResidual == false))
            {
                interactKeyHeld = true;
            }
        }
    }
    private void SellingState()
    {
        currentHit = GetRayHit(budLayer);

        if (cursorManager.currentCursorState != "Shovel")
            cursorManager.ChangeCursor("Shovel");
        cursorManager.DisplayCost();

        if (tutorialMode && tutorial.currentTutorial == TutorialManager.Tutorial.Selling
            && tutorial.currentPart == 1)
        {
            tutorial.AdvanceTutorial(ref tutorial.sellingParts);
        }

        if (currentHit.collider is null)
        {
            currentHit = GetRayHit(buildingLayers);

            if (currentHit.collider is null)
            {
                if (targetBuilding is not null)
                {
                    Building tempBuilding = targetBuilding;
                    ResetInteraction();
                    targetBuilding = tempBuilding;

                    CurrentInteraction = InteractionState.Selling;
                    sellButton.sprite = sellButtonActive;
                }
            }
            else
            {
                targetBuilding = currentHit.collider.gameObject.GetComponent<Building>();

                if (targetBuilding is Meteor || (targetBuilding is Node && (targetBuilding as Node).isResidual))
                {
                    Building tempBuilding = targetBuilding;
                    ResetInteraction();
                    targetBuilding = tempBuilding;

                    cursorManager.DisplayCost();

                    CurrentInteraction = InteractionState.Selling;
                    sellButton.sprite = sellButtonActive;
                }
                else
                {
                    CurrentInteraction = InteractionState.SellingHover;
                    return;
                }
            }
        }
        else
        {
            targetBuilding = currentHit.collider.transform.parent.GetComponent<Building>();

            if (targetBuilding is Meteor || (targetBuilding is Node && (targetBuilding as Node).isResidual))
            {
                Building tempBuilding = targetBuilding;
                ResetInteraction();
                targetBuilding = tempBuilding;

                CurrentInteraction = InteractionState.Selling;
                sellButton.sprite = sellButtonActive;
            }
            else
            {
                CurrentInteraction = InteractionState.SellingHover;
                return;
            }
        }

        if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(cancelKey))
        {
            ResetInteraction();
        }
    }
    private void SellingHoverState()
    {
        currentHit = GetRayHit(budLayer);
        if (currentHit.collider is null) currentHit = GetRayHit(buildingLayers);

        if (currentHit.collider is null)
        {
            Building tempBuilding = targetBuilding;
            ResetInteraction();
            targetBuilding = tempBuilding;
            CurrentInteraction = InteractionState.Selling;
            sellButton.sprite = sellButtonActive;
            return;
        }

        if (targetBuilding is Node)
        {
            targetBuilding.ShowDeactivateLines();
            cursorManager.DisplayCost((targetBuilding as Node).GetNodeSellAmount());
            if(cursorManager.currentCursorState != "HighlightedShovel")
                cursorManager.ChangeCursor("HighlightedShovel");
        }
        else if (targetBuilding is Shroom)
        {
            if (cursorManager.currentCursorState != "HighlightedShovel")
                cursorManager.ChangeCursor("HighlightedShovel");
            cursorManager.DisplayCost((targetBuilding as Shroom).SellPrice());
        }

        if (Input.GetKeyDown(interactKey) || Input.GetKeyUp(interactKey))
        {
            targetBuilding.Sell();

            if (tutorialMode && tutorial.currentTutorial == TutorialManager.Tutorial.Selling
                && tutorial.currentPart == 1)
            {
                tutorial.AdvanceTutorial(ref tutorial.sellingParts);
            }

            ResetInteraction();
        }
    }

    private void PlacingFromMeteorState()
    {
        if (Input.GetKeyDown(cancelKey))
        {
            tutorial.ReverseTutorial(ref tutorial.placementParts);
            ResetInteraction();
            return;
        }

        bool canPlace = false;
        selectionIndicator.enabled = true;
        selectionIndicator.color = Color.red;
        activeBud = targetBuilding.bud;

        (targetBuilding as Meteor).budDetached = true;

        DisplayBuildingRadius(out GameObject radiusDisplay);

        selectionIndicator.rectTransform.position = mouseScreenPosition;

        if (tutorialMode && !tutorial.CorrectTutorialPlacement(mouseScreenPosition))
        {
            if (Input.GetKeyUp(interactKey))
            {
                tutorial.ReverseTutorial(ref tutorial.placementParts);
                ResetInteraction();
            }

            return;
        }

        currentHit = GetRayHit(placableLayers);

        placementCost = GetNewNodeCost();
        bool canBuy = currencyManager.CanDecreaseCurrencyAmount(placementCost);

        if (currentHit.collider is not null)
        {
            cursorManager.DisplayCost(placementCost);
            bool isPlaceable;
            if (placeOnPaths) isPlaceable = levelDataGrid.GetTileTypeAtPoint(currentHit.point) == TileType.Path;
            else isPlaceable = levelDataGrid.GetTileTypeAtPoint(currentHit.point) == TileType.Mud;

            if (isPlaceable)
            {
                bool spaceToPlace = SpaceToPlace(placementExclusionSize, placementBlockers);
                bool spaceForNode = SpaceToPlace(2 * maxDistanceFromNode, nodeLayer);

                float distanceFromMeteor = (targetBuilding.transform.position - new Vector3(currentHit.point.x, 0, currentHit.point.z)).magnitude;

                bool inNodeBuildRange = distanceFromMeteor < 3 * maxDistanceFromNode;

                if (inNodeBuildRange && spaceToPlace && spaceForNode && TargetIsPlane)
                {
                    selectionIndicator.color = Color.green;

                    if (canBuy)
                    {
                        if (cursorManager.currentCursorState != "CanPlaceCanBuy")
                            cursorManager.ChangeCursor("CanPlaceCanBuy");
                        canPlace = true;
                    }
                    else
                        if (cursorManager.currentCursorState != "CanPlaceCannotBuy")
                            cursorManager.ChangeCursor("CanPlaceCannotBuy");
                        
                }
                else
                {
                    if (canBuy)
                        if (cursorManager.currentCursorState != "CannotPlaceCanBuy")
                            cursorManager.ChangeCursor("CannotPlaceCanBuy");
                    else
                        if (cursorManager.currentCursorState != "CannotPlaceCannotBuy")
                            cursorManager.ChangeCursor("CannotPlaceCannotBuy");
                }
            }
            else
            {
                if (canBuy)
                    if (cursorManager.currentCursorState != "CannotPlaceCanBuy")
                        cursorManager.ChangeCursor("CannotPlaceCanBuy");
                    else
                    if (cursorManager.currentCursorState != "CannotPlaceCannotBuy")
                        cursorManager.ChangeCursor("CannotPlaceCannotBuy");
            }
        }
        else
        {
            if (canBuy)
                if (cursorManager.currentCursorState != "CannotPlaceCanBuy")
                    cursorManager.ChangeCursor("CannotPlaceCanBuy");
                else
                if (cursorManager.currentCursorState != "CannotPlaceCannotBuy")
                    cursorManager.ChangeCursor("CannotPlaceCannotBuy");
        }

        if (Input.GetKeyUp(interactKey))
        {
            if (canPlace)
            {
                selectionIndicator.color = Color.white;
                selectionIndicator.rectTransform.sizeDelta = new Vector2(10, 10);

                AttemptToSpawnNode();
            }
            else
            {
                ResetInteraction();
            }

            radiusDisplay.SetActive(false);
        }
    }
    private void PlacingFromNodeState()
    {
        if (Input.GetKeyDown(cancelKey))
        {
            tutorial.ReverseTutorial(ref tutorial.placementParts);
            ResetInteraction();
            return;
        }

        bool canPlace = false;
        bool placingNode = false;
        selectionIndicator.enabled = true;
        selectionIndicator.color = Color.red;
        activeBud = targetBuilding.bud;

        refNode = targetBuilding as Node;
        (targetBuilding as Node).budDetached = true;
        
        DisplayBuildingRadius(out GameObject radiusDisplay);

        cursorManager.DisplayCost();

        selectionIndicator.rectTransform.position = mouseScreenPosition;

        if (tutorialMode && !tutorial.CorrectTutorialPlacement(mouseScreenPosition))
        {
            if (Input.GetKeyUp(interactKey))
            {
                tutorial.ReverseTutorial(ref tutorial.placementParts);
                ResetInteraction();
            }

            return;
        }

        currentHit = GetRayHit(placableLayers);
        if (currentHit.collider != null)
        {
            bool canBuy;

            bool isPlaceable;
            if (placeOnPaths) isPlaceable = levelDataGrid.GetTileTypeAtPoint(currentHit.point) == TileType.Path;
            else isPlaceable = levelDataGrid.GetTileTypeAtPoint(currentHit.point) == TileType.Mud;

            float distanceFromNode = (targetBuilding.transform.position - new Vector3(currentHit.point.x, 0, currentHit.point.z)).magnitude;

            bool inShroomBuildRange = distanceFromNode < maxDistanceFromNode;
            bool inNodeBuildRange = distanceFromNode > 2 * maxDistanceFromNode && distanceFromNode < 3 * maxDistanceFromNode;

            if (isPlaceable)
            {
                bool spaceToPlace = SpaceToPlace(placementExclusionSize, placementBlockers);
                bool spaceForNode = SpaceToPlace(2 * maxDistanceFromNode, nodeLayer);

                bool shroomPlacementCriteria = inShroomBuildRange && spaceToPlace;
                bool nodePlacementCriteria = inNodeBuildRange && spaceToPlace && spaceForNode;

                if (shroomPlacementCriteria || nodePlacementCriteria)
                {
                    if (nodePlacementCriteria)
                    {
                        placementCost = GetNewNodeCost();
                        canBuy = currencyManager.CanDecreaseCurrencyAmount(placementCost);
                        cursorManager.DisplayCost(GetNewNodeCost());

                        placingNode = true;

                        if (canBuy)
                        {
                            cursorManager.ChangeCursor("CanPlaceCanBuy");
                            selectionIndicator.color = Color.green;
                            canPlace = true;
                        }
                        else
                            cursorManager.ChangeCursor("CanPlaceCannotBuy");
                    }
                    else
                    {
                        placementCost = refNode.GetNodeCost();
                        canBuy = currencyManager.CanDecreaseCurrencyAmount(placementCost);
                        cursorManager.DisplayCost(refNode.GetNodeCost());

                        if (canBuy)
                        {
                            cursorManager.ChangeCursor("CanPlaceCanBuy");
                            selectionIndicator.color = Color.green;
                            canPlace = true;
                        }
                        else
                            cursorManager.ChangeCursor("CanPlaceCannotBuy");
                    }
                }
                else
                {
                    if (inShroomBuildRange)
                    {
                        canBuy = currencyManager.CanDecreaseCurrencyAmount(refNode.GetNodeCost());
                        cursorManager.DisplayCost(refNode.GetNodeCost());
                        if (canBuy)
                            cursorManager.ChangeCursor("CannotPlaceCanBuy");
                        else
                            cursorManager.ChangeCursor("CannotPlaceCannotBuy");
                    }
                    else if (inNodeBuildRange)
                    {
                        canBuy = currencyManager.CanDecreaseCurrencyAmount(GetNewNodeCost());
                        cursorManager.DisplayCost(GetNewNodeCost());
                        if (canBuy)
                            cursorManager.ChangeCursor("CannotPlaceCanBuy");
                        else
                            cursorManager.ChangeCursor("CannotPlaceCannotBuy");
                    }
                    else
                        cursorManager.ChangeCursor("CannotPlace");
                }
            }
            else
            {
                if (inShroomBuildRange)
                {
                    canBuy = currencyManager.CanDecreaseCurrencyAmount(refNode.GetNodeCost());
                    cursorManager.DisplayCost(refNode.GetNodeCost());
                    if (canBuy)
                        if (cursorManager.currentCursorState != "CannotPlaceCanBuy")
                            cursorManager.ChangeCursor("CannotPlaceCanBuy");
                        else
                        if (cursorManager.currentCursorState != "CannotPlaceCannotBuy")
                            cursorManager.ChangeCursor("CannotPlaceCannotBuy");
                }
                else if (inNodeBuildRange)
                {
                    canBuy = currencyManager.CanDecreaseCurrencyAmount(GetNewNodeCost());
                    cursorManager.DisplayCost(GetNewNodeCost());
                    if (canBuy)
                        if (cursorManager.currentCursorState != "CannotPlaceCanBuy")
                            cursorManager.ChangeCursor("CannotPlaceCanBuy");
                        else
                        if (cursorManager.currentCursorState != "CannotPlaceCannotBuy")
                            cursorManager.ChangeCursor("CannotPlaceCannotBuy");
                }
                else
                    if (cursorManager.currentCursorState != "CannotPlace") 
                        cursorManager.ChangeCursor("CannotPlace");
            }
        }
        else
            if (cursorManager.currentCursorState != "CannotPlace")
                cursorManager.ChangeCursor("CannotPlace");

        if (Input.GetKeyUp(interactKey))
        {
            if (canPlace)
            {
                if (cursorManager.currentCursorState != "Default")
                    cursorManager.ChangeCursor("Default");
                selectionIndicator.color = Color.white;
                selectionIndicator.rectTransform.sizeDelta = new Vector2(10, 10);

                if (placingNode)
                    AttemptToSpawnNode();
                else
                    CurrentInteraction = InteractionState.ShroomSelection;
            }
            else
            {
                if (cursorManager.currentCursorState != "Default")
                    cursorManager.ChangeCursor("Default");
                ResetInteraction();
            }

            radiusDisplay.SetActive(false);
        }
    }
    private void ShroomSelectionState()
    {
        if (Input.GetKeyDown(cancelKey))
        {
            if (tutorialMode)
            {
                tutorial.ReverseTutorial(ref tutorial.placementParts);
                tutorial.ReverseTutorial(ref tutorial.placementParts);
            }

            ResetInteraction();
            return;
        }

        targetBuilding.radiusDisplay.SetActive(false);

        ShroomRadialMenu(shroomSelectionMenu, shroomSelectionMenuButtons, out int hoveredButtonIndex, 30.0f);
        
        if (Input.GetKeyUp(interactKey) || Input.GetKeyDown(interactKey))
        {
            if (hoveredButtonIndex < 0)
            {
                if (tutorialMode)
                {
                    tutorial.ReverseTutorial(ref tutorial.placementParts);
                    tutorial.ReverseTutorial(ref tutorial.placementParts);
                }

                ResetInteraction();
                return;
            }

            int cost = shroomPrefabs[hoveredButtonIndex].GetComponent<Shroom>().purchaseCost * refNode.GetMultiplier();

            if (!currencyManager.CanDecreaseCurrencyAmount(cost))
            {
                ResetInteraction();
                return;
            }


            Image hoveredButton = shroomSelectionMenuButtons[hoveredButtonIndex];

            placementCost = cost;

            SpawnShroom(hoveredButtonIndex);

            hoveredButton.color = buttonBaseColour;

            ResetInteraction();
        }
    }



    private RaycastHit GetRayHit()
    {
        Physics.Raycast(mainCamera.transform.position, mouseWorldPosition - mainCamera.transform.position, out RaycastHit hit, Mathf.Infinity);
        return hit;
    }
    private RaycastHit GetRayHit(LayerMask targetLayers)
    {
        Physics.Raycast(mainCamera.transform.position, mouseWorldPosition - mainCamera.transform.position, out RaycastHit hit, Mathf.Infinity, targetLayers);
        return hit;
    }

    private bool SpaceToPlace(float detectionArea, LayerMask layerMask)
    {
        Vector3 capsuleTop = new(currentHit.point.x, currentHit.point.y + capsuleCheckBound, currentHit.point.z);
        Vector3 capsuleBottom = new(currentHit.point.x, currentHit.point.y - capsuleCheckBound, currentHit.point.z);

        var targets = Physics.OverlapCapsule(capsuleTop, capsuleBottom, detectionArea, layerMask).ToList();

        return targets.Count == 0;
    }

    private void AttemptToSpawnNode()
    {

        int cost = 0;

        Building parent = activeBud.transform.parent.GetComponent<Building>();

        if (parent is Meteor)
        {
            Meteor parentMeteor = parent as Meteor;
            nodeMultiplier = 1;
            cost = Node.GetNodeBaseCurrency();
            parentMeteor.ClearDestroyedNodes();
        }
        else if (parent is Node)
        {
            Node parentNode = parent as Node;
            nodeMultiplier = parentNode.GetMultiplier() + 1;
            cost = parentNode.GetNodeCost(nodeMultiplier);
        }

        if (!TargetIsPlane || !currencyManager.CanDecreaseCurrencyAmount(cost))
        {
            ResetInteraction();
            return;
        }

        placementCost = cost;
        SpawnNode();
    }

    private void SpawnNode()
    {
        currencyManager.DecreaseCurrencyAmount(placementCost);

        if (activeBud.transform.parent.GetComponent<Meteor>() != null)
        {
            activeBud.transform.parent.GetComponent<Meteor>().ClearDestroyedNodes();
        }

        GameObject nodeInstance = Instantiate(nodePrefab, currentHit.point, Quaternion.identity, GameObject.Find("----|| Buildings ||----").transform);

        nodeInstance.GetComponent<Node>().SetMultiplier(nodeMultiplier);
        
        if (CurrentInteraction == InteractionState.PlacingFromNode)
            (targetBuilding as Node).AddBuilding(nodeInstance.GetComponent<Node>());
        else
        {
            (targetBuilding as Meteor).AddNode(nodeInstance.GetComponent<Node>());
            if (tutorialMode) tutorial.AdvanceTutorial(ref tutorial.placementParts);
        }

        ResetInteraction();
    }

    public void SpawnShroom(int shroomIndex)
    {
        currencyManager.DecreaseCurrencyAmount(placementCost);

        GameObject shroomInstance = Instantiate(shroomPrefabs[shroomIndex], currentHit.point, Quaternion.identity, GameObject.Find("----|| Buildings ||----").transform);

        if (previousInteraction == InteractionState.PlacingFromNode)
        {
            (targetBuilding as Node).AddBuilding(shroomInstance.GetComponent<Shroom>());

            shroomInstance.GetComponent<Shroom>().NewPrice(refNode.GetMultiplier());

            if (tutorialMode) tutorial.AdvanceTutorial(ref tutorial.placementParts);
        }  

        ResetInteraction();
    }

    private void DisplayBuildingRadius(out GameObject radiusDisplay)
    {
        radiusDisplay = targetBuilding.radiusDisplay;
        if (!radiusDisplay.activeSelf)
        {
            radiusDisplay.SetActive(true);
            StartCoroutine(targetBuilding.ExpandRadiusDisplay());
        }
    }
    private void DisplayBuildingHealth(out MeshRenderer healthDisplay)
    {
        healthDisplay = null;
        if (targetBuilding is Meteor)
        {
            healthDisplay = (targetBuilding as Meteor).healthDisplay;
        }
        else if (targetBuilding is Node)
        {
            Node targetNode = (targetBuilding as Node);
            healthDisplay = targetNode.healthDisplay;
            healthDisplay.sharedMaterial.SetFloat("_Value", targetNode.CurrentHealth / targetNode.MaxHealth);
        }
        else return;

        if (!healthDisplay.enabled)
        {
            healthDisplay.enabled = true;
        }
    }

    public void UnlockShroom(int shroomIndex)
    {
        if (unlockedShrooms > maxShroomsUnlockable) return;
        if (shroomIndex != 0) shroomSelectionMenuButtons[shroomIndex].sprite = shroomIconSprites[shroomIndex];
        unlockedShrooms++;
    }

    private void ShroomRadialMenu(GameObject radialMenu, Image[] radialButtons, out int hoveredButtonIndex, float reservedDegrees = 0)
    {
        hoveredButtonIndex = -1;
        shroomSelectionCostText.text = "";

        if (!radialMenu.activeSelf)
            radialMenu.SetActive(true);

        if (startingMousePosition == Vector2.zero)
        {
            startingMousePosition = mouseScreenPosition;
            radialMenu.GetComponent<RectTransform>().position = mouseScreenPosition;
        }

        float buttonAngularSize = (360 - reservedDegrees) / radialButtons.Length;

        if ((startingMousePosition - (Vector2)mouseScreenPosition).magnitude > radialExclusionZone)
        {
            List<float> angles = new();

            for (int angleIndex = 0; angleIndex < radialButtons.Length + 1; angleIndex++)
            {
                float angleToAdd = (reservedDegrees * 0.5f) + (angleIndex * buttonAngularSize);
                angles.Add(angleToAdd);
            }

            Vector2 mouseDirection = (startingMousePosition - (Vector2)mouseScreenPosition).normalized;
            float currentAngle = -Vector2.SignedAngle(Vector2.down, mouseDirection) + 180.0f;

            for (int i = 0; i < unlockedShrooms; i++)
            {
                if (currentAngle >= angles[i] && currentAngle < angles[i + 1])
                {
                    radialButtons[i].color = buttonHoverColour;
                    hoveredButtonIndex = i;
                }
                else
                {
                    radialButtons[i].color = buttonBaseColour;
                }
            }
        }
        else
        {
            foreach (Image radialButton in radialButtons)
            {
                radialButton.color = buttonBaseColour;
            }
        }

        if (hoveredButtonIndex >= 0)
        {
            shroomTooltip.SetActive(true);
            shroomName.text = shroomNames[hoveredButtonIndex];
            shroomDescription.text = shroomDescriptions[hoveredButtonIndex];
            if (cursorManager.currentCursorState != "CanBuy")
                cursorManager.ChangeCursor("CanBuy");
            cursorManager.DisplayCost(placementCost);

            if (shroomRadiusPreview == null)
            {
                shroomRadiusPreview = Instantiate(radiusPreviewPrefab, currentHit.point + new Vector3(0, 0.75f, 0), radiusPreviewPrefab.transform.rotation);
            }
            Material material = shroomRadiusPreview.GetComponent<MeshRenderer>().sharedMaterial;

            if (hoveredButtonIndex == 3) material.SetFloat("_Hole_Radius", 0.1667f);
            else material.SetFloat("_Hole_Radius", 0.0f);

            Shroom shroom = shroomPrefabs[hoveredButtonIndex].GetComponent<Shroom>();
            shroomRadiusPreview.transform.localScale = new Vector3(2 * shroom.TargeterComponent.range, 2 * shroom.TargeterComponent.range);
        }
        else
        {
            shroomTooltip.SetActive(false);

            if (cursorManager.currentCursorState != "Default")
                cursorManager.ChangeCursor("Default");
            cursorManager.DisplayCost();

            if (shroomRadiusPreview != null)
            {
                Destroy(shroomRadiusPreview);
                shroomRadiusPreview = null;
            }
        }
    }

    int GetNewNodeCost()
    {
        Building parent = activeBud.transform.parent.GetComponent<Building>();
        int cost = 0;

        if (parent is Meteor)
        {
            Meteor parentMeteor = parent as Meteor;
            cost = Node.GetNodeBaseCurrency();
            parentMeteor.ClearDestroyedNodes();
        }
        else if (parent is Node)
        {
            Node parentNode = parent as Node;
            cost = parentNode.GetNodeCost(parentNode.GetMultiplier() + 1);
        }
        return cost;
    }

    public void ResetInteraction(GameObject[] extraObjects = null)
    {
        selectionIndicator.enabled = false;
        selectionIndicator.rectTransform.sizeDelta = new Vector2(25, 25);
        startingMousePosition = Vector2.zero;
        CurrentInteraction = InteractionState.None;
        timeHeld = 0.0f;
        interactKeyHeld = false;

        if (sellButton.sprite == sellButtonActive || sellButton.sprite == sellButtonHighlight)
            sellButton.sprite = sellButtonDefault;

        if (targetBuilding != null)
        {
            if (targetBuilding is not Shroom)
            {
                targetBuilding.ResetLines();

                if (targetBuilding is Meteor)
                    (targetBuilding as Meteor).budDetached = false;
                else
                    (targetBuilding as Node).budDetached = false;
            }
        }

        if (activeBud is not null)
        {
            activeBud.SetActive(true);
            activeBud = null;
        }

        if (targetBuilding is not null)
        {
            targetBuilding.radiusDisplay.SetActive(false);
            targetBuilding = null;
        }

        if (shroomSelectionMenu.activeSelf)
            shroomSelectionMenu.SetActive(false);
        if (shroomTooltip.activeSelf)
            shroomTooltip.SetActive(false);

        if (shroomRadiusPreview != null)
        {
            Destroy(shroomRadiusPreview);
            shroomRadiusPreview = null;
        }

        if (extraObjects is not null)
        {
            for (int objectIndex = 0; objectIndex < extraObjects.Length; objectIndex++)
            {
                if (extraObjects[objectIndex] is not null)
                {
                    extraObjects[objectIndex].SetActive(false);
                    extraObjects[objectIndex] = null;
                }
            }
        }
    }



    private void DEBUG()
    {
        if (showMouseDirection)
        {
            if (GetRayHit().collider is null)
                Debug.DrawRay(mainCamera.transform.position, mainCamera.farClipPlane * 10 * (mouseWorldPosition - mainCamera.transform.position), Color.red);
            else
                Debug.DrawRay(mainCamera.transform.position, (mouseWorldPosition - mainCamera.transform.position).normalized * GetRayHit().distance, Color.green);
        }

        if (showCameraProjection)
        {
            if (screenHeight != mainCamera.pixelHeight)
                screenHeight = mainCamera.pixelHeight;
            if (screenWidth != mainCamera.pixelWidth)
                screenWidth = mainCamera.pixelWidth;

            Vector3 cameraPosition = mainCamera.transform.position;

            RaycastHit[] projectionCorners = new RaycastHit[]
            {
                DrawCameraProjectionRay(new Vector3(0, 0, 0.1f), cameraPosition),
                DrawCameraProjectionRay(new Vector3(0, screenHeight, 0.1f), cameraPosition),
                DrawCameraProjectionRay(new Vector3(screenWidth, screenHeight, 0.1f), cameraPosition),
                DrawCameraProjectionRay(new Vector3(screenWidth, 0, 0.1f), cameraPosition)
            };

            for(int pointIndex = 0; pointIndex < projectionCorners.Length; pointIndex++)
            {
                if (projectionCorners[pointIndex].collider is null)
                    continue;

                if (pointIndex == projectionCorners.Length - 1)
                {
                    if (projectionCorners[0].collider is null)
                        continue;

                    Debug.DrawLine(projectionCorners[pointIndex].point, projectionCorners[0].point, Color.white);
                }
                else
                {
                    if (projectionCorners[pointIndex+1].collider is null)
                        continue;

                    Debug.DrawLine(projectionCorners[pointIndex].point, projectionCorners[pointIndex+1].point, Color.white);
                }
            }
        }
    }

    private RaycastHit DrawCameraProjectionRay(Vector3 screenPosition, Vector3 cameraPosition)
    {
        Physics.Raycast(mainCamera.transform.position, mainCamera.ScreenToWorldPoint(screenPosition) - cameraPosition, out RaycastHit hit, Mathf.Infinity);

        if (hit.collider is null)
            Debug.DrawRay(mainCamera.transform.position, mainCamera.farClipPlane * 10 * (mainCamera.ScreenToWorldPoint(screenPosition) - cameraPosition), Color.white);
        else
            Debug.DrawRay(mainCamera.transform.position, (mainCamera.ScreenToWorldPoint(screenPosition) - cameraPosition).normalized * hit.distance, Color.white);
        
        return hit;
    }
}