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
    private bool fortified = false;
    private bool territoryConquered = false;

    public Continent[] continents;
    private TerritoryNode[] allTerritories;
    private TerritoryCard[] allCards;
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
    private Image border;
    private Image attackingCol;
    private Image defendingCol;
    public GameObject setupPanel;
    private GameObject attackUI;
    private GameObject fortifyUI;
    private GameObject reinforceUI;
    private GameObject turnButton;
    [SerializeField] private GameObject explosionPrefab;
    
        //#########################//

    private GameObject attackPanel;
    private Slider attackSlider;
    private Text attackDice;
    private Text attackingCountry;
    private Text attackingSoldierCount;
    private Text defendingCountry;
    private Text defendingSoldierCount;
    private Text attackButton;

    [SerializeField] private GameObject[] attackingDice;
    [SerializeField] private GameObject[] defendingDice;


	// Use this for initialization
	void Start () {
        
        allTerritories = new TerritoryNode[unclaimedTerritories]; 															//Test
        turnInfo = GameObject.Find("Current Turn").GetComponent<Text>();
        phaseInfoTxt = GameObject.Find("Current Phase").GetComponent<Text>();
        turnInfoCol = turnInfo.gameObject.GetComponentInChildren<Image>();
        border = GameObject.Find("Border").GetComponent<Image>();
        attackingCol = GameObject.Find("AttackerColour").GetComponent<Image>();
        defendingCol = GameObject.Find("DefenderColour").GetComponent<Image>();
        attackUI = GameObject.Find("UI - Attack");
        attackPanel = GameObject.Find("UI - AttackSelection");
        fortifyUI = GameObject.Find("UI - Fortification");
        reinforceUI = GameObject.Find("UI - Reinforcement");
        attackDice = GameObject.Find("DiceUI").GetComponent<Text>();                    //# of attack dice OR # of soldiers moving to country
        attackingCountry = GameObject.Find("AttackingCountry").GetComponent<Text>();
        attackingSoldierCount = GameObject.Find("AttackingSoldierCount").GetComponent<Text>();
        defendingCountry = GameObject.Find("DefendingCountry").GetComponent<Text>();
        defendingSoldierCount = GameObject.Find("DefendingSoldierCount").GetComponent<Text>();
        turnButton = GameObject.Find("End Turn Button");
        attackButton = GameObject.Find("AttackButtonText").GetComponent<Text>();
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
                if (activePlayers < 2) {
                    //TODO : GAME OVER STUFF
                }
                switch (currentPhase) {
                    #region RECRUIT
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
                                    currentTerritory.SetCurrentSelection(false);
                                currentTerritory = mouseCast2D.rigidbody.GetComponent<TerritoryNode>();
                                if ( currentPlayers[currentPlayersTurn].GetArmies() > 0 && currentTerritory.DisplayOwner() == currentPlayersTurn)  
                                {
                                    currentTerritory.SetCurrentSelection(true);
                                    currentTerritory.AdjustSoldiers(1);
                                    currentPlayers[currentPlayersTurn].AddArmies(-1);
                                    if (currentPlayers[currentPlayersTurn].GetArmies() == 0)
                                        turnButton.SetActive(true);
                                }
                            }
                        }
                                break;
                    #endregion
                    #region ATTACK
                    case TURN_PHASE.ATTACK:
                        if (!didOnce)
                        {
                            reinforceUI.SetActive(false);
                            attackUI.SetActive(true);
                            fortifyUI.SetActive(false);
                            didOnce = true;
                        }
                        if (territoryConquered) {
                            attackButton.text = "Move";
                            attackSlider.maxValue = currentTerritory.DisplaySoldiers() - 1;
                            attackDice.text = attackSlider.value.ToString();
                            if (!attackPanel.activeInHierarchy)
                                OpenAttackPanel();
                        } else if (Input.GetMouseButtonDown(0) && !attackPanel.activeInHierarchy && currentPlayersTurn != -1) {  //Mouse Input while the attack options panel is closed
                            RaycastHit2D mouseCast2D = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), 100, 1 << LayerMask.NameToLayer("Territory"));
                            if (mouseCast2D) {
                                TerritoryNode newTerritory = mouseCast2D.rigidbody.GetComponent<TerritoryNode>();
                                if(newTerritory!= currentTerritory && newTerritory.GetCurrentSelection()) { // clicked territory is a territory adjacent to selected player territory.
                                    currentTerritory.DeselectAdjacentTerritories();
                                    SelectDefendingCountry(newTerritory);
                                    attackSlider.minValue = 1;
                                    OpenAttackPanel();
                                }
                                else if(newTerritory.DisplayOwner() == currentPlayersTurn && newTerritory.DisplaySoldiers() > 1) {  //clicked territory is owned by the player and is eligible to attack
                                    currentTerritory.DeselectAdjacentTerritories();
                                    SelectAttackingCountry(newTerritory);
                                }
                                else                                                                        //clicked territory does not qualify as an attacking country or defending country
                                    currentTerritory.DeselectAdjacentTerritories();
                            }
                        } 
                        if (attackPanel.activeInHierarchy && !territoryConquered)  //Attack options panel/finalizing the attack.
                        {
                            attackButton.text = "Attack!";
                            if(currentTerritory.DisplaySoldiers() > 3)
                                attackSlider.maxValue = 3;
                            else
                                attackSlider.maxValue = currentTerritory.DisplaySoldiers() - 1;
                            attackDice.text = attackSlider.value.ToString();
                        } 
                        break;
                    #endregion
                    #region FORTIFY
                    case TURN_PHASE.FORTIFY:
                        if (!didOnce)
                        {
                            reinforceUI.SetActive(false);
                            attackUI.SetActive(false);
                            fortifyUI.SetActive(true);
                        }
                        if (Input.GetMouseButtonDown(0) && !attackPanel.activeInHierarchy && currentPlayersTurn != -1 && !fortified)  //Mouse Input while the attack options panel is closed
                        {
                            RaycastHit2D mouseCast2D = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), 100, 1 << LayerMask.NameToLayer("Territory"));
                            if (mouseCast2D)
                            {
                                TerritoryNode newTerritory = mouseCast2D.rigidbody.GetComponent<TerritoryNode>();
                                if (newTerritory != currentTerritory && newTerritory.GetCurrentSelection())   // clicked territory is a territory adjacent to selected player territory.
                                {
                                    currentTerritory.DeselectAdjacentTerritories();
                                    SelectDefendingCountry(newTerritory);
                                    OpenAttackPanel();
                                }
                                else if (newTerritory.DisplayOwner() == currentPlayersTurn && newTerritory.DisplaySoldiers() > 1)    //clicked territory is owned by the player and is eligible to fortify
                                {
                                    currentTerritory.DeselectAdjacentTerritories();
                                    SelectAttackingCountry(newTerritory);
                                }
                                else                                                                        //clicked territory does not qualify as an attacking country or defending country 
                                    currentTerritory.DeselectAdjacentTerritories();
                            } else 
                                currentTerritory.DeselectAdjacentTerritories();
                        }
                        if (attackPanel.activeInHierarchy)  //Fortify options panel. (repurposed attack options panel)                          
                        {
                            attackButton.text = "Fortify";
                            attackSlider.minValue = 1;
                            attackSlider.maxValue = currentTerritory.DisplaySoldiers() - 1;
                            attackDice.text = attackSlider.value.ToString();
                        }
                        if (fortified)
                            NextTurnButton();   // Advance turn when all actions have been taken
                        break;
                    #endregion
                    default:
                        Debug.Log("Its just a phase....");                        break;
                }
            }
            else { //Setup Phase -- Ends when all Players are out of soldiers to place           
                if (Input.GetMouseButtonDown(0) && currentPlayersTurn != -1) {  //Mouse Input handling ( Click on Territory to place soldier)
                    RaycastHit2D mouseCast2D = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), 100, 1 << LayerMask.NameToLayer("Territory"));
                    if (mouseCast2D) {
                        if (currentTerritory) 
                            currentTerritory.SetCurrentSelection(false);
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

                                if (currentTerritory.GetContinent().CheckBonus(currentPlayersTurn)) 
                                    currentTerritory.GetContinent().UpdateBorderColour(GetCurrentPlayer().armyColour);
                                
                                if (unclaimedTerritories <= 0){
                                    allTsClaimed = true;
                                    phaseInfoTxt.text = "Reinforce claimed territories";
                                }
                                AdvanceTurn();
                            }
                        } else {    //can only click on owned territories to add soldiers
                            
                            if (currentTerritory.DisplayOwner() == currentPlayersTurn) {
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
            border.color = currentPlayers[currentPlayersTurn].armyColour;
        } //end of gameplay
	}

    private void FixedUpdate() {
        if (!setup) 
            CheckPlayers();
        if (attackPanel.activeInHierarchy) {
            attackingSoldierCount.text = (currentTerritory.DisplaySoldiers() - (int)attackSlider.value).ToString();
            if(currentPhase == TURN_PHASE.FORTIFY || territoryConquered)
                defendingSoldierCount.text = (defendingTerritory.DisplaySoldiers() + (int)attackSlider.value).ToString();
        }
    }

    public void SetupGame(int nPlayers){
        int territories = GameObject.FindGameObjectsWithTag("Map").Length;
        activePlayers = nPlayers;
        for (int i = 0; i < territories; i++) {
            allTerritories[i] = GameObject.FindGameObjectsWithTag("Map")[i].GetComponent<TerritoryNode>();
            allTerritories[i].SetColor(Color.white);
        }
        currentPlayers = new Player[nPlayers];
		for(int i = 1; i <= nPlayers; i++)                                                //start counter at 1 to match up with player number before
			currentPlayers[i-1] = GameObject.Find("Player" +i).GetComponent<Player>();      // the player is assigned to correct array position.
        for (int i = nPlayers; i <= 5; i++)
            playerInfo[i].SetActive(false);
        
        switch (nPlayers) {
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

        int startingPlayer = UnityEngine.Random.Range(0, nPlayers);
        setupPanel.SetActive(false);
        currentPlayersTurn = startingPlayer;
        phaseInfoTxt.text = "Pick an unclaimed territory (white)";
        activePlayers = nPlayers;

        //Initialize Game Objects
        foreach (TerritoryNode node in allTerritories)
            node.SetSoldiers(0);
        optionsSelected = true;
        foreach (GameObject die in attackingDice) {
            die.SetActive(false);
        }
        foreach (GameObject die in defendingDice) {
            die.SetActive(false);
        }
    }

	private void CalculateReinforcements(Player playerID) {
        //Every 3 territories provide a soldier
        int newArmies = 0;
        for (int i = 1; i <= playerID.GetTerritories().Count; i++) {
            if(i % 3 == 0) 
                newArmies++;
        }
        //Continents provide a set # of soldiers
        foreach(Continent bonusboi in continents)
            if (bonusboi.CheckBonus(currentPlayersTurn)) {
                newArmies += bonusboi.Value;
            }
           
        if (newArmies < 3)
            playerID.AddArmies(3);
        else
            playerID.AddArmies(newArmies);
    }

    private void CheckPlayers() {
        foreach (Player player in currentPlayers) {
            if(player.GetTerritories().Count == 0) 
                player.alive = false;
        }
    }

    private void AdvanceTurn() {
        if(currentPlayersTurn < currentPlayers.Length -1)
            currentPlayersTurn++;
        else 
            currentPlayersTurn = 0;

        if (!currentPlayers[currentPlayersTurn].alive)  //Is this player dead? Skip 'em!
            AdvanceTurn();
        else {
            currentPhase = TURN_PHASE.RECRUIT;
            didOnce = false;
            fortified = false;
            turnButton.SetActive(false);
        }
    }
    private void SelectAttackingCountry(TerritoryNode attacker) { //Highlights the active Territory, before querrying it's adjacentTerritories.
        attackingCountry.text = attacker.name;
        attackingSoldierCount.text = attacker.DisplaySoldiers().ToString();
        switch (currentPhase) {

            case (TURN_PHASE.ATTACK):
                attacker.HighlightAdjacentTerritories(false);  
                break;
            case (TURN_PHASE.FORTIFY):
                attacker.HighlightAdjacentTerritories(true);
                break;
        }
        currentTerritory = attacker;
        attackingCol.color = currentPlayers[currentTerritory.DisplayOwner()].armyColour;
    }
    private void SelectDefendingCountry(TerritoryNode defender) {
        defender.outline.color = Color.red;
        defendingCountry.text = defender.name;
        defendingSoldierCount.text = defender.DisplaySoldiers().ToString();
        defendingTerritory = defender;
        defendingCol.color = currentPlayers[defendingTerritory.DisplayOwner()].armyColour;
    }

    public void AttackButton() {
        if (territoryConquered)
            MoveTroops();
        else if(currentPhase == TURN_PHASE.FORTIFY) {
            MoveTroops();
            fortified = true;
        }
        else 
            RollDice(); 
    } 
    private void RollDice() { //Dice Rolling Algorithm
        int defendersLost = 0;
        int attackersLost = 0;
        int numAttackingDice = (int)attackSlider.value;
        int numDefendingDice = 0;

        if (numAttackingDice > 0) {

            if (defendingTerritory.DisplaySoldiers() == 1 || numAttackingDice == 1)
                numDefendingDice = 1;
            else
                numDefendingDice = 2;
            int[] attackDiceRoll = new int[numAttackingDice];
            int[] defendDiceRoll = new int[numDefendingDice];

            for (int i = 0; i < numAttackingDice; i++) {
                attackingDice[i].SetActive(true);
                attackingDice[i].GetComponent<RiskDie>().Roll();
                attackDiceRoll[i] = attackingDice[i].GetComponent<RiskDie>().GetTopFace();
            }
            for (int i = 0; i < numDefendingDice; i++) {
                defendingDice[i].SetActive(true);
                defendingDice[i].GetComponent<RiskDie>().Roll();
                defendDiceRoll[i] = defendingDice[i].GetComponent<RiskDie>().GetTopFace();
            }

            //Sort dice in descending order to ensure the dice match properly
            SortIntArrayDesc(attackDiceRoll);
            SortIntArrayDesc(defendDiceRoll);

            for (int i = 0; i < numDefendingDice; i++) {
                if (attackDiceRoll[i] > defendDiceRoll[i]) {
                    defendersLost++;

                    GameObject explode = CFX_SpawnSystem.GetNextObject(explosionPrefab, false); //WarFX by JMO
                    Vector3 exPos = defendingTerritory.transform.position;
                    exPos.z = 1.1f;
                    explode.transform.position = exPos;
                    explode.SetActive(true);
                } else {
                    attackersLost++;

                    GameObject explode = CFX_SpawnSystem.GetNextObject(explosionPrefab, false); //WarFX by JMO
                    Vector3 exPos = currentTerritory.transform.position;
                    exPos.z = 1.1f;
                    explode.transform.position = exPos;
                    explode.SetActive(true);
                }
            }
            defendingTerritory.AdjustSoldiers(-defendersLost);
            currentTerritory.AdjustSoldiers(-attackersLost);
            Debug.Log("Defender loses " + defendersLost + " soldiers");
            Debug.Log("Attacker loses " + attackersLost + " soldiers");

            CloseAttackPanelButton();
            //Annex territory, set all properties necessary for the new owner
            if (defendingTerritory.DisplaySoldiers() == 0) {
                currentPlayers[defendingTerritory.DisplayOwner()].RemoveTerritory(defendingTerritory);
                currentPlayers[currentPlayersTurn].AddTerritory(defendingTerritory);
                attackSlider.minValue = numAttackingDice - attackersLost;
                defendingTerritory.SetColor(currentPlayers[currentPlayersTurn].armyColour);
                defendingCol.color = currentPlayers[currentPlayersTurn].armyColour;
                territoryConquered = true;
            }

        } else {
            CloseAttackPanelButton();
        }
    } // end RollDice()

    private void MoveTroops() {
        territoryConquered = false;
        int soldiers = (int)attackSlider.value;
        currentTerritory.AdjustSoldiers(-soldiers);
        defendingTerritory.AdjustSoldiers(soldiers);
        defendingTerritory.SetOwner(currentPlayersTurn);
        currentTerritory.DeselectAdjacentTerritories();

        if (defendingTerritory.GetContinent().CheckBonus(currentPlayersTurn)) {
            defendingTerritory.GetContinent().UpdateBorderColour(GetCurrentPlayer().armyColour);
        } else
            defendingTerritory.GetContinent().ResetBorderColour();
        CloseAttackPanelButton();
    }
    public void NextTurnButton() {
        if (!setup) {
            switch (currentPhase) {
                case TURN_PHASE.RECRUIT:
                    currentPhase = TURN_PHASE.ATTACK;
                    break;
                case TURN_PHASE.ATTACK:
                    if (!attackPanel.activeInHierarchy) 
                        currentPhase = TURN_PHASE.FORTIFY;
                    break;
                case TURN_PHASE.FORTIFY:
                    if(!attackPanel.activeInHierarchy)
                        AdvanceTurn();
                    break;
            }
            didOnce = false;
        }
    }// end NextTurnButton()
    private void DeselectAllTerritories() {
        foreach (TerritoryNode terry in allTerritories)
        {
            terry.SetCurrentSelection(false);
        }
    }
    public void CloseAttackPanelButton() { attackPanel.SetActive(false); }
    public void OpenAttackPanel() { attackPanel.SetActive(true); }
   
    //Helper functions//
    private void SortIntArrayDesc(int[] ary) {
        System.Array.Sort(ary,
           delegate (int a, int b)
           {
               return b - a;
           });
    }

    public Player GetCurrentPlayer() { return currentPlayers[currentPlayersTurn]; }
}
