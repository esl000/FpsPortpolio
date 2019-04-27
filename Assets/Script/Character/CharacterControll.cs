using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FpsLibrary.Controll;
using FpsLibrary.Equipment;
using FpsLibrary.Equipment.Weapon;
using FpsLibrary.Networks;

namespace FpsLibrary.Controll
{
    enum ePlayerMoveState
    {
        E_UNDER = 0,
        E_RUN = 1,
        E_SPRINT = 2,
    }

    public class CharacterControll : MonoBehaviour
    {
        [SerializeField]
        private Transform m_pCharacter;

        [SerializeField]
        private Transform m_pCamera;

        [SerializeField]
        private Animator m_pAnimator;

        [SerializeField]
        private Rigidbody m_pRigidBody;

        [SerializeField]
        private CapsuleCollider m_pCollider;

        [SerializeField]
        private MouseSet m_pMouse = new MouseSet();

        [SerializeField]
        private AudioClip[] m_aAudio;

        [SerializeField]
        private AudioClip m_pJumpAudio;

        [SerializeField]
        private AudioClip m_pLandAudio;

        [SerializeField]
        private AudioSource m_pAudioSouce;

        [SerializeField]
        private float m_fSpeed = 3f;

        [SerializeField]
        private float m_fSprintPerSpeed = 2f;

        [SerializeField]
        private float m_fUnderPerSpeed = 0.6f;

        [SerializeField]
        private float m_fBackPerSpeed = 0.75f;

        [SerializeField]
        private float m_fGroundDistanse = 0.2f;

        [SerializeField]
        private float m_fJumpForce = 30f;

        [SerializeField]
        private float m_fOrgFootStepTime = 0.5f;

        [SerializeField]
        private GameObject m_pSpine;

        [SerializeField]
        private GameObject m_pRHand;

        [SerializeField]
        private GameObject m_pLFoot;

        [SerializeField]
        private GameObject m_pRFoot;

        [SerializeField]
        private EquipmentManager m_pEquip;

        [SerializeField]
        private float m_fHp = 100f;

        [SerializeField]
        private bool m_isPrevGround = true;

        [SerializeField]
        private bool m_isGround = true;

        [SerializeField]
        private bool m_isJumping = false;

        [SerializeField]
        private bool m_isReload = false;

        [SerializeField]
        private bool m_isFire = false;

        [SerializeField]
        private ePlayerMoveState m_eMoveState = ePlayerMoveState.E_RUN;

        [SerializeField]
        private Equip m_pRifle;

        [SerializeField]
        private Equip m_pPistol;

        [SerializeField]
        private GameObject m_pBullet;

        [SerializeField]
        private bool m_isDead = false;

        [SerializeField]
        private RuntimeAnimatorController m_pRifleAnimator;

        [SerializeField]
        private RuntimeAnimatorController m_pPistolAnimator;

        [SerializeField]
        private GameObject m_pMultiplayerPrefap;

        [SerializeField]
        private GameManager m_pManager;

        public GameObject m_pMultiplayer;

        public Vector3 m_vVelocity = new Vector3();

        private float m_fFootStepTime = 0.5f;
        private float m_fCurrentFootStepTime = 0f;

        private bool isFrameReloaded = false;

        private float m_fAirTime = 0f;

        private string m_sKiller;

        private GameObject m_pDamageJoint;


        public GameManager Manager
        {
            get { return m_pManager; }
            set { m_pManager = value; }
        }

        public Animator Animator
        {
            get { return m_pAnimator; }
            set { m_pAnimator = value; }
        }
        public Rigidbody RigidBody
        {
            get { return m_pRigidBody; }
            set { m_pRigidBody = value; }
        }
        public CapsuleCollider Collider
        {
            get { return m_pCollider; }
            set { m_pCollider = value; }
        }

        public float Speed
        {
            get { return m_fSpeed; }
            set { m_fSpeed = value; }
        }
        public float Hp
        {
            get { return m_fHp; }
            set { m_fHp = value; }
        }

        public EquipmentManager Equip
        {
            get { return m_pEquip; }
            set { m_pEquip = value; }
        }

        public bool Dead
        {
            get { return m_isDead; }
        }

