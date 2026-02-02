using System.Collections.Generic;
using UnityEngine;

namespace AtlasPacker {
    /// <summary>
    /// rect矩形的atlas图集封装类
    /// </summary>
    public class RectAtlas {
        #region 内部类

        /// <summary>
        /// 扩展rectatlas的回调方法
        /// </summary>
        public delegate void FnExtend(int pre_w, int pre_h, int width, int height);

        /// <summary>
        /// 二叉树结构的单个atlas内的node
        /// </summary>
        private struct AtlasNode {
            // public bool used;
            public readonly int x; // 位置
            public readonly int y;
            public int w; // 尺寸
            public int h;
            // public UAtlasNode branch0; // 分支
            // public UAtlasNode branch1;

            public uint id;
            
            #region 属性方法

            /// <summary>
            /// 已使用的node
            /// </summary>
            public bool Used {
                get => id == uint.MaxValue;
                set {
                    if (!value) {
                        return;
                    }
                    id = uint.MaxValue;
                }
            }

            /// <summary>
            /// 空node
            /// </summary>
            public static AtlasNode Empty => new AtlasNode() {
                w = 0,
                h = 0,
            };
            
            /// <summary>
            /// 合法的
            /// </summary>
            public bool Valid => w > 0 && h > 0;

            /*
            /// <summary>
            /// 是否有分支
            /// </summary>
            public bool HasBranch => branch0 != null;

            
            public bool HSplit => branch0 != null && branch0.w < w;

            public bool VSplit => branch0 != null && branch0.h < h;
            */

            /// <summary>
            /// right位置
            /// </summary>
            public int R => x + w;

            /// <summary>
            /// bottom位置
            /// </summary>
            public int B => y + h;

            #endregion

            #region ctor

            public AtlasNode(int x, int y, int w, int h) {
                // used = false;
                this.x = x;
                this.y = y;
                this.w = w;
                this.h = h;
                id = 0;
            }

            public AtlasNode(int x, int y, int w, int h, AtlasNode branch0, AtlasNode branch1) {
                // used = false;
                this.x = x;
                this.y = y;
                this.w = w;
                this.h = h;
                id = 0;
                // this.branch0 = branch0;
                // this.branch1 = branch1;
            }

            #endregion
            
            /// <summary>
            /// 清空这个node
            /// </summary>
            /// <param name="w"></param>
            /// <param name="h"></param>
            public void Clear(int w, int h) {
                this.w = w;
                this.h = h;
                // used = false;
                // branch0 = null;
                // branch1 = null;
            }

            public override string ToString() {
                string str = x + ", " + y + ", " + R + ", " + B;
                /*if (branch0 != null) {
                    str = !HSplit ? str + "# -" + branch0.B : str + "# |" + branch0.R;
                }*/
                return str;
            }

            /// <summary>
            /// 是否合适
            /// </summary>
            /// <param name="img_w"></param>
            /// <param name="img_h"></param>
            /// <returns></returns>
            public bool IsFit(int img_w, int img_h) => img_w == w && img_h == h;

            /// <summary>
            /// 判断这个rectatlas是否包含了img
            /// </summary>
            /// <param name="img_w"></param>
            /// <param name="img_h"></param>
            /// <returns></returns>
            public bool IsContaining(int img_w, int img_h) => img_w <= w && img_h <= h;

