using UnityEngine;
using System.Collections;

public class Continent : MonoBehaviour {
	public int Value = 0;
	public TerritoryNode[] territories;
	public bool hasBonus = false;
	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void CheckBonus()
    {
        int owner = territories[0].DisplayOwner();          //first territory belonging to the continent is set as comparison
        bool differentOwners = false;                       //and as soon as a territory with a different owner appears, we don't have to check anymore.
        foreach (TerritoryNode terry in territories)
        {
            int ownerCheck = terry.DisplayOwner();          
            if(owner != ownerCheck)
            {
                differentOwners = true;
                break;                          
            } 
        }
        hasBonus = !differentOwners;
    }
}
