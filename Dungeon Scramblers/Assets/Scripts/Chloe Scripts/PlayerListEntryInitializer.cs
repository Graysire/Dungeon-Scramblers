using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerListEntryInitializer : MonoBehaviour
{
    [Header("UI References")]
    public Text PlayerNameText;
    public Button PlayerReadyButton;
    public Button PlayerReadyPopUp;

    public void Initialize(int playerID, string PlayerName)
    {
        PlayerNameText.text = PlayerName;
    }
}
