using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRenderSwitch : MonoBehaviour
{
    //This is a temp class used to turn off the Overlord's sprite renderer at the start of the game
    public SpriteRenderer[] SpriteRenderers;
    public void SpritesOn()
    {
        foreach(SpriteRenderer SpriteRend in SpriteRenderers)
        {
            SpriteRend.enabled = true;
        }
    }

    public void SpritesOff()
    {

        foreach (SpriteRenderer SpriteRend in SpriteRenderers)
        {
            SpriteRend.enabled = false;
        }
    }
}
