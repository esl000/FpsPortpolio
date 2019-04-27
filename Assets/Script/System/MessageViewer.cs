using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageViewer : MonoBehaviour {

    public GameObject[] m_pMessageUI;

    public Queue<DeathMessage> m_queueDM = new Queue<DeathMessage>();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        for (int i = m_queueDM.Count; i < 5; ++i)
        {
            m_pMessageUI[i].SetActive(false);
        }


        int k = 0;
        foreach (DeathMessage pMessage in m_queueDM)
        {
            m_pMessageUI[k].SetActive(true);
            m_pMessageUI[k].GetComponent<DMessage>().m_pDead.text = pMessage.m_sDead;
            m_pMessageUI[k].GetComponent<DMessage>().m_pKiller.text = pMessage.m_sKill;
            k++;
        }
	}
}
