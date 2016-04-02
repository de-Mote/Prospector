using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoreBoard : MonoBehaviour {
	public static ScoreBoard S;
	public GameObject prefabFloatingScore;

	[SerializeField]
	private int _score = 0;
	public string _scoreString;

	public int score{
		get{
			return(_score);
		}
		set{
			_score = value;
			_scoreString = Utils.AddCommasToNumber(_score);
			GetComponent<GUIText>().text = _scoreString;
		}
	}
	public string scoreString{
		get{
			return _scoreString;
		}
		set{
			_scoreString = value;
			GetComponent<GUIText>().text = _scoreString;
		}
	}
	void Awake(){
		S = this;
	}
	public void FSCallback(FloatingScore fs){
		score += fs.score;
	}
	public FloatingScore CreateFloatingScore(int amt, List<Vector3> pts){
		GameObject go = Instantiate (prefabFloatingScore) as GameObject;
		FloatingScore fs = go.GetComponent<FloatingScore> ();
		fs.score = amt;
		fs.reportFinishTo = this.gameObject;
		fs.Init (pts);
		return(fs);
	}
}
