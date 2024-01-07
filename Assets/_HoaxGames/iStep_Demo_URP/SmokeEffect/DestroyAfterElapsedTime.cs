using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoaxGames
{
    public class DestroyAfterElapsedTime : MonoBehaviour
    {
        [SerializeField] float m_timeToElapseForDestruction = 6.0f;
        [SerializeField] bool m_destroyParent = false;

        // Start is called before the first frame update
        IEnumerator Start()
        {
            yield return new WaitForSeconds(m_timeToElapseForDestruction);
            if (m_destroyParent == false) Destroy(this.gameObject);
            else Destroy(this.transform.parent.gameObject);
        }
    }
}