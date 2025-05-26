using UnityEngine.InputSystem;
using UnityEngine;

public class MouseLockCenter : MonoBehaviour
{
    public static MouseLockCenter Instance;

    private bool IsMouseLocked_ = true;
    private Vector2 ScreenCenter_;
    private InputAction Action_;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        ScreenCenter_ = new Vector2(Screen.width / 2f, Screen.height / 2f);

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

        LockMouse();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (Action_ != null)
        {
            Action_.performed -= _ => ToggleMouseLock();
            Action_.Disable();
        }
    }

    public void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        IsMouseLocked_ = true;
    }

    public void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        IsMouseLocked_ = false;
    }

    private void ToggleMouseLock()
    {
        if (IsMouseLocked_)
            UnlockMouse();
        else
            LockMouse();
    }
}