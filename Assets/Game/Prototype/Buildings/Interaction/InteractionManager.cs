using GameObjectList = System.Collections.Generic.List<UnityEngine.GameObject>;
using FloatList = System.Collections.Generic.List<float>;

using NonReorderable = UnityEngine.NonReorderableAttribute;
using SerializeField = UnityEngine.SerializeField;
using MonoBehaviour = UnityEngine.MonoBehaviour;
using RectTransform = UnityEngine.RectTransform;
using GameObject = UnityEngine.GameObject;
using Physics = UnityEngine.Physics;
using Header = UnityEngine.HeaderAttribute;
using Camera = UnityEngine.Camera;
using Image = UnityEngine.UI.Image;
using Input = UnityEngine.Input;
using Debug = UnityEngine.Debug;
using Space = UnityEngine.SpaceAttribute;
using Time = UnityEngine.Time;

using Quaternion = UnityEngine.Quaternion;
using RaycastHit = UnityEngine.RaycastHit;
using LayerMask = UnityEngine.LayerMask;
using KeyCode = UnityEngine.KeyCode;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Mathf = UnityEngine.Mathf;
using Color = UnityEngine.Color;

using static System.Linq.Enumerable;

public enum InteractionState
{
    None,

    BuildingInteraction,
    PylonMenu,
    TowerMenu,
    DraggingBuilding,

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
    private float interactionDuration = 0.0f;
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
            case InteractionState.TowerMenu:
                TowerMenuState();
                break;
            case InteractionState.DraggingBuilding:
                DraggingBuildingState();
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

            currentHit = GetRayHit(buildingLayers);
            if (currentHit.collider is not null)
            {
                targetBuilding = currentHit.collider.gameObject.GetComponent<Building>();
                CurrentInteraction = InteractionState.BuildingInteraction;
                return;
            }
        }

        if (dragStartPosition != Vector3.zero)
            dragStartPosition = Vector3.zero;
    }

    private void BuildingInteractionState()
    {
        DisplayBuildingRadius(out GameObject radiusDisplay);

        if (startingMousePosition == Vector2.zero)
            startingMousePosition = mouseScreenPosition;

        if (Input.GetKeyDown(interactKey))
        {
            ResetInteraction(new GameObject[] { radiusDisplay });
            DefaultState();
            return;
        }

        if (Input.GetKey(interactKey))
        {
            if ((startingMousePosition - (Vector2)mouseScreenPosition).magnitude > radialExclusionZone)
            {
                if (targetBuilding is not Hub)
                {
                    CurrentInteraction = InteractionState.DraggingBuilding;
                    startingMousePosition = Vector2.zero;
                    return;
                }
            }

            if (interactionDuration > 0.5f)
            {
                if (targetBuilding is Pylon)
                {
                    CurrentInteraction = InteractionState.PylonMenu;
                    startingMousePosition = Vector2.zero;
                    return;
                }
                else if (targetBuilding is Tower)
                {
                    CurrentInteraction = InteractionState.TowerMenu;
                    startingMousePosition = Vector2.zero;
                    return;
                }
            }
            else
                interactionDuration += Time.deltaTime;
        }
    }
    private void PylonMenuState()
    {
        RadialMenu(pylonMenu, pylonMenuButtons, out int hoveredButtonIndex);

        if (Input.GetKeyUp(interactKey) || Input.GetKeyDown(interactKey))
        {
            if (hoveredButtonIndex < 0)
            {
                ResetInteraction();
                return;
            }

            Image hoveredButton = pylonMenuButtons[hoveredButtonIndex];

            if (hoveredButtonIndex == 0)
            {
                if (!(targetBuilding as Pylon).Enhanced)
                {
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

        if (Input.GetKeyUp(interactKey) || Input.GetKeyDown(interactKey))
        {
            if (hoveredButtonIndex < 0)
            {
                ResetInteraction();
                return;
            }

            Image hoveredButton = towerMenuButtons[hoveredButtonIndex];

            if (!(targetBuilding as Tower).Upgraded)
            {
                (targetBuilding as Tower).Upgrade(hoveredButtonIndex);
            }

            Debug.Log(hoveredButton.name + " was selected", hoveredButton);
            hoveredButton.color = buttonBaseColour;

            ResetInteraction();
        }
    }
    private void DraggingBuildingState()
    {
        currentHit = GetRayHit(buildingLayers);
        Building heldBuilding = targetBuilding;

        bool validSellPoint = false;

        if (currentHit.collider is not null)
        {
            Building hoveredBuilding = currentHit.collider.GetComponent<Building>();

            if (hoveredBuilding is Tower)
                return;

            if (hoveredBuilding is Hub)
                validSellPoint = true;
            else if (hoveredBuilding is Pylon)
            {
                if ((hoveredBuilding as Pylon).IsBuildingInList(heldBuilding))
                    validSellPoint = true;
            }
        }

        if (Input.GetKeyUp(interactKey))
        {
            if (validSellPoint)
            {
                heldBuilding.Sell();
                ResetInteraction();
            }
            else
                CurrentInteraction = InteractionState.BuildingInteraction;
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

            if (inPylonBuildRange && spaceToPlace && spaceForPylon)
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
        bool canPlace;
        bool placingPylon = false;

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

            if (inTowerBuildRange & spaceToPlace)
            {
                canPlace = true;
                selectionIndicator.color = Color.green;
            }
            else if (inPylonBuildRange && spaceToPlace && spaceForPylon)
            {
                placingPylon = true;
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
        if (!TargetIsPlane)
        {
            ResetInteraction();
            return;
        }

        RadialMenu(towerSelectionMenu, towerSelectionMenuButtons, out int hoveredButtonIndex, 30.0f);

        if (Input.GetKeyUp(interactKey) || Input.GetKeyDown(interactKey))
        {
            if (hoveredButtonIndex < 0)
            {
                ResetInteraction();
                return;
            }

            Debug.Log(hoveredButtonIndex);

            Image hoveredButton = towerSelectionMenuButtons[hoveredButtonIndex];

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
        if (!TargetIsPlane)
        {
            ResetInteraction();
            return;
        }

        SpawnPylon();
    }

    private void SpawnPylon()
    {
        GameObject pylonInstance = Instantiate(pylonPrefab, currentHit.point, Quaternion.identity);

        if (CurrentInteraction == InteractionState.PlacingFromPylon)
            (targetBuilding as Pylon).AddBuilding(pylonInstance.GetComponent<Pylon>());

        ResetInteraction();
    }

    public void SpawnTower(int towerIndex)
    {
        GameObject towerInstance = Instantiate(towerPrefabs[towerIndex], currentHit.point, Quaternion.identity);

        if (previousInteraction == InteractionState.PlacingFromPylon)
            (targetBuilding as Pylon).AddBuilding(towerInstance.GetComponent<Tower>());

        ResetInteraction();
    }

    private void DisplayBuildingRadius(out GameObject radiusDisplay)
    {
        radiusDisplay = targetBuilding.radiusDisplay;
        if (!radiusDisplay.activeSelf)
            radiusDisplay.SetActive(true);
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
        interactionDuration = 0.0f;
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