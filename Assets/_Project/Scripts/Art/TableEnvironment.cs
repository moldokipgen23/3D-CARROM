using UnityEngine;

public class TableEnvironment : MonoBehaviour
{
    [Header("Table")]
    public float tableSize = 5f;
    public float tableThickness = 0.12f;

    [Header("Background")]
    public Color tableColor = new Color(0.18f, 0.11f, 0.06f);
    public Color floorColor = new Color(0.05f, 0.04f, 0.03f);

    private void Start()
    {
        CreateEnvironment();
    }

    private void CreateEnvironment()
    {
        CreateTable();
        CreateTableLegs();
        CreateFloor();
        CreateBackground();
        CreateVignetteEdges();
    }

    private void CreateTable()
    {
        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "Table";
        table.transform.position = new Vector3(0, -tableThickness / 2f, 0);
        table.transform.localScale = new Vector3(tableSize, tableThickness, tableSize);

        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");

        Material mat = new Material(shader);
        mat.color = tableColor;
        mat.SetFloat("_Metallic", 0.15f);
        mat.SetFloat("_Glossiness", 0.65f);
        table.GetComponent<MeshRenderer>().sharedMaterial = mat;

        GameObject feltTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        feltTop.name = "FeltSurface";
        feltTop.transform.position = new Vector3(0, 0.001f, 0);
        feltTop.transform.localScale = new Vector3(tableSize - 0.1f, 0.002f, tableSize - 0.1f);

        Material feltMat = new Material(shader);
        feltMat.color = new Color(0.15f, 0.38f, 0.18f);
        feltMat.SetFloat("_Metallic", 0.02f);
        feltMat.SetFloat("_Glossiness", 0.6f);
        feltTop.GetComponent<MeshRenderer>().sharedMaterial = feltMat;
    }

    private void CreateTableLegs()
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");

        Material legMat = new Material(shader);
        legMat.color = new Color(0.3f, 0.18f, 0.08f);
        legMat.SetFloat("_Metallic", 0.1f);
        legMat.SetFloat("_Glossiness", 0.5f);

        float legOffset = tableSize / 2f - 0.3f;
        float legHeight = 0.5f;

        Vector3[] legPositions = {
            new Vector3(-legOffset, -tableThickness - legHeight / 2f, -legOffset),
            new Vector3(legOffset, -tableThickness - legHeight / 2f, -legOffset),
            new Vector3(-legOffset, -tableThickness - legHeight / 2f, legOffset),
            new Vector3(legOffset, -tableThickness - legHeight / 2f, legOffset)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = $"TableLeg_{i}";
            leg.transform.position = legPositions[i];
            leg.transform.localScale = new Vector3(0.08f, legHeight / 2f, 0.08f);
            leg.GetComponent<MeshRenderer>().sharedMaterial = legMat;
        }
    }

    private void CreateFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.position = new Vector3(0, -tableThickness - 1.02f, 0);
        floor.transform.localScale = new Vector3(20f, 1f, 20f);

        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");

        Material floorMat = new Material(shader);
        floorMat.color = floorColor;
        floorMat.SetFloat("_Metallic", 0.05f);
        floorMat.SetFloat("_Glossiness", 0.3f);
        floor.GetComponent<MeshRenderer>().sharedMaterial = floorMat;
    }

    private void CreateBackground()
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");

        Material wallMat = new Material(shader);
        wallMat.color = new Color(0.06f, 0.05f, 0.04f);
        wallMat.SetFloat("_Metallic", 0f);
        wallMat.SetFloat("_Glossiness", 0.1f);

        float wallDistance = 8f;
        float wallHeight = 6f;

        CreateWall("Wall_Back", new Vector3(0, wallHeight / 2f - 1f, wallDistance),
            new Vector3(20f, wallHeight, 0.1f), wallMat);

        CreateWall("Wall_Front", new Vector3(0, wallHeight / 2f - 1f, -wallDistance),
            new Vector3(20f, wallHeight, 0.1f), wallMat);

        CreateWall("Wall_Left", new Vector3(-wallDistance, wallHeight / 2f - 1f, 0),
            new Vector3(0.1f, wallHeight, 20f), wallMat);

        CreateWall("Wall_Right", new Vector3(wallDistance, wallHeight / 2f - 1f, 0),
            new Vector3(0.1f, wallHeight, 20f), wallMat);
    }

    private void CreateWall(string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        wall.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private void CreateVignetteEdges()
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");

        Color edgeColor = new Color(0.25f, 0.15f, 0.08f);
        Material edgeMat = new Material(shader);
        edgeMat.color = edgeColor;
        edgeMat.SetFloat("_Metallic", 0.2f);
        edgeMat.SetFloat("_Glossiness", 0.6f);

        float boardHalf = 1.45f;
        float edgeHeight = 0.02f;
        float edgeWidth = 0.04f;

        string[] edgeNames = { "Edge_N", "Edge_S", "Edge_E", "Edge_W" };
        Vector3[] edgePositions = {
            new Vector3(0, edgeHeight / 2f, boardHalf + edgeWidth / 2f),
            new Vector3(0, edgeHeight / 2f, -(boardHalf + edgeWidth / 2f)),
            new Vector3(boardHalf + edgeWidth / 2f, edgeHeight / 2f, 0),
            new Vector3(-(boardHalf + edgeWidth / 2f), edgeHeight / 2f, 0)
        };
        Vector3[] edgeScales = {
            new Vector3(2.9f + edgeWidth * 2f, edgeHeight, edgeWidth),
            new Vector3(2.9f + edgeWidth * 2f, edgeHeight, edgeWidth),
            new Vector3(edgeWidth, edgeHeight, 2.9f + edgeWidth * 2f),
            new Vector3(edgeWidth, edgeHeight, 2.9f + edgeWidth * 2f)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            edge.name = edgeNames[i];
            edge.transform.position = edgePositions[i];
            edge.transform.localScale = edgeScales[i];
            edge.GetComponent<MeshRenderer>().sharedMaterial = edgeMat;
        }
    }
}
