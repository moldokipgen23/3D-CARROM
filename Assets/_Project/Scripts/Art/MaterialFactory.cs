using UnityEngine;

public static class MaterialFactory
{
    private static Shader GetShader()
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");
        return shader;
    }

    public static Material CreateBoardMaterial()
    {
        Material mat = new Material(GetShader());
        mat.color = new Color(0.45f, 0.25f, 0.12f);
        mat.SetFloat("_Metallic", 0.15f);
        mat.SetFloat("_Glossiness", 0.65f);
        Texture2D woodTex = ProceduralTextures.CreateWoodTexture(512, 512);
        mat.mainTexture = woodTex;
        return mat;
    }

    public static Material CreateFeltMaterial()
    {
        Material mat = new Material(GetShader());
        mat.color = new Color(0.15f, 0.38f, 0.18f);
        mat.SetFloat("_Metallic", 0.02f);
        mat.SetFloat("_Glossiness", 0.6f);
        mat.mainTexture = ProceduralTextures.CreateFeltTexture(512, 512);
        return mat;
    }

    public static Material CreatePocketMaterial()
    {
        Material mat = new Material(GetShader());
        mat.color = new Color(0.02f, 0.02f, 0.02f);
        mat.SetFloat("_Metallic", 0.1f);
        mat.SetFloat("_Glossiness", 0.3f);
        return mat;
    }

    public static Material CreateGoldRimMaterial()
    {
        Material mat = new Material(GetShader());
        mat.color = new Color(0.85f, 0.7f, 0.2f);
        mat.SetFloat("_Metallic", 0.85f);
        mat.SetFloat("_Glossiness", 0.95f);
        return mat;
    }

    public static Material CreateWhiteCoinMaterial()
    {
        Material mat = new Material(GetShader());
        mat.color = new Color(0.95f, 0.92f, 0.85f);
        mat.SetFloat("_Metallic", 0.6f);
        mat.SetFloat("_Glossiness", 0.85f);
        mat.mainTexture = ProceduralTextures.CreateCoinTexture(256, 256, new Color(0.95f, 0.92f, 0.85f), true);
        return mat;
    }

    public static Material CreateBlackCoinMaterial()
    {
        Material mat = new Material(GetShader());
        mat.color = new Color(0.12f, 0.1f, 0.08f);
        mat.SetFloat("_Metallic", 0.5f);
        mat.SetFloat("_Glossiness", 0.8f);
        mat.mainTexture = ProceduralTextures.CreateCoinTexture(256, 256, new Color(0.12f, 0.1f, 0.08f), false);
        return mat;
    }

    public static Material CreateQueenMaterial()
    {
        Material mat = new Material(GetShader());
        mat.color = new Color(0.8f, 0.12f, 0.12f);
        mat.SetFloat("_Metallic", 0.5f);
        mat.SetFloat("_Glossiness", 0.9f);
        mat.mainTexture = ProceduralTextures.CreateCoinTexture(256, 256, new Color(0.8f, 0.12f, 0.12f), false);
        return mat;
    }

    public static Material CreateStrikerMaterial()
    {
        Material mat = new Material(GetShader());
        mat.color = new Color(0.92f, 0.88f, 0.82f);
        mat.SetFloat("_Metallic", 0.8f);
        mat.SetFloat("_Glossiness", 0.92f);
        mat.mainTexture = ProceduralTextures.CreateMetallicTexture(256, 256, new Color(0.92f, 0.88f, 0.82f));
        return mat;
    }

    public static Material CreateTableMaterial()
    {
        Material mat = new Material(GetShader());
        mat.color = new Color(0.18f, 0.11f, 0.06f);
        mat.SetFloat("_Metallic", 0.15f);
        mat.SetFloat("_Glossiness", 0.65f);
        mat.mainTexture = ProceduralTextures.CreateWoodTexture(512, 512);
        return mat;
    }

    public static Texture2D CreateWoodNormalMap(int size = 512)
    {
        Texture2D tex = new Texture2D(size, size);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noise = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                float intensity = (noise - 0.5f) * 2f;
                tex.SetPixel(x, y, new Color(0.5f + intensity, 0.5f + intensity, 1f, 1f));
            }
        }
        tex.Apply();
        return tex;
    }
}
