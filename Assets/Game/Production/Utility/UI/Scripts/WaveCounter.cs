using UnityEngine;
using TMPro;

public class WaveCounter : MonoBehaviour
{
    [SerializeField] WaveSpawner waveSpawner;

    [SerializeField] TMP_Text currentWave;
    [SerializeField] TMP_Text maxWaves;


    private void Start()
    {
        currentWave.text = "0";
        maxWaves.text = waveSpawner.Waves.Length.ToString();
    }

    public void IncWaveCounter() {

        currentWave.text = (waveSpawner.CurrentWave + 1).ToString();
    }

}
