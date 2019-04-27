using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using FpsLibrary;

namespace FpsLibrary.Networks
{
    public enum eSceneTag
    {
        E_MAIN = 0,
        E_ROOMNAME = 1,
        E_ROOMPAGE = 2 
    }


    public class NetworkManager : MonoBehaviour
    {
        private const string m_sTypeName = "1stFpsGamePortpolio";

        private bool m_isRefreshingHostList = false;

        private HostData[] m_aHostList = new HostData[0];

        public GameObject m_pCanvas;

        public GameObject[] m_aScenes;

        private GameObject m_pCurrentScene = null;

        public eSceneTag m_eSceneType = eSceneTag.E_MAIN;

        private int m_nRoomPage = 0;

        private bool m_isChange = false;

        public void OnCreateServer()
        {
            ChangeScene(eSceneTag.E_ROOMNAME);
        }

        public void ToMain()
        {
            ChangeScene(eSceneTag.E_MAIN);
        }

        void ChangeScene(eSceneTag eScene)
        {
            m_eSceneType = eScene;
            if(m_pCurrentScene != null)
                m_pCurrentScene.SetActive(false);
            m_pCurrentScene = m_aScenes[(int)eScene];
            m_pCurrentScene.SetActive(true);
        }

        public void OnJoinServer()
        {
            ChangeScene(eSceneTag.E_ROOMPAGE);
        }

        public void OnClose()
        {
            Application.Quit();
        }

        public void OnJoinRoom(int nNum)
        {
            JoinServer(m_aHostList[m_nRoomPage * 5 + nNum]);
        }

        public void OnNextRoomPage()
        {
            if ((m_nRoomPage * 5) + 5 > m_aHostList.Length)
                return;
            ++m_nRoomPage;
        }

        public void OnPrevRoomPage()
        {
            if (m_nRoomPage == 0)
                return;
            --m_nRoomPage;
        }

        public void OnInputRoomName(GameObject pObject)
        {
            Debug.Log("out");
            StartServer(pObject.GetComponent<InputField>().text);
        }

        public void ChangeName(GameObject pObject)
        {
            MainGame.GetInstance().m_sPlayerName = pObject.GetComponent<InputField>().text;
        }

        private void StartServer(string sRoomName)
        {
            Network.InitializeServer(10, 25000, !Network.HavePublicAddress());
            MasterServer.RegisterHost(m_sTypeName, sRoomName);
            MainGame.GetInstance().m_isServer = true; 
        }

        void OnServerInitialized()
        {
            m_isChange = true;
        }

        void GameSceneChange()
        {
            //æ¿ √º¿Œ¡ˆ
            Application.LoadLevel("lost city map");
        }

        void Start()
        {
            ChangeScene(eSceneTag.E_MAIN);
        }

        void Update()
        {
            if (m_isRefreshingHostList)
            {
                m_isRefreshingHostList = false;
                Debug.Log("refresh " + MasterServer.PollHostList().Length);
                m_aHostList = MasterServer.PollHostList();
            }

            if (m_eSceneType == eSceneTag.E_ROOMPAGE)
            {
                RoomController pScript = m_pCurrentScene.GetComponent<RoomController>();

                if (pScript == null)
                    return;

                for(int i = 0; i < 5; ++i)
                {
                    if (m_aHostList.Length <= m_nRoomPage * 5 + i)
                    {
                        pScript.m_aRoomName[i].transform.parent.gameObject.SetActive(false);
                    }
                    else
                    {
                        pScript.m_aRoomName[i].transform.parent.gameObject.SetActive(true);
                        string s = m_nRoomPage * 5 + i + ". " + m_aHostList[m_nRoomPage * 5 + i].gameName + " (" + m_aHostList[m_nRoomPage * 5 + i].connectedPlayers + "/10)";
                        pScript.m_aRoomName[i].text = s;
                    }
                }
            }

            if(m_isChange)
            {
                GameSceneChange();
            }
        }

        public void RefreshHostList()
        {
            if (!m_isRefreshingHostList)
            {
                m_isRefreshingHostList = true;
                MasterServer.RequestHostList(m_sTypeName);
            }
        }


        private void JoinServer(HostData hostData)
        {
            Network.Connect(hostData);
            m_isChange = true;
        }
    }
}
