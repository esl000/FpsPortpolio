using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FpsLibrary;


namespace FpsLibrary
{
    public class MainGame
    {
        private static MainGame m_pInstance = null;

        private MainGame() { }

        public static MainGame GetInstance()
        {
            if(m_pInstance == null)
            {
                m_pInstance = new MainGame();
            }
            return m_pInstance;
        }

        public Dictionary<Bullet, string> m_mapNames = new Dictionary<Bullet, string>();

        public string m_sPlayerName = "Unknown";

        public bool m_isServer = false;
    }
}
