using UnityEngine;
using System.Collections;

public class CursorLock : MonoBehaviour {

    public bool m_isLock = false;

	// Use this for initialization
	void Start () {
        SetCursorLock(m_isLock);
	}

    void SetCursorLock(bool isLock)
    {
        Cursor.visible = !isLock;
        if(isLock)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
    }
	
	// Update is called once per frame
	void Update () {
        SetCursorLock(m_isLock);
	}
}
