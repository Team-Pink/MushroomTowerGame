using GameObjectList = System.Collections.Generic.List<UnityEngine.GameObject>;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum InteractionState
{
    None,
    DraggingBud,
    PositionSelected
}

public class TowerCreation : MonoBehaviour
{
    private Camera mainCamera;

    [Header("Objects")]
    [SerializeField] GameObject targetPlane;
    [SerializeField, NonReorderable] private GameObjectList towerPrefabs = new(4);
    private const int towerPrefabAmount = 4;

    [Header("Placement")]
    [SerializeField] LayerMask layersToCheck;
    [SerializeField] float placementExclusionSize = 5;
    private const float capsuleCheckBound = 5;

    [Header("UI")]
    [SerializeField] GameObject radialMenu;
    [SerializeField] Image selectionIndicator;

    [Header("Debug")]
    [SerializeField] bool showMouseDirection;
    [SerializeField] bool showCameraProjection;
    private float screenWidth;
    private float screenHeight;
    [SerializeField] bool logCurrentInteraction;
    [SerializeField] KeyCode interactKey = KeyCode.Mouse0;

    private Vector3 mouseScreenPosition;
    private Vector3 mouseWorldPosition;

    private InteractionState currentInteraction = InteractionState.None;
    private RaycastHit currentHit;

    private void Awake()
    {
        mainCamera = Camera.main;
        radialMenu.SetActive(false);
        selectionIndicator.enabled = false;
    }

    private void OnValidate()
    {
        if (towerPrefabs.Count == towerPrefabAmount)
            return;

        Debug.LogWarning("Stop that, the list tpwer prefabs should be exactly " + towerPrefabAmount + " elements!", this);

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
            case InteractionState.DraggingBud:
                DraggingBudState();
                break;
            case InteractionState.PositionSelected:
                PositionSelectedState();
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
                currentHit.collider.gameObject.SetActive(false);
                selectionIndicator.enabled = true;

                currentInteraction = InteractionState.DraggingBud;
            }
        }
    }


    private void DraggingBudState()
    {
        if (Input.GetKeyUp(interactKey))
        {
            currentHit = GetRayHit();

            if (currentHit.collider is not null)
                currentInteraction = InteractionState.PositionSelected;
            else
                currentInteraction = InteractionState.None;
        }

        selectionIndicator.rectTransform.position = mouseScreenPosition;
    }

    private void PositionSelectedState()
    {
        if (!TargetIsPlane())
        {
            currentInteraction = InteractionState.None;
            return;
        }

        if (SpaceForTower())
        {
            if (radialMenu.activeSelf == false)
            {
                radialMenu.SetActive(true);
            }
        }
        else currentInteraction = InteractionState.None;
    }



    private RaycastHit GetRayHit()
    {
        Physics.Raycast(mainCamera.transform.position, mouseWorldPosition - mainCamera.transform.position, out RaycastHit hit, Mathf.Infinity);
        return hit;
    }

    private bool TargetIsPlane()
    {
        return currentHit.collider.gameObject == targetPlane;
    }

    private bool SpaceForTower()
    {
        Vector3 capsuleTop = new(currentHit.point.x, currentHit.point.y + capsuleCheckBound, currentHit.point.z);
        Vector3 capsuleBottom = new(currentHit.point.x, currentHit.point.y - capsuleCheckBound, currentHit.point.z);

        var targets = Physics.CapsuleCastAll(capsuleTop, capsuleBottom, placementExclusionSize, Vector3.up, Mathf.Infinity, layersToCheck).ToList();
        
        //Debug.Log(targets.Count);
        return targets.Count == 0; 
    }

    public void SpawnTower(int towerIndex)
    {
        Instantiate(towerPrefabs[towerIndex], currentHit.point, Quaternion.identity);

        radialMenu.SetActive(false);
        selectionIndicator.enabled = false;

        currentInteraction = InteractionState.None;
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

            for(int i = 0; i < projectionCorners.Length; i++)
            {
                if (projectionCorners[i].collider is null)
                    continue;

                if (i == projectionCorners.Length - 1)
                {
                    if (projectionCorners[0].collider is null)
                        continue;

                    Debug.DrawLine(projectionCorners[i].point, projectionCorners[0].point, Color.white);
                }
                else
                {
                    if (projectionCorners[i+1].collider is null)
                        continue;

                    Debug.DrawLine(projectionCorners[i].point, projectionCorners[i+1].point, Color.white);
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