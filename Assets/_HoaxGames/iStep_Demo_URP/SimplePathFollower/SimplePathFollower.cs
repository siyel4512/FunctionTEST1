using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoaxGames
{
    public class SimplePathFollower : MonoBehaviour
    {
        [SerializeField] Transform m_pathTarget;
        [SerializeField] bool m_rotateTowardsPathNodeRotation = false;

        public enum Direction
        {
            FORWARD,
            BACKWARD
        }

        [SerializeField] Direction m_direction = Direction.FORWARD;
        [SerializeField] int m_startAtNodeIdx = 0;
        [SerializeField, Range(0, 10)] int m_followSpeed = 2;

        public class Node
        {
            public Node(Node _next, Node _prev, Transform _transform) { next = _next; prev = _prev; transform = _transform; }
            public Node next;
            public Node prev;
            public Transform transform;
        }

        Transform m_transform;
        List<Node> m_path = new List<Node>();

        Node m_targetNode;

        private void Awake()
        {
            m_transform = this.transform;

            for (int i = 0; i < m_pathTarget.childCount; i++) m_path.Add(new Node(null, null, m_pathTarget.GetChild(i)));

            for (int i = 0; i < m_path.Count; i++)
            {
                int prev = i - 1;
                int next = i + 1;
                if (prev < 0) prev = m_pathTarget.childCount - 1;
                if (next >= m_pathTarget.childCount) next = 0;

                Node n = m_path[i];
                n.prev = m_path[prev];
                n.next = m_path[next];
            }

            m_targetNode = m_path[m_startAtNodeIdx];
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Vector3 dirVec = m_targetNode.transform.position - m_transform.position;
            m_transform.position += dirVec.normalized * m_followSpeed * Time.deltaTime;

            Vector3 planarDirVec = dirVec - Vector3.Project(dirVec, Vector3.up);

            if (m_rotateTowardsPathNodeRotation == false)
            {
                m_transform.rotation = Quaternion.Slerp(m_transform.rotation, Quaternion.LookRotation(planarDirVec.normalized, Vector3.up), Time.deltaTime * 5.0f);
            }
            else
            {
                m_transform.rotation = Quaternion.Slerp(m_transform.rotation, m_targetNode.transform.rotation, Time.deltaTime * 5.0f);
            }

            if (dirVec.magnitude < m_followSpeed * Time.deltaTime * 5.0f)
            {
                if (m_direction == Direction.FORWARD) m_targetNode = m_targetNode.next;
                else m_targetNode = m_targetNode.prev;
            }
        }
    }
}