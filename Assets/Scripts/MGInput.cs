using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// This script is best attached to an object that persists between scenes or is in its own scene that is always loaded
public class MGInput : MonoBehaviour
{
    public static InputMaster controls;
    public string[] actions; // As it is now every action name needs to be registered here manually
    public static List<InputAction> inputActions;
    bool created = false;

    void Awake()
    {
        DontDestroyOnLoad (gameObject);
        if(controls == null) controls = new InputMaster();
        created = true;
    }

    public void OnEnable()
    {
        controls.Enable();

        print("Controls enabled");
    }
    void OnDisable()
    {
        if(controls!=null&&created)
        {
            controls.Disable();
            print("Controls disabled");
        }
    }
    public static bool GetButtonDown(InputAction action)
    {
        return action.WasPressedThisFrame();
    }
    public static bool GetButtonUp(InputAction action)
    {
        return action.WasReleasedThisFrame();
    }
    public static bool GetButton(InputAction action)
    {
        return action.ReadValue<float>() > 0 ? true:false;
    }
    public static Vector2 GetDpad(InputAction action)
    {
        return action.ReadValue<Vector2>();
    }
    public static float GetDpadX(InputAction action)
    {
        return action.ReadValue<Vector2>().x;
    }
    public static float GetDpadY(InputAction action)
    {
        return action.ReadValue<Vector2>().y;
    }
    public static int GetDpadXRaw(InputAction action)
    {
        return (int)Mathf.Clamp(action.ReadValue<Vector2>().x*2,-1,1);
    }
    public static int GetDpadYRaw(InputAction action)
    {
        return (int)Mathf.Clamp(action.ReadValue<Vector2>().y*2,-1,1);
    }
    public static bool GetBKeyDown(ButtonControl key)
    {
        return key.wasPressedThisFrame && Application.isFocused;
    }
    public static bool GetBKey(ButtonControl key)
    {
        return key.isPressed && Application.isFocused;
    }
    public static bool GetBKeyUp(ButtonControl key)
    {
        return key.wasReleasedThisFrame && Application.isFocused;
    }
    public static bool GetKeyDown(KeyControl key)
    {
        return key.wasPressedThisFrame && Application.isFocused;
    }
    public static bool AnyKeyDown()
    {
        return Application.isFocused && (Keyboard.current.anyKey.wasPressedThisFrame || MGInput.controls.Player.Any.WasPressedThisFrame());
    }
    public static bool AnyKey()
    {
        return Application.isFocused && (Keyboard.current.anyKey.isPressed || MGInput.controls.Player.Any.IsPressed());
    }
    public static bool GetKey(KeyControl key)
    {
        return key.isPressed && Application.isFocused;
    }
    public static bool GetKeyUp(KeyControl key)
    {
        return key.wasReleasedThisFrame && Application.isFocused;
    }
    // Start is called before the first frame update
    void Start()
    {
        inputActions = new List<InputAction>();
    }
    // Input handling (When looking for input presses in other places whether you know time would be frozen or not use fixedupdate for unfrozen time and update for frozen time)
    void FixedUpdate()
    {
        // Set input updates in the project settings to script based
        InputSystem.Update();
    }
    void Update()
    {
        if(Time.timeScale==0) InputSystem.Update();
    }

    // Example uses: MGInput.GetDpadYRaw(MGInput.controls.Player.Movement);
    // MGInput.GetButton(MGInput.controls.Player.Jump)
    // MGInput.GetButtonDown(MGInput.controls.Player.Jump)
}
