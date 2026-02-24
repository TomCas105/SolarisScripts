using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public record SpriteData : DataDefinition
{
    [System.Serializable]
    public struct SpriteVariant
    {
        public string variant;
        public string variantSprite;

        public SpriteVariant(string variant, string variantSprite)
        {
            this.variant = variant;
            this.variantSprite = variantSprite;
        }
    }

    public string type = "standard"; //standard for faction variants, random for random sprites
    public string spriteDefault = "sprite.png";
    public int pixelsPerUnit = 100; //should be kept at 100 for ships
    public string[] randomSprites = {};
    public List<SpriteVariant> spriteVariants;
    private Sprite LoadedSpriteDefault { get; set; }
    private Dictionary<string, Sprite> LoadedSpriteVariants { get; set; }
    private List<Sprite> LoadedRandomSprites { get; set; }

    public SpriteData()
    {
        spriteVariants = new();
        LoadedSpriteVariants = new();
        LoadedRandomSprites = new();
    }

    public void Merge(SpriteData _other)
    {
        if (type == "random")
        {
            foreach (var _sprite in _other.LoadedRandomSprites)
            {
                LoadedRandomSprites.Add(_sprite);
            }

            return;
        }

        var _variants = _other.LoadedSpriteVariants;

        foreach (var _variant in _variants)
        {
            if (!LoadedSpriteVariants.ContainsKey(_variant.Key))
            {
                LoadedSpriteVariants.Add(_variant.Key, _variant.Value);
            }
            else
            {
                LoadedSpriteVariants[_variant.Key] = _variant.Value;
            }
        }
    }

    public bool HasVariant(string _variant)
    {
        return LoadedSpriteVariants.ContainsKey(_variant);
    }

    public Sprite GetSprite(string _variant = "")
    {
        if (type == "random")
        {
            return LoadedRandomSprites[Random.Range(0, LoadedRandomSprites.Count)];
        }
        else if (type == "single")
        {
            return LoadedSpriteDefault;
        }

        if (_variant == "")
        {
            return LoadedSpriteDefault;
        }
        else
        {
            return LoadedSpriteVariants[_variant];
        }
    }

    public Sprite GetDefaultSprite()
    {
        if (type == "random")
        {
            return LoadedRandomSprites[Random.Range(0, LoadedRandomSprites.Count)];
        }

        return LoadedSpriteDefault;
    }

    public void LoadSprites(string _modulePath)
    {
        if (type == "random")
        {
            foreach (var _sprite in randomSprites)
            {
                LoadedRandomSprites.Add(LoadSprite(_modulePath + "/Textures/" + _sprite));
            }
            return;
        }

        LoadedSpriteDefault = LoadSprite(_modulePath + "/Textures/" + spriteDefault);

        if (type == "single")
        {
            return;
        }

        foreach (var _variant in spriteVariants)
        {
            LoadedSpriteVariants.Add(_variant.variant, LoadSprite(_modulePath + "/Textures/" + _variant.variantSprite));
        }
    }

    private Sprite LoadSprite(string _path)
    {
        Texture2D _texture = AssetManager.LoadTexture(_path);

        if (_texture == null)
        {
            return null;
        }

        return Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }
}
