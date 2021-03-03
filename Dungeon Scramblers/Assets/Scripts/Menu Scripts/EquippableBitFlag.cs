using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine.UI;

public class EquippableBitFlag : MonoBehaviour
{
    // Refer to this link for Bitcode legend: https://docs.google.com/spreadsheets/d/12xuLHZSDCkMI4G0byxqrIRxpuUSsBOtuQ4uvpDbt47w/edit#gid=0

    public string playerBitFlag;    //Stores which player inventory to change 
    public string bitFlagCode;      //Stores the bits of the equippable this object relates to

    public UIButton armorButton;    //The button that must be pressed to set this equippable as the selected equippable for category

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


    }
}
