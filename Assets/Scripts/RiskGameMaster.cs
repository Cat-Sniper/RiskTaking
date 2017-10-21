using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RiskGameMaster : MonoBehaviour {
	enum TURN_PHASE {RECRUIT,ATTACK,FORTIFY};
	enum ARMY_DENOMINATIONS {INFANTRY, CAVALRY, ARTILLERY};
    private bool setup = true;
    private bool optionsSelected = false;
    private bool allTsClaimed = false;
    private bool didOnce = false;
    private bool selectedAttacker = false;

    public Continent[] continents;
    private TerritoryNode[] allTerritories;
    private TerritoryNode currentTerritory;
    private TerritoryNode defendingTerritory;
    private int unclaimedTerritories = 43;

    private Player[] currentPlayers;
    private int activePlayers = 0;
    private int currentPlayersTurn = -1;
    [SerializeField] private GameObject[] playerInfo;

    private TURN_PHASE currentPhase;
    private Text turnInfo;
    private Text phaseInfoTxt;
    private Image turnInfoCol;
    public GameObject setupPanel;
    private GameObject attackUI;
    private GameObject fortifyUI;
    private GameObject reinforceUI;

    private GameObject attackPanel;
    private Slider attackSlider;
    private Text attackDice;
    private Text attackingCountry;
    private Text attackingSoldierCount;
    private Text defendingCountry;
    private Text defendingSoldierCount;

    [SerializeField] private GameObject[] attackingDice;
    [SerializeField] private GameObject[] defendingDice;
    private List<GameObject> currentRoll;

	// Use this for initialization
	void Start () {
        allTerritories = new TerritoryNode[unclaimedTerritories]; 															//Test
        turnInfo = GameObject.Find("Current Turn").GetComponent<Text>();
        phaseInfoTxt = GameObject.Find("Current Phase").GetComponent<Text>();
        turnInfoCol = turnInfo.gameObject.GetComponentInChildren<Image>();
        attackUI = GameObject.Find("UI - Attack");
        attackPanel = GameObject.Find("UI - AttackSelection");
        fortifyUI = GameObject.Find("UI - Fortification");
        reinforceUI = GameObject.Find("UI - Reinforcement");
        attackDice = GameObject.Find("DiceUI").GetComponent<Text>();
        attackingCountry = GameObject.Find("AttackingCountry").GetComponent<Text>();
        attackingSoldierCount = GameObject.Find("AttackingSoldierCount").GetComponent<Text>();
        defendingCountry = GameObject.Find("DefendingCountry").GetComponent<Text>();
        defendingSoldierCount = GameObject.Find("DefendingSoldierCount").GetComponent<Text>();
        attackSlider = GameObject.Find("SoldierSlider").GetComponent<Slider>();
        attackUI.SetActive(false);
        fortifyUI.SetActive(false);
        reinforceUI.SetActive(false);
        attackPanel.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        if (optionsSelected)    //gameplay
        {  
            if (!setup) {                                         
                switch (currentPhase) {
                    case TURN_PHASE.RECRUIT:
                       
                        if (!didOnce)
                        {
                            reinforceUI.SetActive(true);
                            attackUI.SetActive(false);
                            fortifyUI.SetActive(false);
                            CalculateReinforcements(currentPlayers[currentPlayersTurn]);
                            didOnce = true;
                        }
                        if (Input.GetMouseButtonDown(0) && currentPlayersTurn != -1)    
                        {
                            RaycastHit2D mouseCast2D = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), 100, 1 << LayerMask.NameToLayer("Territory"));
                            if (mouseCast2D)
                            {
                                if (currentTerritory)
                                {
                                    currentTerritory.SetCurrentSelection(false);
                                }
                                currentTerritory = mouseCast2D.rigidbody.GetComponent<TerritoryNode>();
                                if ( currentPlayers[currentPlayersTurn].GetArmies() > 0 && currentTerritory.DisplayOwner() == currentPlayersTurn)  
                                {
                                    currentTerritory.SetCurrentSelection(true);
                                    currentTerritory.AdjustSoldiers(1);
                                    currentPlayers[currentPlayersTurn].AddArmies(-1);
                                }
                            }
                        }
                                break;
                    case TURN_PHASE.ATTACK:
                        if (!didOnce)
                        {
                            reinforceUI.SetActive(false);
                            attackUI.SetActive(true);
                            fortifyUI.SetActive(false);
                            didOnce = true;
                        }
                        if (Input.GetMouseButtonDown(0) && !attackPanel.activeInHierarchy && currentPlayersTurn != -1)  //Mouse Input while the attack options panel is closed
                        {
                            RaycastHit2D mouseCast2D = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), 100, 1 << LayerMask.NameToLayer("Territory"));
                            if (mouseCast2D)
                            {
                                TerritoryNode newTerritory = mouseCast2D.rigidbody.GetComponent<TerritoryNode>();
                                if(newTerritory!= currentTerritory && newTerritory.GetCurrentSelection())   // clicked territory is a territory adjacent to selected player territory.
                                {
                                    currentTerritory.DeselectAdjacentTerritories();
                                    SelectDefendingCountry(newTerritory);
                                    OpenAttackPanel();
                                }
                                else if(newTerritory.DisplayOwner() == currentPlayersTurn && newTerritory.DisplaySoldiers() >= 1)    //clicked territory is owned by the player and is eligible to attack
                                {
                                    currentTerritory.DeselectAdjacentTerritories();
                                    SelectAttackingCountry(newTerritory);
                                }
                                else                                                                        //clicked territory does not qualify as an attacking country or defending country
                                {
                                    currentTerritory.DeselectAdjacentTerritories();
                                }
                                
                            }
                        }
                        if (attackPanel.activeInHierarchy)  //Attack options panel/finalizing the attack.
                        {
                            if(currentTerritory.DisplaySoldiers() > 3)
                            {
                                attackSlider.maxValue = 3;
                            } else
                            {
                                attackSlider.maxValue = currentTerritory.DisplaySoldiers() - 1;
                            }
                            attackDice.text = attackSlider.value.ToString();
                        }
                        break;
                    case TURN_PHASE.FORTIFY:
                        reinforceUI.SetActive(false);
                        attackUI.SetActive(false);
                        fortifyUI.SetActive(true);
                        break;
                    default:
                        Debug.Log("Its just a phase....");                        break;
                }
            }
            else  //Setup Phase -- Ends when all Players are out of soldiers to place
            {

                //Mouse Input handling ( Click on Territory to place soldier)
                if (Input.GetMouseButtonDown(0) && currentPlayersTurn != -1) {
                    RaycastHit2D mouseCast2D = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), 100, 1 << LayerMask.NameToLayer("Territory"));
                    if (mouseCast2D) {
                        
                        if (currentTerritory) {
                            currentTerritory.SetCurrentSelection(false);
                        }
                        currentTerritory = mouseCast2D.rigidbody.GetComponent<TerritoryNode>();

                        if (!allTsClaimed) {  //can only pick neutral territories until there are no more
                           
                            if (currentTerritory.DisplayOwner() < 0) {
                                currentTerritory.SetCurrentSelection(true);
                                currentTerritory.AdjustSoldiers(1);
                                currentPlayers[currentPlayersTurn].AddArmies(-1);
                                currentPlayers[currentPlayersTurn].AddTerritory(currentTerritory);
                                currentTerritory.SetColor(currentPlayers[currentPlayersTurn].armyColour);
                                currentTerritory.SetOwner(currentPlayersTurn);
                                unclaimedTerritories -= 1;

                                if (unclaimedTerritories <= 0){
                                    allTsClaimed = true;
                                    phaseInfoTxt.text = "Reinforce claimed territories";
                                }
                                AdvanceTurn();
                            }
                        } else {    //can only click on owned territories to add soldiers
                            
                            if (currentTerritory.DisplayOwner() == currentPlayersTurn)
                            {
                                currentTerritory.SetCurrentSelection(true);
                                currentTerritory.AdjustSoldiers(1);
                                currentPlayers[currentPlayersTurn].AddArmies(-1);

                                AdvanceTurn();
                            }
                        }
                    }   //end of raycasts
                }   // end of input

               if (currentPlayers[currentPlayersTurn].GetArmies() < 1) {
                  setup = false;
                    phaseInfoTxt.text = "Current Phase: ";
                }
            }   //end of setup phase

            //Display Info
            int uITurnInfo = currentPlayersTurn + 1;
            turnInfo.text = "Player " + uITurnInfo + "'s Turn!";
            turnInfoCol.color = currentPlayers[currentPlayersTurn].armyColour;

        } //end of gameplay
	}   

	public void SetupGame(int nPlayers){
        int territories = GameObject.FindGameObjectsWithTag("Map").Length;
        activePlayers = nPlayers;
        for (int i = 0; i < territories; i++) {
            allTerritories[i] = GameObject.FindGameObjectsWithTag("Map")[i].GetComponent<TerritoryNode>();
            allTerritories[i].GetComponent<SpriteRenderer>().color = Color.white;
        }
        currentPlayers = new Player[nPlayers];
		for(int i = 1; i <= nPlayers; i++) {                                                 //start counter at 1 to match up with player number before
			currentPlayers[i-1] = GameObject.Find("Player" +i).GetComponent<Player>();      // the player is assigned to correct array position.
        }

        for (int i = nPlayers; i <= 5; i++) {
            playerInfo[i].SetActive(false);
        }
        switch (nPlayers){
		case 2:
			foreach (Player newPlayer in currentPlayers) {
				newPlayer.AddArmies(24);
                newPlayer.InitializePlayer();
			}
               
			break;
		case 3:
			foreach(Player newPlayer in currentPlayers) {
				newPlayer.AddArmies(35);
                newPlayer.InitializePlayer();
            }
			break;
		case 4:
			foreach (Player newPlayer in currentPlayers) {
				newPlayer.AddArmies(30);
                newPlayer.InitializePlayer();
            }
			break;
		case 5:
			foreach (Player newPlayer in currentPlayers) {
				newPlayer.AddArmies(25);
                newPlayer.InitializePlayer();
            }
			break;
		case 6:
			foreach (Player newPlayer in currentPlayers) {
				newPlayer.AddArmies(20);
                newPlayer.InitializePlayer();
			}
			break;
		default:
			Debug.Log("invalid number of players for gamesetup");
			break;
		}

        int startingPlayer = Random.Range(0, nPlayers);
        setupPanel.SetActive(false);
        currentPlayersTurn = startingPlayer;
        phaseInfoTxt.text = "Pick an unclaimed territory (white)";

        foreach (TerritoryNode node in allTerritories) {
            node.SetSoldiers(0);
        }
        optionsSelected = true;
    }

	private void CalculateReinforcements(Player playerID) {
        int newArmies = 0;
        newArmies = playerID.GetTerritories().Count / 3;
        playerID.AddArmies(newArmies);
    }

    private void AdvanceTurn() {
        if(currentPlayersTurn < currentPlayers.Length -1) {
            currentPlayersTurn++;
        } else {
            currentPlayersTurn = 0;
        }
        currentPhase = TURN_PHASE.RECRUIT;
        didOnce = false;
    }
    private void SelectAttackingCountry(TerritoryNode attacker)   //Highlights the active Territory, before querrying it's adjacentTerritories.
    {
        attackingCountry.text = attacker.name;
        attackingSoldierCount.text = attacker.DisplaySoldiers().ToString();
        attacker.HighlightAdjacentTerritories();
        currentTerritory = attacker;
    }
    private void SelectDefendingCountry(TerritoryNode defender)
    {
        defender.outline.color = Color.red;
        defendingCountry.text = defender.name;
        defendingSoldierCount.text = defender.DisplaySoldiers().ToString();
        
    }
    public void NextTurnButton()
    {
        if (!setup)
        {
            switch (currentPhase)
            {
                case TURN_PHASE.RECRUIT:
                    if (currentPlayers[currentPlayersTurn].GetArmies() == 0)
                    {
                        currentPhase = TURN_PHASE.ATTACK;
                        didOnce = false;
                    }
                    break;
                case TURN_PHASE.ATTACK:
                    if (!attackPanel.activeInHierarchy)
                    {
                        currentPhase = TURN_PHASE.FORTIFY;
                        didOnce = false;
                    }
                    break;
                case TURN_PHASE.FORTIFY:
                    AdvanceTurn();
                    break;
            }
        }
    }
    public void CloseAttackPanelButton() { attackPanel.SetActive(false); }
    public void OpenAttackPanel() { attackPanel.SetActive(true); }
}
