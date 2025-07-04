using DG.Tweening;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EffectManager : MonoBehaviour
{
    [SerializeField] private ScriptableRendererFeature Hurt;
    [SerializeField] private ScriptableRendererFeature LowHealth;
    [SerializeField] private Material hurtMaterial;
    [SerializeField] private Light2D globalLight;

    private int vignetteIntensityProperty = Shader.PropertyToID("_VignetteIntensity");
    private int voronoiPowerProperty = Shader.PropertyToID("_VoronoiPower");
    private int voronoiIntensityProperty = Shader.PropertyToID("_VoronoiIntensity");

    private const float StartingVignetteIntensity = 1f;
    private const float StartingVoronoiIntensity = .495f;

    public static EffectManager Instance { get; private set; }
    Coroutine hurt;
    private void Awake()
    {
        if(Instance == null) Instance = this;
        Hurt.SetActive(false);
        DisplayLowHealth(false);
    }
    private void OnDestroy()
    {
        hurtMaterial.SetFloat(vignetteIntensityProperty, StartingVignetteIntensity);
        hurtMaterial.SetFloat(voronoiIntensityProperty, StartingVoronoiIntensity);

        Hurt.SetActive(false);
        LowHealth.SetActive(false);
    }
    private void OnApplicationQuit()
    {
        hurtMaterial.SetFloat(vignetteIntensityProperty, StartingVignetteIntensity);
        hurtMaterial.SetFloat(voronoiIntensityProperty, StartingVoronoiIntensity);
    }
    void Start()
    {
        Hurt.SetActive(true);
        hurtMaterial.SetFloat(vignetteIntensityProperty, 0);
    }
    public async void FreezeHit(float duration = 0.2f)
    {
        duration = Mathf.Clamp(duration,0.05f, 0.25f);
        Time.timeScale = 0;
        int duratiomMS = (int)(duration * 1000);
        await Task.Delay(duratiomMS);
        Time.timeScale = 1;
    }
    public void DisplayLowHealth(bool value) => LowHealth.SetActive(value);
    public void DisplayHurt()
    {
        if(hurt != null)StopCoroutine(hurt);
        hurt = StartCoroutine(VisualHurt());
    }
    public void DisplayDead()
    {
        DisplayLowHealth(false);
        StartCoroutine(VisualDead());
    }
    private IEnumerator VisualHurt()
    {
        float vignneteValue = 0;
        DOTween.To(() => vignneteValue, x => vignneteValue = x, StartingVignetteIntensity, 0.05f).OnUpdate(() =>
        {
            hurtMaterial.SetFloat(vignetteIntensityProperty, vignneteValue);
        });
        yield return new WaitForSeconds(1.22f);
        DOTween.To(() => vignneteValue, x => vignneteValue = x, 0, 0.83f).OnUpdate(() =>
        {
            hurtMaterial.SetFloat(vignetteIntensityProperty, vignneteValue);
        });
    }
    private IEnumerator VisualDead()
    {
        float vignneteValue = 0;
        DOTween.To(() => vignneteValue, x => vignneteValue = x, vignetteIntensityProperty, 0.12f).OnUpdate(() =>
        {
            hurtMaterial.SetFloat(vignetteIntensityProperty, vignneteValue);
        });
        yield return new WaitForSeconds(0.32f);
        DOTween.To(() => vignneteValue, x => vignneteValue = x, 0, 0.22f).OnUpdate(() =>
        {
            hurtMaterial.SetFloat(vignetteIntensityProperty, vignneteValue);
        });
    }
    public IEnumerator DisplayLoseEffect()
    {
        Color currentColor = Color.white;
        float currentLight = 1;
        yield return null;
        DOTween.To(() => currentColor, x => currentColor = x, Color.red, 9f).OnUpdate(() =>
        {
            globalLight.color = currentColor;
        });
        DOTween.To(() => currentLight, x => currentLight= x, 0.1f, 10f).OnUpdate(() =>
        {
            globalLight.intensity = currentLight;
        });
    }
}
