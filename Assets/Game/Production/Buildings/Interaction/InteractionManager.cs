using UnityEngine;
using UnityEngine.UI;
using static System.Linq.Enumerable;
using FloatList = System.Collections.Generic.List<float>;
using GameObjectList = System.Collections.Generic.List<UnityEngine.GameObject>;
using TMPro;

public enum InteractionState
{
    None,

    BuildingInteraction,
    PylonMenu,
    ResidualMenu,
    TowerMenu,

    PlacingFromHub,
    PlacingFromPylon,
    TowerSelection
}

public class InteractionManager : MonoBehaviour
{
    [Header("Objects")]
    #region Object Variables
    [SerializeField] GameObject targetPlane;
    [SerializeField] GameObject pylonPrefab;
    [SerializeField, NonReorderable] private GameObjectList towerPrefabs = new(5);
    private Camera mainCamera;
    private const int towerPrefabAmount = 5;
    #endregion

    [Header("Building Selection")]
    #region Building Selection Variables
    [SerializeField] LayerMask buildingLayers;
    private Building targetBuilding;
    [SerializeField] GameObject towerTooltips;
    [SerializeField] TMP_Text towerName;
    [SerializeField] string[] towerNames;
    [SerializeField] TMP_Text towerDescription;
    [SerializeField, TextArea] string[] towerDescriptions;
    [SerializeField] GameObject towerRadiusPreviewPrefab;
    private GameObject towerRadiusPreview;
    #endregion

    [Header("Placement")]
    #region Placement Variables
    [SerializeField] bool placeOnPaths;
    LevelDataGrid levelDataGrid;
    [SerializeField] LayerMask placementBlockers;
    private LayerMask pylonLayer;
    private LayerMask placableLayers;
    private LayerMask budLayer;

    private GameObject activeBud;
    //private Vector3 dragStartPosition;

    [SerializeField] float placementExclusionSize = 1;
    [SerializeField] float maxDistanceFromPylon = 10;
    private const float capsuleCheckBound = 5;

    [Space]
    [SerializeField] int maxPylonsPerHub = 6;
    [HideInInspector] public static int hubMaxPylons;
    [SerializeField] int maxTowersPerPylon = 5;
    [HideInInspector] public static int pylonMaxTowers;
    [SerializeField] int maxPylonsPerPylon = 2;
    [HideInInspector] public static int pylonMaxPylons;
    #endregion

    [Header("Currency")]
    #region Cost Of Placement
    private int pylonMultiplier = 1;
    private int placementCost = 0;
    private CurrencyManager currencyManager;
    Pylon refPylon;
    Tower refTower;
    RadialType radialType;

    enum RadialType
    {
        Pylon,
        Residual,
        Tower,
        TowerSelection,
    }
    #endregion

    [Header("UI")]
    #region UI Variables
    [SerializeField] private Color buttonBaseColour;
    [SerializeField] private Color buttonHoverColour;

    [SerializeField, Space()] Image selectionIndicator;
    [SerializeField] private float radialExclusionZone = 10.0f;
    private Vector2 startingMousePosition;

    [SerializeField, Space()] GameObject pylonMenu;
    [SerializeField, NonReorderable] Image[] pylonMenuButtons;
    private Sprite[] pylonMenuDefaultButtons;
    [SerializeField] Sprite[] pylonMenuAltButtons;
    [SerializeField] TMP_Text pylonMenuCostText;

    [SerializeField, Space()] GameObject residualMenu;
    [SerializeField, NonReorderable] Image[] residualMenuButtons;
    private Sprite[] residualMenuDefaultButtons;
    [SerializeField] Sprite[] residualMenuAltButtons;
    [SerializeField] TMP_Text residualMenuCostText;

    [SerializeField, Space()] GameObject towerMenu;
    [SerializeField, NonReorderable] Image[] towerMenuButtons;
    private Sprite[] towerMenuDefaultButtons;
    [SerializeField] Sprite[] towerMenuAltButtons;
    [SerializeField] TMP_Text towerMenuCostText;