        private Vector2 m_vDir = new Vector2(0f, 0f);
        private Vector2 m_vNextDir = new Vector2(0f, 0f);




        // Use this for initialization
        void Start()
        {

            GameObject[] aObjects = GameObject.FindGameObjectsWithTag("CharCollider");

            foreach(GameObject pObject in aObjects)
            {
                if (pObject.GetComponent<BoxCollider>())
                    pObject.GetComponent<BoxCollider>().isTrigger = true;

                if (pObject.GetComponent<CapsuleCollider>())
                    pObject.GetComponent<CapsuleCollider>().isTrigger = true;

                pObject.GetComponent<Rigidbody>().isKinematic = true;
            }

            m_pMouse.Init(m_pCharacter, m_pCamera);

            m_pEquip.Init(eRifleWeapons.E_AK47, ePistolWeapons.E_EAGLE);
            m_pEquip.ChangeWeapon(eWeaponType.E_RIFLE, m_pRifle, m_pCamera.transform, m_pRHand.transform);

            //pRifle.transform.parent = m_pRHand.transform;
            //p1stRifle.transform.parent = m_pCamera.transform;

            //pRifle.transform.localPosition = new Vector3();
            //p1stRifle.transform.localPosition = new Vector3();

            //m_pAnimation = p1stRifle.GetComponent<Animation>();
        }

        void FixedUpdate()
        {
            GroundCheck();
            m_pMouse.LookRotation(m_pCharacter, m_pCamera);
            ChangeWeapons();
            Move();
        }

        // Update is called once per frame
        void Update()
        {
            if ((Network.isClient || Network.isServer) && m_pMultiplayer == null)
            {
                GameObject pOwnObject = (GameObject)Network.Instantiate(m_pMultiplayerPrefap, transform.position, transform.rotation, 0);

                pOwnObject.GetComponent<MultiplayerController>().SetRendering(false);

                m_pMultiplayer = pOwnObject;
            }
        }

        void LateUpdate()
        {
            SettingCollider();
            AnimationLerp();
            AnimationControll();
            SendData();
        }

        void ChangeWeapons()
        {
            if(Input.GetKeyDown(KeyCode.Alpha1) && m_pEquip.m_pCurrentEquipment.m_eWeaponType == eWeaponType.E_PISTOL)
            {
                m_pEquip.ChangeWeapon(eWeaponType.E_RIFLE, m_pRifle, m_pCamera.transform, m_pRHand.transform);
                m_pAnimator.runtimeAnimatorController = m_pRifleAnimator;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && m_pEquip.m_pCurrentEquipment.m_eWeaponType == eWeaponType.E_RIFLE)
            {
                m_pEquip.ChangeWeapon(eWeaponType.E_PISTOL, m_pPistol, m_pCamera.transform, m_pRHand.transform);
                m_pAnimator.runtimeAnimatorController = m_pPistolAnimator;
            }
        }

        void SettingCollider()
        {
            if(m_eMoveState == ePlayerMoveState.E_UNDER)
            {
                if(m_pCollider.height > 1.001f)
                {
                    m_pCollider.height -= Time.deltaTime * 3f;
                    transform.Translate(0f, -Time.deltaTime * 3f, 0f);
                }

                if (m_pCollider.height <= 1f)
                {
                    m_pCollider.height = 1f;
                }
            }
            else
            {
                if (m_pCollider.height < 1.799f)
                {
                    m_pCollider.height += Time.deltaTime * 3f;
                    transform.Translate(0f, Time.deltaTime * 3f, 0f);
                }

                if (m_pCollider.height >= 1.8f)
                {
                    m_pCollider.height = 1.8f;
                }
            }


            //float fFootYPos = (m_pLFoot.transform.position.y <= m_pRFoot.transform.position.y) ? m_pLFoot.transform.position.y : m_pRFoot.transform.position.y;
            
            //m_pCollider.center =
            //    new Vector3(m_pCollider.center.x, m_pSpine.transform.position.y - transform.position.y, m_pCollider.center.z);

            //float fPrevHeight = m_pCollider.height;

            //m_pCollider.height =
            //    Mathf.Abs(m_pSpine.transform.position.y - fFootYPos) * 2;

            //transform.Translate(0f, (m_pCollider.height - fPrevHeight) / 2f - 0.005f, 0f);
        }

