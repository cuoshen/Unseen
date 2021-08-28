using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TriggerPause : MonoBehaviour
{

    [SerializeField]
    public GameObject PauseUI;
    public bool isPauseUIEnabled;

    // Start is called before the first frame update
    void Start()
    {
        isPauseUIEnabled = false;
        PauseUI.SetActive(isPauseUIEnabled);
    }

    public void togglePauseUI(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isPauseUIEnabled = !isPauseUIEnabled;
            PauseUI.SetActive(isPauseUIEnabled);
        }
    }

    public void togglePauseUIButton()
    {
        isPauseUIEnabled = !isPauseUIEnabled;
        PauseUI.SetActive(isPauseUIEnabled);
    }
}
