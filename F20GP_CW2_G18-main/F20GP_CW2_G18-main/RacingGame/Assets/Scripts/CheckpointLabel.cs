using UnityEngine;
using TMPro;

public class CheckpointLabel : MonoBehaviour
{
    [Header("Label Settings")]
    public string labelText = "Checkpoint";
    public Vector3 offset = new Vector3(0, 2f, 0);
    public float fontSize = 20f;
    public Color fontColor = Color.white;

    private TextMeshPro textMesh;

    void Start()
    {
        // GameObject textObj = new GameObject("CheckpointLabel");
        // textObj.transform.SetParent(transform);
        // textObj.transform.localPosition = offset;
        // textObj.transform.localRotation = Quaternion.identity;

        // textMesh = textObj.AddComponent<TextMeshPro>();
        // textMesh.text = labelText;
        // textMesh.fontSize = fontSize;
        // textMesh.alignment = TextAlignmentOptions.Center;
        // textMesh.color = fontColor;

        // textMesh.enableWordWrapping = false;
        // textMesh.isOrthographic = false;
    }

}
