using UnityEngine;

[ExecuteAlways]
public class WaveLighting : MonoBehaviour
{
    public Light sunLight;        
    public int currentWave = 1;   
    public int totalWaves = 5;

    [Header("Lighting")]
    public Gradient lightColor;          
    public AnimationCurve lightIntensity; 

    void Update()
    {
        float t = Mathf.InverseLerp(1, totalWaves, currentWave);
        sunLight.color = lightColor.Evaluate(t);
        sunLight.intensity = lightIntensity.Evaluate(t);
    }

    public void SetWave(int wave)
    {
        currentWave = wave;
    }
}
