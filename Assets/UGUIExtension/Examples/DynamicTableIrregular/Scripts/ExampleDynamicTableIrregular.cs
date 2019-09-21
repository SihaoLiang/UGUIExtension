using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ExampleDynamicTableIrregular : MonoBehaviour
{
    public class TestGrid
    {
        public RectTransform m_Root;
        public Text m_Text;

        public ContentSizeFitter m_ContentSizeFitter;
        public LayoutNode m_LayoutGroup;
        public TestGrid(RectTransform root)
        {
            m_Root = root;
            InitUI();
        }

        public void InitUI()
        {
            m_LayoutGroup = m_Root.GetComponent<LayoutNode>();
            m_Text = m_Root.GetComponentInChildren<Text>();
            m_ContentSizeFitter = m_Text.transform.GetComponent<ContentSizeFitter>();
        }

        public void SetContent(int index,string content)
        {
            m_Text.text =string.Format("第{0}条:{1}", index,content);
            m_ContentSizeFitter.SetLayoutVertical();
            m_ContentSizeFitter.SetLayoutHorizontal();
            m_LayoutGroup.SetDirty();
        }


    }
    public InputField m_InputCountField;
    public InputField m_InputIndexField;

    public Button m_AsyncLoadButton;
    public Button m_SyncLoadButton;



    public InputField m_InputField;
    public Button m_Button;
    public DynamicTableIrregular DynamicTable;
    Dictionary<Transform, TestGrid> GridDic = null;
    List<string> m_ChatContentList = new List<string>();

    private void Awake()
    {
        GridDic = new Dictionary<Transform, TestGrid>();
        m_Button.onClick.AddListener(OnButtonSenderClick);
        m_AsyncLoadButton.onClick.AddListener(OnBtnAsyncClick);
        m_SyncLoadButton.onClick.AddListener(OnBtnSyncClick);
        DynamicTable.DynamicTableGridDelegate = OnDynamicTableViewCallBack;
        
    }

    private void Start()
    {
       /// DynamicTable.TotalCount = m_ChatContentList.Count;
        //DynamicTable.ReloadDataAsync();
    }

    void OnButtonSenderClick()
    {
        if (m_InputField == null)
            return;

        m_ChatContentList.Add(m_InputField.text);
        DynamicTable.TotalCount = m_ChatContentList.Count;
        DynamicTable.ReloadDataAsync(m_ChatContentList.Count);
    }


    /// <summary>
    /// 异步加载
    /// </summary>
    void OnBtnAsyncClick()
    {
        if (m_InputCountField == null || string.IsNullOrEmpty(m_InputCountField.text))
        {
            Debug.LogError("请输入数量");
            return;
        }

        if (m_InputIndexField == null || string.IsNullOrEmpty(m_InputIndexField.text))
        {
            Debug.LogError("请输入索引");
            return;
        }

        m_ChatContentList.Clear();

        int index = int.Parse(m_InputIndexField.text);
        int count = int.Parse(m_InputCountField.text);
        for (int i = 0; i < count; i++)
        {
            m_ChatContentList.Add(i.ToString());
        }

        DynamicTable.TotalCount = m_ChatContentList.Count;
        DynamicTable.ReloadDataAsync(index);
    }

    /// <summary>
    /// 同步加载
    /// </summary>
    void OnBtnSyncClick()
    {
        if (m_InputCountField == null || string.IsNullOrEmpty(m_InputCountField.text))
        {
            Debug.LogError("请输入数量");
            return;
        }

        if (m_InputIndexField == null || string.IsNullOrEmpty(m_InputIndexField.text))
        {
            Debug.LogError("请输入索引");
            return;
        }


        m_ChatContentList.Clear();
        int count = int.Parse(m_InputCountField.text);
        int index = int.Parse(m_InputIndexField.text);

        for (int i = 0; i < count; i++)
        {
            m_ChatContentList.Add(i.ToString());
        }

        DynamicTable.TotalCount = m_ChatContentList.Count;
        DynamicTable.ReloadDataSync(index);
    }


    void OnDynamicTableViewCallBack(int evt, int index)
    {
        if (evt == (int)LayoutRule.DYNAMIC_DELEGATE_EVENT.DYNAMIC_GRID_ATINDEX)
        {
            int rand = index % 3 + 1;
            RectTransform trans = DynamicTable.PreDequeueGrid(rand.ToString(), index);

            if (trans == null || GridDic == null)
                return;

            TestGrid grid = null;
            if (!GridDic.TryGetValue(trans, out grid))
            {
                grid = new TestGrid(trans);
                GridDic.Add(trans, grid);
            }

            if (grid == null)
                return;

            string content = m_ChatContentList[index - 1];
            grid.SetContent(index, content);
        }
        else if (evt == (int)LayoutRule.DYNAMIC_DELEGATE_EVENT.DYNAMIC_GRID_RECYCLE) { }

    }
}
