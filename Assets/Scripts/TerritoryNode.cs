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
    private bool friendlySelection = false;

	private Color ownerCol = Color.white;
    private Color enemyCol = Color.red;
    private Color lineCol = Color.white;
    public SpriteOutline outline;
    private SpriteRenderer sRend;
	// Use this for initialization
	void Start () {
        outline = gameObject.GetComponent<SpriteOutline>();
        sRend = gameObject.GetComponent<SpriteRenderer>();
        ownerCol = Color.white;
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
            
        }

        if (currentSelection) {
            outline.outlineSize = 5;
            
        }
        else {
            outline.outlineSize = 0;
        }
        sRend.color = ownerCol;
        
    }
    public void AdjustSoldiers(int newSoldiers){ soldierCount += newSoldiers;}
    public void SetSoldiers(int newSoldiers) { soldierCount = newSoldiers; }
	public int DisplaySoldiers(){return soldierCount;}

    public void SetCurrentSelection(bool selection)
    {
        currentSelection = selection;
        if (!selection)
        {
            outline.color = Color.white;
        }
    }
    public bool GetCurrentSelection() { return currentSelection; }

	public void SetOwner(int newOwner){ playerOwner = newOwner;}
	public int DisplayOwner() {return playerOwner;}
    public TerritoryNode[] GetAdjacentNodes() { return adjacentNodes; }

	public void SetColor(Color colour){ ownerCol = colour;}
	void OnDrawGizmosSelected(){
		Gizmos.color = Color.white;
		for(int i = 0; i < adjacentNodes.Length; i++) {
			Gizmos.DrawLine(gameObject.transform.position, adjacentNodes[i].transform.position);
		}
	}

    public void HighlightAdjacentTerritories(bool highlightFriendlies)
    {
        currentSelection = true;
        if (highlightFriendlies)
        {
            foreach (TerritoryNode territory in adjacentNodes)
            {
                if (territory.DisplayOwner() == playerOwner)
                {
                    territory.SetCurrentSelection(true);
                    territory.outline.color = ownerCol;
                    lineCol = ownerCol;
                    DrawLine(transform.position, territory.transform.position, 1f);
                }
            }
        }
        else
        {
            foreach (TerritoryNode territory in adjacentNodes)
            {
                if (territory.DisplayOwner() != playerOwner)
                {
                    territory.SetCurrentSelection(true);
                    territory.outline.color = enemyCol;
                    lineCol = enemyCol;
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
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(ownerCol,0.0f), new GradientColorKey(lineCol,1.0f)},
            new GradientAlphaKey[] { new GradientAlphaKey(0.5f, 0.0f), new GradientAlphaKey(1.0f,1.0f)}
            );
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.colorGradient = gradient;
        lr.startWidth = 0.015f;
        lr.endWidth = 0.0001f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, destination);
        GameObject.Destroy(myLine, duration);
    }
}
