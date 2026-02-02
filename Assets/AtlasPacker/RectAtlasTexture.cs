using UnityEngine;

namespace AtlasPacker {
    /// <summary>
    /// rect atlas生成器
    /// </summary>
    public class RectAtlasTexture : RectAtlas {
        private RenderTexture _tx; // 内部维护的纹理
        public string name_header = ""; // header
        public bool use_mipmap; // 是否使用mipmap
        public int depth; // depth
        private RenderTextureFormat _format; // rtx的格式
        private static RenderTexture _firstTxOnCreateRect; // 在创建rect时的pre rtx对象
        public bool copy_previous_image = true;

        #region 属性方法

        /// <summary>
        /// 主texture
        /// </summary>
        public RenderTexture Tx => _tx;
        
        /// <summary>
        /// 纹理的texture revw
        /// </summary>
        public float texture_rw { get; private set; }
        
        /// <summary>
        /// 纹理的texture revh
        /// </summary>
        public float texture_rh { get; private set; }

        #endregion

        #region ctor

        public RectAtlasTexture(int w, int h, string name_header = "", bool use_mipmap = false, int depth = 0,
            RenderTextureFormat format = RenderTextureFormat.ARGB32) : base(0, 0) {
            this.name_header = name_header;
            this.use_mipmap = use_mipmap;
            this.depth = depth;
            _format = format;
            if (w <= 0 || h <= 0) {
                return;
            }

            Clear(w, h);
        }

        #endregion
        
        public override void Clear(int w, int h) {
            base.Clear(w, h);
            _tx = new RenderTexture(width, height, depth, _format);
            _tx.filterMode = FilterMode.Point;
            texture_rw = 1f / width;
            texture_rh = 1f / height;
        }

        protected override void WholeExtendAfter(int pre_w, int pre_h) {
            _tx = CreateTexture(width, height, _tx == _firstTxOnCreateRect ? null : _tx,
                name_header + "(" + width + "," + height + ")");
            _tx.filterMode = FilterMode.Point;
            texture_rw = 1f / width;
            texture_rh = 1f / height;
            base.WholeExtendAfter(pre_w, pre_h);
        }

        /// <summary>
        /// 为指定尺寸的img打包rect
        /// </summary>
        /// <param name="img_w"></param>
        /// <param name="img_h"></param>
        /// <param name="cost"></param>
        /// <param name="first_tx">先前纹理</param>
        /// <param name="auto_dispose">自动dispose先前纹理</param>
        /// <returns></returns>
        public RectInt CreateRect(int img_w, int img_h, out int cost, out RenderTexture first_tx,
            bool auto_dispose = true) {
            first_tx = _firstTxOnCreateRect = _tx; // 保存create前的tx
            RectInt rc = CreateRect(img_w, img_h, out cost); // 为指定的宽高的图形构建一个rect
            if (first_tx == _tx) {
                return rc;
            }

            if (copy_previous_image) {
                MeshDrawer.Clear(_tx); // 清空rtx
                // 复制纹理
                MeshDrawer.PasteToCen(_tx, first_tx, first_tx.width * 0.5f, first_tx.height * 0.5f); // 复制纹理
            }
            if (!auto_dispose) {
                return rc;
            }

            first_tx.Release(); // 将先前的dispose
            return rc;
        }
        
        /// <summary>
        /// 构建texture
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="first_tx">create前的纹理</param>
        /// <param name="tx_name"></param>
        /// <returns></returns>
        private RenderTexture CreateTexture(int w, int h, RenderTexture first_tx = null, string tx_name = null) {
            if (string.IsNullOrEmpty(tx_name)) {
                tx_name = name_header + w + "x" + h;
            }
            // 构建纹理
            RenderTexture tx = new RenderTexture(w, h, depth, _format);
            tx.filterMode = FilterMode.Point;
            tx.name = tx_name;
            MeshDrawer.Clear(tx); // 清空内容
            RenderTexture.active = null;
            if (first_tx == null) {
                return tx;
            }

            first_tx.Release();
            return tx;
        }

        /// <summary>
        /// destroy
        /// </summary>
        public void Destruct() => _tx.Release();

        /// <summary>
        /// 输出纹理
        /// </summary>
        /// <returns></returns>
        public RenderTexture ExtractTx() {
            RenderTexture tx = _tx;
            _tx = null;
            return tx;
        }
    }
}