using GameObjectList = System.Collections.Generic.List<UnityEngine.GameObject>;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum InteractionState
{
    None,
    BuildingInteraction,

    PlacingFromHub,

    PylonMenu,
    PlacingFromPylon,

    TowerSelection,
    TowerMenu
}

public class InteractionManager : MonoBehaviour
{
    [Header("Objects")]
        [SerializeField] GameObject targetPlane;
        [SerializeField] GameObject pylonPrefab;
        [SerializeField, NonReorderable] private GameObjectList towerPrefabs = new(4);
        private Camera mainCamera;
        private const int towerPrefabAmount = 4;

    [Header("Building Selection")]
        [SerializeField] LayerMask buildings;
        private Building targetBuilding;
        private float interactionDuration = 0.0f;

    [Header("Placement")]
        [SerializeField] LayerMask placementBlockers;
        private LayerMask pylonLayer;
        private LayerMask placableLayers;
        private LayerMask budLayer;

        private GameObject activeBud;
        private Vector3 dragStartPosition;

        [SerializeField] float placementExclusionSize = 1;
        [SerializeField] float maxDistanceFromPylon = 10;
        private const float capsuleCheckBound = 5;

    [Header("UI")]
        [SerializeField] GameObject radialMenu;
        [SerializeField] Image selectionIndicator;

    [Header("Interaction")]
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

    [Header("Debug")]
    [SerializeField] bool showMouseDirection;
    [SerializeField] bool showCameraProjection;
    private float screenWidth;
    private float screenHeight;
    [SerializeField] bool logInteractionChange;

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

        Debug.LogWarning("Stop that, the list tower prefabs should be exactly " + towerPrefabAmount + " elements!", this);

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

            case InteractionState.PlacingFromHub:
                PlacingFromHubState();
                break;

            case InteractionState.PylonMenu:
                PylonMenuState();
                break;
            case InteractionState.PlacingFromPylon:
                PlacingFromPylonState();
                break;

            case InteractionState.TowerSelection:
                TowerSelectionState();
                break;
            case InteractionState.TowerMenu:
                TowerMenuState();
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

            currentHit = GetRayHit(buildings);
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
        Debug.Log(interactionDuration);

        if (Input.GetKeyDown(interactKey))
        {
            ResetInteraction(new GameObject[] { radiusDisplay });
            return;
        }

        if (Input.GetKey(interactKey))
        {
            if (interactionDuration > 0.5f)
            {
                if (targetBuilding is Pylon)
                {
                    CurrentInteraction = InteractionState.PylonMenu;
                    return;
                }
                else if (targetBuilding is Tower)
                {
                    CurrentInteraction = InteractionState.TowerMenu;
                    return;
                }
            }
            else
                interactionDuration += Time.deltaTime;
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

    private void PylonMenuState()
    {

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

        if (radialMenu.activeSelf == false)
            radialMenu.SetActive(true);
    }
    private void TowerMenuState()
    {

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

    public void ResetInteraction(GameObject[] extraObjects = null)
    {
        radialMenu.SetActive(false);
        selectionIndicator.enabled = false;
        selectionIndicator.rectTransform.sizeDelta = new Vector2(25, 25);
        interactionDuration = 0.0f;
        CurrentInteraction = InteractionState.None;

        if (activeBud is not null)
        {
            activeBud.SetActive(true);
            activeBud = null;
        }

        if (targetBuilding is not null)
            targetBuilding = null;

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