        void fire()
        {
            if (!m_pEquip.m_pCurrentEquipment.AllowFire())
            {
                m_isFire = false;
                m_pAnimator.SetBool("isFire", false);
                m_pEquip.m_pCurrentEquipment.m_pAnimator.SetBool("isFire", false);
                return;
            }
            Vector3 vStart;
            if (!m_pEquip.m_pCurrentEquipment.Fire(out vStart))
                return;

            m_isFire = true;
            m_pAnimator.SetBool("isFire", true);
            m_pEquip.m_pCurrentEquipment.m_pAnimator.SetBool("isFire", true);


            GameObject pBullet = (GameObject)Network.Instantiate(m_pBullet, m_pCamera.transform.position + m_pCamera.transform.forward * 8f, m_pCamera.transform.rotation, 0);
            pBullet.GetComponent<Bullet>().SendName();
            pBullet.GetComponent<Bullet>().m_sShooter = MainGame.GetInstance().m_sPlayerName;
            pBullet.GetComponent<Bullet>().m_isMine = true;

            pBullet.GetComponent<Bullet>().m_fDamage = m_pEquip.m_pCurrentEquipment.m_fDamage;
        }

        public void Hit(float fDamage, string sShooter)
        {
            m_fHp -= fDamage;

            if (m_fHp <= 0f)
            {
                m_fHp = 0f;
                Death(sShooter);
            }
        }

        public void Death(string sShooter)
        {
            m_isDead = true;

            m_sKiller = sShooter;

            m_pManager.m_pCurrentMSender.GetComponent<MessageManager>().SendDeathMessage(MainGame.GetInstance().m_sPlayerName, m_sKiller);

            SendData();

            DestroyObject(m_pEquip.m_pCurrentEquipment.m_pObject.m_p1stObject);

            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<CapsuleCollider>().enabled = false;
            m_pAnimator.enabled = false;

            GameObject[] aObjects = GameObject.FindGameObjectsWithTag("CharCollider");

            foreach (GameObject pObject in aObjects)
            {
                if (pObject.GetComponent<BoxCollider>())
                    pObject.GetComponent<BoxCollider>().isTrigger = false;

                if (pObject.GetComponent<CapsuleCollider>())
                    pObject.GetComponent<CapsuleCollider>().isTrigger = false;

                pObject.GetComponent<Rigidbody>().isKinematic = false;
            }
            m_pManager.PlayerDeath();
            GetComponent<CharacterControll>().enabled = false;
        }

        void SendData()
        {
            PlayerDataFormat pData = new PlayerDataFormat();
            pData.m_eType = eDataType.E_PLAYERDATA;

            pData.m_vDest = transform.position;
            pData.m_qDest = transform.rotation;
            pData.m_vVelocity = m_vVelocity;

            pData.m_qSpine = m_pCamera.transform.localRotation;

            if (m_pEquip.m_pCurrentEquipment.m_eWeaponType == eWeaponType.E_RIFLE)
                pData.m_isRifle = true;
            else
                pData.m_isRifle = false;

            pData.m_isDead = m_isDead;

            pData.m_eRifle = eRifleWeapons.E_AK47;
            pData.m_ePistol = ePistolWeapons.E_EAGLE;

            pData.m_nState = m_pAnimator.GetInteger("nState");
            pData.m_nJump = m_pAnimator.GetInteger("nJump");

            pData.m_fFront = m_pAnimator.GetFloat("fFront");
            pData.m_fSide = m_pAnimator.GetFloat("fSide");


            pData.m_isGround = m_pAnimator.GetBool("isGround");
            pData.m_isFire = m_pAnimator.GetBool("isFire");
            pData.m_isReload = m_isReload;

            if (m_pMultiplayer != null)
                m_pMultiplayer.GetComponent<MultiplayerController>().AddData(pData);
        }

        void AnimationControll() 
        {
            AnimationReload();
            AnimationFire();
            AnimationRotateYofCamara();
        }

        void AnimationReload()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                if(!m_isReload)
                {
                    if (!m_pEquip.m_pCurrentEquipment.AllowReload())
                        return;

                    m_isReload = true;
                    m_pAnimator.SetBool("isReload", true);
                    m_pEquip.m_pCurrentEquipment.m_pAnimator.SetBool("isReload", true);
                }
            }
            else
            {
                m_pAnimator.SetBool("isReload", false);
                m_pEquip.m_pCurrentEquipment.m_pAnimator.SetBool("isReload", false);
            }

