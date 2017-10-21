using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiskDie : Die {
    public enum DieType {ATTACKER, DEFENDER };
    [SerializeField]
    private DieType dieType = DieType.ATTACKER;
    private bool canFight = true;
    private SpriteRenderer sprRend;
	// Use this for initialization
	void Awake () {
        sprRend = gameObject.GetComponent<SpriteRenderer>();
        if(dieType == DieType.ATTACKER)
        {
            sprRend.color = Color.red;
        } else
        {
            sprRend.color = Color.white;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public bool GetCanFight() { return canFight; }
    public void SetCanFight(bool fight) { canFight = fight; }
    public DieType GetDieType() { return dieType; }
    public void SetDieType(DieType die)
    {
        dieType = die;
        if (dieType == DieType.ATTACKER)
            sprRend.color = Color.red;
        else
            sprRend.color = Color.white;
    }
    


}
