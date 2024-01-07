using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoaxGames
{
    public class CharacterClone : MonoBehaviour
    {
        [SerializeField] protected Animator m_animatorTarget;
        [SerializeField] List<string> m_animatorFloatParamsToUpdate;
        [SerializeField] List<string> m_animatorBoolParamsToUpdate;

        protected Transform m_transformTarget;
        protected Transform m_transformSource;
        protected Animator m_animatorSource;

        protected List<int> m_animFloatParamsHashes = new List<int>();
        protected List<int> m_animBoolParamsHashes = new List<int>();

        private void Awake()
        {
            m_transformTarget = m_animatorTarget.transform;
            m_transformSource = this.transform;
            m_animatorSource = this.GetComponent<Animator>();
        }

        // Start is called before the first frame update
        void Start()
        {
            foreach (string s in m_animatorFloatParamsToUpdate) m_animFloatParamsHashes.Add(Animator.StringToHash(s));
            foreach (string s in m_animatorBoolParamsToUpdate) m_animBoolParamsHashes.Add(Animator.StringToHash(s));
        }

        // Update is called once per frame
        void LateUpdate()
        {
            m_transformSource.position = m_transformTarget.position;
            m_transformSource.rotation = m_transformTarget.rotation;

            foreach (int param in m_animFloatParamsHashes) m_animatorSource.SetFloat(param, m_animatorTarget.GetFloat(param));
            foreach (int param in m_animBoolParamsHashes) m_animatorSource.SetBool(param, m_animatorTarget.GetBool(param));
        }
    }
}