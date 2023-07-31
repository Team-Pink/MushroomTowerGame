using GameObjectList = System.Collections.Generic.List<UnityEngine.GameObject>;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum InteractionState
{
    None,
    DraggingFromPylon,
    PlacingTower,
    DraggingFromHub,
    PlacingPylon
}

public class TowerCreation : MonoBehaviour
{
    private Camera mainCamera;

    [Header("Objects")]
    [SerializeField] GameObject targetPlane;
    [SerializeField] GameObject pylonPrefab;
    [SerializeField, NonReorderable] private GameObjectList towerPrefabs = new(4);
    private const int towerPrefabAmount = 4;

    [Header("Placement")]
    [SerializeField] LayerMask placementBlockers;
    private LayerMask pylonLayer;
    private LayerMask placableLayers;
    private bool placedFromPylon;
    [SerializeField] float placementExclusionSize = 1;
    [SerializeField] float maxDistanceFromPylon = 10;
    private const float capsuleCheckBound = 5;
    private Vector3 dragStartPosition;

    [Header("UI")]
    [SerializeField] GameObject radialMenu;
    [SerializeField] Image selectionIndicator;

    [Header("Debug")]
    [SerializeField] bool showMouseDirection;
    [SerializeField] bool showCameraProjection;
    private float screenWidth;
    private float screenHeight;
    [SerializeField] bool logCurrentInteraction;

    [SerializeField, Space()] KeyCode interactKey = KeyCode.Mouse0;

    private Vector3 mouseScreenPosition;
    private Vector3 mouseWorldPosition;

    private GameObject activeBud;
    private InteractionState currentInteraction = InteractionState.None;
    private RaycastHit currentHit;

    private void Awake()
    {
        mainCamera = Camera.main;
        radialMenu.SetActive(false);
        selectionIndicator.enabled = false;

        placableLayers = LayerMask.GetMask("Ground");
        pylonLayer = LayerMask.GetMask("Pylon");
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

        switch(currentInteraction)
        {
            case InteractionState.None:
                DefaultState();
                break;
            case InteractionState.DraggingFromHub:
                DraggingFromHubState();
                break;
            case InteractionState.PlacingPylon:
                PlacingPylonState();
                break;
            case InteractionState.DraggingFromPylon:
                DraggingFromPylonState();
                break;
            case InteractionState.PlacingTower:
                PlacingTowerState();
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
            currentHit = GetRayHit();

            if (currentHit.collider is null)
                return;

            if (currentHit.collider.CompareTag("Bud"))
            {
                activeBud = currentHit.collider.gameObject;
                activeBud.SetActive(false);
                selectionIndicator.enabled = true;

                currentInteraction = InteractionState.DraggingFromPylon;
            }
            else if (currentHit.collider.CompareTag("HubBud"))
            {
                activeBud = currentHit.collider.gameObject;
                activeBud.SetActive(false);
                selectionIndicator.enabled = true;

                currentInteraction = InteractionState.DraggingFromHub;
            }
        }

        if (dragStartPosition != Vector3.zero)
            dragStartPosition = Vector3.zero;
    }

    private void DraggingFromHubState()
    {
        bool canPlace;

        currentHit = GetRayHit(placableLayers);

        if (dragStartPosition == Vector3.zero)
            dragStartPosition = new Vector3(activeBud.transform.position.x, 0, activeBud.transform.position.z);

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

                currentInteraction = InteractionState.PlacingPylon;
            }
            else
            {
                activeBud.SetActive(true);
                activeBud = null;
                selectionIndicator.color = Color.white;
                selectionIndicator.enabled = false;
                currentInteraction = InteractionState.None;
            }
        }
    }

    private void PlacingPylonState()
    {
        if (!TargetIsPlane())
        {
            currentInteraction = InteractionState.None;
            return;
        }

        SpawnPylon();
    }

    private void DraggingFromPylonState()
    {
        bool canPlace;
        bool placingPylon = false;

        currentHit = GetRayHit(placableLayers);

        if (dragStartPosition == Vector3.zero)
            dragStartPosition = new Vector3(activeBud.transform.position.x, 0, activeBud.transform.position.z);

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

                placedFromPylon = true;
                if (placingPylon)
                    currentInteraction = InteractionState.PlacingPylon;
                else
                    currentInteraction = InteractionState.PlacingTower;
            }
            else
            {
                activeBud.SetActive(true);
                activeBud = null;
                selectionIndicator.color = Color.white;
                selectionIndicator.enabled = false;
                currentInteraction = InteractionState.None;
            }
        }
    }

    private void PlacingTowerState()
    {
        if (!TargetIsPlane())
        {
            currentInteraction = InteractionState.None;
            return;
        }

        if (radialMenu.activeSelf == false)
            radialMenu.SetActive(true);
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

    private bool TargetIsPlane()
    {
        return currentHit.collider.gameObject == targetPlane;
    }

    private bool SpaceToPlace(float detectionArea, LayerMask layerMask)
    {
        Vector3 capsuleTop = new(currentHit.point.x, currentHit.point.y + capsuleCheckBound, currentHit.point.z);
        Vector3 capsuleBottom = new(currentHit.point.x, currentHit.point.y - capsuleCheckBound, currentHit.point.z);

        var targets = Physics.OverlapCapsule(capsuleTop, capsuleBottom, detectionArea, layerMask).ToList();

        return targets.Count == 0;
    }

    private void SpawnPylon()
    {
        GameObject pylonInstance = Instantiate(pylonPrefab, currentHit.point, Quaternion.identity);

        if (placedFromPylon)
        {
            activeBud.transform.parent.GetComponent<Pylon>().AddBuilding(pylonInstance.GetComponent<Pylon>());
        }

        selectionIndicator.enabled = false;
        selectionIndicator.rectTransform.sizeDelta = new Vector2(25, 25);

        currentInteraction = InteractionState.None;
        activeBud.SetActive(true);
        activeBud = null;
    }

    public void SpawnTower(int towerIndex)
    {
        GameObject towerInstance = Instantiate(towerPrefabs[towerIndex], currentHit.point, Quaternion.identity);

        if (placedFromPylon)
        {
            activeBud.transform.parent.GetComponent<Pylon>().AddBuilding(towerInstance.GetComponent<Tower>());
        }

        radialMenu.SetActive(false);
        selectionIndicator.enabled = false;
        selectionIndicator.rectTransform.sizeDelta = new Vector2(25, 25);

        currentInteraction = InteractionState.None;
        activeBud.SetActive(true);
        activeBud = null;
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

        if (logCurrentInteraction)
            Debug.Log(currentInteraction);
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