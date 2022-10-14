using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FadeInFadeOutImage : MonoBehaviour
{
    [SerializeField] Image m_Image;
    [SerializeField] float m_FadeInDuration;
    [SerializeField] float m_StayDuration;
    [SerializeField] float m_FadeOutDuration;
    [SerializeField] UnityEvent m_AnimationHasEndedEvent;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        float elapsedTime = 0;
        while (elapsedTime<m_FadeInDuration)
        {
            float k = elapsedTime / m_FadeInDuration;
            m_Image.color = new Color(m_Image.color.r, m_Image.color.g, m_Image.color.b, k);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(m_StayDuration);

         elapsedTime = 0;
        while (elapsedTime < m_FadeInDuration)
        {
            float k = elapsedTime / m_FadeOutDuration;
            m_Image.color = new Color(m_Image.color.r, m_Image.color.g, m_Image.color.b, 1-k);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (m_AnimationHasEndedEvent != null) m_AnimationHasEndedEvent.Invoke();
    }

}
