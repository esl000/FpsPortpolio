using System;
using UnityEngine;
using System.Collections;
using FpsLibrary.Equipment.Weapon;
 
namespace FpsLibrary.Equipment
{

    [Serializable]
    public class Equip
    {
        public GameObject m_p3rdObject;
        public GameObject m_p1stObject;
    }

    [Serializable]
    public abstract class Equipment
    {
        public Equip m_pObject;
        public Animator m_pAnimator;

        public eWeaponType m_eWeaponType;

        public int m_nMaxMagazine;
        public int m_nCurMagazine;

        public int m_nMaxBullet;
        public int m_nCurBullet;

        public float m_fDamage;

        public float m_fShootSpeed;
        public float m_fCurTime = 0f;

        public void Init(eWeaponType eType, Equip pObject, Transform pCameraParent, Transform pHandParent)
        {
            m_eWeaponType = eType;
            m_pObject = pObject;

            Vector3 v1stPos = m_pObject.m_p1stObject.transform.position;
            Quaternion q1stRot = m_pObject.m_p1stObject.transform.rotation;

            Vector3 v3rdPos = m_pObject.m_p3rdObject.transform.position;
            Quaternion q3rdRot = m_pObject.m_p3rdObject.transform.rotation;

            m_pObject.m_p1stObject.transform.parent = pCameraParent;
            m_pObject.m_p3rdObject.transform.parent = pHandParent;

            m_pObject.m_p1stObject.transform.localPosition = v1stPos;
            m_pObject.m_p1stObject.transform.localRotation = q1stRot;

            m_pObject.m_p3rdObject.transform.localPosition = v3rdPos;
            m_pObject.m_p3rdObject.transform.localRotation = q3rdRot;

            m_pAnimator = m_pObject.m_p1stObject.GetComponent<Animator>();
        }


        public bool Fire(out Vector3 vPoint)
        {
            if (m_fCurTime < 0f)
            {
                m_fCurTime = m_fShootSpeed;
                --m_nCurBullet;
                Debug.Log("cur bullet : " + m_nCurBullet);

                m_pObject.m_p1stObject.GetComponent<PointFinder>().m_pPoint.GetComponent<GunEffect>().InitEffect(0.1f);
                vPoint = m_pObject.m_p1stObject.GetComponent<PointFinder>().m_pPoint.transform.position;
                m_pObject.m_p3rdObject.transform.Find("point").gameObject.GetComponent<GunEffect>().InitEffect(0.1f);

                return true;
            }
            vPoint = new Vector3();
            return false;
        }

        public bool AllowFire()
        {
            if (m_nCurBullet == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool AllowReload()
        {
            if (m_nCurMagazine == 0)
                return false;
            else
                return true;
        }

        public void Reload()
        {
            --m_nCurMagazine;
            m_nCurBullet = m_nMaxBullet;
        }

        public void FrameUpdate()
        {
            if (m_fCurTime >= 0f)
                m_fCurTime -= Time.deltaTime;
        }
        
    }
}

namespace FpsLibrary.Equipment.Weapon
{
    public enum eWeaponType
    {
        E_RIFLE = 0,
        E_PISTOL = 1
    }

    public enum eRifleWeapons
    {
        E_AK47 = 0
    }

    public enum ePistolWeapons
    {
        E_EAGLE = 0
    }

    [Serializable]
    public class Rifle : Equipment
    {
        public eRifleWeapons m_eRifleType;
    }

    [Serializable]
    public class Pistol : Equipment
    {
        public ePistolWeapons m_ePistolType;
    }


    [Serializable]
    public class AK_47Rifle : Rifle
    {
        public AK_47Rifle()
        {
            m_fDamage = 65f;

            m_fShootSpeed = 0.167f;

            m_eRifleType = eRifleWeapons.E_AK47;

            m_nMaxMagazine = 5;
            m_nCurMagazine = 5;

            m_nMaxBullet = 30;
            m_nCurBullet = 30;
        }
    }

    [Serializable]
    public class Eagle : Pistol
    {
        public Eagle()
        {
            m_fDamage = 50f;

            m_fShootSpeed = 0.333f;

            m_ePistolType = ePistolWeapons.E_EAGLE;

            m_nMaxMagazine = 5;
            m_nCurMagazine = 5;

            m_nMaxBullet = 7;
            m_nCurBullet = 7;
        }
    }

}
