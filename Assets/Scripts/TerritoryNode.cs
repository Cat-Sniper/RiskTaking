using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TerritoryNode : MonoBehaviour {

     [SerializeField]private TerritoryNode[] adjacentNodes;
     [SerializeField]private Text soldierDisplay;
     private int soldierCount = 0;
     public int playerOwner = -1;                                                                   // -1 means by default every territory will be neutral with no owner
     private bool currentSelection = false;
     private Continent continent;
     // private bool friendlySelection = false;

     private Color ownerCol = Color.white;
     private Color enemyCol = Color.red;
     private Color lineCol = Color.white;
     private Material circleMat;
     public SpriteOutline outline;
     private Color territoryColor;
     [SerializeField]private SpriteRenderer sRend;

     private GameObject[] currentLines;
     private int activeLines = 0;


     // Use this for initialization
     void Start () {

          outline = gameObject.GetComponent<SpriteOutline>();
          ownerCol = Color.white;
          circleMat = sRend.material;
          territoryColor = gameObject.GetComponent<SpriteRenderer>().color;

          currentLines = new GameObject[adjacentNodes.Length];

     }
	
     // Update is called once per frame
     void Update () {

          DisplayUpdate();

     }

     private void DisplayUpdate() {
        
          soldierDisplay.text = soldierCount.ToString();

          if (currentSelection) {

               outline.outlineSize = 6;
               outline.color = ownerCol;
            
          } else {

               outline.outlineSize = 0;

          }

          ownerCol.a = 0.8f;
          sRend.color = ownerCol;
          circleMat.color = ownerCol;
        
     }

     public void AdjustSoldiers(int newSoldiers){ soldierCount += newSoldiers;}
     public void SetSoldiers(int newSoldiers) { soldierCount = newSoldiers; }
     public int DisplaySoldiers(){return soldierCount; }

     public void SetCurrentSelection(bool selection) { currentSelection = selection; }

     public bool GetCurrentSelection() { return currentSelection; }
     public Continent GetContinent() { return continent; }
     public void SetContinent(Continent con) { continent = con; }

     public void SetOwner(int newOwner){ playerOwner = newOwner;}
     public int DisplayOwner() {return playerOwner;}
     public TerritoryNode[] GetAdjacentNodes() { return adjacentNodes; }

     public void SetColor(Color colour){ ownerCol = colour;}

     void OnDrawGizmosSelected() {

	     Gizmos.color = Color.white;

	     for(int i = 0; i < adjacentNodes.Length; i++) {

		     Gizmos.DrawLine(gameObject.transform.position, adjacentNodes[i].transform.position);

	     }

     }

     public void HighlightAdjacentTerritories(bool highlightFriendlies)
     {
          currentSelection = true;

          if (highlightFriendlies) {

               foreach (TerritoryNode territory in adjacentNodes) {

                    if (territory.DisplayOwner() == playerOwner) {

                         territory.SetCurrentSelection(currentSelection);
                         territory.outline.color = ownerCol;
                         lineCol = ownerCol;
                         DrawLine(transform.position, territory.transform.position, -1);

                    }

               }

          } else {

               foreach (TerritoryNode territory in adjacentNodes) {

                    if (territory.DisplayOwner() != playerOwner) {

                         territory.SetCurrentSelection(true);
                         territory.outline.color = enemyCol;
                         lineCol = enemyCol;
                         DrawLine(transform.position, territory.transform.position, -1);

                    }

               }

          }

     }

     public void DeselectAdjacentTerritories() {

          DestroyAllLines();

          foreach(TerritoryNode territory in adjacentNodes) {

               territory.SetCurrentSelection(false);

          }


     }

     private void DrawLine(Vector3 start, Vector3 destination, float duration) {

          start.z = 1;
          destination.z = 1;
          GameObject myLine = new GameObject();
          currentLines[activeLines] = myLine;
          activeLines++;

          Gradient gradient = new Gradient();

          gradient.SetKeys(
                           new GradientColorKey[] { new GradientColorKey(ownerCol,0.0f), new GradientColorKey(lineCol,1.0f)},
                           new GradientAlphaKey[] { new GradientAlphaKey(0.5f, 0.0f), new GradientAlphaKey(1.0f,1.0f)}
                           );

          myLine.transform.position = start;
          myLine.AddComponent<LineRenderer>();
          LineRenderer lr = myLine.GetComponent<LineRenderer>();
          lr.material = new Material(Shader.Find("Particles/Standard Unlit"));
          lr.colorGradient = gradient;
          lr.startWidth = 0.015f;
          lr.endWidth = 0.0001f;
          lr.SetPosition(0, start);
          lr.SetPosition(1, destination);

          // Delete the line after 'duration' seconds. Or last forever if duration is less than zero
          if(duration > 0) {
               GameObject.Destroy(myLine, duration);
               activeLines--;
          }

     }

     private void DestroyAllLines(){

          for (int i = 0; i < activeLines; i++){
               
               if(currentLines[i] != null){
                    GameObject.Destroy(currentLines[i]);
               }
          }

          activeLines = 0;
     }
     public Color GetTerritoryColor() { return territoryColor; }
     public SpriteRenderer GetSpriteRenderer() { return sRend; }

}
