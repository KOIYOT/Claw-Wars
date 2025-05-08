using UnityEngine;

public class ExclamationAnimation : MonoBehaviour
{
    public float FloatingAmplitude_ = 0.05f;
    public float FloatingSpeed_ = 1.5f;
    public float RotationSpeed_ = 90f;

    private Vector3 InitialPosition_;
    private float FloatingTime_;

    void Start()
    {
        InitialPosition_ = transform.position;
        FloatingTime_ = Time.time;
    }

    void Update()
    {
        float VerticalMovement_ = FloatingAmplitude_ * Mathf.Sin((Time.time - FloatingTime_) * FloatingSpeed_);
        Vector3 NewPos_ = InitialPosition_ + Vector3.up * VerticalMovement_;

        transform.position = Vector3.Lerp(transform.position, NewPos_, Time.deltaTime * 5f);
        transform.Rotate(Vector3.up * RotationSpeed_ * Time.deltaTime);
    }
}