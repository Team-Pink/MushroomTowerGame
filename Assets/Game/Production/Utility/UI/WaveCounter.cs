using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveCounter : MonoBehaviour
{
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
            fallSpeed.y = -(UnityEngine.Random.value + 1); // 
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

        bitParent = transform.GetChild(0);
        bits = new FallingBit[3];
        for (int i = 0; i < 3; i++)
        {
            bits[i] = new FallingBit(bitParent.GetChild(i).gameObject);
        }

    }

    public void SetWaveCounterFill(float fill = 0)
    {
        counterBits.fillAmount = fill;
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
}
