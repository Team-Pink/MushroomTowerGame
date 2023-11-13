using UnityEngine;
using TMPro;

public class WaveCounter : MonoBehaviour
{

    [SerializeField] TMP_Text currentWave;
    [SerializeField] TMP_Text maxWaves;


    private void Start()
    {
        currentWave.text = "0";
        //maxWaves.text = waveManager.maxWaves; // not its actual name
    }

    public void IncWaveCounter() {

        //currentWave.text = waveManager.currentWave; // not its actual name
    
    }





    // All old stuff for the visual wave counter which has now been scrapped - James
    /*
    private class FallingBit
    {
        private GameObject bit;
        private bool animateBit;

        private Vector3 startPos;
        private Vector3 fallSpeed;
        private Vector3 fallRotation;


        public FallingBit(GameObject Bit)
        {
            bit = Bit;
            animateBit = false;
            startPos = bit.transform.position;
            fallSpeed = Vector3.zero;
            fallRotation = Vector3.zero;
        }

        public void SetFall()
        {
            animateBit = true;
            bit.SetActive(animateBit);
            fallSpeed.y = -(UnityEngine.Random.value + 0.4f); // 
            fallRotation.z = UnityEngine.Random.Range(-10, 10);
        }

        public void UpdateBitMotion()
        {
            if (!animateBit) return;
            bit.transform.position += fallSpeed;
            bit.transform.Rotate(fallRotation);
            if (bit.transform.localPosition.y < -75)
            {
                animateBit = false;
                bit.transform.position = startPos;
                bit.SetActive(false);
            }
        }
    }

    [SerializeField] private Image counterBits;
    Transform bitParent;
    FallingBit[] bits;

    // Start is called before the first frame update
    void Start()
    {
        counterBits = GetComponent<Image>();
        counterBits.fillAmount = 0;

        bitParent = transform.parent;
        bits = new FallingBit[3];
        for (int i = 0; i < 3; i++)
        {
            bits[i] = new FallingBit(bitParent.GetChild(i).gameObject);
        }

        //counterBits.transform.position = transform.position -= new Vector3(0, counterBits.rectTransform.rect.height, 0);

    }

    public void SetWaveCounterFill(float fill = 0)
    {
        // move image up from original position based on image height / fill
        //counterBits.transform.position = transform.position += new Vector3(0, fill * counterBits.rectTransform.rect.height, 0);

        counterBits.fillAmount = fill; // fill image to match movement
    }

    private void Update()
    {
        for (int i = 0; i < 3; i++)
        {
            bits[i].UpdateBitMotion();
        }
    }

    public void AnimateBitsFalling()
    {
        for (int i = 0; i < 3; i++)
        {
            bits[i].SetFall();
        }
    }

    */
}
