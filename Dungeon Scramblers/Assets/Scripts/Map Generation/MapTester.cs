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
    int startingSeed;

    //the MapMaker used to generate maps
    [SerializeField]
    MapMaker mapper;

    //used to brute force setting random seeds
    //An unknown portion of Unity modfies Unity.Random's seed during the first two active frames
    //The only solution I have found to work around this is brute forcing it to ensure
    //that all map generation for testing starts during the 3rd frame
    bool hasTriggered = false;
    int frameCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 50; i++)
        {
            Random.InitState(2);
            Random.Range(0, 200);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasTriggered && frameCount == 2)
        {
            StartCoroutine("GenerateMapSeries");
            hasTriggered = true;
        }
        else if (!hasTriggered)
        {
            frameCount++;
        }
        
    }

    //
    IEnumerator GenerateMapSeries()
    {

        for (int i = startingSeed; i < numberOfMaps + startingSeed; i++)
        {
            Debug.Log("Testing Seed: " + i);
            mapper.ClearMap();
            Random.InitState(i);
            mapper.StartCoroutine("GenerateMap");
            yield return new WaitUntil(mapper.IsMapFinished);
            yield return new WaitForSeconds(waitTimeBetweenMaps);
        }

        //Debug.Log("Total Corridors: " + MapMaker.totalCorridors + ", Corner Cases: " + MapMaker.cornerCount + "\n" + "Corner Case Rate: " + (((double) MapMaker.cornerCount) / MapMaker.totalCorridors));
        yield return null; 
    }
}
