using UnityEngine;
using System.Collections;
using FpsLibrary.Equipment;
using FpsLibrary.Equipment.Weapon;

namespace FpsLibrary.Equipment
{
    public class EquipmentManager : MonoBehaviour
    {
        public Equipment m_pCurrentEquipment = new Rifle();
        public Equipment m_pRifle;
        public Equipment m_pPistol;

        public void Init(eRifleWeapons eRifle, ePistolWeapons eKnife)
        {
            m_pRifle = new AK_47Rifle();
            m_pPistol = new Eagle();
        }

        public void Update()
        {
            m_pRifle.FrameUpdate();
            m_pPistol.FrameUpdate();
        }

        public void ChangeWeapon(eWeaponType eType, Equip pObject, Transform pCameraParent, Transform pHandParent)
        {

            Equip pEquip = new Equip();

            pEquip.m_p1stObject = Instantiate(pObject.m_p1stObject);
            pEquip.m_p3rdObject = Instantiate(pObject.m_p3rdObject);

            if (eType == eWeaponType.E_RIFLE)
            {
                m_pCurrentEquipment = m_pRifle;
                m_pRifle.Init(eWeaponType.E_RIFLE, pEquip, pCameraParent, pHandParent);
                if (m_pPistol.m_pObject != null)
                {
                    DestroyObject(m_pPistol.m_pObject.m_p1stObject);
                    DestroyObject(m_pPistol.m_pObject.m_p3rdObject);
                    m_pPistol.m_pObject = null;
                }
            }
            else if (eType == eWeaponType.E_PISTOL)
            {
                m_pCurrentEquipment = m_pPistol;
                m_pPistol.Init(eWeaponType.E_PISTOL, pEquip, pCameraParent, pHandParent);
                if (m_pRifle.m_pObject != null)
                {
                    DestroyObject(m_pRifle.m_pObject.m_p1stObject);
                    DestroyObject(m_pRifle.m_pObject.m_p3rdObject);
                    m_pRifle.m_pObject = null;
                }
            }
        }
    }
}
