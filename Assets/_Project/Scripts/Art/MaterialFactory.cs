using UnityEngine;

public static class MaterialFactory
{
    public static Material CreateBoardMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.5f, 0.3f, 0.2f);
        mat.SetFloat("_Metallic", 0.1f);
        mat.SetFloat("_Smoothness", 0.4f);
        return mat;
    }

    public static Material CreateLaminateMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.7f, 0.7f, 0.7f);
        mat.SetFloat("_Metallic", 0.5f);
        mat.SetFloat("_Smoothness", 0.5f);
        return mat;
    }

    public static Material CreatePocketMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Color.black;
        mat.SetFloat("_Metallic", 0.1f);
        mat.SetFloat("_Smoothness", 0.4f);
        return mat;
    }

    public static Material CreateWhiteCoinMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Color.white;
        mat.SetFloat("_Smoothness", 0.6f);
        return mat;
    }

    public static Material CreateBlackCoinMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Color.black;
        mat.SetFloat("_Smoothness", 0.6f);
        return mat;
    }

    public static Material CreateQueenMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Color.red;
        mat.SetFloat("_Smoothness", 0.6f);
        return mat;
    }

    public static Texture2D CreateWoodNormalMap(int size = 1024)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noise = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                float intensity = (noise - 0.5f) * 2f;
                pixels[y * size + x] = new Color(0.5f + intensity, 0.5f + intensity, 1f, 1f);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
