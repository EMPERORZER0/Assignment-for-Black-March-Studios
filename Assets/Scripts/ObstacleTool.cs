using UnityEditor;
using UnityEngine;

public class ObstacleToolWindow : EditorWindow
{
    private ObstacleData obstacleData;

    [MenuItem("Tools/Obstacle Tool")]
    public static void ShowWindow()
    {
        GetWindow<ObstacleToolWindow>("Obstacle Tool");
    }

    private void OnGUI()
    {
        if (obstacleData == null)
        {
            obstacleData = (ObstacleData)EditorGUILayout.ObjectField("Obstacle Data", obstacleData, typeof(ObstacleData), false);
        }

        if (obstacleData != null)
        {
            EditorGUILayout.Space();
            for (int y = 0; y < 10; y++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < 10; x++)
                {
                    int index = y * 10 + x;
                    obstacleData.obstacles[index] = GUILayout.Toggle(obstacleData.obstacles[index], "");
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Save"))
            {
                EditorUtility.SetDirty(obstacleData);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
