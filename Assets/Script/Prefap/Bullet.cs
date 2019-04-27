using UnityEngine;
using System.Collections;
using FpsLibrary;
using FpsLibrary.Controll;

public class Bullet : MonoBehaviour {

    public GameObject m_pBlood;
    public GameObject m_pGunSound;
    public GameObject m_pFlare;
    public bool m_isMine = false;

    public float m_fDamage;

    public string m_sShooter;

    private Vector3 m_vDest = new Vector3();

    private float m_fTime = 0f;
    private float m_fDestroyTime = 0f;
    private float m_fShowTime = 7f;


    private bool m_isFrameCheck = false;

    private bool m_isNetworkDestroy = false;


    public void Init()
    {
        GetComponent<Rigidbody>().AddForce(transform.forward * 60f);
    }

    void Awake()
    {
        m_fTime = Time.time;
    }

	// Use this for initialization
	void Start () {
        Init();
        Instantiate(m_pGunSound, transform.position, transform.rotation);
	}

    void FixedUpdate()
    {
        //GetComponent<Rigidbody>().MovePosition(Vector3.Lerp(GetComponent<Rigidbody>().position, m_vDest, 0.2f));
    }
	
	// Update is called once per frame
	void Update () {

        if(m_isFrameCheck)
        {
            m_isNetworkDestroy = true;
        }

        m_fShowTime -= Time.deltaTime;

        if (m_isNetworkDestroy)
        {
            m_fDestroyTime += Time.deltaTime;
        }

        if (m_fShowTime <= 0f && m_isMine)
        {
            Network.RemoveRPCs(GetComponent<NetworkView>().viewID);
            Network.Destroy(GetComponent<NetworkView>().viewID);
        }

        if(Time.time - m_fTime > 2f && m_fDestroyTime > 2f && m_isNetworkDestroy)
        {
            Network.RemoveRPCs(GetComponent<NetworkView>().viewID);
            Network.Destroy(GetComponent<NetworkView>().viewID);
        }
	}

    void OnTriggerEnter(Collider pObject)
    {
        if (!m_isNetworkDestroy && GetComponent<CapsuleCollider>().enabled)
        {
            if (pObject.gameObject.tag.Equals("MapCollider"))
                return;

            if (pObject.gameObject.tag.Equals("MultiCollider"))
            {
                Instantiate(m_pBlood, pObject.transform.position, transform.rotation);
            }
            else if (pObject.gameObject.tag.Equals("CharCollider"))
            {
                Instantiate(m_pBlood, pObject.transform.position, transform.rotation);
                m_isFrameCheck = true;

                string sName;
                MainGame.GetInstance().m_mapNames.TryGetValue(this, out sName);

                if (pObject.gameObject.GetComponentInParent<CharacterControll>().enabled)
                    pObject.gameObject.GetComponentInParent<CharacterControll>().Hit(m_fDamage, sName);
            }
            else
            {
                if(!GetComponent<NetworkView>().isMine)
                    m_isFrameCheck = true;
                Instantiate(m_pFlare, transform.position, transform.rotation);
            }

            GetComponent<CapsuleCollider>().enabled = false;
        }
    }

    void OnSerializeNetworkView(BitStream pStream, NetworkMessageInfo pInfo)
    {
        if(pStream.isWriting)
        {
            //Vector3 vPos = GetComponent<Rigidbody>().position;
            //Vector3 vVelocity = GetComponent<Rigidbody>().velocity;

            //pStream.Serialize(ref vPos);

            //pStream.Serialize(ref vVelocity);

            pStream.Serialize(ref m_fDamage);
        }
        else
        {
            //Vector3 vPos = new Vector3();
            //Vector3 vVelocity = new Vector3();

            //pStream.Serialize(ref vPos);
            //pStream.Serialize(ref vVelocity);

            //m_vDest = vPos + vVelocity * (float)(Network.time - pInfo.timestamp);

            pStream.Serialize(ref m_fDamage);

        }
    }

    public void SendName()
    {
        RPCSendName(MainGame.GetInstance().m_sPlayerName);
    }

    [RPC] void RPCSendName(string sShooter)
    {
        m_sShooter = sShooter;
        MainGame.GetInstance().m_mapNames.Add(this, m_sShooter);

        if (GetComponent<NetworkView>().isMine)
            GetComponent<NetworkView>().RPC("RPCSendName", RPCMode.OthersBuffered, sShooter);
    }

    void OnDestroy()
    {
        MainGame.GetInstance().m_mapNames.Remove(this);
    }

}
