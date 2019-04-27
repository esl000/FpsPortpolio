using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FpsLibrary;
using FpsLibrary.Networks;
using FpsLibrary.Equipment;
using FpsLibrary.Equipment.Weapon;

public class MultiplayerController : MonoBehaviour {

    [SerializeField]
    private Animator m_pAnimator;

    [SerializeField]
    private GameObject m_pSpine;

    [SerializeField]
    private GameObject m_pRHand;

    [SerializeField]
    private GameObject[] m_pRifles;

    [SerializeField]
    private GameObject[] m_pPistols;

    [SerializeField]
    private eRifleWeapons m_eRifle;

    [SerializeField]
    private ePistolWeapons m_ePistol;

    [SerializeField]
    private bool m_isRifle = true;

    [SerializeField]
    private RuntimeAnimatorController m_pRifleAnimator;

    [SerializeField]
    private RuntimeAnimatorController m_pPistolAnimator;

    [SerializeField]
    private GameObject[] m_aJoints;

    [SerializeField]
    private GameObject[] m_aMeshs;

    [SerializeField]
    private AudioSource m_pAudioSouce;

    [SerializeField]
    private AudioClip[] m_aSounds;

    private NetworkPlayer m_pSender;

    private PlayerDataFormat m_pData = new PlayerDataFormat();

    private SoundDataFormat m_pSoundData;

    private GameObject m_pCurrentWeapon;

    private PlayerDataFormat m_pLastData;

    private Vector3 m_vDest;
    private Quaternion m_qDest;

    private Vector3 m_vVelocity;

    private bool m_isDead = false;

    private float m_fTime = 0f;


    public PlayerDataFormat LastData
    {
        get { return m_pLastData; }
        set { m_pLastData = value; }
    }

    public void SetRendering(bool isRendering)
    {
        if(isRendering)
        {
            for(int i = 0 ; i < m_aMeshs.Length; ++i)
            {
                m_aMeshs[i].layer = 0;
            }
        }
        else
        {
            for(int i = 0 ; i < m_aMeshs.Length; ++i)
            {
                m_aMeshs[i].layer = 8;
            }
        }
    }

    void Awake()
    {
        m_fTime = Time.deltaTime;
    }

	// Use this for initialization
	void Start () {

        foreach (GameObject pObject in m_aJoints)
        {
            if (pObject.GetComponent<BoxCollider>())
                pObject.GetComponent<BoxCollider>().isTrigger = true;

            if (pObject.GetComponent<CapsuleCollider>())
                pObject.GetComponent<CapsuleCollider>().isTrigger = true;

            pObject.GetComponent<Rigidbody>().isKinematic = true;
        }
	}
	
	// Update is called once per frame
	void Update () {

        if(MainGame.GetInstance().m_isServer && m_pSender != null)
        {
            bool isDisConnect = true;

            if (Network.player == m_pSender)
                isDisConnect = false;

            for(int i = 0; i < Network.connections.Length; ++i)
            {
                if(Network.connections[i] == m_pSender)
                {
                    isDisConnect = false;
                }
            }

            if(isDisConnect && Time.time - m_fTime > 2f)
            {
                Network.RemoveRPCs(GetComponent<NetworkView>().viewID);
                Network.Destroy(GetComponent<NetworkView>().viewID);
            }
        }

        if (m_pLastData != null && !m_isDead)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, m_pLastData.m_qDest, 0.2f);

