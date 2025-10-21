using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingTextSpawner : MonoBehaviour
{
    public static FloatingTextSpawner I;

    [Header("References")]
    public TextMeshProUGUI prefab;   // assign FloatingText prefab here
    public Canvas canvas;            // assign your HUD Canvas

    [Header("Tuning")]
    public float risePixels = 40f;   // how far it floats up
    public float lifeSeconds = 1f;   // how long it lasts

    void Awake()
    {
        I = this;
        if (!canvas) canvas = GetComponentInParent<Canvas>();
    }

    /// <summary>Spawn a floating label at a world position.</summary>
    public void Show(string msg, Vector3 worldPos, float life = -1f)
    {
        if (life <= 0f) life = lifeSeconds;
        if (!prefab || !canvas || !Camera.main) return;

        var ui = Instantiate(prefab, canvas.transform);
        ui.text = msg;
        ui.alpha = 1f;

        // Place over the world position
        ui.transform.position = Camera.main.WorldToScreenPoint(worldPos);

        StartCoroutine(FadeAndRise(ui, life));
    }

    private IEnumerator FadeAndRise(TextMeshProUGUI t, float life)
    {
        Vector3 start = t.transform.position;
        float e = 0f;

        while (e < life)
        {
            e += Time.unscaledDeltaTime;          // ignore Time.timeScale
            float k = e / life;
            t.alpha = 1f - k;                     // fade out
            t.transform.position = start + new Vector3(0f, risePixels * k, 0f); // rise
            yield return null;
        }

        if (t) Destroy(t.gameObject);
    }
}
