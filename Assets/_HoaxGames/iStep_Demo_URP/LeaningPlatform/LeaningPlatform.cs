using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoaxGames
{
    public class LeaningPlatform : MonoBehaviour
    {
        public class FootIkOverrideSettings
        {
            public FootIkOverrideSettings (float _ikRotationStiffness, float _antiDownwardsIntersectionStiffness, float _isGluedToGroundDistanceThreshold)
            {
                ikRotationStiffness = _ikRotationStiffness;
                antiDownwardsIntersectionStiffness = _antiDownwardsIntersectionStiffness;
                isGluedToGroundDistanceThreshold = _isGluedToGroundDistanceThreshold;
            }

            public float ikRotationStiffness;
            public float antiDownwardsIntersectionStiffness;
            public float isGluedToGroundDistanceThreshold;
        }

        Dictionary<CharacterController, float> m_skinWidthLookupDic = new Dictionary<CharacterController, float>();
        Dictionary<FootIK, FootIkOverrideSettings> m_footIkLookupDic = new Dictionary<FootIK, FootIkOverrideSettings>();

        // Start is called before the first frame update
        void Start()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            CharacterController cc = other.GetComponent<CharacterController>();
            if(cc)
            {
                if(m_skinWidthLookupDic.ContainsKey(cc) == false) m_skinWidthLookupDic.Add(cc, cc.skinWidth);
                cc.skinWidth = 0.0001f;
            }

            FootIK fik = other.GetComponent<FootIK>();
            if (fik)
            {
                if (m_footIkLookupDic.ContainsKey(fik) == false) m_footIkLookupDic.Add(fik, new FootIkOverrideSettings(fik.getIkRotationStiffness(), fik.getAntiDownwardsIntersectionStiffness(), fik.getIsGluedToGroundDistanceThreshold()));
                fik.setIkRotationStiffness(16);
                fik.setAntiDownwardsIntersectionStiffness(250);
                fik.setIsGluedToGroundDistanceThreshold(0.1f);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc)
            {
                if (m_skinWidthLookupDic.ContainsKey(cc)) cc.skinWidth = m_skinWidthLookupDic[cc];
            }

            FootIK fik = other.GetComponent<FootIK>();
            if (fik)
            {
                if (m_footIkLookupDic.ContainsKey(fik))
                {
                    FootIkOverrideSettings settings = m_footIkLookupDic[fik];
                    fik.setIkRotationStiffness(settings.ikRotationStiffness);
                    fik.setAntiDownwardsIntersectionStiffness(settings.antiDownwardsIntersectionStiffness);
                    fik.setIsGluedToGroundDistanceThreshold(settings.isGluedToGroundDistanceThreshold);
                }
            }
        }
    }
}
