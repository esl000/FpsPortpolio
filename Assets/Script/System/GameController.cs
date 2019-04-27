using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FpsLibrary.Equipment;
using FpsLibrary.Equipment.Weapon;


namespace FpsLibrary.Networks
{
    public class DataFormat
    {
        public eDataType m_eType;
        public double m_dDeley;
    }

    public class SoundDataFormat : DataFormat
    {
        public eDataType m_eType;
        public double m_dDeley;

        public int m_nAudioClip;
    }

    public class PlayerDataFormat : DataFormat
    {

        public Vector3 m_vDest;
        public Quaternion m_qDest;

        public Vector3 m_vVelocity;

        public Quaternion m_qSpine;

        public bool m_isRifle;

        public bool m_isDead;

        public eRifleWeapons m_eRifle;
        public ePistolWeapons m_ePistol;

        //animation parameter

        public int m_nState;
        public int m_nJump;

        public float m_fFront;
        public float m_fSide;

        public bool m_isGround;
        public bool m_isFire;
        public bool m_isReload;

        public string m_sKiller;
    }

    public enum eDataType
    {
        E_PLAYERDATA = 0,
        E_SOUND = 1
    }

    public class GameController : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_pPlayer;

        [SerializeField]
        private GameObject m_pBullet;

        private Dictionary<NetworkPlayer, PlayerDataFormat> m_mapLastReceiveData = new Dictionary<NetworkPlayer, PlayerDataFormat>();
        private Dictionary<NetworkPlayer, MultiplayerController> m_mapMultiPlayers = new Dictionary<NetworkPlayer, MultiplayerController>();

        private Queue<DataFormat> m_queueData = new Queue<DataFormat>();

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnSerializeNetworkView(BitStream pStream, NetworkMessageInfo pInfo)
        {
            Debug.Log("OK");



            if (pStream.isWriting)
            {
                PlayerDataFormat pPlayerData = new PlayerDataFormat();

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
            }

        }

        public void AddData(DataFormat pFormat)
        {
            m_queueData.Enqueue(pFormat);

        }
    }
}