            /// <summary>
            /// 为当前这个node插入一个node
            /// </summary>
            /// <param name="con"></param>
            /// <param name="cost"></param>
            /// <param name="img_w"></param>
            /// <param name="img_h"></param>
            /// <returns></returns>
            public AtlasNode Insert(RectAtlas con, ref int cost, int img_w, int img_h) {
                if (Used || !IsContaining(img_w, img_h)) {
                    return Empty;
                }
                
                ++cost;
                if (IsFit(img_w, img_h)) {
                    con.RemoveNode(this);
                    Used = true;
                    return this;
                }

                AtlasNode replaceNode;
                AtlasNode node;
                if (w - img_w > h - img_h) {
                    replaceNode = new AtlasNode(x, y, img_w, h);
                    node = new AtlasNode(x + img_w, y, w - img_w, h);
                }
                else {
                    replaceNode = new AtlasNode(x, y, w, img_h);
                    node = new AtlasNode(x, y + img_h, w, h - img_h);
                }

                con.RemoveNode(this, ref replaceNode);
                con.AssignNode(ref node);
                return replaceNode.Insert(con, ref cost, img_w, img_h); // 调用替换的进行insert

                /*UAtlasNode atlasNode;
                if (branch0 != null) { // 调用brance0或者1插入
                    atlasNode = branch0.Insert(ref cost, img_w, img_h) ?? branch1.Insert(ref cost, img_w, img_h);
                }
                else {
                    if (IsFit(img_w, img_h)) {
                        used = true;
                        return this;
                    }

                    if (w - img_w > h - img_h) {
                        branch0 = new UAtlasNode(x, y, img_w, h);
                        branch1 = new UAtlasNode(x + img_w, y, w - img_w, h);
                    }
                    else {
                        branch0 = new UAtlasNode(x, y, w, img_h);
                        branch1 = new UAtlasNode(x, y + img_h, w, h - img_h);
                    }

                    atlasNode = branch0.Insert(ref cost, img_w, img_h);
                }

                if (branch0.used && branch1.used) {
                    used = true;
                }
                return atlasNode;*/
            }

            /*/// <summary>
            /// 切分rect
            /// </summary>
            /// <param name="extend_vertical"></param>
            /// <param name="pre_w"></param>
            /// <param name="pre_h"></param>
            /// <param name="clip_w"></param>
            /// <param name="clip_h"></param>
            /// <returns></returns>
            public UAtlasNode ClipRect(bool extend_vertical, int pre_w, int pre_h, int clip_w, int clip_h) {
                if (used) {
                    return null;
                }

                if (extend_vertical) {
                    if (B >= pre_h) {
                        h = clip_h - y;
                        if (h <= 0) {
                            used = true;
                        }
                    }
                }else if (R >= pre_h) {
                    w = clip_w - x;
                    if (w <= 0) {
                        used = true;
                    }
                }

                if (branch0 != null) {
                    branch0.ClipRect(extend_vertical, pre_w, pre_h, clip_w, clip_h);
                    branch1.ClipRect(extend_vertical, pre_w, pre_h, clip_w, clip_h);
                }

                return null;
            }*/
        }

        #endregion

        public int width; // 宽度
        public int height; // 高度
        public int use_w; // 已经使用的宽度
        public int use_h; // 已经使用的高度
        // private UAtlasNode _first_wrapper; // 根节点
        public FnExtend fld_fnExtend;

        private List<AtlasNode> _Anode;
        private uint _current_id = 1;

        #region ctor

        public RectAtlas(int w = 0, int h = 0) {
            // _first_wrapper = new UAtlasNode(0, 0, w, h); // 创建根节点
            _current_id = 1;
            _Anode = new List<AtlasNode>(64);
            if (w <= 0 || h <= 0) {
                return;
            }
            Clear(w, h);
        }

        #endregion

        /// <summary>
        /// 以wh进行清空
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public virtual void Clear(int w, int h) {
            // _first_wrapper = new UAtlasNode(0, 0, w, h); // 创建根节点
            AtlasNode atlasNode = new AtlasNode(0, 0, w, h) {
                id = 1
            };
            width = w;
            height = h;
            _current_id = 1;
            _Anode.Clear();
            _Anode.Add(atlasNode);
            use_w = use_h = 0;
        }

        /// <summary>
        /// 添加node
        /// </summary>
        /// <param name="node"></param>
        private void AssignNode(ref AtlasNode node) {
            if (_Anode.Count >= _Anode.Capacity) {
                _Anode.Capacity = _Anode.Count * 2;
            }

            node.id = ++_current_id;
            _Anode.Add(node);
        }

