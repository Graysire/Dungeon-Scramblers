using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.InputSystem.Layouts;

public class OnScreenStickModifications : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] protected int joystickNumber;
    [SerializeField] protected List<GameObject> AllOtherIndependentJoysticks;
    [SerializeField] protected Player associatedPlayer;
    protected List<UnityEngine.InputSystem.OnScreen.OnScreenStick> AllOtherIndependentJoystickFunctions;
    protected List<OnScreenStickModifications> AllOtherIndependentJoystickModifications;

    private void OnEnable()
    {
        if (AllOtherIndependentJoysticks != null)
        {
            AllOtherIndependentJoystickFunctions = new List<UnityEngine.InputSystem.OnScreen.OnScreenStick>();
            AllOtherIndependentJoystickModifications = new List<OnScreenStickModifications>();
            for (int i = 0; i < AllOtherIndependentJoysticks.Count; i++)
            {
                AllOtherIndependentJoystickFunctions.Add(AllOtherIndependentJoysticks[i].GetComponent<UnityEngine.InputSystem.OnScreen.OnScreenStick>());
                AllOtherIndependentJoystickModifications.Add(AllOtherIndependentJoysticks[i].GetComponent<OnScreenStickModifications>());
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (AllOtherIndependentJoysticks != null) {
            for (int i = 0; i < AllOtherIndependentJoysticks.Count; i++)
            {
                AllOtherIndependentJoystickFunctions[i].enabled = false;
                AllOtherIndependentJoystickModifications[i].enabled = false;
                //Debug.Log("Other is disabled.");
                // Let the Player know which attack to use
                associatedPlayer.SetActiveIndependentJoystick(joystickNumber);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (AllOtherIndependentJoysticks != null)
        {
            for (int i = 0; i < AllOtherIndependentJoysticks.Count; i++)
            {
                AllOtherIndependentJoystickFunctions[i].enabled = true;
                AllOtherIndependentJoystickModifications[i].enabled = true;
                //Debug.Log("Other is enabled.");
            }
        }
    }
}
