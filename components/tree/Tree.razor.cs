﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace AntDesign
{
    public partial class Tree<TItem> : AntDomComponentBase
    {
        #region fields

        /// <summary>
        /// 记录所有的的node
        /// </summary>
        internal List<TreeNode<TItem>> _allNodes = new List<TreeNode<TItem>>();

        /// <summary>
        /// 所有勾选的nodes
        /// </summary>
        private ConcurrentDictionary<long, TreeNode<TItem>> _checkedNodes = new ConcurrentDictionary<long, TreeNode<TItem>>();

        #endregion


        #region Tree

        /// <summary>
        /// 节点前添加展开图标
        /// </summary>
        [Parameter]
        public bool ShowExpand { get; set; } = true;

        /// <summary>
        /// 是否展示连接线
        /// </summary>
        [Parameter]
        public bool ShowLine { get; set; }

        /// <summary>
        /// 是否展示 TreeNode title 前的图标
        /// </summary>
        [Parameter]
        public bool ShowIcon { get; set; }

        /// <summary>
        /// 是否节点占据一行
        /// </summary>
        [Parameter]
        public bool BlockNode { get; set; }

        /// <summary>
        /// 设置节点可拖拽
        /// </summary>
        [Parameter]
        public bool Draggable { get; set; }

        /// <summary>
        /// 将树禁用
        /// </summary>
        [Parameter]
        public bool Disabled { get; set; }

        /// <summary>
        /// 显示子叶图标
        /// </summary>
        [Parameter]
        public bool ShowLeafIcon { get; set; } = false;



        private void SetClassMapper()
        {
            ClassMapper
                .Add("ant-tree")
                .If("ant-tree-show-line", () => ShowLine)
                .If("ant-tree-icon-hide", () => ShowIcon)
                .If("ant-tree-block-node", () => BlockNode)
                .If("draggable-tree", () => Draggable)
                .If("ant-tree-unselectable", () => !Selectable)
                .If("ant-tree-rtl", () => RTL);
        }

        #endregion Tree

        #region Node

        /// <summary>
        /// 
        /// </summary>
        [Parameter]
        public RenderFragment Nodes { get; set; }

        /// <summary>
        /// 子节点
        /// </summary>
        [Parameter]
        public List<TreeNode<TItem>> ChildNodes { get; set; } = new List<TreeNode<TItem>>();

        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="treeNode"></param> 
        internal void AddNode(TreeNode<TItem> treeNode)
        {
            ChildNodes.Add(treeNode);
        }

        #endregion Node

        #region Selected

        /// <summary>
        /// 是否可选中
        /// </summary>
        [Parameter]
        public bool Selectable { get; set; } = true;

        /// <summary>
        /// 支持点选多个节点（节点本身）
        /// </summary>
        [Parameter]
        public bool Multiple { get; set; }

        /// <summary>
        /// 选中的树节点
        /// </summary>
        internal Dictionary<long, TreeNode<TItem>> SelectedNodesDictionary { get; set; } = new Dictionary<long, TreeNode<TItem>>();

        public List<string> SelectedTitles => SelectedNodesDictionary.Select(x => x.Value.Title).ToList();

        internal void SelectedNodeAdd(TreeNode<TItem> treeNode)
        {
            if (SelectedNodesDictionary.ContainsKey(treeNode.NodeId) == false)
                SelectedNodesDictionary.Add(treeNode.NodeId, treeNode);

            UpdateBindData();
        }

        internal void SelectedNodeRemove(TreeNode<TItem> treeNode)
        {
            if (SelectedNodesDictionary.ContainsKey(treeNode.NodeId) == true)
                SelectedNodesDictionary.Remove(treeNode.NodeId);

            UpdateBindData();
        }

        public void DeselectAll()
        {
            foreach (var item in SelectedNodesDictionary.Select(x => x.Value).ToList())
            {
                item.SetSelected(false);
            }
        }

        /// <summary>
        /// 选择的Key
        /// </summary>
        [Parameter]
        public string SelectedKey { get; set; }

        [Parameter]
        public EventCallback<string> SelectedKeyChanged { get; set; }

        /// <summary>
        /// 选择的节点
        /// </summary>
        [Parameter]
        public TreeNode<TItem> SelectedNode { get; set; }

        [Parameter]
        public EventCallback<TreeNode<TItem>> SelectedNodeChanged { get; set; }

        /// <summary>
        /// 选择的数据
        /// </summary>
        [Parameter]
        public TItem SelectedData { get; set; }

        [Parameter]
        public EventCallback<TItem> SelectedDataChanged { get; set; }

        /// <summary>
        /// 选择的Key集合
        /// </summary>
        [Parameter]
        public string[] SelectedKeys { get; set; }

        [Parameter]
        public EventCallback<string[]> SelectedKeysChanged { get; set; }

        /// <summary>
        /// 选择的节点集合
        /// </summary>
        [Parameter]
        public TreeNode<TItem>[] SelectedNodes { get; set; }

        /// <summary>
        /// 选择的数据集合
        /// </summary>
        [Parameter]
        public TItem[] SelectedDatas { get; set; }

        /// <summary>
        /// 更新绑定数据
        /// </summary>
        private void UpdateBindData()
        {
            if (SelectedNodesDictionary.Count == 0)
            {
                SelectedKey = null;
                SelectedNode = null;
                SelectedData = default(TItem);
                SelectedKeys = Array.Empty<string>();
                SelectedNodes = Array.Empty<TreeNode<TItem>>();
                SelectedDatas = Array.Empty<TItem>();
            }
            else
            {
                var selectedFirst = SelectedNodesDictionary.FirstOrDefault();
                SelectedKey = selectedFirst.Value?.Key;
                SelectedNode = selectedFirst.Value;
                SelectedData = selectedFirst.Value.DataItem;
                SelectedKeys = SelectedNodesDictionary.Select(x => x.Value.Key).ToArray();
                SelectedNodes = SelectedNodesDictionary.Select(x => x.Value).ToArray();
                SelectedDatas = SelectedNodesDictionary.Select(x => x.Value.DataItem).ToArray();
            }

            if (SelectedKeyChanged.HasDelegate) SelectedKeyChanged.InvokeAsync(SelectedKey);
            if (SelectedNodeChanged.HasDelegate) SelectedNodeChanged.InvokeAsync(SelectedNode);
            if (SelectedDataChanged.HasDelegate) SelectedDataChanged.InvokeAsync(SelectedData);
            if (SelectedKeysChanged.HasDelegate) SelectedKeysChanged.InvokeAsync(SelectedKeys);
        }

        #endregion Selected

        #region Checkable

        /// <summary>
        /// 节点前添加 Checkbox 复选框
        /// </summary>
        [Parameter]
        public bool Checkable { get; set; }

        /// <summary>
        /// checkable 状态下节点选择完全受控（父子节点选中状态不再关联）
        /// </summary>
        [Parameter]
        public bool CheckStrictly { get; set; }

        /// <summary>
        /// 勾选的数据 keys
        /// </summary>
        [Parameter]
        public List<string> CheckedKeys { get; set; } = new List<string>();

        /// <summary>
        ///  勾选的数据 keys
        /// </summary>
        public EventCallback<string> CheckedKeysChanged { get; set; }

        public List<TreeNode<TItem>> CheckedNodes => GetCheckedNodes(ChildNodes);

        /// <summary>
        /// 勾选的数据 keys
        /// </summary>
        //public List<string> CheckedKeys => GetCheckedNodes(ChildNodes).Select(x => x.Key).ToList();

        /// <summary>
        /// 勾选的标题数据
        /// </summary>
        public List<string> CheckedTitles => GetCheckedNodes(ChildNodes).Select(x => x.Title).ToList();

        private List<TreeNode<TItem>> GetCheckedNodes(List<TreeNode<TItem>> childs)
        {
            List<TreeNode<TItem>> checkeds = new List<TreeNode<TItem>>();
            foreach (var item in childs)
            {
                if (item.Checked) checkeds.Add(item);
                checkeds.AddRange(GetCheckedNodes(item.ChildNodes));
            }
            return checkeds;
        }

        //取消所有选择项目
        public void DecheckedAll()
        {
            foreach (var item in ChildNodes)
            {
                item.SetChecked(false);
            }
        }

        /// <summary>
        /// 默认勾选的节点
        /// </summary>
        [Parameter]
        public IList<string> DefaultCheckedKeys { get; set; }

        /// <summary>
        /// 禁用节点Checkbox
        /// </summary>
        public IList<string> DisableCheckKeys { get; set; }

        /// <summary>
        /// 记录或移除勾选节点
        /// </summary>
        /// <param name="treeNode"></param>
        internal void AddOrRemoveCheckNode(TreeNode<TItem> treeNode)
        {
            if (treeNode.Checked)
                _checkedNodes.TryAdd(treeNode.NodeId, treeNode);
            else
                _checkedNodes.TryRemove(treeNode.NodeId, out TreeNode<TItem> _);
            CheckedKeys = _checkedNodes.Select(x => x.Value.Key).ToList();
        }

        #endregion Checkable

        #region Search

        public string _searchValue;

        /// <summary>
        /// 按需筛选树,双向绑定
        /// </summary>
        [Parameter]
        public string SearchValue
        {
            get => _searchValue;
            set
            {
                if (_searchValue == value) return;
                _searchValue = value;
                if (string.IsNullOrEmpty(value)) return;
                foreach (var item in ChildNodes)
                {
                    SearchNode(item);
                }
            }
        }

        /// <summary>
        /// 返回一个值是否是页节点
        /// </summary>
        [Parameter]
        public Func<TreeNode<TItem>, bool> SearchExpression { get; set; }

        /// <summary>
        /// 查询节点
        /// </summary>
        /// <param name="treeNode"></param>
        /// <returns></returns>
        private bool SearchNode(TreeNode<TItem> treeNode)
        {
            if (SearchExpression != null)
                treeNode.Matched = SearchExpression(treeNode);
            else
                treeNode.Matched = treeNode.Title.Contains(SearchValue);

            var hasChildMatched = treeNode.Matched;
            foreach (var item in treeNode.ChildNodes)
            {
                var itemMatched = SearchNode(item);
                hasChildMatched = hasChildMatched || itemMatched;
            }
            treeNode.HasChildMatched = hasChildMatched;

            return hasChildMatched;
        }

        #endregion Search

        #region DataBind

        [Parameter]
        public IList<TItem> DataSource { get; set; }

        /// <summary>
        /// 指定一个方法，该表达式返回节点的文本。
        /// </summary>
        [Parameter]
        public Func<TreeNode<TItem>, string> TitleExpression { get; set; }

        /// <summary>
        /// 指定一个返回节点名称的方法。
        /// </summary>
        [Parameter]
        public Func<TreeNode<TItem>, string> KeyExpression { get; set; }

        /// <summary>
        /// 指定一个返回节点名称的方法。
        /// </summary>
        [Parameter]
        public Func<TreeNode<TItem>, string> IconExpression { get; set; }

        /// <summary>
        /// 返回一个值是否是页节点
        /// </summary>
        [Parameter]
        public Func<TreeNode<TItem>, bool> IsLeafExpression { get; set; }

        /// <summary>
        /// 返回子节点的方法
        /// </summary>
        [Parameter]
        public Func<TreeNode<TItem>, IList<TItem>> ChildrenExpression { get; set; }

        #endregion DataBind

        #region Event

        /// <summary>
        /// 延迟加载
        /// </summary>
        /// <remarks>必须使用async，且返回类型为Task，否则可能会出现载入时差导致显示问题</remarks>
        [Parameter]
        public EventCallback<TreeEventArgs<TItem>> OnNodeLoadDelayAsync { get; set; }

        /// <summary>
        /// 点击树节点触发
        /// </summary>
        [Parameter]
        public EventCallback<TreeEventArgs<TItem>> OnClick { get; set; }

        /// <summary>
        /// 双击树节点触发
        /// </summary>
        [Parameter]
        public EventCallback<TreeEventArgs<TItem>> OnDblClick { get; set; }

        /// <summary>
        /// 右键树节点触发
        /// </summary>
        [Parameter]
        public EventCallback<TreeEventArgs<TItem>> OnContextMenu { get; set; }

        /// <summary>
        /// 点击树节点 Checkbox 触发
        /// </summary>
        [Parameter]
        public EventCallback<TreeEventArgs<TItem>> OnCheckBoxChanged { get; set; }

        /// <summary>
        /// 点击展开树节点图标触发
        /// </summary>
        [Parameter]
        public EventCallback<TreeEventArgs<TItem>> OnExpandChanged { get; set; }

        /// <summary>
        /// 搜索节点时调用(与SearchValue配合使用)
        /// </summary>
        [Parameter]
        public EventCallback<TreeEventArgs<TItem>> OnSearchValueChanged { get; set; }

        ///// <summary>
        ///// 开始拖拽时调用
        ///// </summary>
        //public EventCallback<TreeEventArgs> OnDragStart { get; set; }

        ///// <summary>
        ///// dragenter 触发时调用
        ///// </summary>
        //public EventCallback<TreeEventArgs> OnDragEnter { get; set; }

        ///// <summary>
        ///// dragover 触发时调用
        ///// </summary>
        //public EventCallback<TreeEventArgs> OnDragOver { get; set; }

        ///// <summary>
        ///// dragleave 触发时调用
        ///// </summary>
        //public EventCallback<TreeEventArgs> OnDragLeave { get; set; }

        ///// <summary>
        ///// drop 触发时调用
        ///// </summary>
        //public EventCallback<TreeEventArgs> OnDrop { get; set; }

        ///// <summary>
        ///// dragend 触发时调用
        ///// </summary>
        //public EventCallback<TreeEventArgs> OnDragEnd { get; set; }

        #endregion Event

        #region Template

        /// <summary>
        /// 缩进模板
        /// </summary>
        [Parameter]
        public RenderFragment<TreeNode<TItem>> IndentTemplate { get; set; }

        /// <summary>
        /// 标题模板
        /// </summary>
        [Parameter]
        public RenderFragment<TreeNode<TItem>> TitleTemplate { get; set; }

        /// <summary>
        /// 图标模板
        /// </summary>
        [Parameter]
        public RenderFragment<TreeNode<TItem>> TitleIconTemplate { get; set; }

        /// <summary>
        /// 切换图标模板
        /// </summary>
        [Parameter]
        public RenderFragment<TreeNode<TItem>> SwitcherIconTemplate { get; set; }

        #endregion Template

        protected override void OnInitialized()
        {
            SetClassMapper();
            base.OnInitialized();

        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
        }

        /// <summary>
        /// Find Node
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="recursive">Recursive Find</param>
        /// <returns></returns>
        public TreeNode<TItem> FindFirstOrDefaultNode(Func<TreeNode<TItem>, bool> predicate, bool recursive = true)
        {
            foreach (var child in ChildNodes)
            {
                if (predicate != null && predicate.Invoke(child))
                {
                    return child;
                }

                if (recursive)
                {
                    var find = child.FindFirstOrDefaultNode(predicate, recursive);
                    if (find != null)
                    {
                        return find;
                    }
                }
            }
            return null;
        }

        #region Expand

        /// <summary>
        /// 默认展开所有树节点
        /// </summary>
        [Parameter]
        public bool DefaultExpandAll { get; set; }

        /// <summary>
        /// 默认展开父节点
        /// </summary>
        [Parameter]
        public bool DefaultExpandParent { get; set; }

        /// <summary>
        /// 默认展开指定的树节点
        /// </summary>
        [Parameter]
        public IList<string> DefaultExpandedKeys { get; set; }

        /// <summary>
        /// （受控）展开指定的树节点
        /// </summary>
        [Parameter]
        public IList<string> ExpandedKeys { get; set; }

        /// <summary>
        /// from node expand to root
        /// </summary>
        /// <param name="node">Node</param>
        public void ExpandToNode(TreeNode<TItem> node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            var parentNode = node.ParentNode;
            while (parentNode != null)
            {
                parentNode.Expand(true);
                parentNode = parentNode.ParentNode;
            }
        }

        /// <summary>
        /// 展开全部节点
        /// </summary>
        public void ExpandAll()
        {
            this.ChildNodes.ForEach(node => Switch(node, true));
        }

        /// <summary>
        /// 折叠全部节点
        /// </summary>
        public void CollapseAll()
        {
            this.ChildNodes.ForEach(node => Switch(node, false));
        }

        /// <summary>
        /// 节点展开关闭
        /// </summary>
        /// <param name="node"></param>
        /// <param name="expanded"></param>
        private void Switch(TreeNode<TItem> node, bool expanded)
        {
            node.Expand(expanded);
            node.ChildNodes.ForEach(n => Switch(n, expanded));
        }


        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Task OnFirstAfterRenderAsync()
        {
            System.Diagnostics.Debug.WriteLine($"Tree OnFirstAfterRenderAsync at {DateTime.Now}");
            return base.OnFirstAfterRenderAsync();
        }
    }
}
