using UnityEngine;

namespace AtlasPacker {
    public interface IAtlasRect {
        string id { get; }
        
        Texture tex { get; set; }
        
        public int x { get; set; }
        
        public int y { get; set; }
        
        int w { get; set; }
        
        int h { get; set; }
        
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

        public RectInt ToRect() => new RectInt(x, y, w, h);

        public Rect ToUvRect() =>
            new Rect((float)x / tex.width, (float)y / tex.height, (float)w / tex.width, (float)h / tex.height);
    }
}