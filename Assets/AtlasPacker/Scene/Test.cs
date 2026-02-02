using System;
using System.Collections.Generic;
using System.Linq;
using AtlasPacker;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {
    public Sprite[] Asprite;
    public RenderTexture rtx;
    public string key;
    private SpriteRenderer _spr;
    private List<AtlasRect> _fin;
    public Texture2D texture;
    
    // Start is called before the first frame update
    void Start() {
        _spr = gameObject.GetComponent<SpriteRenderer>();
        var rects = GenerateAtlasRects(Asprite);
        AtlasPacker.AtlasPacker.PackTexture(rects, ref _fin, out rtx);
        RenderTexture.active = rtx;
        texture = new Texture2D(rtx.width, rtx.height, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, rtx.width, rtx.height), 0, 0);
        texture.Apply();
        RenderTexture.active = null;

        Sprite sp = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 64);
        _spr.sprite = sp;
    }
    
    /// <summary>
    /// 生成atlas rect
    /// </summary>
    /// <param name="sprites"></param>
    /// <returns></returns>
    private static AtlasRect[] GenerateAtlasRects(Sprite[] sprites) {
        if (sprites == null || sprites.Length == 0) {
            return null;
        }
        int count = sprites.Length;
        AtlasRect[] rects = new AtlasRect[count];
        for (int i = 0; i < count; i++) {
            Sprite sp = sprites[i];
            AtlasRect rc = new AtlasRect() {
                id = sp.name,
                tex = sp.texture,
                x = (int)sp.rect.x,
                y = (int)sp.rect.y,
                w = (int)sp.rect.width,
                h = (int)sp.rect.height,
            };
            rects[i] = rc;
        }
        
        return rects;
    }
}
