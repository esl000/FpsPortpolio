using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using FpsLibrary.Controll;
using FpsLibrary.Equipment;
using FpsLibrary.Equipment.Weapon;

public class GameManager : MonoBehaviour {

    [SerializeField]
    private GameObject m_pGameGui;

    [SerializeField]
    private Text m_pHp;

    [SerializeField]
    private Text m_pBullet;

    [SerializeField]
    private GameObject m_pMenuGui;

    [SerializeField]
    private GameObject m_pCharacter;

    [SerializeField]
    private GameObject m_pCurrentcharacter;

    [SerializeField]
    private Vector3 m_vSpawnZone;

    [SerializeField]
    private float m_fRadius;

    [SerializeField]
    private float m_fRespawnTime = 5f;

    [SerializeField]
    private GameObject m_pMouseController;

    [SerializeField]
    private GameObject m_pMessageSender;

    public GameObject m_pCurrentMSender;

    private bool m_isGame = true;
    private bool m_isToMain = false;
    private bool m_isDead = false;

    private float m_fCurrentTime = 0f;

	// Use this for initialization
	void Start () {
        RespawnPlayer();
	}

    void RespawnPlayer()
    {
        Vector3 vStart = new Vector3(m_vSpawnZone.x - m_fRadius / 2f + Random.Range(0, m_fRadius),
            m_vSpawnZone.y,
            m_vSpawnZone.z - m_fRadius / 2f + Random.Range(0, m_fRadius));

        m_pCurrentcharacter = (GameObject)Instantiate(m_pCharacter, vStart, new Quaternion());
        m_pCurrentcharacter.GetComponent<CharacterControll>().Manager = this;
    }
	
	// Update is called once per frame
	void Update () {

        if ((Network.isClient || Network.isServer) && m_pCurrentMSender == null)
        {
            m_pCurrentMSender = (GameObject)Network.Instantiate(m_pMessageSender, transform.position, transform.rotation, 0);
        }

        if(m_isDead)
        {
            m_fCurrentTime += Time.deltaTime;
            if(m_fCurrentTime >= m_fRespawnTime)
            {
                m_fCurrentTime = 0f;
                m_isDead = false;
                DestroyObject(m_pCurrentcharacter);
                RespawnPlayer();
            }
        }

        if (m_isGame)
        {
            float fHp = m_pCurrentcharacter.GetComponent<CharacterControll>().Hp;
            int nBullet = m_pCurrentcharacter.GetComponent<CharacterControll>().Equip.m_pCurrentEquipment.m_nCurBullet;
            int nMagazine = m_pCurrentcharacter.GetComponent<CharacterControll>().Equip.m_pCurrentEquipment.m_nCurMagazine;

            m_pHp.text = fHp.ToString();

            m_pBullet.text = nBullet + "     " + nMagazine + "/" + 5;
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (m_isGame)
            {
                m_isGame = false;
                m_pGameGui.SetActive(false);
                m_pMenuGui.SetActive(true);
                m_pCurrentcharacter.GetComponent<CharacterControll>().enabled = false;
                m_pMouseController.GetComponent<CursorLock>().m_isLock = false;
            }
            else
            {
                m_isGame = true;
                m_pGameGui.SetActive(true);
                m_pMenuGui.SetActive(false);
                m_pCurrentcharacter.GetComponent<CharacterControll>().enabled = true;
                m_pMouseController.GetComponent<CursorLock>().m_isLock = true;
            }
        }

        if (m_isToMain)
        {
            Network.RemoveRPCs(m_pCurrentcharacter.GetComponent<CharacterControll>().m_pMultiplayer.GetComponent<NetworkView>().viewID);
            Network.Destroy(m_pCurrentcharacter.GetComponent<CharacterControll>().m_pMultiplayer.GetComponent<NetworkView>().viewID);
            Network.RemoveRPCs(Network.player, 0);
            Network.DestroyPlayerObjects(Network.player);
            Network.Disconnect();
            Application.LoadLevel(0);
        }
        
	}

    public void PlayerDeath()
    {
        m_isDead = true;
    }

    public void ToMain()
    {
        m_isToMain = true;
    }

    public void BackGame()
    {
        m_isGame = true;
        m_pGameGui.SetActive(true);
        m_pMenuGui.SetActive(false);
        m_pCurrentcharacter.GetComponent<CharacterControll>().enabled = true;
        m_pMouseController.GetComponent<CursorLock>().m_isLock = true;
    }


    void OnDestroy()
    {
        Network.RemoveRPCs(Network.player, 0);
        Network.DestroyPlayerObjects(Network.player);
        Network.Disconnect();
    }

}
