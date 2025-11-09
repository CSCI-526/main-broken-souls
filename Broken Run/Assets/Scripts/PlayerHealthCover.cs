using UnityEngine;

public class PlayerHealthCover : MonoBehaviour
{
    [Header("Cover Setup")]
    public Transform cover;                 
    public float fullCoverScaleY = 1f;      

    private float maxHealth = 100f;
    private float currentHealth;
    private float _targetScaleY;

    void Start()
    {
        currentHealth = maxHealth;

        if (fullCoverScaleY <= 0f)
        {
            fullCoverScaleY = (cover != null && cover.localScale.y > 0f) ? cover.localScale.y : 1f;
        }

        if (cover != null)
        {
            var s = cover.localScale;
            s.y = 0f;
            cover.localScale = s;
        }
    }

    void Update()
    {

    }


    public void SetHealth(float value)
    {
        if (cover == null) return;
        currentHealth = value;


        float pct = currentHealth / maxHealth;   
        float coverPct = 1f - pct;                  
        _targetScaleY = fullCoverScaleY * coverPct;

        Vector3 s = cover.localScale;
        s.y = _targetScaleY;
        cover.localScale = s;
    }
}

