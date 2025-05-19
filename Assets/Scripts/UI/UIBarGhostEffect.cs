using UnityEngine.UI;
using UnityEngine;

public class UIBarGhostEffect : MonoBehaviour
{
    public Slider MainSlider_;
    public Slider GhostSlider_;
    public float GhostSpeed_ = 1f;

    private float targetValue_;

    public void UpdateInstant_(float value)
    {
        targetValue_ = value;
        MainSlider_.value = targetValue_;
    }

    private void Update()
    {
        if (GhostSlider_.value > targetValue_)
        {
            GhostSlider_.value = Mathf.MoveTowards(GhostSlider_.value, targetValue_, GhostSpeed_ * Time.deltaTime);
        }
        else
        {
            GhostSlider_.value = targetValue_;
        }
    }
}
// © 2025 KOIYOT. All rights reserved.