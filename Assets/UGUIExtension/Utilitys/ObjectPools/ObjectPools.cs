using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPools : MonoBehaviour
{

    [System.Serializable]
    public class PoolInfo
    {
        [SerializeField]
        public string PoolName;
        [SerializeField]
        public GameObject Prefab;
        [SerializeField]
        public int LimitSize = -1;
    }

    /// <summary>
    /// 对象池
    /// </summary>
    Dictionary<string, Pool> m_ObjectsPoolDic = new Dictionary<string, Pool>();

    /// <summary>
    /// 编辑初始化
    /// </summary>
    [SerializeField]
    PoolInfo[] m_PoolInfos;


    void Awake()
    {
        InitPool();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    void InitPool()
    {
        if (m_PoolInfos == null || m_PoolInfos.Length <= 0)
            return;

        for (int index = 0; index < m_PoolInfos.Length; index++)
        {
            AddPoolWithPoolInfo(m_PoolInfos[index]);
        }
    }


    /// <summary>
    /// 添加一个缓冲池
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="prefab"></param>
    /// <param name="limitSize"></param>
    public void AddPoolWithPoolInfo(PoolInfo info)
    {
        if (info == null)
            return;

        AddPool(info.PoolName, info.Prefab, info.LimitSize);
    }

    /// <summary>
    /// 添加一个缓冲池
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="prefab"></param>
    /// <param name="limitSize"></param>
    public void AddPool(string poolName, Object prefab, int limitSize = -1)
    {
        if (m_ObjectsPoolDic != null && !m_ObjectsPoolDic.ContainsKey(poolName))
        {
            Pool pool = new Pool(poolName, prefab, limitSize);
            m_ObjectsPoolDic.Add(poolName, pool);
        }
    }

    /// <summary>
    /// 清空一个池
    /// </summary>
    /// <param name="poolName"></param>
    public void RemovePoolByName(string poolName)
    {
        if (m_ObjectsPoolDic != null && m_ObjectsPoolDic.ContainsKey(poolName))
        {
            Pool pool = m_ObjectsPoolDic[poolName];
            pool.Clear();

            m_ObjectsPoolDic.Remove(poolName);
        }
    }

    /// <summary>
    /// 获取一个Object
    /// </summary>
    /// <param name="poolName"></param>
    /// <returns></returns>
    public PoolObject Spawn(string poolName)
    {
        if (string.IsNullOrEmpty(poolName))
        {
            Debug.LogWarning(string.Format("Spawn Error :PoolName :{0} is invalid!!", poolName));
            return null;
        }

        if (m_ObjectsPoolDic == null || !m_ObjectsPoolDic.ContainsKey(poolName))
        {
            Debug.LogWarning(string.Format("Spawn Error :PoolName :{0} is not Exist!!", poolName));
            return null;
        }

        Pool pool = m_ObjectsPoolDic[poolName];
        PoolObject poolObject = pool.Spawn();

        if (poolObject != null)
        {
            poolObject.gameObject.transform.SetParent(this.transform, false);
            poolObject.gameObject.transform.localPosition = Vector3.zero;
        }

        return poolObject;
    }

    /// <summary>
    /// 入池
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="poolObject"></param>
    public void Despawn(PoolObject poolObject)
    {
        if (string.IsNullOrEmpty(poolObject.poolName))
        {
            Debug.LogWarning(string.Format("Despawn Error :PoolName :{0} is invalid!!", poolObject.poolName));
            return;
        }

        if (m_ObjectsPoolDic == null || !m_ObjectsPoolDic.ContainsKey(poolObject.poolName))
        {
            Debug.LogWarning(string.Format("Despawn Error :PoolName :{0} is not Exist!!", poolObject.poolName));
            return;
        }

        Pool pool = m_ObjectsPoolDic[poolObject.poolName];
        pool.Despawn(poolObject);
    }


    /// <summary>
    /// 入池
    /// </summary>
    /// <param name="obj"></param>
    public void DespawnByObject(GameObject obj)
    {
        if (obj == null)
            return;

        PoolObject poolObject = obj.GetComponent<PoolObject>();

        if (poolObject != null)
            Despawn(poolObject);
    }
}
