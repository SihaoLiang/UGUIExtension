using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool
{
    public Pool(string poolName, Object prefab, int limit = -1)
    {
        m_PoolName = poolName;
        m_PoolPerfab = prefab;
        m_UsingList = new List<PoolObject>();
        m_UnusedList = new Stack<PoolObject>();
    }

    /// <summary>
    /// 池的名字
    /// </summary>
    private string m_PoolName = string.Empty;

    /// <summary>
    /// 池的预设
    /// </summary>
    private Object m_PoolPerfab = null;

    /// <summary>
    /// 上限
    /// </summary>
    private int m_LimitSize = -1;

    /// <summary>
    /// 池的容量
    /// </summary>
    private int m_PoolSize = 0;

    /// <summary>
    /// 使用中的对象
    /// </summary>
    private List<PoolObject> m_UsingList = null;

    /// <summary>
    /// 闲置中的对象
    /// </summary>
    private Stack<PoolObject> m_UnusedList = null;

    /// <summary>
    /// 实例化
    /// </summary>
    /// <returns></returns>
    private PoolObject PoolObjectInstantiate()
    {
        if (m_PoolPerfab == null)
        {
            Debug.LogErrorFormat("Pool {0} Instantiate fail due to m_PoolPerfab is null", m_PoolName);
            return null;
        }

        GameObject obj = GameObject.Instantiate(m_PoolPerfab) as GameObject;

        PoolObject poolObject = obj.GetComponent<PoolObject>();
        if (poolObject == null)
            poolObject = obj.AddComponent<PoolObject>();

        poolObject.poolName = m_PoolName;
        poolObject.isPooled = true;
        obj.SetActive(false);

        return poolObject;
    }

    /// <summary>
    /// 出池
    /// </summary>
    /// <returns></returns>
    public PoolObject Spawn()
    {
        PoolObject poolObject = null;
        if (m_UnusedList.Count > 0)
        {
            poolObject = m_UnusedList.Pop();
            poolObject.isPooled = false;
            m_UsingList.Add(poolObject);
        }
        else if (m_LimitSize > 0 && m_LimitSize == m_PoolSize)
        {
            Debug.LogErrorFormat("Pool {0} run out of Pool limitSize({1})", m_PoolName, m_LimitSize);
        }
        else
        {
            poolObject = PoolObjectInstantiate();
            poolObject.isPooled = false;
            m_UsingList.Add(poolObject);
            m_PoolSize++;
        }

        return poolObject;
    }

    /// <summary>
    /// 入池
    /// </summary>
    /// <param name="poolObject"></param>
    public void Despawn(PoolObject poolObject)
    {
        if (poolObject == null)
            return;

        poolObject.isPooled = true;
        poolObject.gameObject.SetActive(false);

        if (m_UsingList != null && m_UsingList.Contains(poolObject))
            m_UsingList.Remove(poolObject);

        if (m_UnusedList != null)
            m_UnusedList.Push(poolObject);
    }

    /// <summary>
    /// 清理
    /// </summary>
    public void Clear()
    {
        if (m_UsingList != null && m_UsingList.Count > 0)
        {
            for (int index = 0; index < m_UsingList.Count; index++)
            {
                PoolObject poolObject = m_UsingList[index];
                m_UsingList.Remove(poolObject);
                Object.Destroy(poolObject);
                poolObject = null;
                index--;
            }
        }


        if (m_UnusedList != null && m_UnusedList.Count > 0)
        {
            while(m_UnusedList.Count > 0)
            {
                PoolObject poolObject = m_UnusedList.Pop();
                Object.Destroy(poolObject);
                poolObject = null;
            }
        }

    }

}
