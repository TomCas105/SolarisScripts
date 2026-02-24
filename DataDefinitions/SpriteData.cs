using System.Collections.Generic;
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

    public string type = "standard"; //standard for faction variants, random for randomly picked sprites
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
    public override void OnPostLoad(string modulePath)
    {
        LoadSprites(modulePath);
    }

    public void Merge(SpriteData other)
    {
        if (type == "random")
        {
            foreach (var _sprite in other.LoadedRandomSprites)
            {
                LoadedRandomSprites.Add(_sprite);
            }

            return;
        }

        var _variants = other.LoadedSpriteVariants;

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

    public bool HasVariant(string variant)
    {
        return LoadedSpriteVariants.ContainsKey(variant);
    }

    public Sprite GetSprite(string variant = "")
    {
        if (type == "random")
        {
            return LoadedRandomSprites[Random.Range(0, LoadedRandomSprites.Count)];
        }
        else if (type == "single")
        {
            return LoadedSpriteDefault;
        }

        if (variant == "")
        {
            return LoadedSpriteDefault;
        }
        else
        {
            return LoadedSpriteVariants[variant];
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

    public void LoadSprites(string modulePath)
    {
        if (type == "random")
        {
            foreach (var _sprite in randomSprites)
            {
                LoadedRandomSprites.Add(LoadSprite(modulePath + "/Textures/" + _sprite));
            }
            return;
        }

        LoadedSpriteDefault = LoadSprite(modulePath + "/Textures/" + spriteDefault);

        if (type == "single")
        {
            return;
        }

        foreach (var _variant in spriteVariants)
        {
            LoadedSpriteVariants.Add(_variant.variant, LoadSprite(modulePath + "/Textures/" + _variant.variantSprite));
        }
    }

    private Sprite LoadSprite(string path)
    {
        Texture2D _texture = AssetManager.LoadTexture(path);

        if (_texture == null)
        {
            return null;
        }

        return Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }
}