            if (!isFrameReloaded && m_pEquip.m_pCurrentEquipment.m_pAnimator.GetCurrentAnimatorStateInfo(0).IsName("reload") &&
                m_pEquip.m_pCurrentEquipment.m_pAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.8f)
            {
                m_pEquip.m_pCurrentEquipment.Reload();
                m_isReload = false;
                isFrameReloaded = true;
            }

            if (!isFrameReloaded && !m_pEquip.m_pCurrentEquipment.m_pAnimator.GetCurrentAnimatorStateInfo(0).IsName("reload") & m_isReload)
            {
                m_isReload = false;
            }

            if (!m_pEquip.m_pCurrentEquipment.m_pAnimator.GetCurrentAnimatorStateInfo(0).IsName("reload") ||
                m_pEquip.m_pCurrentEquipment.m_pAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.8f)
            {
                isFrameReloaded = false;
            }
        }

        void AnimationRotateYofCamara()
        {
            Quaternion qRotation1;

			float fXangle = m_pCamera.localRotation.eulerAngles.x;

			if (fXangle > 180f) 
			{
				fXangle -= 360f;
			}

            qRotation1 = Quaternion.Euler(m_pSpine.transform.localRotation.eulerAngles.x,
                                          m_pSpine.transform.localRotation.eulerAngles.y + fXangle * 0.25f,
                                          m_pSpine.transform.localRotation.eulerAngles.z - fXangle * 0.75f);


			m_pSpine.transform.localRotation = qRotation1;
        }

        void AnimationFire()
        {
            if (!m_isReload && Input.GetAxis("Fire1") > 0.001f)
            {
                fire();
            }
            else
            {
                m_isFire = false;
                m_pAnimator.SetBool("isFire", false);
                m_pEquip.m_pCurrentEquipment.m_pAnimator.SetBool("isFire", false);
            }
        }

        void AnimationLerp()
        {
            //AnimationCameraPos();
            AnimationDirectionLerp();
        }

        void AnimationCameraPos()
        {
            if(m_eMoveState == ePlayerMoveState.E_UNDER)
                m_pCamera.localPosition = Vector3.Lerp(m_pCamera.localPosition, new Vector3(0f, -0.2f, 0f), 0.3f);
            else
                m_pCamera.localPosition = Vector3.Lerp(m_pCamera.localPosition, new Vector3(0f, 0.5f, 0f), 0.3f);
        }

        void AnimationDirectionLerp()
        {
            m_vDir = Vector2.Lerp(m_vDir, m_vNextDir, 0.2f);

            if (Vector2.Distance(m_vDir, m_vNextDir) < 0.1f)
                m_vDir = m_vNextDir;

            m_pAnimator.SetFloat("fFront", m_vDir.x);
            m_pAnimator.SetFloat("fSide", m_vDir.y);
        }



        void Move()
        {
            float fSpeed;

            if (Input.GetKey(KeyCode.LeftControl))
            {
                m_eMoveState = ePlayerMoveState.E_UNDER;
                fSpeed = m_fUnderPerSpeed * m_fSpeed;
                m_pAnimator.SetInteger("nState", 0);
                m_pEquip.m_pCurrentEquipment.m_pAnimator.SetBool("isSprint", false);
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                m_eMoveState = ePlayerMoveState.E_SPRINT;
                fSpeed = m_fSprintPerSpeed * m_fSpeed;
                m_pAnimator.SetInteger("nState", 2);
            }
            else
            {
                m_eMoveState = ePlayerMoveState.E_RUN;
                fSpeed = m_fSpeed;
                m_pAnimator.SetInteger("nState", 1);
                m_pEquip.m_pCurrentEquipment.m_pAnimator.SetBool("isSprint", false);
            }

			Vector2 vDirection = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
			vDirection = vDirection.normalized;

			m_vNextDir = vDirection;


            if(Input.GetKeyDown(KeyCode.Space) && m_isGround && !m_isJumping)
            {
                m_pRigidBody.AddForce(new Vector3(0f, m_fJumpForce, 0f), ForceMode.Impulse);
                m_isJumping = true;
                m_pAnimator.SetInteger("nJump", 1);
                PlaySound(m_pJumpAudio);
                SendNetworkFootStepSound(4);
            }

			if (m_eMoveState == ePlayerMoveState.E_SPRINT) 
			{
				if(m_vDir.x < 0f)
				{
					fSpeed = m_fSpeed;
				}
				else
				{
					fSpeed = m_fSpeed + (m_fSprintPerSpeed - 1f) * m_fSpeed * m_vDir.x;
				}
			}

			if(m_vDir.x < -0.01f)
			{
				fSpeed *= m_fBackPerSpeed;
			}

			fSpeed *= Vector2.Distance (new Vector2 (), m_vDir);

			if (m_eMoveState == ePlayerMoveState.E_SPRINT) 
			{
				if(vDirection.x > 0.01f)
					m_pEquip.m_pCurrentEquipment.m_pAnimator.SetBool("isSprint", true);
				else
					m_pEquip.m_pCurrentEquipment.m_pAnimator.SetBool("isSprint", false);
			}

            if(Vector2.Distance(new Vector2(), vDirection) > 0.1f)
            {
                FootStepController(fSpeed);
            }

            m_vVelocity = transform.forward * vDirection.x * fSpeed +
                transform.right * vDirection.y * fSpeed;

            m_pRigidBody.MovePosition(transform.position +
                transform.forward * vDirection.x * fSpeed * Time.deltaTime +
                transform.right * vDirection.y * fSpeed * Time.deltaTime);

        }

        void GroundCheck()
        {
            m_isPrevGround = m_isGround;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position + m_pCollider.center, m_pCollider.radius, Vector3.down, out hitInfo,
                                   ((m_pCollider.height / 2f) - m_pCollider.radius) + m_fGroundDistanse))
            {
                m_isGround = true;
                m_pAnimator.SetBool("isGround", true);
            }
            else
            {
                m_fAirTime += Time.deltaTime;
                m_isGround = false;
                m_pAnimator.SetBool("isGround", false);
            }

            if (!m_isPrevGround && m_isGround)
            {
                if(m_isJumping)
                {
                    m_isJumping = false;
                    m_pAnimator.SetBool("isGround", false);
                    m_pAnimator.SetInteger("nJump", 2);
                }

                if (m_fAirTime > 1f)
                {
                    PlaySound(m_pLandAudio);
                    SendNetworkFootStepSound(5);
                }

                m_fAirTime = 0f;
            }
        }

        void PlaySound(AudioClip pAudio)
        {
            m_pAudioSouce.clip = pAudio;
            m_pAudioSouce.PlayOneShot(m_pAudioSouce.clip);
        }

        void FootStepController(float fSpeed)
        {
            m_fFootStepTime = m_fOrgFootStepTime / (fSpeed / m_fSpeed);
            m_fFootStepTime /= Vector2.Distance(new Vector2(), m_vDir);
            m_fCurrentFootStepTime += Time.deltaTime;
            if(m_fCurrentFootStepTime >= m_fFootStepTime)
            {
                m_fCurrentFootStepTime -= m_fFootStepTime;
                PlayFootStepSound();
            }
        }

        void PlayFootStepSound()
        {
            if (!m_isGround)
            {
                return;
            }
            int n = Random.Range(1, m_aAudio.Length);
            PlaySound(m_aAudio[n]);
            SendNetworkFootStepSound(n);
            AudioClip pTemp = m_aAudio[n];
            m_aAudio[n] = m_aAudio[0];
            m_aAudio[0] = pTemp;
        }

        void SendNetworkFootStepSound(int n)
        {
            SoundDataFormat pData = new SoundDataFormat();

            pData.m_eType = eDataType.E_SOUND;
            pData.m_nAudioClip = n;

            if (m_pMultiplayer != null)
                m_pMultiplayer.GetComponent<MultiplayerController>().AddSoundData(pData);
        }

        void OnDestroy()
        {
            Network.RemoveRPCs(m_pMultiplayer.GetComponent<NetworkView>().viewID);
            Network.Destroy(m_pMultiplayer.GetComponent<NetworkView>().viewID);
        }
    }
}