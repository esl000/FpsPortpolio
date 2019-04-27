using UnityEngine;
using System.Collections;

public class EffectController : MonoBehaviour {

    public float m_fRemainTime;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        m_fRemainTime -= Time.deltaTime;

        if (m_fRemainTime <= 0f)
            DestroyObject(gameObject);
	}
}
