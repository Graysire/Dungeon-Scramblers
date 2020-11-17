using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverlordUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ObstacleClicked(GameObject EnemyInstance)
    {
        Image image = EnemyInstance.GetComponent<Image>();
        Hover.GetHover().Activate(image.sprite);
        Hover.GetHover().SetEnemyIntance(EnemyInstance);
    }

   
}
