using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomGen : MonoBehaviour
{
    public GameObject prefabRoof;
    public GameObject player;

    GameObject frontFab;
    GameObject midFab;
    GameObject mid2Fab;
    GameObject mid3Fab;
    GameObject backFab;

    // Start is called before the first frame update
    void Start()
    {
        backFab = Instantiate(prefabRoof, new Vector3(0, 0, -20), Quaternion.identity);
        midFab = Instantiate(prefabRoof, new Vector3(0, 0, 0), Quaternion.identity);
        mid2Fab = Instantiate(prefabRoof, new Vector3(0, 0, 20), Quaternion.identity);
        mid3Fab = Instantiate(prefabRoof, new Vector3(0, 0, 40), Quaternion.identity);
        frontFab = Instantiate(prefabRoof, new Vector3(0, 0, 60), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 backCoords = backFab.transform.position;
        Debug.Log("Me: " + player.transform.position.z + " Back: " + backCoords.z);
        if(player.transform.position.z - 20 > backCoords.z)
        {
            OutWithTheOld();
        }
    }


    void OutWithTheOld()
    {
        Destroy(backFab);
        backFab = midFab;
        midFab = mid2Fab;
        mid2Fab = mid3Fab;
        mid3Fab = frontFab;
        frontFab = Instantiate(prefabRoof, midFab.transform.position + new Vector3(0,0,60), Quaternion.identity);
        
    }
}
