using UnityEngine;

namespace AtlasPacker {
    /// <summary>
    /// atlas单个rect with id
    /// </summary>
    public struct AtlasRect {
        public string id;
        public Texture tex;
        public int x;
        public int y;
        public int w;
        public int h;
        
        public Vector2 LB => new Vector2((float)x / tex.width, (float)y / tex.height);
        
        public Vector2 LT => new Vector2((float)x / tex.width, (float)ymax / tex.height);
        
        public Vector2 RT => new Vector2((float)xmax / tex.width, (float)ymax / tex.height);
        
        public Vector2 RB => new Vector2((float)xmax / tex.width, (float)y / tex.height);

        /// <summary>
        /// 最小x
        /// </summary>
        public int xmin {
            get => Mathf.Min(x, x + w);
            set {
                int pre_xmax = xmax;
                x = value;
                w = pre_xmax - x;
            }
        }

        /// <summary>
        /// x的最大
        /// </summary>
        public int xmax {
            get => Mathf.Max(x, x + w);
            set => w = value - x;
        }

        /// <summary>
        /// 最小y
        /// </summary>
        public int ymin {
            get => Mathf.Min(y, y + h);
            set {
                int pre_ymax = ymax;
                y = value;
                h = pre_ymax - y;
            }
        }

        /// <summary>
        /// 最大y
        /// </summary>
        public int ymax {
            get => Mathf.Max(y, y + h);
            set => h = value - y;
        }
        
        public AtlasRect(string id, Texture tex, int x = 0, int y = 0, int w = 0, int h = 0) {
            this.id = id;
            this.tex = tex;
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
        }

        /// <summary>
        /// set
        /// </summary>
        /// <param name="rc"></param>
        public void Set(RectInt rc) {
            x = rc.x;
            y = rc.y;
            w = rc.width;
            h = rc.height;
        }

        public override string ToString() {
            return id + " (" + x + ", " + y + ", " + w + ", " + h + ")";
        }
    }
}