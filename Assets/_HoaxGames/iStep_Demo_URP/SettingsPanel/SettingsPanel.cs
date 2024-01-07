using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsPanel : MonoBehaviour
{
    RectTransform m_rectTransform;
    float m_xValueOpen;
    float m_xValueClosed;

    private void Awake()
    {
        m_rectTransform = this.GetComponent<RectTransform>();
        m_xValueClosed = -m_rectTransform.sizeDelta.x;
        m_xValueOpen = m_rectTransform.anchoredPosition.x;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            m_rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(m_rectTransform.anchoredPosition.x, m_xValueClosed, Time.deltaTime * 10.0f), m_rectTransform.anchoredPosition.y);
        }
        else
        {
            m_rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(m_rectTransform.anchoredPosition.x, m_xValueOpen, Time.deltaTime * 10.0f), m_rectTransform.anchoredPosition.y);
        }
    }
}