        /// <summary>
        /// 移除指定的node
        /// </summary>
        /// <param name="node"></param>
        private void RemoveNode(AtlasNode node) {
            AtlasNode replaceNode = AtlasNode.Empty;
            RemoveNode(node, ref replaceNode);
        }

        /// <summary>
        /// 移除指定node并由指定的替换
        /// </summary>
        /// <param name="node"></param>
        /// <param name="replace_node"></param>
        private void RemoveNode(AtlasNode node, ref AtlasNode replace_node) {
            for (int i = _Anode.Count - 1; i >= 0; i--) {
                AtlasNode atlasNode = _Anode[i];
                // 寻找id与指定相同的进行移除或者替换
                if (atlasNode.id == node.id) {
                    if (replace_node.Valid) { // 进行替换
                        replace_node.id = atlasNode.id;
                        _Anode[i] = replace_node;
                        break;
                    }

                    _Anode.RemoveAt(i); // 移除先前的
                    if (node.id != _current_id) {
                        break;
                    }

                    --_current_id;
                    break;
                }
            }
        }

        /// <summary>
        /// 进行全局extend
        /// </summary>
        /// <param name="pre_w"></param>
        /// <param name="pre_h"></param>
        protected virtual void WholeExtendAfter(int pre_w, int pre_h) {
            if (fld_fnExtend == null) {
                return;
            }

            fld_fnExtend(pre_w, pre_h, width, height);
        }

        /// <summary>
        /// 为img构建rect
        /// </summary>
        /// <param name="img_w"></param>
        /// <param name="img_h"></param>
        /// <param name="cost"></param>
        /// <returns></returns>
        protected virtual RectInt CreateRect(int img_w, int img_h, out int cost) {
            AtlasNode atlasNode = AtlasNode.Empty;
            cost = 0;
            while (true) {
                int count = _Anode.Count;
                for (int i = 0; i < count; i++) {
                    atlasNode = _Anode[i].Insert(this, ref cost, img_w, img_h);
                    if (atlasNode.Valid) {
                        break;
                    }
                }

                if (!atlasNode.Valid) {
                    int w = width;
                    int h = height;
                    if (width > height) { // 高度扩展
                        height *= 2;
                        WholeExtendAfter(w, h);
                        _Anode.Add(new AtlasNode(0, h, w, height - h));
                    }
                    else { // 宽度扩展
                        width *= 2;
                        WholeExtendAfter(w, h);
                        _Anode.Add(new AtlasNode(w, 0, width - w, h));
                    }
                }
                else {
                    break;
                }
            }
            // 更新use wh
            use_w = Mathf.Max(atlasNode.R, use_w);
            use_h = Mathf.Max(atlasNode.B, use_h);
            cost = 4;
            return new RectInt(atlasNode.x, atlasNode.y, atlasNode.w, atlasNode.h);
            /*cost = 0; // 消耗
            UAtlasNode atlasNode;
            UAtlasNode branch1;
            for (; (atlasNode = _first_wrapper.Insert(ref cost, img_w, img_h)) == null; _first_wrapper = new UAtlasNode(0, 0, width, height, _first_wrapper, branch1)) {
                int w = width;
                int h = height;
                if (width > height) {
                    height *= 2;
                    WholeExtendAfter();
                    _first_wrapper.ClipRect(true, w, h, use_w, use_h);
                    branch1 = new UAtlasNode(0, use_h, w, height - use_h);
                }
                else {
                    width *= 2;
                    WholeExtendAfter();
                    _first_wrapper.ClipRect(false, w, h, use_w, use_h);
                    branch1 = new UAtlasNode(use_w, 0, width - use_w, height);
                }
            }
            use_w = UHelper.Max(atlasNode.R, use_w);
            use_h = UHelper.Max(atlasNode.B, use_h);
            cost = 4;
            return new RectInt(atlasNode.x, atlasNode.y, atlasNode.w, atlasNode.h);*/
        }
    }
}