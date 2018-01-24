using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TerritoryNode : MonoBehaviour {

	[SerializeField]
	private TerritoryNode[] adjacentNodes;
    [SerializeField]
    private Text[] soldierDisplay;
	private int soldierCount = 0;
	private int playerOwner = -1;
    private bool currentSelection = false;

	private Color ownerColour = Color.white;
    public SpriteOutline outline;
    private SpriteRenderer sRend;
	// Use this for initialization
	void Start () {
        outline = gameObject.GetComponent<SpriteOutline>();
        sRend = gameObject.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        DisplayUpdate();
	}

    private void DisplayUpdate() {
        foreach (Text t in soldierDisplay) {
            t.text = soldierCount.ToString();
        }

        if (soldierCount == 0) {
            playerOwner = -1;
            ownerColour = Color.white;
        }

        if (currentSelection) {
            outline.outlineSize = 5;
            
        }
        else {
            outline.outlineSize = 0;
        }
        sRend.color = ownerColour;
        
    }
    public void AdjustSoldiers(int newSoldiers){ soldierCount += newSoldiers;}
    public void SetSoldiers(int newSoldiers) { soldierCount = newSoldiers; }
	public int DisplaySoldiers(){return soldierCount;}

    public void SetCurrentSelection(bool selection) { currentSelection = selection; }
    public bool GetCurrentSelection() { return currentSelection; }

	public void SetOwner(int newOwner){ playerOwner = newOwner;}
	public int DisplayOwner() {return playerOwner;}
    public TerritoryNode[] GetAdjacentNodes() { return adjacentNodes; }

	public void SetColor(Color colour){ ownerColour = colour;}
	void OnDrawGizmosSelected(){
		Gizmos.color = Color.white;
		for(int i = 0; i < adjacentNodes.Length; i++) {
			Gizmos.DrawLine(gameObject.transform.position, adjacentNodes[i].transform.position);
		}
	}

    public void HighlightAdjacentTerritories(bool highlightFriendly)
    {
        currentSelection = true;
        foreach (TerritoryNode territory in adjacentNodes)
        {
            if (highlightFriendly)
            {
                if (territory.DisplayOwner() == playerOwner)
                {
                    territory.SetCurrentSelection(true);
                    territory.outline.color = Color.blue;
                    DrawLine(transform.position, territory.transform.position, 1f);
                }
            } else
            {
                if(territory.DisplayOwner() != playerOwner)
                {
                    territory.SetCurrentSelection(true);
                    territory.outline.color = Color.red;
                    DrawLine(transform.position, territory.transform.position, 1f);
                }
            }
        }
    }
    public void DeselectAdjacentTerritories()
    {
        currentSelection = false;
        foreach(TerritoryNode territory in adjacentNodes)
        {
            territory.SetCurrentSelection(false);
            territory.outline.color = Color.white;
        }
    }

    private void DrawLine(Vector3 start, Vector3 destination, float duration)
    {
        start.z = 1;
        destination.z = 1;
        GameObject myLine = new GameObject();
        //myLine.transform.parent = GameObject.Find("Canvas 2 Map Boogaloo").transform;
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.startColor = ownerColour;
        lr.endColor = Color.red;
        lr.startWidth = 0.015f;
        lr.endWidth = 0.0001f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, destination);
        GameObject.Destroy(myLine, duration);
    }
}
