using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardBehaviour : MonoBehaviour
{
    [SerializeField] Transform m_lookAtTarget;
    [SerializeField] bool m_onlyAroundUpAxis = false;

    Transform m_transform;

    private void Awake()
    {
        m_transform = this.transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dirVec = m_lookAtTarget.position - m_transform.position;
        if (m_onlyAroundUpAxis)
        {
            dirVec -= Vector3.Project(dirVec, Vector3.up);
            m_transform.LookAt(m_transform.position - dirVec, Vector3.up);
        }
        else
        {
            m_transform.LookAt(m_transform.position - dirVec, m_lookAtTarget.up);
        }
    }
}