    [SerializeField, Space()] GameObject towerSelectionMenu;
    [SerializeField, NonReorderable] Image[] towerSelectionMenuButtons;
    [SerializeField] Sprite lockedTowerSprite;
    private readonly Sprite[] towerIconSprites = new Sprite[5];
    private int unlockedTowers = 0;
    private readonly int maxTowersUnlockable = 5;
    [SerializeField] bool unlockAllTowers = false;
    [SerializeField] TMP_Text towerSelectionCostText;

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
    private Vector3 initialInteractPosition;
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

    private TutorialManager tutorialManager;

    private void Awake()
    {
        ResetInteraction();

        mainCamera = Camera.main;
        levelDataGrid = GetComponent<LevelDataGrid>();

        placableLayers = LayerMask.GetMask("Ground");
        pylonLayer = LayerMask.GetMask("Pylon");
        budLayer = LayerMask.GetMask("Bud");

        currencyManager = gameObject.GetComponent<CurrencyManager>();
        cursorManager = gameObject.GetComponent<CursorManager>();

        for (int i = 0; i < towerSelectionMenuButtons.Length; i++)
        {
            towerIconSprites[i] = towerSelectionMenuButtons[i].sprite;

            if (i >= unlockedTowers)
            {
                towerSelectionMenuButtons[i].sprite = lockedTowerSprite;
            }
        }

        pylonMenuDefaultButtons = new Sprite[pylonMenuButtons.Length];
        for (int i = 0; i < pylonMenuButtons.Length; i++)
            pylonMenuDefaultButtons[i] = pylonMenuButtons[i].sprite;

        residualMenuDefaultButtons = new Sprite[residualMenuButtons.Length];
        for (int i = 0; i < residualMenuButtons.Length; i++)
            residualMenuDefaultButtons[i] = residualMenuButtons[i].sprite;

        towerMenuDefaultButtons = new Sprite[towerMenuButtons.Length];
        for (int i = 0; i < towerMenuButtons.Length; i++)
            towerMenuDefaultButtons[i] = towerMenuButtons[i].sprite;


        if (unlockAllTowers)
        {
            for (int i = unlockedTowers - 1; i < maxTowersUnlockable; i++)
            {
                UnlockTower(i);
            }
        }
        hubMaxPylons = maxPylonsPerHub;
        pylonMaxTowers = maxTowersPerPylon;
        pylonMaxPylons = maxPylonsPerPylon;

        tutorialManager = GetComponent<TutorialManager>();
    }

    private void OnValidate()
    {
        if (towerPrefabs.Count == towerPrefabAmount)
            return;

        Debug.LogWarning("Stop that, the list towerPrefabs should be exactly " + towerPrefabAmount + " elements!", this);

        while (towerPrefabs.Count < towerPrefabAmount)
        {
            towerPrefabs.Add(null);
        }
        while (towerPrefabs.Count > towerPrefabAmount)
        {
            towerPrefabs.RemoveAt(towerPrefabs.Count - 1);
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
            case InteractionState.PylonMenu:
                PylonMenuState();
                break;
            case InteractionState.ResidualMenu:
                ResidualMenuState();
                break;
            case InteractionState.TowerMenu:
                TowerMenuState();
                break;

            case InteractionState.PlacingFromHub:
                PlacingFromHubState();
                break;
            case InteractionState.PlacingFromPylon:
                PlacingFromPylonState();
                break;
            case InteractionState.TowerSelection:
                TowerSelectionState();
                break;
        }

#if UNITY_EDITOR
        DEBUG();
#endif
    }

