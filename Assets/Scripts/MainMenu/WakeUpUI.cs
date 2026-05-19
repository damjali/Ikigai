using UnityEngine;

public class WakeUpUI : MonoBehaviour
{
    [Header("Heartbeat Settings")]
    public float beatSpeed = 3f;      // Kelajuan degupan
    public float scaleAmount = 0.15f; // Berapa besar dia mengembang

    private Vector3 baseScale;

    void Start()
    {
        // Simpan saiz asal butang masa game mula
        baseScale = transform.localScale;
    }

    void Update()
    {
        // Guna formula Sine Wave untuk buat efek berdegup yang smooth!
        float pulse = Mathf.Sin(Time.time * beatSpeed) * scaleAmount;
        transform.localScale = baseScale + new Vector3(pulse, pulse, 0f);
    }
}