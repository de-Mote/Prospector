using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card : MonoBehaviour {

	public string    suit;
	public int       rank;
	public Color     color = Color.black;
	public string    colS = "Black";

	public List<GameObject> decoGOs = new List<GameObject>();
	public List<GameObject> pipGOs = new List<GameObject>();

	public GameObject back;  
	public CardDefinition def;	

	public SpriteRenderer[] spriteRenderes;

	void Start(){
		SetSortingOrder (0);
	}

	public void PopulateSpriteRenderes(){
		if (spriteRenderes == null || spriteRenderes.Length == 0) {
			spriteRenderes = GetComponentsInChildren<SpriteRenderer>();
		}
	}

	public void SetSortingLayerName(string tSLN){
		PopulateSpriteRenderes ();

		foreach(SpriteRenderer tSR in spriteRenderes){
			tSR.sortingLayerName = tSLN;
		}
	}

	public void SetSortingOrder(int sOrd){
		PopulateSpriteRenderes();
		foreach(SpriteRenderer tSR in spriteRenderes){
			if(tSR.gameObject == this.gameObject){
				tSR.sortingOrder = sOrd;
				continue;
			}
			switch(tSR.gameObject.name){
			case "back":
				tSR.sortingOrder = sOrd+2;
				break;
			case"face":
			default:
				tSR.sortingOrder=sOrd+1;
				break;
			}
		}
	}
	virtual public void OnMouseUpAsButton(){
		Debug.Log (name);
	}

	public bool faceUp{
		get{
			return(!back.activeSelf);
		}
		set {
			back.SetActive(!value);
		}
	}

} // class Card

[System.Serializable]
public class Decorator{
	public string	type;
	public Vector3	loc;	
	public bool		flip = false;	
	public float 	scale = 1.0f;
}

[System.Serializable]
public class CardDefinition{
	public string	face;
	public int		rank;
	public List<Decorator>	
	pips = new List<Decorator>();
}
