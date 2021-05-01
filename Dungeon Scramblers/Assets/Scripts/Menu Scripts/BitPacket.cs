using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BitPacket
{
    //Stores the inventory bit flags and audio into an object for saving
    [SerializeField]
    public int mageInvBitsPacked = 0;
    [SerializeField]
    public int knightInvBitsPacked = 0;
    [SerializeField]
    public int rogueInvBitsPacked = 0;
    [SerializeField]
    public int overlordInvBitsPacked = 0;
    [SerializeField]
    public float sfxValue = 0;
    [SerializeField]
    public float bgmValue = 0;

}
