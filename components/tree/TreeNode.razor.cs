﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AntDesign
{
    public partial class TreeNode<TItem> : AntDomComponentBase
    {
        #region Node

        /// <summary>
        /// 树控件本身
        /// </summary>
        [CascadingParameter(Name = "Tree")]
        public Tree<TItem> TreeComponent { get; set; }

        /// <summary>
        /// 上一级节点
        /// </summary>
        [CascadingParameter(Name = "Node")]
        public TreeNode<TItem> ParentNode { get; set; }

        /// <summary>
        /// 子节点
        /// </summary>
        [Parameter]
        public RenderFragment Nodes { get; set; }

        public List<TreeNode<TItem>> ChildNodes { get; set; } = new List<TreeNode<TItem>>();

        public bool HasChildNodes => ChildNodes?.Count > 0;

        /// <summary>
        /// 当前节点级别
        /// </summary>
        public int TreeLevel => (ParentNode?.TreeLevel ?? -1) + 1;//因为第一层是0，所以默认是-1

        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="treeNode"></param>
        internal void AddNode(TreeNode<TItem> treeNode)
        {
            ChildNodes.Add(treeNode);
            IsLeaf = false;
        }

        /// <summary>
        /// Find a node
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="recursive">Recursive Find</param>
        /// <returns></returns>
        public TreeNode<TItem> FindFirstOrDefaultNode(Func<TreeNode<TItem>, bool> predicate, bool recursive = true)
        {
            foreach (var child in ChildNodes)
            {
                if (predicate.Invoke(child))
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

        /// <summary>
        /// 获得上级数据集合
        /// </summary>
        /// <returns></returns>
        public List<TreeNode<TItem>> GetParentNodes()
        {
            if (this.ParentNode != null)
                return this.ParentNode.ChildNodes;
            else
                return this.TreeComponent.ChildNodes;
        }

        public TreeNode<TItem> GetPreviousNode()
        {
            var parentNodes = GetParentNodes();
            var index = parentNodes.IndexOf(this);
            if (index == 0) return null;
            else return parentNodes[index - 1];
        }

        public TreeNode<TItem> GetNextNode()
        {
            var parentNodes = GetParentNodes();
            var index = parentNodes.IndexOf(this);
            if (index == parentNodes.Count - 1) return null;
            else return parentNodes[index + 1];
        }

        #endregion Node

        #region TreeNode

        private static long _nextNodeId;

        internal long NodeId { get; private set; }

        public TreeNode()
        {
            NodeId = Interlocked.Increment(ref _nextNodeId);
        }

        private string _key;

        /// <summary>
        /// 指定当前节点的唯一标识符名称。
        /// </summary>
        [Parameter]
        public string Key
        {
            get
            {
                if (TreeComponent.KeyExpression != null)
                    return TreeComponent.KeyExpression(this);
                else
                    return _key;
            }
            set
            {
                _key = value;
            }
        }

        private bool _disabled;

        /// <summary>
        /// 是否禁用
        /// 禁用状态受制于父节点
        /// </summary>
        [Parameter]
        public bool Disabled
        {
            get { return _disabled || (ParentNode?.Disabled ?? false); }
            set { _disabled = value; }
        }

        private bool _selected;

        /// <summary>
        /// 是否已选中
        /// </summary>
        [Parameter]
        public bool Selected
        {
            get => _selected;
            set
            {
                if (_selected == value) return;
                SetSelected(value);
            }
        }

        /// <summary>
        /// 点击选择
        /// </summary>
        /// <param name="value"></param>
        public void SetSelected(bool value)
        {
            if (Disabled) return;
            if (!TreeComponent.Selectable && TreeComponent.Checkable)
            {
                SetChecked(!Checked);
                return;
            }
            if (_selected == value) return;
            _selected = value;
            if (value == true)
            {
                if (TreeComponent.Multiple == false) TreeComponent.DeselectAll();
                TreeComponent.SelectedNodeAdd(this);
            }
            else
            {
                TreeComponent.SelectedNodeRemove(this);
            }
            StateHasChanged();
        }

        /// <summary>
        /// 是否异步加载状态(影响展开图标展示)
        /// </summary>
        [Parameter]
        public bool Loading { get; set; }

        private void SetTreeNodeClassMapper()
        {
            ClassMapper.Clear().Add("ant-tree-treenode")
                .If("ant-tree-treenode-disabled", () => Disabled)
                .If("ant-tree-treenode-switcher-open", () => SwitcherOpen)
                .If("ant-tree-treenode-switcher-close", () => SwitcherClose)
                .If("ant-tree-treenode-checkbox-checked", () => Checked)
                .If("ant-tree-treenode-checkbox-indeterminate", () => Indeterminate)
                .If("ant-tree-treenode-selected", () => Selected)
                .If("ant-tree-treenode-loading", () => Loading);
        }

        #endregion TreeNode

        #region Switcher

        private bool _isLeaf = true;

        /// <summary>
        /// 是否为叶子节点
        /// </summary>
        [Parameter]
        public bool IsLeaf
        {
            get
            {
                if (TreeComponent.IsLeafExpression != null)
                    return TreeComponent.IsLeafExpression(this);
                else
                    return _isLeaf;
            }
            set
            {
                if (_isLeaf == value) return;
                _isLeaf = value;
                StateHasChanged();
            }
        }

        /// <summary>
        /// 是否已展开
        /// </summary>
        [Parameter]
        public bool Expanded { get; set; }

        /// <summary>
        /// 折叠节点
        /// </summary>
        /// <param name="expanded"></param>
        public void Expand(bool expanded)
        {
            Expanded = expanded;
        }

        /// <summary>
        /// 真实的展开状态，路径上只要存在折叠，那么下面的全部折叠
        /// </summary>
        internal bool RealDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(TreeComponent.SearchValue))
                {//普通模式下节点显示规则
                    if (ParentNode == null) return true;//第一级节点默认显示
                    if (ParentNode.Expanded == false) return false;//上级节点如果是折叠的，必定折叠
                    return ParentNode.RealDisplay; //否则查找路径三的级节点显示情况
                }
                else
                {//筛选模式下不考虑节点是否展开，只要节点符合条件，或者存在符合条件的子节点是就展开显示
                    return Matched || HasChildMatched;
                }
            }
        }

        /// <summary>
        /// 节点开关
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task OnSwitcherClick(MouseEventArgs args)
        {
            this.Expanded = !this.Expanded;
            if (TreeComponent.OnNodeLoadDelayAsync.HasDelegate && this.Expanded == true)
            {
                //自有节点被展开时才需要延迟加载
                //如果支持异步载入，那么在展开时是调用异步载入代码
                this.Loading = true;
                await TreeComponent.OnNodeLoadDelayAsync.InvokeAsync(new TreeEventArgs<TItem>(TreeComponent, this, args));
                this.Loading = false;
            }
            if (TreeComponent.OnExpandChanged.HasDelegate)
                await TreeComponent.OnExpandChanged.InvokeAsync(new TreeEventArgs<TItem>(TreeComponent, this, args));
        }

        /// <summary>
        /// 展开
        /// </summary>
        private bool SwitcherOpen => Expanded && !IsLeaf;

        /// <summary>
        /// 关闭
        /// </summary>
        private bool SwitcherClose => !Expanded && !IsLeaf;

        /// <summary>
        /// 冒泡展开
        /// </summary> 
        private void OpenPropagation()
        {
            this.Expand(true);
            if (this.ParentNode != null)
                this.ParentNode.OpenPropagation();
        }

        #endregion Switcher

        #region Checkbox

        /// <summary>
        /// 显示勾选
        /// </summary>
        [Parameter]
        public bool Checked { get; set; }

        [Parameter]
        public bool Indeterminate { get; set; }


        private bool _disableCheckbox;
        /// <summary>
        /// 是否可以选择
        /// </summary>
        [Parameter]
        public bool DisableCheckbox
        {
            get
            {
                return _disableCheckbox || (TreeComponent?.DisableCheckKeys?.Any(k => k == Key) ?? false);
            }
            set
            {
                _disableCheckbox = value;
            }
        }

        /// <summary>
        /// 当点击选择框是触发
        /// </summary>
        private async void OnCheckBoxClick(MouseEventArgs args)
        {
            if (DisableCheckbox)
                return;
            SetChecked(!Checked);
            if (TreeComponent.OnCheckBoxChanged.HasDelegate)
                await TreeComponent.OnCheckBoxChanged.InvokeAsync(new TreeEventArgs<TItem>(TreeComponent, this, args));
        }

        /// <summary>
        /// 设置选中状态
        /// </summary>
        /// <param name="check"></param>
        public void SetChecked(bool check)
        {
            if (!Disabled)
            {
                if (TreeComponent.CheckStrictly)
                {
                    this.Checked = check;
                }
                else
                {
                    SetChildChecked(this, check);
                    if (ParentNode != null)
                        ParentNode.UpdateCheckState();
                }
            }
            TreeComponent.AddOrRemoveCheckNode(this);
            StateHasChanged();
        }
        /// <summary>
        /// 设置子节点状态
        /// </summary>
        /// <param name="subnode"></param>
        /// <param name="check"></param>
        private void SetChildChecked(TreeNode<TItem> subnode, bool check)
        {
            if (Disabled) return;
            this.Checked = DisableCheckbox ? false : check;
            this.Indeterminate = false;
            TreeComponent.AddOrRemoveCheckNode(this);
            if (subnode.HasChildNodes)
                foreach (var child in subnode.ChildNodes)
                    child?.SetChildChecked(child, check);
        }

        /// <summary>
        /// 更新选中状态
        /// </summary>
        /// <param name="halfChecked"></param>
        private void UpdateCheckState(bool? halfChecked = null)
        {
            if (halfChecked == true)
            {//如果子元素存在不确定状态，父元素必定存在不确定状态
                this.Checked = false;
                this.Indeterminate = true;
            }
            else if (HasChildNodes == true && !DisableCheckbox)
            {//判断当前节点的选择状态
                bool hasChecked = false;
                bool hasUnchecked = false;

                foreach (var item in ChildNodes)
                {
                    if (!item.DisableCheckbox && !item.Disabled)
                    {
                        if (item.Indeterminate == true) break;
                        if (item.Checked == true) hasChecked = true;
                        if (item.Checked == false) hasUnchecked = true;
                    }
                }

                if (hasChecked && !hasUnchecked)
                {
                    this.Checked = true;
                    this.Indeterminate = false;
                }
                else if (!hasChecked && hasUnchecked)
                {
                    this.Checked = false;
                    this.Indeterminate = false;
                }
                else if (hasChecked && hasUnchecked)
                {
                    this.Checked = false;
                    this.Indeterminate = true;
                }
            }
            TreeComponent.AddOrRemoveCheckNode(this);

            if (ParentNode != null)
                ParentNode.UpdateCheckState(this.Indeterminate);

            //当达到最顶级后进行刷新状态，避免每一级刷新的性能问题
            if (ParentNode == null)
                StateHasChanged();
        }

        #endregion Checkbox

        #region Title

        [Parameter]
        public bool Draggable { get; set; }

        private string _icon;

        /// <summary>
        /// 节点前的图标，与 `ShowIcon` 组合使用
        /// </summary>
        [Parameter]
        public string Icon
        {
            get
            {
                if (TreeComponent.IconExpression != null)
                    return TreeComponent.IconExpression(this);
                else
                    return _icon;
            }
            set
            {
                _icon = value;
            }
        }

        private string _title;

        /// <summary>
        /// 文本
        /// </summary>
        [Parameter]
        public string Title
        {
            get
            {
                if (TreeComponent.TitleExpression != null)
                    return TreeComponent.TitleExpression(this);
                else
                    return _title;
            }
            set
            {
                _title = value;
            }
        }

        /// <summary>
        /// title是否包含SearchValue(搜索使用)
        /// </summary>
        public bool Matched { get; set; }

        /// <summary>
        /// 子节点存在满足搜索条件，所以夫节点也需要显示
        /// </summary>
        internal bool HasChildMatched { get; set; }

        #endregion Title

        #region 数据绑定

        [Parameter]
        public TItem DataItem { get; set; }

        private IList<TItem> ChildDataItems
        {
            get
            {
                if (TreeComponent.ChildrenExpression != null)
                    return TreeComponent.ChildrenExpression(this) ?? new List<TItem>();
                else
                    return new List<TItem>();
            }
        }

        /// <summary>
        /// 获得上级数据集合
        /// </summary>
        /// <returns></returns>
        public IList<TItem> GetParentChildDataItems()
        {
            if (this.ParentNode != null)
                return this.ParentNode.ChildDataItems;
            else
                return this.TreeComponent.DataSource;
        }

        #endregion 数据绑定

        #region 节点数据操作

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="dataItem"></param>
        public void AddChildNode(TItem dataItem)
        {
            ChildDataItems.Add(dataItem);
        }

        /// <summary>
        /// 节点后面添加节点
        /// </summary>
        /// <param name="dataItem"></param>
        public void AddNextNode(TItem dataItem)
        {
            var parentChildDataItems = GetParentChildDataItems();
            var index = parentChildDataItems.IndexOf(this.DataItem);
            parentChildDataItems.Insert(index + 1, dataItem);

            AddNodeAndSelect(dataItem);
        }

        /// <summary>
        /// 节点前面添加节点
        /// </summary>
        /// <param name="dataItem"></param>
        public void AddPreviousNode(TItem dataItem)
        {
            var parentChildDataItems = GetParentChildDataItems();
            var index = parentChildDataItems.IndexOf(this.DataItem);
            parentChildDataItems.Insert(index, dataItem);

            AddNodeAndSelect(dataItem);
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        public void Remove()
        {
            var parentChildDataItems = GetParentChildDataItems();
            parentChildDataItems.Remove(this.DataItem);
        }

        public void MoveInto(TreeNode<TItem> treeNode)
        {
            if (treeNode == this || this.DataItem.Equals(treeNode.DataItem)) return;
            var parentChildDataItems = GetParentChildDataItems();
            parentChildDataItems.Remove(this.DataItem);
            treeNode.AddChildNode(this.DataItem);
        }

        /// <summary>
        /// 上移节点
        /// </summary>
        public void MoveUp()
        {
            var parentChildDataItems = GetParentChildDataItems();
            var index = parentChildDataItems.IndexOf(this.DataItem);
            if (index == 0) return;
            parentChildDataItems.RemoveAt(index);
            parentChildDataItems.Insert(index - 1, this.DataItem);
        }

        /// <summary>
        /// 下移节点
        /// </summary>
        public void MoveDown()
        {
            var parentChildDataItems = GetParentChildDataItems();
            var index = parentChildDataItems.IndexOf(this.DataItem);
            if (index == parentChildDataItems.Count - 1) return;
            parentChildDataItems.RemoveAt(index);
            parentChildDataItems.Insert(index + 1, this.DataItem);
        }

        /// <summary>
        /// 降级节点
        /// </summary>
        public void Downgrade()
        {
            var previousNode = GetPreviousNode();
            if (previousNode == null) return;
            var parentChildDataItems = GetParentChildDataItems();
            parentChildDataItems.Remove(this.DataItem);
            previousNode.AddChildNode(this.DataItem);
        }

        /// <summary>
        /// 升级节点
        /// </summary>
        public void Upgrade()
        {
            if (this.ParentNode == null) return;
            var parentChildDataItems = this.ParentNode.GetParentChildDataItems();
            var index = parentChildDataItems.IndexOf(this.ParentNode.DataItem);
            Remove();
            parentChildDataItems.Insert(index + 1, this.DataItem);
        }

        #endregion 节点数据操作

        protected override void OnInitialized()
        {
            SetTreeNodeClassMapper();
            if (ParentNode != null)
                ParentNode.AddNode(this);
            else
            {
                TreeComponent.AddNode(this);
                if (!TreeComponent.DefaultExpandAll && TreeComponent.DefaultExpandParent)
                    Expand(true);
            }
            TreeComponent._allNodes.Add(this);

            if (TreeComponent.DisabledExpression != null)
                Disabled = TreeComponent.DisabledExpression(this);

            if (TreeComponent.DefaultExpandAll)
                Expand(true);
            else if (TreeComponent.ExpandedKeys != null)
            {
                Expand(TreeComponent.ExpandedKeys.Any(k => k == this.Key));
            }

            if (TreeComponent.Selectable && TreeComponent.SelectedKeys != null)
            {
                this.Selected = TreeComponent.SelectedKeys.Any(k => k == this.Key);
            }
            base.OnInitialized();
        }

        protected override void OnParametersSet()
        {
            SetTreeNodeClassMapper();
            base.OnParametersSet();
        }

        private void AddNodeAndSelect(TItem dataItem)
        {
            var tn = ChildNodes.FirstOrDefault(treeNode => treeNode.DataItem.Equals(dataItem));
            if (tn != null)
            {
                this.Expand(true);
                tn.SetSelected(true);
            }
        }

        /// <summary>
        /// 首次渲染
        /// </summary>
        /// <returns></returns>
        protected override Task OnFirstAfterRenderAsync()
        {
            if (this.Checked)
                this.SetChecked(true);
            TreeComponent.DefaultCheckedKeys?.ForEach(k =>
            {
                var node = TreeComponent._allNodes.FirstOrDefault(x => x.Key == k);
                if (node != null)
                    node.SetChecked(true);
            });
            if (!TreeComponent.DefaultExpandAll)
            {
                if (this.Expanded)
                    this.OpenPropagation();
                TreeComponent.DefaultExpandedKeys?.ForEach(k =>
                {
                    var node = TreeComponent._allNodes.FirstOrDefault(x => x.Key == k);
                    if (node != null)
                        node.OpenPropagation();
                });
            }
            return base.OnFirstAfterRenderAsync();
        }

    }
}
