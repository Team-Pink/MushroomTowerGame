using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public enum Tutorial { None, Placement, Warning, Regrow, Selling }
    
    [HideInInspector] public Tutorial currentTutorial = Tutorial.None;
    [HideInInspector] public int currentPart;

    [SerializeField] private float gracePeriod;
    private float elapsedGracePeriod;

    [Header("Boomy")]
    [SerializeField] Animator boomy;


    [Header("Placement")]
    public GameObject[] placementParts;
    [SerializeField] GameObject[] placementSpotlights;
    [HideInInspector] public bool placementHasPlayed;

    [Header("Warning")]
    public int warningWave = 2;
    public float warningWaitTime = 3.0f;
    [HideInInspector] public float elapsedWarningWaitTime = 0.0f;
    [SerializeField] GameObject[] warningParts;
    [HideInInspector] public bool warningHasPlayed;

    [Header("Regrow")]
    public GameObject[] regrowParts;
    [SerializeField] GameObject[] regrowSpotlights;
    [HideInInspector] public bool regrowHasPlayed;

    [Header("Selling")]
    public int sellingWave = 3;
    public GameObject[] sellingParts;
    [SerializeField] GameObject[] sellingSpotlights;
    [HideInInspector] public bool sellingHasPlayed;


    private CanvasScaler canvasScaler;

    // Components
    InteractionManager interactionManager;

    private void Awake()
    {
        interactionManager = GetComponent<InteractionManager>();

        canvasScaler = GameObject.Find("Canvas").GetComponent<CanvasScaler>();
    }

    private void Update()
    {
        switch (currentTutorial)
        {
            case Tutorial.None:
                break;
            case Tutorial.Placement:
                PlacementTutorial();
                break;
            case Tutorial.Warning:
                WarningTutorial();
                break;
            case Tutorial.Regrow:
                RegrowTutorial();
                break;
            case Tutorial.Selling:
                SellingTutorial();
                break;
        }
    }

    private void PlacementTutorial()
    {
        switch (currentPart)
        {
            case 0:
                if (interactionManager.currentInteraction == InteractionState.PlacingFromHub)
                    AdvanceTutorial(ref placementParts);
                break;
            case 1:
                // Waits for Advancement from Interaction Manager
                break;
            case 2:
                if (interactionManager.currentInteraction == InteractionState.PlacingFromPylon)
                    AdvanceTutorial(ref placementParts);
                break;
            case 3:
                if (interactionManager.currentInteraction == InteractionState.TowerSelection)
                    AdvanceTutorial(ref placementParts);
                break;
            case 4:
                // Waits for Advancement from Interaction Manager
                break;
            case 5:
                if (elapsedGracePeriod >= gracePeriod)
                {
                    if (Input.GetKeyDown(interactionManager.interactKey))
                    {
                        AdvanceTutorial(ref placementParts);
                    }
                }
                else
                {
                    elapsedGracePeriod += Time.deltaTime;
                }
                break;
            default: break;
        }
    }

    private void WarningTutorial()
    {
        if (elapsedGracePeriod >= gracePeriod)
        {
            if (Input.GetKeyDown(interactionManager.interactKey))
            {
                AdvanceTutorial(ref warningParts);
            }
        }
        else
        {
            elapsedGracePeriod += Time.deltaTime;
        }
    }

    private void RegrowTutorial()
    {
        switch (currentPart)
        {
            case 0:
                if (interactionManager.currentInteraction == InteractionState.ResidualMenu)
                    AdvanceTutorial(ref placementParts);
                break;
            case 1:
                // Waits for Advancement from Interaction Manager
                break;
            case 2:
                if (elapsedGracePeriod >= gracePeriod)
                {
                    if (Input.GetKeyDown(interactionManager.interactKey))
                    {
                        AdvanceTutorial(ref placementParts);
                    }
                }
                else
                {
                    elapsedGracePeriod += Time.deltaTime;
                }
                break;
            default: break;
        }
    }

    private void SellingTutorial()
    {
        switch (currentPart)
        {
            case 0:
                if (interactionManager.currentInteraction == InteractionState.TowerMenu)
                    AdvanceTutorial(ref placementParts);
                break;
            case 1:
                // Waits for Advancement from Interaction Manager
                break;
            case 2:
                if (elapsedGracePeriod >= gracePeriod)
                {
                    if (Input.GetKeyDown(interactionManager.interactKey))
                    {
                        AdvanceTutorial(ref placementParts);
                    }
                }
                else
                {
                    elapsedGracePeriod += Time.deltaTime;
                }
                break;
            default: break;
        }
    }

    public bool CorrectTutorialPlacement(Vector2 mousePosition)
    {

        RectTransform spotlight = currentTutorial switch
        {
            Tutorial.Placement => currentPart < placementSpotlights.Length ? placementSpotlights[currentPart].GetComponent<RectTransform>() : null,
            Tutorial.Regrow => regrowSpotlights[currentPart].GetComponent<RectTransform>(),
            Tutorial.Selling => sellingSpotlights[currentPart].GetComponent<RectTransform>(),
            _ => null,
        };

        if (spotlight == null) return true;

        return (new Vector2(Screen.width / 2, Screen.height / 2) +
            (spotlight.anchoredPosition * Screen.height / canvasScaler.referenceResolution.y) - mousePosition).magnitude < 20 * spotlight.localScale.x;
    }

    public void StartTutorial(Tutorial tutorial)
    {
        currentTutorial = tutorial;
        InteractionManager.tutorialMode = true;
        InteractionManager.gamePaused = true;

        boomy.SetTrigger("Entry");

        switch (tutorial)
        {
            case Tutorial.None:
                Debug.LogError("Tutorial.None is not a valid tutorial.");
                break;
            case Tutorial.Placement:
                placementParts[0].SetActive(true);
                placementHasPlayed = true;
                break;
            case Tutorial.Warning:
                warningParts[0].SetActive(true);
                warningHasPlayed = true;
                break;
            case Tutorial.Regrow:
                regrowParts[0].SetActive(true);
                regrowHasPlayed = true;
                break;
            case Tutorial.Selling:
                sellingParts[0].SetActive(true);
                sellingHasPlayed = true;
                break;
        }
    }

    public void AdvanceTutorial(ref GameObject[] parts)
    {
        elapsedGracePeriod = 0;

        parts[currentPart].SetActive(false);

        if (currentPart + 1 == parts.Length)
        {
            EndTutorial();
            return;
        }

        boomy.SetTrigger("Jiggle");

        currentPart++;
        parts[currentPart].SetActive(true);
    }

    public void ReverseTutorial(ref GameObject[] parts)
    {
        elapsedGracePeriod = 0;

        parts[currentPart].SetActive(false);

        if (currentPart == 0)
        {
            Debug.LogError("Should not reverse tutorial when on first page.");
            return;
        }

        currentPart--;
        parts[currentPart].SetActive(true);
    }

    public void EndTutorial()
    {
        switch (currentTutorial)
        {
            case Tutorial.None:
                Debug.LogWarning("EndTutorial() is unnecessary when no tutorial is active.");
                break;
            case Tutorial.Placement: case Tutorial.Regrow: case Tutorial.Selling:
                InteractionManager.tutorialMode = false;
                break;
            default: break;
        }

        boomy.SetTrigger("Exit");

        currentPart = 0;
        currentTutorial = Tutorial.None;

        InteractionManager.tutorialMode = false;
        InteractionManager.gamePaused = false;
    }
}