using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ScoreEvent{
	draw,
	mine,
	mineGold,
	gameWin,
	gameLoss
}

public class Prospector : MonoBehaviour {

	static public Prospector 	S;
	static public int SCORE_FROM_PREV_ROUND = 0;
	static public int HIGH_SCORE = 0;

	public float reloadDelay = 1f;

	public Vector3 fsPosMid = new Vector3(0.5f,0.9f,0);
	public Vector3 fsPosRun = new Vector3 (0.5f,.75f,0);
	public Vector3 fsPosMid2 = new Vector3 (.5f, .5f, 0);
	public Vector3 fsPosEnd = new Vector3(1f,.65f,0);

	public Deck					deck;
	public TextAsset			deckXML;
	public Vector3 				layoutCenter;
	public float 				xOffset = 3;
	public float 				yOffset = -2.5f;
	public Transform			layoutAnchor;

	public CardProspector target;
	public List<CardProspector> tableau;
	public List<CardProspector> discardPile;

	public Layout layout;
	public TextAsset layoutXML;

	public List<CardProspector> drawPile;

	public int chain=0;
	public int scoreRun=0;
	public int score=0;
	public FloatingScore fsRun;

	public GUIText GTGameOver;
	public GUIText GTRoundResult;

	void Awake(){
		S = this;
		if(PlayerPrefs.HasKey("ProspectorHighScore")){
			HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
		}
		score += SCORE_FROM_PREV_ROUND;
		SCORE_FROM_PREV_ROUND = 0;

		GameObject go = GameObject.Find ("GameOver");
		if(go != null){
			GTGameOver = go.GetComponent<GUIText>();
		}
		go = GameObject.Find ("RoundResult");
		if(go != null){
			GTRoundResult=  go.GetComponent<GUIText>();
		}
		ShowResultsGTs (false);
		go = GameObject.Find ("HighScore");
		string hScore = "High Score: "+Utils.AddCommasToNumber(HIGH_SCORE);
		go.GetComponent<GUIText> ().text = hScore;
	}
	void ShowResultsGTs(bool show){
		GTGameOver.gameObject.SetActive (show);
		GTRoundResult.gameObject.SetActive (show);
	}

	void Start() {
		ScoreBoard.S.score = score;
		deck = GetComponent<Deck> ();
		deck.InitDeck (deckXML.text);
		deck.shuffle (ref deck.cards);

		layout = GetComponent<Layout> ();
		layout.ReadLayout (layoutXML.text);
		drawPile = ConvertListCardsToListCardPorspectors (deck.cards);

		LayoutGame ();
	}

	CardProspector Draw(){
		CardProspector cd = drawPile [0];
		drawPile.RemoveAt (0);
		return(cd);
	}

	CardProspector FindCardByLayoutID(int layoutID){
		foreach(CardProspector tCP in tableau){
			if(tCP.layoutID == layoutID){
				return(tCP);
			}
		}
		return null;
	}

	void LayoutGame(){
		if(layoutAnchor == null){
			GameObject tGO = new GameObject ("_LayoutAnchor");
			layoutAnchor = tGO.transform;
			layoutAnchor.transform.position = layoutCenter;
		}

		CardProspector cp;

		foreach(SlotDef tSD in layout.slotDefs){
			cp = Draw();

			cp.faceUp = tSD.faceUp;
			cp.transform.parent=layoutAnchor;

			cp.transform.localPosition = new Vector3(
				layout.multipler.x * tSD.x,
				layout.multipler.y * tSD.y,
				-tSD.layerID);

			cp.layoutID = tSD.id;
			cp.slotDef = tSD;
			cp.state = CardState.tableau;
			cp.SetSortingLayerName(tSD.layerName);
			tableau.Add(cp);
		}

		foreach (CardProspector tCP in tableau) {
			foreach(int hid in tCP.slotDef.hiddenBy){
				cp= FindCardByLayoutID(hid);
				tCP.hiddenBy.Add(cp);
			}
		}

		MoveToTarget (Draw ());
		UpdateDrawPile ();
	}

