using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//generates a series of maps with specific seeds
public class MapTester : MonoBehaviour
{
    //totla number of maps to generate
    [SerializeField]
    int numberOfMaps;

    //the time between a map finishing generation and the next one beginning
    [SerializeField]
    float waitTimeBetweenMaps;

    //the initial seed usedfor generating maps
    [SerializeField]
    int startingSeed = 0;

    //the MapMaker used to generate maps
    [SerializeField]
    MapMaker mapper;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("GenerateMapSeries");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //
    IEnumerator GenerateMapSeries()
    {
        for (int i = startingSeed; i < numberOfMaps + startingSeed; i++)
        {
            Debug.Log("Testing Seed: " + i);
            Random.InitState(i);
            mapper.ClearMap();
            mapper.StartCoroutine("GenerateMap");
            yield return new WaitUntil(mapper.IsMapFinished);
            yield return new WaitForSeconds(waitTimeBetweenMaps);
        }

        yield return null; 
    }
}
