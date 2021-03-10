using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationObjectPooling : MonoBehaviour
{
    //So each 'section' will have a beginning, a middle, and an end. These will be chosen randomly later in the script.
    public GameObject start1, mid1, end1;
    //public List<GameObject> list;
    public GameObject player;

    private GameObject nextPrefab;
    
    //recycle distance is how far away the player has to be from the first platform in order for it to go bye bye.
    public float recycleDistance;
    //number of prefabs on the screen at a time.
    public int numberOfObjects;
    //take a guess fam
    public Vector3 startPosition;

    //next position prefab is in, changes every time a prefab is put somewhere.
    private Vector3 nextPosition;
    //All the prefabs are here
    private List<GameObject> objList;

    // Start is called before the first frame update
    void Start()
    {
        //This is the code thats run when the game starts, it will create 12 prefabs, which should be the amount on screen at all times.
        //This creates the list of prefabs, like I said, it'll be 12 long.
        objList = new List<GameObject>(numberOfObjects);
        nextPosition = startPosition;
        //for each number of objects (12)...
        for (int i = 0; i < numberOfObjects; i++)
        {
            //We will pick a prefab we want to use...
            RandomiseNextPrefab(out nextPrefab);
            //Put that prefab on the screen...
            GameObject pf = Instantiate(nextPrefab);
            //and put it after the last one that was made.
            pf.transform.position = nextPosition;
            nextPosition.z = pf.transform.localPosition.z + 20;
            //we add the object to the list, this is because we want to know which object is the last one (or first one technically)
            objList.Add(pf);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if the last (first) object in the list's distance is a equal to or higher than a variable (recycleDistance)...
        if(objList[0].transform.position.z + recycleDistance < player.transform.position.z)
        {
            //We grab that object, then remove it for the list
            //Keep in mind that once an object at the start of a list is removed, the rest of the list gets shifted, so the object in the 2nd position will now be in the 1st position etc
            GameObject pf = objList[0];
            objList.Remove(pf);
            //After we've removed it from the list, we place it at the end of the platforms (with a distance of 20 in between)
            pf.transform.position = nextPosition;
            nextPosition.z += 20;
            //We'll change the prefab a bit here if we end up getting multiple, just because it adds variety and challenge. 
            //Can't do this atm because i'm waiting on art.

            //We add the object that we just removed, repositioned and altered back into the list.
            //"Why would you do that" I hear you ask! Well my friend, when we took it out of the list it was at the 0'th index, meaning it was the very first in the list.
            //After we've added it again, it gets put at the 11th index.
            objList.Add(pf);
        }
    }

    //This is the function that grabs a random prefab, I might have to remake this once I get some more art because reasons.
    private void RandomiseNextPrefab(out GameObject nextPrefab)
    {
        //Atm its just getting a random number between 2 and 3, excluding 3.
        //so in other words its returning 2 every time...
        //It's cos we only have one working prefab, like I said I'll redo this later.
        int randnum = Random.Range(2, 3);
        if (randnum == 1)
        {
            //returns prefab 1
            nextPrefab = mid1;
        }
        else if (randnum == 2)
        {
            //also returns prefab 1
            nextPrefab = mid1;
        }
        //this will never happen but i gotta appease the code gods so here it is.
        else nextPrefab = null;
    }
}
