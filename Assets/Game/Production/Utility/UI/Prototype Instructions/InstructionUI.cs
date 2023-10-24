using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Input = UnityEngine.Input;

[System.Serializable]
struct Page
{
    [Tooltip("(960,540) Images")] public Texture2D image;
    [TextArea] public string text;
}

[System.Serializable]
class PageIndex
{
    int currentIndex;
    [Tooltip("(50,50) Images")] public Texture2D indexTex;
    [Tooltip("(50,50) Images")] public Texture2D accessedIndexTex;
    [Tooltip("For Each new page just create another image, position it how you want, and add it to the array.")] public Image[] indexes;

    Sprite indexSprite;
    Sprite accessedIndexSprite;
    public void SetUp()
    {
        indexSprite = Sprite.Create(indexTex, new Rect(0f, 0f, indexTex.width, indexTex.height), new Vector2(0.5f, 0.5f), 100);
        accessedIndexSprite = Sprite.Create(accessedIndexTex, new Rect(0f, 0f, accessedIndexTex.width, accessedIndexTex.height), new Vector2(0.5f, 0.5f), 100);

        foreach(Image image in indexes)
        {
            image.sprite = indexSprite;
        }
    }
    public void ChangeIndexHighlight(int index)
    {
        indexes[currentIndex].sprite = indexSprite;
        indexes[index].sprite = accessedIndexSprite;
        currentIndex = index;
    }
}

public class InstructionUI : MonoBehaviour
{
    [SerializeField] Page[] instructionPages;

    [Space(10)]

    [SerializeField] PageIndex pageIndex;

    [Space(10)]

    [SerializeField] Button openInstButton;
    [SerializeField] Button pageRightButton;
    [SerializeField] Button pageLeftButton;

    [Space(10)]

    [SerializeField] GameObject instructionPanel;
    [SerializeField] Image instructionImage;
    [SerializeField] TMP_Text instructionText;

    int index = 0;

    private void Start()
    {
        pageIndex.SetUp();
        OpenInst();
    }

    private void Update()
    {
        if (index == 0)
            pageLeftButton.interactable = false;
        else
            pageLeftButton.interactable = true;


        if (index == instructionPages.Count()-1)
            pageRightButton.interactable = false;
        else
            pageRightButton.interactable = true;

        if (instructionPanel.activeSelf)
        {
            if (index > 0 && Input.GetKeyDown(KeyCode.LeftArrow))
                TurnPageLeft();
            if (index < instructionPages.Count() - 1 && Input.GetKeyDown(KeyCode.RightArrow))
                TurnPageRight();
            if (Input.GetKeyDown(KeyCode.Escape))
                CloseInst();
        }
    }


    public void OpenInst()
    {

        instructionPanel.SetActive(true);
        openInstButton.interactable = false;
        index = 0;

        Texture2D tex = instructionPages[index].image;
        instructionImage.sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);
        instructionText.text = instructionPages[index].text;

        pageIndex.ChangeIndexHighlight(index);
    }

    public void CloseInst()
    {
        instructionPanel.SetActive(false);
        openInstButton.interactable = true;
    }

    public void TurnPageLeft()
    {
        index--;

        Texture2D tex = instructionPages[index].image;
        Sprite imageSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);
        instructionImage.overrideSprite = imageSprite;
        instructionImage.rectTransform.rect.Set(imageSprite.rect.x, imageSprite.rect.y, imageSprite.rect.width, imageSprite.rect.height);
        instructionText.text = instructionPages[index].text;

        pageIndex.ChangeIndexHighlight(index);
    }

    public void TurnPageRight()
    {
        index++;
 

        Texture2D tex = instructionPages[index].image;
        Sprite imageSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);
        instructionImage.overrideSprite = imageSprite;
        instructionImage.rectTransform.rect.Set(imageSprite.rect.x, imageSprite.rect.y, imageSprite.rect.width, imageSprite.rect.height);
        instructionText.text = instructionPages[index].text;

        pageIndex.ChangeIndexHighlight(index);
    }
}
