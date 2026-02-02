using System;
using UnityEngine;

namespace AtlasPacker {
    /// <summary>
    /// 普通网格绘制器
    /// </summary>
    public class MeshDrawer {
        protected Rect _rectuv; // uv的矩形
        public Color32 color = new Color32(255, 255, 255, 255); // 顶点颜色
        protected int _cver; // 顶点数
        protected int _ctri; // 三角形数
        protected Vector3[] _Avertices;
        protected Vector2[] _Amesh_uv; // 网格uv
        protected Color32[] _Acolor;
        protected int[] _Atri;

        #region 属性方法

        /// <summary>
        /// 顶点数
        /// </summary>
        public int VerticesCount => _cver;

        /// <summary>
        /// 三角形配置数
        /// </summary>
        public int TriangleCount => _ctri;

        #endregion
        
        #region 顶点、三角形
        
        /// <summary>
        /// 添加三角形
        /// </summary>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public MeshDrawer Tri(int t0, int t1, int t2) {
            AllocTri(_ctri, 18);

            _Atri[_ctri++] = _cver + t0;
            _Atri[_ctri++] = _cver + t1;
            _Atri[_ctri++] = _cver + t2;
            return this;
        }

        /// <summary>
        /// 添加顶点
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="uv"></param>
        /// <param name="transform_mat"></param>
        /// <returns></returns>
        public MeshDrawer Ver(float x, float y, Vector2 uv, Matrix4x4 transform_mat) {
            AllocVer(_cver);

            int num = _cver % 3;
            _Amesh_uv[_cver] = uv;
            Vector3 point = new Vector3(x, y, 0f);
            point = transform_mat.MultiplyPoint3x4(point);
            _Avertices[_cver].Set(point.x, point.y, 0f);
            _Acolor[_cver++] = color;
            return this;
        }
        
        /// <summary>
        /// 根据tri扩展triangle数组
        /// </summary>
        /// <param name="tri"></param>
        /// <param name="back"></param>
        /// <returns></returns>
        public MeshDrawer AllocTri(int tri, int back = 60) {
            if (_Atri == null || tri >= _Atri.Length) {
                Array.Resize(ref _Atri, tri + back);
            }

            return this;
        }
        
        /// <summary>
        /// 申请顶点
        /// </summary>
        /// <param name="ver"></param>
        /// <param name="back"></param>
        /// <returns></returns>
        public MeshDrawer AllocVer(int ver, int back = 64) {
            if (_Avertices == null || ver >= _Avertices.Length) {
                ver += back;
                Array.Resize(ref _Avertices, ver);
                Array.Resize(ref _Amesh_uv, ver);
                Array.Resize(ref _Acolor, ver);
            }

            return this;
        }

        #endregion

        /// <summary>
        /// 获取uv配置
        /// </summary>
        /// <param name="cver"></param>
        /// <returns></returns>
        public Vector2[] GetRawUv(out int cver) {
            cver = _cver;
            return _Amesh_uv;
        }

        /// <summary>
        /// 获取三角形配置
        /// </summary>
        /// <param name="ctri"></param>
        /// <returns></returns>
        public int[] GetRawTri(out int ctri) {
            ctri = _ctri;
            return _Atri;
        }

        /// <summary>
        /// 获取顶点颜色
        /// </summary>
        /// <param name="cver"></param>
        /// <returns></returns>
        public Color32[] GetRawColor(out int cver) {
            cver = _cver;
            return _Acolor;
        }

        /// <summary>
        /// 获取顶点
        /// </summary>
        /// <param name="cver">输出顶点数</param>
        /// <returns></returns>
        public Vector3[] GetRawVer(out int cver) {
            cver = _cver;
            return _Avertices;
        }

        /// <summary>
        /// 获取ver
        /// </summary>
        /// <returns></returns>
        public Vector3[] GetVerticesArray() => _Avertices;

        /// <summary>
        /// 获取uv
        /// </summary>
        /// <returns></returns>
        public Vector2[] GetUvArray() => _Amesh_uv;

        /// <summary>
        /// 获取color
        /// </summary>
        /// <returns></returns>
        public Color32[] GetColorArray() => _Acolor;

        /// <summary>
        /// 获取tri
        /// </summary>
        /// <returns></returns>
        public int[] GetTrangleArray() => _Atri;

        /// <summary>
        /// 清理索引
        /// </summary>
        public void Clear() {
            _cver = 0;
            _ctri = 0;
        }

        /// <summary>
        /// 使用GL绘制网格到
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="use_mtr"></param>
        /// <param name="tri_max"></param>
        public static void RenderToGLMesh(MeshDrawer mesh, Material use_mtr, int tri_max = -1) {
            tri_max = tri_max < 0 ? mesh._ctri : tri_max;
            if (tri_max <= 0)
                return;
            use_mtr.SetPass(0);
            GL.Begin(GL.TRIANGLES);
            Vector3[] vertexArray = mesh.GetRawVer(out int _);
            Vector2[] uvArray = mesh.GetRawUv(out int _);
            Color32[] colorArray = mesh.GetRawColor(out int _);
            int[] triangleArray = mesh.GetRawTri(out int _);

            int ver_i = 0;
            while (--tri_max >= 0) {
                int verIndex = triangleArray[ver_i++];
                GL.Color(colorArray[verIndex]);
                GL.TexCoord(uvArray[verIndex]);
                GL.Vertex(vertexArray[verIndex]);
            }
            
            GL.End();
        }
        
        /// <summary>
        /// clear tx内容
        /// </summary>
        /// <param name="tx"></param>
        public static void Clear(RenderTexture tx) {
            if (tx != null) {
                Graphics.SetRenderTarget(tx); // 设定渲染目标，以特定亚瑟进行clear
                GL.Clear(true, true, new Color(0, 0, 0, 0));
                Graphics.SetRenderTarget(null);
            }
        }

        // paste用
        private static Material paste_mtr;
        
        /// <summary>
        /// 将src paste到dst的xy位置
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="src"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void PasteToCen(RenderTexture dst, RenderTexture src, float x, float y) {
            if (paste_mtr == null) {
                paste_mtr = new Material(Shader.Find("Buffer/NormalClearing"));
            }
            float draw_realw = src.width; // 绘制的实际width
            float draw_realh = src.height; // 绘制的实际height
            Vector2 draw_dst_lb = new Vector2(x - draw_realw * 0.5f, y - draw_realh * 0.5f); // src左下角在dst的位置
            // 使用指定区域绘制
            draw_realw = src.width; // 实际绘制的宽度
            draw_realh = src.height; // 实际绘制的像素高度
            draw_dst_lb = new Vector2(x - draw_realw * 0.5f, y - draw_realh * 0.5f); // 左下角位置
            float xpos = draw_dst_lb.x / dst.width; // x位置在dst上的ratio
            float wpos = draw_realw / dst.width; // 要绘制的宽度在dst上的ratio
            float ypos = draw_dst_lb.y / dst.height;
            float hpos = draw_realh / dst.height;
            paste_mtr.SetTexture("_MainTex", src);
            Vector4 basePos = new Vector4(xpos, ypos, wpos, hpos); // 绘制位置
            paste_mtr.SetVector("_BasePos", basePos);
            Graphics.Blit(src, dst, paste_mtr); // 将src使用useMtr绘制到dst
            paste_mtr.SetTexture("_MainTex", null); // 清空所使用的材质的tx
        }
    }
}