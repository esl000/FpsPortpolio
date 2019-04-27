using System;
using UnityEngine;
using System.Collections;

namespace FpsLibrary.Controll
{
    [Serializable]
    public class MouseSet
    {
        public float m_fXsensityvity = 2f;
        public float m_fYsensityvity = 2f;
        public float m_fMinimumX = -90F;
        public float m_fMaximumX = 90F;
        public float m_fSmoothTime = 5f;

        private Quaternion m_qCharacterTargetRot;
        private Quaternion m_qCameraTargetRot;

        public void Init(Transform pCharacter, Transform pCamera)
        {
            m_qCharacterTargetRot = pCharacter.localRotation;
            m_qCameraTargetRot = pCamera.localRotation;
        }

        public void LookRotation(Transform pCharacter, Transform pCamera)
        {
            float fYRot = Input.GetAxis("Mouse X") * m_fXsensityvity;
            float fXRot = Input.GetAxis("Mouse Y") * m_fYsensityvity;

            m_qCharacterTargetRot *= Quaternion.Euler(0f, fYRot, 0f);
            m_qCameraTargetRot *= Quaternion.Euler(-fXRot, 0f, 0f);

            m_qCameraTargetRot = ClampRotationAroundXAxis(m_qCameraTargetRot);


            pCharacter.localRotation = Quaternion.Slerp(pCharacter.localRotation, m_qCharacterTargetRot,
                m_fSmoothTime * Time.deltaTime);
            pCamera.localRotation = Quaternion.Slerp(pCamera.localRotation, m_qCameraTargetRot,
                m_fSmoothTime * Time.deltaTime);
        }


        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float fAngleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            fAngleX = Mathf.Clamp(fAngleX, m_fMinimumX, m_fMaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * fAngleX);

            return q;
        }
    }
}

