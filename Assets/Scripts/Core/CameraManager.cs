using UnityEngine;

namespace ChromaticCascade.Core
{
    /// <summary>
    /// Manages camera positioning and sizing for optimal grid view
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraManager : MonoBehaviour
    {
        [Header("Grid Reference")]
        [SerializeField] private GridManager gridManager;
        
        [Header("Camera Settings")]
        [SerializeField] private float verticalPadding = 1f;
        [SerializeField] private float horizontalPadding = 0.5f;
        
        private Camera cam;
        
        private void Awake()
        {
            cam = GetComponent<Camera>();
            
            if (gridManager == null)
            {
                gridManager = FindFirstObjectByType<GridManager>();
            }
        }
        
        private void Start()
        {
            // Delay to ensure GridManager is initialized
            Invoke(nameof(AdjustCameraToGrid), 0.1f);
        }
        
        /// <summary>
        /// Adjust camera size and position to fit the grid perfectly
        /// </summary>
        public void AdjustCameraToGrid()
        {
            if (gridManager == null)
            {
                Debug.LogError("CameraManager: GridManager not assigned!");
                return;
            }
            
            if (cam == null)
            {
                Debug.LogError("CameraManager: Camera component missing!");
                return;
            }
            
            int gridWidth = gridManager.GridWidth;
            int gridHeight = gridManager.GridHeight;
            float cellSize = gridManager.CellSize;
            Vector3 gridOrigin = gridManager.GridOrigin;
            
            // Calculate grid bounds in world space
            float gridWorldWidth = gridWidth * cellSize;
            float gridWorldHeight = gridHeight * cellSize;
            
            // Calculate grid center position
            float gridCenterX = gridOrigin.x + (gridWorldWidth / 2f);
            float gridCenterY = gridOrigin.y + (gridWorldHeight / 2f);
            
            // Position camera at grid center
            Vector3 cameraPos = transform.position;
            cameraPos.x = gridCenterX;
            cameraPos.y = gridCenterY;
            transform.position = cameraPos;
            
            // Calculate required camera size
            float aspectRatio = (float)Screen.width / Screen.height;
            
            // Size to fit height (with padding)
            float heightBasedSize = (gridWorldHeight / 2f) + verticalPadding;
            
            // Size to fit width (with padding)
            float widthBasedSize = (gridWorldWidth + horizontalPadding * 2f) / (2f * aspectRatio);
            
            // Use the larger size to ensure everything fits
            float requiredSize = Mathf.Max(heightBasedSize, widthBasedSize);
            
            cam.orthographicSize = requiredSize;
            
            Debug.Log($"Camera adjusted: Size={requiredSize:F2}, AspectRatio={aspectRatio:F3}, Position=({cameraPos.x:F1}, {cameraPos.y:F1})");
        }
        
        private void OnValidate()
        {
            // Adjust in editor when values change
            if (Application.isPlaying && cam != null)
            {
                AdjustCameraToGrid();
            }
        }
    }
}