            transform.position = Vector3.Lerp(transform.position, m_vDest, 0.2f);
        }
	}

    void LateUpdate()
    {
        if (m_pLastData != null && !m_isDead)
        {
            Quaternion qRotation1;

            float fXangle = m_pLastData.m_qSpine.eulerAngles.x;

            if (fXangle > 180f)
            {
                fXangle -= 360f;
            }

            qRotation1 = Quaternion.Euler(m_pSpine.transform.localRotation.eulerAngles.x,
                                          m_pSpine.transform.localRotation.eulerAngles.y + fXangle * 0.25f,
                                          m_pSpine.transform.localRotation.eulerAngles.z - fXangle * 0.75f);


            m_pSpine.transform.localRotation = qRotation1;
        }
    }

    public void Synchronization(PlayerDataFormat pLast, PlayerDataFormat pNew)
    {
        m_vDest = m_vVelocity * (float)(pNew.m_dDeley) + pNew.m_vDest;

        m_pAnimator.SetInteger("nState", pNew.m_nState);
        m_pAnimator.SetInteger("nJump", pNew.m_nJump);

        m_pAnimator.SetFloat("fFront", pNew.m_fFront);
        m_pAnimator.SetFloat("fSide", pNew.m_fSide);

        m_pAnimator.SetBool("isGround", pNew.m_isGround);
        m_pAnimator.SetBool("isFire", pNew.m_isFire);
        m_pAnimator.SetBool("isReload", pNew.m_isReload);

        if(m_isRifle && !pNew.m_isRifle)
        {
            ChangeWeapon(eWeaponType.E_PISTOL);
            m_isRifle = false;
        }
        else if(!m_isRifle && pNew.m_isRifle)
        {
            ChangeWeapon(eWeaponType.E_RIFLE);
            m_isRifle = true;
        }

        if(!m_isDead && pNew.m_isDead)
        {
            m_isDead = true;
            Death();
        }
    }

    void Death()
    {
        m_pAnimator.enabled = false;

        foreach (GameObject pObject in m_aJoints)
        {
            if (pObject.GetComponent<BoxCollider>())
                pObject.GetComponent<BoxCollider>().isTrigger = false;

            if (pObject.GetComponent<CapsuleCollider>())
                pObject.GetComponent<CapsuleCollider>().isTrigger = false;

            pObject.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    void ChangeWeapon(eWeaponType eType)
    {
        if(m_pCurrentWeapon != null)
        {
            DestroyObject(m_pCurrentWeapon);
        }

        if(eType == eWeaponType.E_RIFLE)
        {
            m_pAnimator.runtimeAnimatorController = m_pRifleAnimator;

            GameObject pObject = Instantiate(m_pRifles[(int)m_eRifle]);

            Vector3 vPos = pObject.transform.position;
            Quaternion qRot = pObject.transform.rotation;

            pObject.transform.parent = m_pRHand.transform;

            pObject.transform.localPosition = vPos;
            pObject.transform.localRotation = qRot;

            m_pCurrentWeapon = pObject;
        }
        else
        {
            m_pAnimator.runtimeAnimatorController = m_pPistolAnimator;

            GameObject pObject = Instantiate(m_pPistols[(int)m_ePistol]);

            Vector3 vPos = pObject.transform.position;
            Quaternion qRot = pObject.transform.rotation;

            pObject.transform.parent = m_pRHand.transform;

            pObject.transform.localPosition = vPos;
            pObject.transform.localRotation = qRot;

            m_pCurrentWeapon = pObject;
        }
    }

    void OnSerializeNetworkView(BitStream pStream, NetworkMessageInfo pInfo)
    {
        if (pStream.isWriting)
        {
            PlayerDataFormat pPlayerData = m_pData;

            int nType = (int)pPlayerData.m_eType;

            int nRifleType = (int)pPlayerData.m_eRifle;
            int nPistolType = (int)pPlayerData.m_ePistol;

            pStream.Serialize(ref nType);

            pStream.Serialize(ref pPlayerData.m_vDest);
            pStream.Serialize(ref pPlayerData.m_qDest);

            pStream.Serialize(ref pPlayerData.m_vVelocity);

            pStream.Serialize(ref pPlayerData.m_qSpine);

            pStream.Serialize(ref nRifleType);
            pStream.Serialize(ref nPistolType);

            pStream.Serialize(ref pPlayerData.m_isRifle);

            pStream.Serialize(ref pPlayerData.m_isDead);

            pStream.Serialize(ref pPlayerData.m_nState);
            pStream.Serialize(ref pPlayerData.m_nJump);

            pStream.Serialize(ref pPlayerData.m_fFront);
            pStream.Serialize(ref pPlayerData.m_fSide);

            pStream.Serialize(ref pPlayerData.m_isGround);
            pStream.Serialize(ref pPlayerData.m_isFire);
            pStream.Serialize(ref pPlayerData.m_isReload);

        }
        else
        {
            m_pSender = pInfo.sender;

            int nType = 0;
            pStream.Serialize(ref nType);

            double dDeley = Network.time - pInfo.timestamp;

            Vector3 vDest = new Vector3();

            Quaternion qDest = new Quaternion();

            Vector3 vVelocity = new Vector3();

            Quaternion qSpine = new Quaternion();

            int nRifleType = 0;
            int nPistolType = 0;

            bool isRifle = true;
            bool isDead = false;

            int nState = 0;
            int nJump = 0;

            float fFront = 0f;
            float fSide = 0f;

            bool isGround = true;
            bool isFire = false;
            bool isReload = false;

            pStream.Serialize(ref vDest);
            pStream.Serialize(ref qDest);

            pStream.Serialize(ref vVelocity);

            pStream.Serialize(ref qSpine);

            pStream.Serialize(ref nRifleType);
            pStream.Serialize(ref nPistolType);

            pStream.Serialize(ref isRifle);

            pStream.Serialize(ref isDead);

            pStream.Serialize(ref nState);
            pStream.Serialize(ref nJump);

            pStream.Serialize(ref fFront);
            pStream.Serialize(ref fSide);

            pStream.Serialize(ref isGround);
            pStream.Serialize(ref isFire);
            pStream.Serialize(ref isReload);

            PlayerDataFormat pPlayerData = new PlayerDataFormat();

            pPlayerData.m_dDeley = dDeley;

            pPlayerData.m_vDest = vDest;
            pPlayerData.m_qDest = qDest;

            pPlayerData.m_vVelocity = vVelocity;

            pPlayerData.m_qSpine = qSpine;

            pPlayerData.m_eRifle = (eRifleWeapons)nRifleType;
            pPlayerData.m_ePistol = (ePistolWeapons)nPistolType;

            pPlayerData.m_isRifle = isRifle;
            pPlayerData.m_isDead = isDead;

            pPlayerData.m_nState = nState;
            pPlayerData.m_nJump = nJump;

            pPlayerData.m_fFront = fFront;
            pPlayerData.m_fSide = fSide;

            pPlayerData.m_isGround = isGround;
            pPlayerData.m_isFire = isFire;
            pPlayerData.m_isReload = isReload;

            if (m_pLastData != null)
            {
                Synchronization(m_pLastData, pPlayerData);
            }
            else
            {
                m_eRifle = pPlayerData.m_eRifle;
                m_ePistol = pPlayerData.m_ePistol;
                ChangeWeapon(eWeaponType.E_RIFLE);
            }

            m_pLastData = pPlayerData;
            m_pData = pPlayerData;
            
        }
    }

    void PlaySound(int nIndex)
    {
        m_pAudioSouce.clip = m_aSounds[nIndex];
        m_pAudioSouce.PlayOneShot(m_pAudioSouce.clip);
    }

    public void AddData(PlayerDataFormat pFormat)
    {
        m_pData = pFormat;

    }

    public void AddSoundData(SoundDataFormat pFormat)
    {
        m_pSoundData = pFormat;

    }

}
