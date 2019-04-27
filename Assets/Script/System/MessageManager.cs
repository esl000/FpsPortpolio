using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FpsLibrary;

public enum eMessageType
{
    E_DEATH,
    E_JOIN,
    E_QUIT
}

public class MessageData
{
    public eMessageType m_eType;
}

public class DeathMessage : MessageData
{
    public string m_sDead;
    public string m_sKill;
}

public class MessageManager : MonoBehaviour {

    public MessageViewer m_pManager;

    private NetworkPlayer m_pSender;

    private float m_fTime = 0f;


    void Awake()
    {
        m_fTime = Time.deltaTime;
    }

	// Use this for initialization
	void Start () {
     
	}
	
	// Update is called once per frame
	void Update () {

        if (m_pManager == null)
        {
            m_pManager = GameObject.Find("MessageView").GetComponent<MessageViewer>();
        }

        if (MainGame.GetInstance().m_isServer && m_pSender != null)
        {
            bool isDisConnect = true;

            if (Network.player == m_pSender)
                isDisConnect = false;

            for (int i = 0; i < Network.connections.Length; ++i)
            {
                if (Network.connections[i] == m_pSender)
                {
                    isDisConnect = false;
                }
            }

            if (isDisConnect && Time.time - m_fTime > 2f)
            {
                Network.RemoveRPCs(GetComponent<NetworkView>().viewID);
                Network.Destroy(GetComponent<NetworkView>().viewID);
            }
        }

	}

    public void SendDeathMessage(string sDeath, string sKiller)
    {
        RPCDeathMessage(sDeath, sKiller);
    }

    void OnSerializeNetworkView(BitStream pStream, NetworkMessageInfo pInfo)
    {
        if(pStream.isWriting)
        {

        }
        else
        {
            m_pSender = pInfo.sender;
        }
    }

    [RPC] void RPCDeathMessage(string sDeath, string sKiller)
    {

        DeathMessage pDM = new DeathMessage();

        pDM.m_eType = eMessageType.E_DEATH;
        pDM.m_sDead = sDeath;
        pDM.m_sKill = sKiller;

        if (m_pManager.m_queueDM.Count == 5)
        {
            m_pManager.m_queueDM.Dequeue();
        }

        m_pManager.m_queueDM.Enqueue(pDM);

        if (GetComponent<NetworkView>().isMine)
        {
            GetComponent<NetworkView>().RPC("RPCDeathMessage", RPCMode.OthersBuffered, sDeath, sKiller);
        }
    }
}