    private void DefaultState()
    {
        currentHit = GetRayHit(budLayer);

        cursorManager.ChangeCursor("Default");

        if (tutorialMode && !tutorialManager.CorrectTutorialPlacement(mouseScreenPosition))
        {
            ResetInteraction();
            return;
        }

        if (currentHit.collider is null)
        {
            currentHit = GetRayHit(buildingLayers);

            if (currentHit.collider is null)
            {
                if (targetBuilding is not null)
                    ResetInteraction();
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

        if (targetBuilding is not Tower)
        {
            if (targetBuilding is Pylon && (targetBuilding as Pylon).isResidual)
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
                if (targetBuilding is Hub && !(targetBuilding as Hub).AtMaxPylons)
                {
                    radiusDisplay.SetActive(false);
                    if (healthDisplay != null) healthDisplay.enabled = false;
                    CurrentInteraction = InteractionState.PlacingFromHub;
                    return;
                }
                else if (targetBuilding is Pylon && !(targetBuilding as Pylon).AtMaxBuildings)
                {
                    radiusDisplay.SetActive(false);
                    if (healthDisplay != null) healthDisplay.enabled = false;
                    CurrentInteraction = InteractionState.PlacingFromPylon;
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

                if (targetBuilding is Pylon)
                {
                    if (!(targetBuilding as Pylon).pylonResidual.activeSelf)
                    {
                        CurrentInteraction = InteractionState.PylonMenu;
                    }
                    else
                    {
                        CurrentInteraction = InteractionState.ResidualMenu;
                    }
                }
                else if (targetBuilding is Tower)
                {
                    CurrentInteraction = InteractionState.TowerMenu;
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
            interactKeyHeld = true;
            initialInteractPosition = mouseScreenPosition;
        }
    }
    private void PylonMenuState()
    {
        if (Input.GetKeyDown(cancelKey))
        {
            ResetInteraction();
            return;
        }
        radialType = RadialType.Pylon;

        refPylon = targetBuilding as Pylon;

        RadialMenu(pylonMenu, ref pylonMenuButtons, pylonMenuDefaultButtons, pylonMenuAltButtons, out int hoveredButtonIndex);

        if (hoveredButtonIndex == 0) refPylon.ShowDeactivateLines();
        else if (hoveredButtonIndex == 1) refPylon.ShowSellLines();
        else refPylon.ShowDefaultLines();

        if (Input.GetKeyDown(interactKey))
        {
            if (hoveredButtonIndex < 0)
            {
                ResetInteraction();
                return;
            }

            Image hoveredButton = pylonMenuButtons[hoveredButtonIndex];

            if (hoveredButtonIndex == 0)
            {
                targetBuilding.Sell();
            } // Sell
            else if (hoveredButtonIndex == 1)
            {
                (targetBuilding as Pylon).SellAll();
            } // Sell All
            hoveredButton.color = buttonBaseColour;

            ResetInteraction();
        }
    }
    private void ResidualMenuState()
    {
        radialType = RadialType.Residual;

        Pylon targetPylon = targetBuilding as Pylon;
        refPylon = targetPylon;

        RadialMenu(residualMenu, ref residualMenuButtons, residualMenuDefaultButtons, residualMenuAltButtons, out int hoveredButtonIndex);

        if (hoveredButtonIndex == 0) refPylon.ShowDefaultLines();
        else if (hoveredButtonIndex == 1) refPylon.ShowSellLines();
        else refPylon.ShowDeactivateLines();

        if (Input.GetKeyDown(interactKey))
        {
            if (hoveredButtonIndex < 0)
            {
                ResetInteraction();
                return;
            }

            Image hoveredButton = pylonMenuButtons[hoveredButtonIndex];

            if (hoveredButtonIndex == 0)
            {
                if (currencyManager.CanDecreaseCurrencyAmount(targetPylon.GetPylonCost()))
                {
                    currencyManager.DecreaseCurrencyAmount(targetPylon.GetPylonCost());
                    targetPylon.CurrentHealth = targetPylon.MaxHealth;
                    targetPylon.ToggleResidual(false);
                }
            } // Repair
            else if (hoveredButtonIndex == 1)
            {
                (targetBuilding as Pylon).SellAll();
            } // Sell All
            hoveredButton.color = buttonBaseColour;

            ResetInteraction();
        }
    }
    private void TowerMenuState()
    {
        if (Input.GetKeyDown(cancelKey))
        {
            ResetInteraction();
            return;
        }
        radialType = RadialType.Tower;

        refTower = targetBuilding as Tower;

        RadialMenu(towerMenu, ref towerMenuButtons, towerMenuDefaultButtons, towerMenuAltButtons, out int hoveredButtonIndex, 270);

        if (Input.GetKeyDown(interactKey))
        {
            if (hoveredButtonIndex < 0)
            {
                ResetInteraction();
                return;
            }

            Image hoveredButton = towerMenuButtons[hoveredButtonIndex];

            if (hoveredButtonIndex == 0)
            {
                (targetBuilding as Tower).Sell();
            }

            Debug.Log(hoveredButton.name + " was selected", hoveredButton);
            hoveredButton.color = buttonBaseColour;

            ResetInteraction();
        }
    }

    private void PlacingFromHubState()
    {
        if (Input.GetKeyDown(cancelKey))
        {
            tutorialManager.ReverseTutorial(ref tutorialManager.placementParts);
            ResetInteraction();
            return;
        }

        bool canPlace = false;
        selectionIndicator.enabled = true;
        selectionIndicator.color = Color.red;
        activeBud = targetBuilding.bud;

        (targetBuilding as Hub).budDetached = true;

        DisplayBuildingRadius(out GameObject radiusDisplay);

        selectionIndicator.rectTransform.position = mouseScreenPosition;

        if (tutorialMode && !tutorialManager.CorrectTutorialPlacement(mouseScreenPosition))
        {
            if (Input.GetKeyUp(interactKey))
            {
                tutorialManager.ReverseTutorial(ref tutorialManager.placementParts);
                ResetInteraction();
            }

            return;
        }

        currentHit = GetRayHit(placableLayers);

        if (currentHit.collider is not null)
        {
            bool isPlaceable;
            if (placeOnPaths) isPlaceable = levelDataGrid.GetTileTypeAtPoint(currentHit.point) == TileType.Path;
            else isPlaceable = levelDataGrid.GetTileTypeAtPoint(currentHit.point) == TileType.Mud;

            if (isPlaceable)
            {
                bool spaceToPlace = SpaceToPlace(placementExclusionSize, placementBlockers);
                bool spaceForPylon = SpaceToPlace(2 * maxDistanceFromPylon, pylonLayer);

                float distanceFromHub = (targetBuilding.transform.position - new Vector3(currentHit.point.x, 0, currentHit.point.z)).magnitude;

                bool inPylonBuildRange = distanceFromHub < 3 * maxDistanceFromPylon;

                if (inPylonBuildRange && spaceToPlace && spaceForPylon && TargetIsPlane)
                {
                    canPlace = true;
                    selectionIndicator.color = Color.green;
                    cursorManager.ChangeCursor("CanPlace");
                }
                else cursorManager.ChangeCursor("CannotPlace");
            }
            else cursorManager.ChangeCursor("CannotPlace");
        }
        else cursorManager.ChangeCursor("CannotPlace");

        if (Input.GetKeyUp(interactKey))
        {
            if (canPlace)
            {
                selectionIndicator.color = Color.white;
                selectionIndicator.rectTransform.sizeDelta = new Vector2(10, 10);

                AttemptToSpawnPylon();
            }
            else
            {
                ResetInteraction();
            }

            radiusDisplay.SetActive(false);
        }
    }
    private void PlacingFromPylonState()
    {
        if (Input.GetKeyDown(cancelKey))
        {
            tutorialManager.ReverseTutorial(ref tutorialManager.placementParts);
            ResetInteraction();
            return;
        }

        bool canPlace = false;
        bool placingPylon = false;
        selectionIndicator.enabled = true;
        selectionIndicator.color = Color.red;
        activeBud = targetBuilding.bud;

        refPylon = targetBuilding as Pylon;
        (targetBuilding as Pylon).budDetached = true;
        
        DisplayBuildingRadius(out GameObject radiusDisplay);

        selectionIndicator.rectTransform.position = mouseScreenPosition;

        if (tutorialMode && !tutorialManager.CorrectTutorialPlacement(mouseScreenPosition))
        {
            if (Input.GetKeyUp(interactKey))
            {
                tutorialManager.ReverseTutorial(ref tutorialManager.placementParts);
                ResetInteraction();
            }

            return;
        }

        currentHit = GetRayHit(placableLayers);
        if (currentHit.collider != null)
        {
            bool isPlaceable;
            if (placeOnPaths) isPlaceable = levelDataGrid.GetTileTypeAtPoint(currentHit.point) == TileType.Path;
            else isPlaceable = levelDataGrid.GetTileTypeAtPoint(currentHit.point) == TileType.Mud;

            if (isPlaceable)
            {
                bool spaceToPlace = SpaceToPlace(placementExclusionSize, placementBlockers);
                bool spaceForPylon = SpaceToPlace(2 * maxDistanceFromPylon, pylonLayer);

                float distanceFromPylon = (targetBuilding.transform.position - new Vector3(currentHit.point.x, 0, currentHit.point.z)).magnitude;

                bool inTowerBuildRange = distanceFromPylon < maxDistanceFromPylon;
                bool inPylonBuildRange = distanceFromPylon > 2 * maxDistanceFromPylon && distanceFromPylon < 3 * maxDistanceFromPylon;

                bool towerPlacementCriteria = inTowerBuildRange && spaceToPlace && !(targetBuilding as Pylon).AtMaxTowers;
                bool pylonPlacementCriteria = inPylonBuildRange && spaceToPlace && spaceForPylon && !(targetBuilding as Pylon).AtMaxPylons;

                if (towerPlacementCriteria || pylonPlacementCriteria)
                {
                    canPlace = true;
                    selectionIndicator.color = Color.green;

                    //bubble logic for cursor goes here... TODO IN GOLD!!!!
                    cursorManager.ChangeCursor("CanPlace");

                    if (pylonPlacementCriteria)
                        placingPylon = true;
                }
                else cursorManager.ChangeCursor("CannotPlace");
            }
            else cursorManager.ChangeCursor("CannotPlace");
        }
        else cursorManager.ChangeCursor("CannotPlace");

        if (Input.GetKeyUp(interactKey))
        {
            if (canPlace)
            {
                cursorManager.ChangeCursor("Default");
                selectionIndicator.color = Color.white;
                selectionIndicator.rectTransform.sizeDelta = new Vector2(10, 10);

                if (placingPylon)
                    AttemptToSpawnPylon();
                else
                    CurrentInteraction = InteractionState.TowerSelection;
            }
            else
            {
                cursorManager.ChangeCursor("Default");
                ResetInteraction();
            }

            radiusDisplay.SetActive(false);
        }
    }
    private void TowerSelectionState()
    {
        if (Input.GetKeyDown(cancelKey))
        {
            if (tutorialMode)
            {
                tutorialManager.ReverseTutorial(ref tutorialManager.placementParts);
                tutorialManager.ReverseTutorial(ref tutorialManager.placementParts);
            }

            ResetInteraction();
            return;
        }

        targetBuilding.radiusDisplay.SetActive(false);

        radialType = RadialType.TowerSelection;

        TowerRadialMenu(towerSelectionMenu, towerSelectionMenuButtons, out int hoveredButtonIndex, 30.0f);
        
        if (Input.GetKeyUp(interactKey) || Input.GetKeyDown(interactKey))
        {
            bool atTowerLimit;

            atTowerLimit = activeBud.transform.parent.GetComponent<Pylon>().connectedTowersCount >= maxTowersPerPylon;

            if (hoveredButtonIndex < 0 || atTowerLimit)
            {
                if (tutorialMode)
                {
                    tutorialManager.ReverseTutorial(ref tutorialManager.placementParts);
                    tutorialManager.ReverseTutorial(ref tutorialManager.placementParts);
                }

                ResetInteraction();
                return;
            }

            int cost = towerPrefabs[hoveredButtonIndex].GetComponent<Tower>().purchaseCost * refPylon.GetMultiplier();

            if (!currencyManager.CanDecreaseCurrencyAmount(cost))
            {
                ResetInteraction();
                return;
            }


            Image hoveredButton = towerSelectionMenuButtons[hoveredButtonIndex];

            placementCost = cost;

            SpawnTower(hoveredButtonIndex);

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

    private void AttemptToSpawnPylon()
    {
        int cost = 0;
        bool notMaxPylons = false;

        Building parent = activeBud.transform.parent.GetComponent<Building>();

        if (parent is Hub)
        {
            Hub parentHub = parent as Hub;
            pylonMultiplier = 1;
            cost = Pylon.GetPylonBaseCurrency();
            parentHub.ClearDestroyedPylons();
            notMaxPylons = parentHub.pylonCount < maxPylonsPerHub;
        }
        else if (parent is Pylon)
        {
            Pylon parentPylon = parent as Pylon;
            pylonMultiplier = parentPylon.GetMultiplier() + 1;
            cost = parentPylon.GetPylonCost(pylonMultiplier);
            
            notMaxPylons = parentPylon.connectedPylonsCount < maxPylonsPerPylon;
        }

        if (!TargetIsPlane || !currencyManager.CanDecreaseCurrencyAmount(cost) || !notMaxPylons)
        {
            ResetInteraction();
            return;
        }

        placementCost = cost;
        SpawnPylon();
    }

    private void SpawnPylon()
    {
        currencyManager.DecreaseCurrencyAmount(placementCost);

        if (activeBud.transform.parent.GetComponent<Hub>() != null)
        {
            activeBud.transform.parent.GetComponent<Hub>().ClearDestroyedPylons();
        }

        GameObject pylonInstance = Instantiate(pylonPrefab, currentHit.point, Quaternion.identity, GameObject.Find("----|| Buildings ||----").transform);

        pylonInstance.GetComponent<Pylon>().SetMultiplier(pylonMultiplier);
        
        if (CurrentInteraction == InteractionState.PlacingFromPylon)
            (targetBuilding as Pylon).AddBuilding(pylonInstance.GetComponent<Pylon>());
        else
        {
            (targetBuilding as Hub).AddPylon(pylonInstance.GetComponent<Pylon>());
            if (tutorialMode) tutorialManager.AdvanceTutorial(ref tutorialManager.placementParts);
        }

        ResetInteraction();
    }

    public void SpawnTower(int towerIndex)
    {
        currencyManager.DecreaseCurrencyAmount(placementCost);

        GameObject towerInstance = Instantiate(towerPrefabs[towerIndex], currentHit.point, Quaternion.identity, GameObject.Find("----|| Buildings ||----").transform);

        if (previousInteraction == InteractionState.PlacingFromPylon)
        {
            (targetBuilding as Pylon).AddBuilding(towerInstance.GetComponent<Tower>());

            towerInstance.GetComponent<Tower>().NewPrice(refPylon.GetMultiplier());

            if (tutorialMode) tutorialManager.AdvanceTutorial(ref tutorialManager.placementParts);
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
        if (targetBuilding is Hub)
        {
            healthDisplay = (targetBuilding as Hub).healthDisplay;
        }
        else if (targetBuilding is Pylon)
        {
            Pylon targetPylon = (targetBuilding as Pylon);
            healthDisplay = targetPylon.healthDisplay;
            healthDisplay.sharedMaterial.SetFloat("_Value", targetPylon.CurrentHealth / targetPylon.MaxHealth);
        }
        else return;

        if (!healthDisplay.enabled)
        {
            healthDisplay.enabled = true;
        }
    }


    public void UnlockTower(int towerIndex)
    {
        if (unlockedTowers > maxTowersUnlockable) return;
        if (towerIndex != 0) towerSelectionMenuButtons[towerIndex].sprite = towerIconSprites[towerIndex];
        unlockedTowers++;
    }
    private void RadialMenu(GameObject radialMenu, ref Image[] radialButtons,
        Sprite[] buttonSprites, Sprite[] altbuttonSprites,
        out int hoveredButtonIndex, float reservedDegrees = 0)
    {
        hoveredButtonIndex = -1;
        towerMenuCostText.text = "";
        pylonMenuCostText.text = "";
        residualMenuCostText.text = "";

        if (!radialMenu.activeSelf)
            radialMenu.SetActive(true);

        if (startingMousePosition == Vector2.zero)
        {
            startingMousePosition = initialInteractPosition;
            radialMenu.GetComponent<RectTransform>().position = initialInteractPosition;
        }

        float buttonAngularSize = (360 - reservedDegrees) / radialButtons.Length;

        if ((startingMousePosition - (Vector2)mouseScreenPosition).magnitude > radialExclusionZone)
        {
            FloatList angles = new();

            for (int angleIndex = 0; angleIndex < radialButtons.Length + 1; angleIndex++)
            {
                float angleToAdd = (reservedDegrees * 0.5f) + (angleIndex * buttonAngularSize);
                angles.Add(angleToAdd);
            }

            Vector2 mouseDirection = (startingMousePosition - (Vector2)mouseScreenPosition).normalized;
            float currentAngle = -Vector2.SignedAngle(Vector2.down, mouseDirection) + 180.0f;

            for (int i = 0; i < radialButtons.Length; i++)
            {
                if (currentAngle >= angles[i] && currentAngle < angles[i+1])
                {
                    radialButtons[i].sprite = altbuttonSprites[i];
                    hoveredButtonIndex = i;
                    RadialCostDisplays(hoveredButtonIndex);
                }
                else
                {
                    radialButtons[i].sprite = buttonSprites[i];
                }
            }
        }
        else
        {
            for (int i = 0; i < radialButtons.Length; i++)
            {
                radialButtons[i].sprite = buttonSprites[i];
            }
        }
    }
    private void TowerRadialMenu(GameObject radialMenu, Image[] radialButtons, out int hoveredButtonIndex, float reservedDegrees = 0)
    {
        hoveredButtonIndex = -1;
        towerSelectionCostText.text = "";

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
            FloatList angles = new();

            for (int angleIndex = 0; angleIndex < radialButtons.Length + 1; angleIndex++)
            {
                float angleToAdd = (reservedDegrees * 0.5f) + (angleIndex * buttonAngularSize);
                angles.Add(angleToAdd);
            }

            Vector2 mouseDirection = (startingMousePosition - (Vector2)mouseScreenPosition).normalized;
            float currentAngle = -Vector2.SignedAngle(Vector2.down, mouseDirection) + 180.0f;

            for (int i = 0; i < unlockedTowers; i++)
            {
                if (currentAngle >= angles[i] && currentAngle < angles[i + 1])
                {
                    radialButtons[i].color = buttonHoverColour;
                    hoveredButtonIndex = i;
                    RadialCostDisplays(i);
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
            towerTooltips.SetActive(true);
            towerName.text = towerNames[hoveredButtonIndex];
            towerDescription.text = towerDescriptions[hoveredButtonIndex];

            if (towerRadiusPreview == null)
            {
                towerRadiusPreview = Instantiate(towerRadiusPreviewPrefab, currentHit.point + new Vector3(0, 0.75f, 0), towerRadiusPreviewPrefab.transform.rotation);
            }
            Material material = towerRadiusPreview.GetComponent<MeshRenderer>().sharedMaterial;

            if (hoveredButtonIndex == 3) material.SetFloat("_Hole_Radius", 0.1667f);
            else material.SetFloat("_Hole_Radius", 0.0f);

            Tower tower = towerPrefabs[hoveredButtonIndex].GetComponent<Tower>();
            towerRadiusPreview.transform.localScale = new Vector3(2 * tower.TargeterComponent.range, 2 * tower.TargeterComponent.range);
        }
        else
        {
            towerTooltips.SetActive(false);

            if (towerRadiusPreview != null)
            {
                Destroy(towerRadiusPreview);
                towerRadiusPreview = null;
            }
        }
    }

    void RadialCostDisplays(int index)
    {
        
        switch(radialType)
        {
            case (RadialType.Pylon):
                int pylonCost;
                if (index == 0)
                {
                    pylonCost = refPylon.GetPylonSellAmount();
                    pylonMenuCostText.text = "+ " + pylonCost.ToString();
                    pylonMenuCostText.color = sellColour;
                }//Sell
                if (index == 1)
                {
                    pylonCost = refPylon.GetPylonSellAllAmount();
                    pylonMenuCostText.text = "+ " + pylonCost.ToString();
                    pylonMenuCostText.color = sellColour;
                }//Sell All
                break;

            case (RadialType.Residual):
                int residualCost;
                if (index == 0)
                {
                    residualCost = refPylon.GetPylonCost();
                    residualMenuCostText.text = "- " + residualCost.ToString();
                    if (!currencyManager.CanDecreaseCurrencyAmount(residualCost))
                        residualMenuCostText.color = canNotPurchaseColour;
                    else residualMenuCostText.color = canPurchaseColour;
                }//Repair
                if (index == 1)
                {
                    residualCost = refPylon.GetPylonSellAllAmount();
                    residualMenuCostText.text = "+ " + residualCost.ToString();
                    residualMenuCostText.color = sellColour;
                }//Sell All
                break;

            case (RadialType.Tower):
                int towerCost;
                if (index == 1)
                {
                    towerCost = refTower.SellPrice();
                    towerMenuCostText.text = "+ " + towerCost.ToString();
                    towerMenuCostText.color = sellColour;
                }//Sell
                break;

            case (RadialType.TowerSelection):
                int towerSelectionCost = 10 * refPylon.GetMultiplier();
                towerSelectionCostText.text = "- " + towerSelectionCost.ToString();
                if (!currencyManager.CanDecreaseCurrencyAmount(towerSelectionCost))
                    towerSelectionCostText.color = canNotPurchaseColour;
                else towerSelectionCostText.color = canPurchaseColour;
                break;
        }
    }

    public void ResetInteraction(GameObject[] extraObjects = null)
    {
        selectionIndicator.enabled = false;
        selectionIndicator.rectTransform.sizeDelta = new Vector2(25, 25);
        startingMousePosition = Vector2.zero;
        CurrentInteraction = InteractionState.None;
        timeHeld = 0.0f;
        interactKeyHeld = false;
        initialInteractPosition = Vector3.zero;

        if (targetBuilding != null)
        {
            if (targetBuilding is not Tower)
            {
                targetBuilding.ResetLines();

                if (targetBuilding is Hub)
                    (targetBuilding as Hub).budDetached = false;
                else
                    (targetBuilding as Pylon).budDetached = false;
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

        if (pylonMenu.activeSelf)
            pylonMenu.SetActive(false);

        if (residualMenu.activeSelf)
            residualMenu.SetActive(false);

        if (towerMenu.activeSelf)
            towerMenu.SetActive(false);

        if (towerSelectionMenu.activeSelf)
            towerSelectionMenu.SetActive(false);
        if (towerTooltips.activeSelf)
            towerTooltips.SetActive(false);

        if (towerRadiusPreview != null)
        {
            Destroy(towerRadiusPreview);
            towerRadiusPreview = null;
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