	public void CardClicked(CardProspector cd){
		switch (cd.state) {
		case CardState.target:
			break;
		case CardState.drawpile:
			MoveToDiscard(target);
			MoveToTarget(Draw ());
			UpdateDrawPile();
			ScoreManager(ScoreEvent.draw);
			break;
		case CardState.tableau:
			bool  validMatch=true;
			if(!cd.faceUp){
				validMatch = false;
			}
			if(!AdjacentRank(cd, target)){
				validMatch = false;
			}
			if(!validMatch){
				return;
			}
			tableau.Remove(cd);
			MoveToTarget(cd);
			SetTableauFaces();
			ScoreManager(ScoreEvent.mine);
			break;
		}
		CheckForGameOver ();
	}
	void ScoreManager(ScoreEvent sEvt){
		List<Vector3> fsPts;
		switch (sEvt) {
		case ScoreEvent.draw:
		case ScoreEvent.gameWin:
		case ScoreEvent.gameLoss:
			GTGameOver.text = "Game Over";
			chain=0;
			score+=scoreRun;
			scoreRun = 0; 
			if(fsRun != null){
				fsPts = new List<Vector3>();
				fsPts.Add(fsPosRun);
				fsPts.Add(fsPosMid2);
				fsPts.Add(fsPosEnd);
				fsRun.reportFinishTo = ScoreBoard.S.gameObject;
				fsRun.Init(fsPts,0,1);
				fsRun.fontSizes = new List<float>(new float[] {28,36,4});
				fsRun = null;
			}
			break;
		case ScoreEvent.mine:
			chain++;
			scoreRun += chain;
			FloatingScore fs;
			Vector3 pO = Input.mousePosition;
			pO.x /= Screen.width;
			pO.y /= Screen.height;
			fsPts = new List<Vector3>();
			fsPts.Add(pO);
			fsPts.Add(fsPosMid);
			fsPts.Add(fsPosRun);
			fs = ScoreBoard.S.CreateFloatingScore(chain,fsPts);
			fs.fontSizes = new List<float>(new float[] {4,50,28});
			if(fsRun == null){
				fsRun = fs;
				fsRun.reportFinishTo = null;
			}
			else{
				fs.reportFinishTo = fsRun.gameObject;
			}
			break;
		}
		switch (sEvt) {
		case ScoreEvent.gameWin:
			Prospector.SCORE_FROM_PREV_ROUND = score;
			GTGameOver.text= "Round Over";
			Prospector.SCORE_FROM_PREV_ROUND = score;
			GTRoundResult.text = "You win this round!\nRound Score: "+score;
			ShowResultsGTs(true);
			break;
		case ScoreEvent.gameLoss:
			GTGameOver.text="Game Over";
			if(Prospector.HIGH_SCORE <= score){
				print("New High Score");
				Prospector.HIGH_SCORE=score;
				string sRR = "You got the high score!\nHigh score: "+score;
				GTRoundResult.text= sRR;
				PlayerPrefs.SetInt("ProspectorHighScore",score);
			}
			else{
				print ("Your final score was"+score);
				GTRoundResult.text = "Your final score was: "+score;
			}
			ShowResultsGTs(true);
			break;
		default:
			print("score"+score+" scoreRun"+scoreRun+" chain"+chain);
			break;
		}
	}

	void CheckForGameOver(){
		if (tableau.Count == 0) {
			GameOver(true);
			return;
		}
		if (drawPile.Count > 0) {
			return;
		}
		foreach (CardProspector cd in tableau) {
			if(AdjacentRank(cd,target)){
				return;
			}
		}
		GameOver(false);
	}

	void GameOver(bool won){
		if (won == true) {
			ScoreManager(ScoreEvent.gameWin);
		} else {
			ScoreManager(ScoreEvent.gameLoss);
		}
		Invoke("reloadLevel",reloadDelay);
	}
	void ReloadLevel(){
		Application.LoadLevel (Application.loadedLevelName);
	}

	void SetTableauFaces(){
		foreach(CardProspector cd in tableau){
			bool fup = true;
			foreach(CardProspector cover in cd.hiddenBy){
				if(cover.state==CardState.tableau){
					fup = false;
				}
			}
			cd.faceUp=fup;
		}
	}

	bool AdjacentRank(CardProspector c0, CardProspector c1){
		if (!c0.faceUp || !c1.faceUp) {
			return false;
		}
		if(Mathf.Abs(c0.rank-c1.rank) == 1){
			return true;
		}
		if(c0.rank == 1 && c1.rank == 13){
			return true;
		}
		if(c0.rank == 13 && c1.rank == 1){
			return true;
		}
		return false;
	}

	void MoveToDiscard(CardProspector cd){
		cd.state = CardState.discard;
		discardPile.Add (cd);
		cd.transform.parent = layoutAnchor;
		cd.transform.localPosition = new Vector3 (
			layout.multipler.x*layout.discardPile.x,
			layout.multipler.y*layout.discardPile.y,
			-layout.discardPile.layerID + .5f);
		cd.faceUp = true;
		cd.SetSortingLayerName (layout.discardPile.layerName);
		cd.SetSortingOrder (-100 + discardPile.Count);
	}
	void MoveToTarget(CardProspector cd){
		if (target != null) {
			MoveToDiscard(target);
		}
		target = cd;
		cd.state = CardState.target;
		cd.transform.parent= layoutAnchor;
		cd.transform.localPosition = new Vector3 (
			layout.multipler.x * layout.discardPile.x,
			layout.multipler.y * layout.discardPile.y,
			-layout.discardPile.layerID);
		cd.faceUp = true;
		cd.SetSortingLayerName (layout.discardPile.layerName);
		cd.SetSortingOrder (0);
	}
	void UpdateDrawPile(){
		CardProspector cd;
		for (int i=0; i< drawPile.Count; i++) {
			cd =drawPile[i];
			cd.transform.parent=layoutAnchor;
			Vector2 dpStagger=layout.drawPile.stagger;

			cd.transform.localPosition = new Vector3(
				layout.multipler.x * ( layout.drawPile.x + i * dpStagger.x),
				layout.multipler.y * ( layout.drawPile.y + i * dpStagger.y),
				-layout.drawPile.layerID+0.1f*i);
			cd.faceUp =false;
			cd.state = CardState.drawpile;
			cd.SetSortingLayerName(layout.drawPile.layerName);
			cd.SetSortingOrder(-10*i);

		}
	}

	List<CardProspector> ConvertListCardsToListCardPorspectors(List<Card> lCD){

		List<CardProspector> lCP = new List<CardProspector>();
		CardProspector tCP;
		foreach(Card tCD in lCD){
			tCP = tCD as CardProspector;
			lCP.Add(tCP);
		}
		return(lCP);
	}
}
