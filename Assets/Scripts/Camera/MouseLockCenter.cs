using UnityEngine.InputSystem;
using UnityEngine;

public class MouseLockCenter : MonoBehaviour
{
    private bool IsMouseLocked_ = true;
    private Vector2 ScreenCenter_;
    private InputAction Action_;

    void Awake()
    {
        ScreenCenter_.x = Screen.width / 2f;
        ScreenCenter_.y = Screen.height / 2f;
        
        InputActionAsset inputActionAsset = GetComponent<PlayerInput>()?.actions;
        
        if (inputActionAsset != null)
        {
            Action_ = inputActionAsset.FindAction("UI/Cancel");

            if (Action_ != null)
            {
                Action_.Enable();
                Action_.performed += _ => ToggleMouseLock();
            }
        }
        
        LockAndCenterMouse();
    }

    void OnDestroy()
    {
        if (Action_ != null)
        {
            Action_.performed -= _ => ToggleMouseLock();
            Action_.Disable();
        }
    }

    void ToggleMouseLock()
    {
        IsMouseLocked_ = !IsMouseLocked_;

        if (IsMouseLocked_)
        {
            LockAndCenterMouse();
        }
        else
        {
            UnlockMouse();
        }
    }

    void LockAndCenterMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); 
    }

    void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Cursor.SetCursor(null, ScreenCenter_, CursorMode.Auto);
    }
}