using UnityEngine;
using System.Collections;

public class GunEffect : MonoBehaviour {

    [SerializeField]
    private GameObject m_pEffect;

    private GameObject m_pCurEffect = null;

    private float m_fRemainTime = 0.2f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        m_fRemainTime -= Time.deltaTime;

        if(m_fRemainTime < 0f)
        {
            DestroyObject(m_pCurEffect);
        }
	}

    public void InitEffect(float fPlayTime)
    {
        if(m_pCurEffect)
        {
            DestroyObject(m_pCurEffect);
        }

        m_pCurEffect = Instantiate(m_pEffect);

        m_pCurEffect.transform.parent = transform;
        m_pCurEffect.transform.localPosition = new Vector3();
        m_pCurEffect.transform.localRotation = new Quaternion();

        m_fRemainTime = fPlayTime;

    }
}
