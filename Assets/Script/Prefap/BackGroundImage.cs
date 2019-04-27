using UnityEngine;
using System.Collections;

public class BackGroundImage : MonoBehaviour {

	// Use this for initialization
	void Start () {
        float fWidthScale = Screen.width / 100f;
        float fHeightScale = Screen.height / 100f;
        GetComponent<RectTransform>().localScale = new Vector3(fWidthScale, fHeightScale, 1f);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
