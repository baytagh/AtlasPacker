using System.Collections.Generic;
using UnityEngine;

namespace AtlasPacker {
    public static class AtlasPacker {
        private static MeshDrawer _mesh_buf;
        private static Material _mtr_buf;
        private static RectAtlasTexture _gen; // 生成器
        private static Texture _now_tx_source;

        /// <summary>
        /// 移除用于绘制的网格
        /// </summary>
        public static void ReleaseBufMesh() {
            if (_mesh_buf == null) {
                return;
            }

            _mesh_buf = null;
            Object.Destroy(_mtr_buf);
            _mtr_buf = null;
            if (_now_tx_source != null && _now_tx_source is RenderTexture rtx) {
                rtx.Release();
            }
            Object.Destroy(_now_tx_source);
            _now_tx_source = null;
        }

        /// <summary>
        /// check buf mesh
        /// </summary>
        private static void CheckBufMesh() {
            if (_mesh_buf == null) { // 初始化生成器
                _gen = new RectAtlasTexture(0, 0, "GEN_ATLAS_");
                _mesh_buf = new MeshDrawer();
                Shader shad = Shader.Find("Buffer/Normal");
                _mtr_buf = new Material(shad);
                _mtr_buf.EnableKeyword("NO_PIXELSNAP");
            }
            
            _gen.Clear(128, 128);
        }

        /// <summary>
        /// reference type
        /// </summary>
        /// <param name="Auv0"></param>
        /// <param name="tx"></param>
        /// <param name="margin_lt"></param>
        /// <param name="margin_rb"></param>
        /// <param name="src"></param>
        public static void PackTexture(IAtlasRect[] Auv0, out RenderTexture tx, int margin_lt = 1, int margin_rb = 1,
            RectAtlasTexture src = null) {
            CheckBufMesh();
            src = src ?? _gen;
            
            // 初始化输出
            // 遍历uv
            int len = Auv0.Length;
            for (int i = 0; i < len; i++) {
                IAtlasRect uv = Auv0[i];
                // 保存先前用来绘制
                RectInt pre = uv.ToRect();
                Rect pre_uv = uv.ToUvRect();
                Texture pre_tex = uv.tex;
                // check更新绘制的纹理到网格
                FineMeshToTexture(uv.tex, src: src);
                RectInt atlas_rectuv = src.CreateRect(pre.width + margin_lt + margin_rb, pre.height + margin_lt + margin_rb,
                    out _, out _);
                // 进行偏移
                atlas_rectuv.x += margin_lt;
                atlas_rectuv.y += margin_lt;
                atlas_rectuv.width = pre.width;
                atlas_rectuv.height = pre.height;
                
                uv.Set(atlas_rectuv);
                // 绘制uv及ver
                _mesh_buf.Tri(0, 1, 2).Tri(0, 2, 3);
                _mesh_buf.Ver(atlas_rectuv.x, atlas_rectuv.y, new Vector2(pre_uv.x, pre_uv.y), Matrix4x4.identity);
                _mesh_buf.Ver(atlas_rectuv.x, atlas_rectuv.yMax, new Vector2(pre_uv.x, pre_uv.yMax), Matrix4x4.identity);
                _mesh_buf.Ver(atlas_rectuv.xMax, atlas_rectuv.yMax, new Vector2(pre_uv.xMax, pre_uv.yMax), Matrix4x4.identity);
                _mesh_buf.Ver(atlas_rectuv.xMax, atlas_rectuv.y, new Vector2(pre_uv.xMax, pre_uv.y), Matrix4x4.identity);
            }
            
            // 输出纹理
            FlushBufferRenderToTexture();
            // 生成新的rect
            for (int i = 0; i < Auv0.Length; i++) {
                IAtlasRect atlasRect = Auv0[i];
                atlasRect.tex = src.Tx;
            }
            tx = src.ExtractTx();
        }

