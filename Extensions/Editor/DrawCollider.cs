using UnityEditor;
using UnityEngine;

namespace Extensions.Editor {
    public class ColliderView : EditorWindow {
        Vector2 _scrollPosition;
        Color _colliderColor = Color.green;
        bool _showColliders = true;

        [MenuItem("Window/Collider Viewer", false, 1000)]
        public static void ShowWindow() {
            GetWindow<ColliderView>("Collider Viewer");
        }

        void OnGUI() {
            GUILayout.Label("Collider Viewer Settings", EditorStyles.boldLabel);

            // Toggle for showing/hiding colliders
            _showColliders = EditorGUILayout.Toggle("Show Colliders", _showColliders);

            // Color picker for collider gizmo color
            _colliderColor = EditorGUILayout.ColorField("Collider Gizmo Color", _colliderColor);

            if (GUILayout.Button("Refresh Scene View"))
            {
                SceneView.RepaintAll();
            }

            GUILayout.Space(10);
            GUILayout.Label("Selected GameObjects and their Colliders", EditorStyles.boldLabel);

            if (Selection.gameObjects.Length == 0)
            {
                EditorGUILayout.HelpBox("No GameObject selected.", MessageType.Info);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (GameObject obj in Selection.gameObjects)
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField($"GameObject: {obj.name}", EditorStyles.boldLabel);

                Collider[] colliders = obj.GetComponentsInChildren<Collider>();
                if (colliders.Length > 0)
                {
                    foreach (Collider collider in colliders)
                    {
                        EditorGUILayout.ObjectField("Collider", collider, typeof(Collider), true);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No Colliders attached.", EditorStyles.wordWrappedLabel);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;

        void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

        void OnSceneGUI(SceneView sceneView) {
            if (!_showColliders) return;

            foreach (var obj in Selection.gameObjects) {
                var colliders = obj.GetComponentsInChildren<Collider>();
                foreach (var collider in colliders) {
                    Handles.color = collider is MeshCollider ? Color.red : _colliderColor;
                    DrawColliderGizmo(collider);
                }
            }
        }

        void DrawColliderGizmo(Collider collider) {
            switch (collider) {
                case BoxCollider boxCollider: {
                    Handles.color = _colliderColor;

                    // Create a transformation matrix for the BoxCollider
                    Matrix4x4 matrix = Matrix4x4.TRS(
                        boxCollider.transform.position,
                        boxCollider.transform.rotation,
                        boxCollider.transform.lossyScale
                    );

                    Vector3 center = boxCollider.center;
                    Vector3 size = boxCollider.size;

                    // Calculate the corners of the BoxCollider
                    Vector3[] corners = new Vector3[8]
                    {
                        center + new Vector3(-size.x, -size.y, -size.z) * 0.5f,
                        center + new Vector3(size.x, -size.y, -size.z) * 0.5f,
                        center + new Vector3(size.x, -size.y, size.z) * 0.5f,
                        center + new Vector3(-size.x, -size.y, size.z) * 0.5f,
                        center + new Vector3(-size.x, size.y, -size.z) * 0.5f,
                        center + new Vector3(size.x, size.y, -size.z) * 0.5f,
                        center + new Vector3(size.x, size.y, size.z) * 0.5f,
                        center + new Vector3(-size.x, size.y, size.z) * 0.5f
                    };

                    // Transform the corners to world space
                    for (int i = 0; i < corners.Length; i++)
                    {
                        corners[i] = matrix.MultiplyPoint3x4(corners[i]);
                    }

                    // Draw the edges of the BoxCollider
                    Handles.DrawLine(corners[0], corners[1]);
                    Handles.DrawLine(corners[1], corners[2]);
                    Handles.DrawLine(corners[2], corners[3]);
                    Handles.DrawLine(corners[3], corners[0]);
                    Handles.DrawLine(corners[4], corners[5]);
                    Handles.DrawLine(corners[5], corners[6]);
                    Handles.DrawLine(corners[6], corners[7]);
                    Handles.DrawLine(corners[7], corners[4]);
                    Handles.DrawLine(corners[0], corners[4]);
                    Handles.DrawLine(corners[1], corners[5]);
                    Handles.DrawLine(corners[2], corners[6]);
                    Handles.DrawLine(corners[3], corners[7]);
                    break;
                }
                case SphereCollider sphereCollider: {
                    Handles.color = _colliderColor;
                    Vector3 position = sphereCollider.transform.TransformPoint(sphereCollider.center);
                    float radius = sphereCollider.radius * Mathf.Max(sphereCollider.transform.lossyScale.x, sphereCollider.transform.lossyScale.y, sphereCollider.transform.lossyScale.z);
                    Handles.DrawWireDisc(position, Vector3.up, radius);
                    Handles.DrawWireDisc(position, Vector3.right, radius);
                    Handles.DrawWireDisc(position, Vector3.forward, radius);
                    break;
                }
                case CapsuleCollider capsuleCollider: {
                    Handles.color = _colliderColor;
                    Vector3 position = capsuleCollider.transform.TransformPoint(capsuleCollider.center);
                    float radius = capsuleCollider.radius * Mathf.Max(capsuleCollider.transform.lossyScale.x, capsuleCollider.transform.lossyScale.z);
                    float height = capsuleCollider.height * capsuleCollider.transform.lossyScale.y;

                    Vector3 up = capsuleCollider.transform.up;
                    Vector3 point1 = position + up * (height / 2 - radius);
                    Vector3 point2 = position - up * (height / 2 - radius);

                    Handles.DrawWireArc(point1, capsuleCollider.transform.right, capsuleCollider.transform.forward, 360, radius);
                    Handles.DrawWireArc(point2, capsuleCollider.transform.right, capsuleCollider.transform.forward, 360, radius);
                    Handles.DrawWireArc(point1, capsuleCollider.transform.forward, capsuleCollider.transform.right, 360, radius);
                    Handles.DrawWireArc(point2, capsuleCollider.transform.forward, capsuleCollider.transform.right, 360, radius);
                    Handles.DrawWireArc(position, capsuleCollider.transform.up, capsuleCollider.transform.right, 360, radius);
                    break;
                }
                case MeshCollider meshCollider when meshCollider.sharedMesh != null: {
                    Handles.color = Color.red;

                    Mesh nonConvexMesh = meshCollider.sharedMesh;
                    Transform transform = meshCollider.transform;

                    Vector3[] vertices = nonConvexMesh.vertices;
                    int[] triangles = nonConvexMesh.triangles;

                    for (int i = 0; i < triangles.Length; i += 3)
                    {
                        Vector3 v0 = transform.TransformPoint(vertices[triangles[i]]);
                        Vector3 v1 = transform.TransformPoint(vertices[triangles[i + 1]]);
                        Vector3 v2 = transform.TransformPoint(vertices[triangles[i + 2]]);

                        Handles.DrawLine(v0, v1);
                        Handles.DrawLine(v1, v2);
                        Handles.DrawLine(v2, v0);
                    }

                    break;
                }
            }
        }
    }
}