using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockIndicator : MonoBehaviour
{
    public List<GameObject> m_Indicators = new List<GameObject>();
    
    
    // 在组件上右键点击，会出现 "Arrange Indicators" 选项
    [ContextMenu("Arrange Indicators")]
    public void Arrange()
    {
        if (m_Indicators.Count == 0) return;

        var startPos = m_Indicators[0].transform.localPosition;
        var radius = startPos.z;

        for (int i = 0; i < m_Indicators.Count; i++)
        {
            float newRotY = 30f * i;
            float rad = newRotY * Mathf.Deg2Rad;
            
            float newPosX = radius * Mathf.Sin(rad);
            float newPosZ = radius * Mathf.Cos(rad);
            
            m_Indicators[i].transform.localPosition = new Vector3(newPosX, startPos.y, newPosZ);
            m_Indicators[i].transform.localRotation = Quaternion.Euler(0, newRotY, 0);
            
            // 重要：标记场景已改变，否则可能无法保存
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(m_Indicators[i].transform);
#endif
        }
        Debug.Log("时钟刻度已重新排列并保存！");
    }
}