        /// <summary>
        /// 打包纹理，uvrect为lb为基准点。x→正，y↑正
        /// </summary>
        /// <param name="Auv0"></param>
        /// <param name="Auv"></param>
        /// <param name="tx"></param>
        /// <param name="margin_lt"></param>
        /// <param name="margin_rb"></param>
        /// <param name="src">使用的src texture</param>
        public static void PackTexture(AtlasRect[] Auv0, ref List<AtlasRect> Auv, out RenderTexture tx,
            int margin_lt = 1, int margin_rb = 1, RectAtlasTexture src = null) {
            CheckBufMesh();
            src = src ?? _gen;
            
            // 初始化输出
            Auv = Auv ?? new List<AtlasRect>();
            Auv.Clear();
            // 遍历uv
            int len = Auv0.Length;
            for (int i = 0; i < len; i++) {
                AtlasRect uv = default;
                AtlasRect pre = Auv0[i];
                // check更新绘制的纹理到网格
                FineMeshToTexture(pre.tex, src: src);
                RectInt atlas_rectuv = src.CreateRect(pre.w + margin_lt + margin_rb, pre.h + margin_lt + margin_rb,
                    out _, out _);
                // 进行偏移
                atlas_rectuv.x += margin_lt;
                atlas_rectuv.y += margin_lt;
                atlas_rectuv.width = pre.w;
                atlas_rectuv.height = pre.h;
                
                uv.id = pre.id;
                uv.Set(atlas_rectuv);
                // 绘制uv及ver
                _mesh_buf.Tri(0, 1, 2).Tri(0, 2, 3);
                _mesh_buf.Ver(atlas_rectuv.x, atlas_rectuv.y, pre.LB, Matrix4x4.identity);
                _mesh_buf.Ver(atlas_rectuv.x, atlas_rectuv.yMax, pre.LT, Matrix4x4.identity);
                _mesh_buf.Ver(atlas_rectuv.xMax, atlas_rectuv.yMax, pre.RT, Matrix4x4.identity);
                _mesh_buf.Ver(atlas_rectuv.xMax, atlas_rectuv.y, pre.RB, Matrix4x4.identity);
                Auv.Add(uv);
            }
            
            // 输出纹理
            FlushBufferRenderToTexture();
            // 生成新的rect
            for (int i = 0; i < Auv.Count; i++) {
                AtlasRect atlasRect = Auv[i];
                atlasRect.tex = src.Tx;
                Auv[i] = atlasRect;
            }
            tx = src.ExtractTx();
        }

        /// <summary>
        /// 更新网格到gen纹理
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="force"></param>
        /// <param name="src"></param>
        private static void FineMeshToTexture(Texture tx, bool force = false, RectAtlasTexture src = null) {
            if (_mesh_buf == null || tx == _now_tx_source) {
                return;
            }

            src = src ?? _gen;
            // 渲染网格到gen的纹理
            if (_mesh_buf.TriangleCount > 0 && _now_tx_source != null) {
                // 将网格的内容渲染到纹理的rtx
                _mtr_buf.SetTexture("_MainTex", _now_tx_source);
                RenderTexture rtx = src.Tx;
                GL.PushMatrix();
                Graphics.SetRenderTarget(rtx);
                GL.LoadProjectionMatrix(Matrix4x4.Ortho(0f, rtx.width, 0f, rtx.height, -1f, 100f));
                MeshDrawer.RenderToGLMesh(_mesh_buf, _mtr_buf, _mesh_buf.TriangleCount);
                Graphics.SetRenderTarget(null);
                GL.PopMatrix();
                _mesh_buf.Clear();
                src.copy_previous_image = true;
            }
            _now_tx_source = tx;
        }

        /// <summary>
        /// 更新绘制buffer到纹理
        /// </summary>
        private static void FlushBufferRenderToTexture() {
            FineMeshToTexture(null);
            GL.Flush();
        }
    }
}