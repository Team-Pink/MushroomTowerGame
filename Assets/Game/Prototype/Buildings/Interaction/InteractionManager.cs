using UnityEngine;
using UnityEngine.UI;
using static System.Linq.Enumerable;
using FloatList = System.Collections.Generic.List<float>;
using GameObjectList = System.Collections.Generic.List<UnityEngine.GameObject>;

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
    //private float interactionDuration = 0.0f;
    #endregion

    [Header("Placement")]
    #region Placement Variables
    [SerializeField] LayerMask placementBlockers;
    private LayerMask pylonLayer;
    private LayerMask placableLayers;
    private LayerMask budLayer;

    private GameObject activeBud;
    private Vector3 dragStartPosition;

    [SerializeField] float placementExclusionSize = 1;
    [SerializeField] float maxDistanceFromPylon = 10;
    private const float capsuleCheckBound = 5;

    [Space]
    [SerializeField] int maxPylonsPerHub = 6;
    [SerializeField] int maxTowersPerPylon = 5;
    [SerializeField] int maxPylonsPerPylon = 2;
    #endregion

    [Header("Currency")]
    #region Cost Of Placement
    private int pylonMultiplier = 1;
    private int placementCost = 0;
    private CurrencyManager currencyManager;
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

    [SerializeField, Space()] GameObject residualMenu;
    [SerializeField, NonReorderable] Image[] residualMenuButtons;

    [SerializeField, Space()] GameObject towerMenu;
    [SerializeField, NonReorderable] Image[] towerMenuButtons;

    [SerializeField, Space()] GameObject towerSelectionMenu;
    [SerializeField, NonReorderable] Image[] towerSelectionMenuButtons;
    #endregion

    [Header("Interaction")]
    #region Interaction Variables
    [SerializeField, Space()] KeyCode interactKey = KeyCode.Mouse0;
    private Vector3 mouseScreenPosition;
    private Vector3 mouseWorldPosition;

    private InteractionState currentInteraction = InteractionState.None;
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

    private void Awake()
    {
        ResetInteraction();

        mainCamera = Camera.main;

        placableLayers = LayerMask.GetMask("Ground");
        pylonLayer = LayerMask.GetMask("Pylon");
        budLayer = LayerMask.GetMask("Bud");

        currencyManager = gameObject.GetComponent<CurrencyManager>();
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

        if (Input.GetKeyDown(interactKey))
        {
            currentHit = GetRayHit(budLayer);
            if (currentHit.collider is not null)
            {
                if (currentHit.collider.CompareTag("Hub"))
                {
                    activeBud = currentHit.collider.gameObject;
                    activeBud.SetActive(false);
                    selectionIndicator.enabled = true;

                    CurrentInteraction = InteractionState.PlacingFromHub;
                }
                else if (currentHit.collider.CompareTag("Pylon"))
                {
                    activeBud = currentHit.collider.gameObject;
                    activeBud.SetActive(false);
                    selectionIndicator.enabled = true;

                    CurrentInteraction = InteractionState.PlacingFromPylon;
                }

                return;
            }
        }

        if (dragStartPosition != Vector3.zero)
            dragStartPosition = Vector3.zero;
    }

    private void BuildingInteractionState()
    {
        currentHit = GetRayHit(buildingLayers);

        if (currentHit.collider is null)
        {
            ResetInteraction();
            return;
        }

        DisplayBuildingRadius(out GameObject radiusDisplay);

        if (startingMousePosition == Vector2.zero)
            startingMousePosition = mouseScreenPosition;

        if (Input.GetKeyDown(interactKey))
        {
            radiusDisplay.SetActive(false);
            if (targetBuilding is Pylon)
            {
                Pylon targetPylon = targetBuilding as Pylon;
                if (!targetPylon.pylonResidual.activeSelf)
                {
                    CurrentInteraction = InteractionState.PylonMenu;
                }
                else
                {
                    CurrentInteraction = InteractionState.ResidualMenu;
                }
                startingMousePosition = Vector2.zero;
            }
            else if (targetBuilding is Tower)
            {
                CurrentInteraction = InteractionState.TowerMenu;
                startingMousePosition = Vector2.zero;
            }
            else
            {
                ResetInteraction(new GameObject[] { radiusDisplay });
                DefaultState();
            }
        }
    }
    private void PylonMenuState()
    {
        RadialMenu(pylonMenu, pylonMenuButtons, out int hoveredButtonIndex);

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
                if (!(targetBuilding as Pylon).Enhanced && currencyManager.CanDecreaseCurrencyAmount((targetBuilding as Pylon).GetForceEnhanceCost()))
                {
                    currencyManager.DecreaseCurrencyAmount((targetBuilding as Pylon).GetForceEnhanceCost());
                    (targetBuilding as Pylon).Enhance();
                }
                else
                {
                    hoveredButton.color = buttonBaseColour;
                    ResetInteraction();
                    return;
                }

            } // Force Enhance
            else if (hoveredButtonIndex == 1)
            {
                targetBuilding.Sell();
            } // Sell
            else if (hoveredButtonIndex == 2)
            {
                (targetBuilding as Pylon).SellAll();
            } // Sell All
            hoveredButton.color = buttonBaseColour;

            ResetInteraction();
        }
    }
    private void ResidualMenuState()
    {
        RadialMenu(residualMenu, residualMenuButtons, out int hoveredButtonIndex);

        Pylon targetPylon = targetBuilding as Pylon;

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
                targetPylon.CurrentHealth = targetPylon.MaxHealth;
                targetPylon.ToggleResidual(false);
            } // Repair
            else if (hoveredButtonIndex == 1)
            {
                (targetBuilding as Pylon).SellAll();
            } // Sell All

            Debug.Log(hoveredButton.name + " was selected", hoveredButton);
            hoveredButton.color = buttonBaseColour;

            ResetInteraction();
        }
    }
    private void TowerMenuState()
    {
        RadialMenu(towerMenu, towerMenuButtons, out int hoveredButtonIndex);

        if (Input.GetKeyDown(interactKey))
        {
            if (hoveredButtonIndex < 0)
            {
                ResetInteraction();
                return;
            }

            Image hoveredButton = towerMenuButtons[hoveredButtonIndex];

            if (hoveredButtonIndex == 1)
            {
                (targetBuilding as Tower).Sell();
            }
            else if (!(targetBuilding as Tower).Upgradeable)
            {
                (targetBuilding as Tower).Upgrade(hoveredButtonIndex);
            }

            Debug.Log(hoveredButton.name + " was selected", hoveredButton);
            hoveredButton.color = buttonBaseColour;

            ResetInteraction();
        }
    }

    private void PlacingFromHubState()
    {
        bool canPlace;

        targetBuilding = activeBud.transform.parent.GetComponent<Building>();
        DisplayBuildingRadius(out GameObject radiusDisplay);

        if (dragStartPosition == Vector3.zero)
            dragStartPosition = new Vector3(activeBud.transform.position.x, 0, activeBud.transform.position.z);

        currentHit = GetRayHit(placableLayers);
        if (currentHit.collider is not null)
        {
            bool spaceToPlace = SpaceToPlace(placementExclusionSize, placementBlockers);
            bool spaceForPylon = SpaceToPlace(2 * maxDistanceFromPylon, pylonLayer);

            float distanceFromHub = (dragStartPosition - new Vector3(currentHit.point.x, 0, currentHit.point.z)).magnitude;

            bool inPylonBuildRange = distanceFromHub < 3 * maxDistanceFromPylon;

            if (inPylonBuildRange && spaceToPlace && spaceForPylon && TargetIsPlane)
            {
                canPlace = true;
                selectionIndicator.color = Color.green;
            }
            else
            {
                canPlace = false;
                selectionIndicator.color = Color.red;
            }
        }
        else
        {
            canPlace = false;
            selectionIndicator.color = Color.red;
        }

        selectionIndicator.rectTransform.position = mouseScreenPosition;

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
        bool canPlace = false;
        bool placingPylon = false;

        selectionIndicator.color = Color.red;

        targetBuilding = activeBud.transform.parent.GetComponent<Building>();
        DisplayBuildingRadius(out GameObject radiusDisplay);

        if (dragStartPosition == Vector3.zero)
            dragStartPosition = new Vector3(activeBud.transform.position.x, 0, activeBud.transform.position.z);

        currentHit = GetRayHit(placableLayers);
        if (currentHit.collider is not null)
        {
            bool spaceToPlace = SpaceToPlace(placementExclusionSize, placementBlockers);
            bool spaceForPylon = SpaceToPlace(2 * maxDistanceFromPylon, pylonLayer);

            float distanceFromPylon = (dragStartPosition - new Vector3(currentHit.point.x, 0, currentHit.point.z)).magnitude;

            bool inTowerBuildRange = distanceFromPylon < maxDistanceFromPylon;
            bool inPylonBuildRange = distanceFromPylon > 2 * maxDistanceFromPylon && distanceFromPylon < 3 * maxDistanceFromPylon;

            bool towerPlacementCriteria = inTowerBuildRange && spaceToPlace;
            bool pylonPlacementCriteria = (targetBuilding as Pylon).Enhanced && inPylonBuildRange && spaceToPlace && spaceForPylon;

            if (towerPlacementCriteria || pylonPlacementCriteria)
            {
                canPlace = true;
                selectionIndicator.color = Color.green;

                if (pylonPlacementCriteria)
                    placingPylon = true;
            }
        }

        selectionIndicator.rectTransform.position = mouseScreenPosition;

        if (Input.GetKeyUp(interactKey))
        {
            if (canPlace)
            {
                selectionIndicator.color = Color.white;
                selectionIndicator.rectTransform.sizeDelta = new Vector2(10, 10);

                if (placingPylon)
                    AttemptToSpawnPylon();
                else
                    CurrentInteraction = InteractionState.TowerSelection;
            }
            else
            {
                ResetInteraction();
            }

            radiusDisplay.SetActive(false);
        }
    }
    private void TowerSelectionState()
    {
        targetBuilding.radiusDisplay.SetActive(false);

        RadialMenu(towerSelectionMenu, towerSelectionMenuButtons, out int hoveredButtonIndex, 30.0f);
        
        if (Input.GetKeyUp(interactKey) || Input.GetKeyDown(interactKey))
        {
            bool atTowerLimit;

            atTowerLimit = activeBud.transform.parent.GetComponent<Pylon>().connectedTowersCount >= maxTowersPerPylon;

            if (hoveredButtonIndex < 0 || atTowerLimit)
            {
                ResetInteraction();
                return;
            }

            int cost = towerPrefabs[hoveredButtonIndex].GetComponent<Tower>().purchaseCost;

            if (!currencyManager.CanDecreaseCurrencyAmount(cost))
            {
                ResetInteraction();
                return;
            }

            Debug.Log(hoveredButtonIndex);

            Image hoveredButton = towerSelectionMenuButtons[hoveredButtonIndex];

            placementCost = cost;

            SpawnTower(hoveredButtonIndex);

            Debug.Log(hoveredButton.name + " was selected", hoveredButton);
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

        if (activeBud.transform.parent.GetComponent<Hub>() != null)
        {
            pylonMultiplier = 1;
            cost = Pylon.GetPylonBaseCurrency();
            notMaxPylons = activeBud.transform.parent.GetComponent<Hub>().pylonCount < maxPylonsPerHub;
        }
        else if (activeBud.transform.parent.GetComponent<Pylon>() != null)
        {
            Pylon ParentPylon = activeBud.transform.parent.GetComponent<Pylon>();
            pylonMultiplier = ParentPylon.GetMultiplier() + 1;
            cost = ParentPylon.GetPylonCost(pylonMultiplier);
            
            notMaxPylons = activeBud.transform.parent.GetComponent<Pylon>().connectedPylonsCount < maxPylonsPerPylon;

            if (!(targetBuilding as Pylon).Enhanced)
            {
                ResetInteraction();
                return;
            }
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
            (targetBuilding as Hub).pylonCount++;
        }
        else
        {
            (targetBuilding as Pylon).connectedPylonsCount++;
        }

        GameObject pylonInstance = Instantiate(pylonPrefab, currentHit.point, Quaternion.identity);

        pylonInstance.GetComponent<Pylon>().SetMultiplier(pylonMultiplier);
        pylonInstance.GetComponent<Pylon>().parent = targetBuilding;

        if (CurrentInteraction == InteractionState.PlacingFromPylon)
            (targetBuilding as Pylon).AddBuilding(pylonInstance.GetComponent<Pylon>());

        ResetInteraction();
    }

    public void SpawnTower(int towerIndex)
    {
        currencyManager.DecreaseCurrencyAmount(placementCost);

        GameObject towerInstance = Instantiate(towerPrefabs[towerIndex], currentHit.point, Quaternion.identity);

        towerInstance.GetComponent<Tower>().parent = targetBuilding;

        if (previousInteraction == InteractionState.PlacingFromPylon)
        {
            (targetBuilding as Pylon).AddBuilding(towerInstance.GetComponent<Tower>());
            (targetBuilding as Pylon).connectedTowersCount++;
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

    private void RadialMenu(GameObject radialMenu, Image[] radialButtons, out int hoveredButtonIndex, float reservedDegrees = 0)
    {
        hoveredButtonIndex = -1;

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

            for (int i = 0; i < radialButtons.Length; i++)
            {
                if (currentAngle >= angles[i] && currentAngle < angles[i+1])
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
    }

    public void ResetInteraction(GameObject[] extraObjects = null)
    {
        selectionIndicator.enabled = false;
        selectionIndicator.rectTransform.sizeDelta = new Vector2(25, 25);
        startingMousePosition = Vector2.zero;
        CurrentInteraction = InteractionState.None;

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