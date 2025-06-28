using UnityEngine;
using UnityEngine.InputSystem;

public class IslandPlacementMode : MonoBehaviour
{
    public IslandChunkGenerator islandGenerator;
    public Camera mainCamera;
    public PlayerMovement playerController; // ��� ������ ��������� �������

    private bool placementModeActive = false;
    private float normalCameraSize;
    public float placementCameraSize = 20f;

    void Start()
    {
        normalCameraSize = mainCamera.orthographicSize;

        // �������� ����������� ����� ��� �����
        int centerX = islandGenerator.mapWidth / 2;
        int centerY = islandGenerator.mapHeight / 2;
        islandGenerator.GenerateIslandAt(centerX, centerY);

        // ������������, �� ����� ��������� ������� ���������
        ExitPlacementMode();
    }

    void Update()
    {
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            placementModeActive = !placementModeActive;

            if (placementModeActive)
                EnterPlacementMode();
            else
                ExitPlacementMode();
        }
    }

    void EnterPlacementMode()
    {
        mainCamera.orthographicSize = placementCameraSize;

        if (playerController != null)
            playerController.enabled = false;

        Debug.Log("����� ��������� ������� ����������");
    }

    void ExitPlacementMode()
    {
        mainCamera.orthographicSize = normalCameraSize;

        if (playerController != null)
            playerController.enabled = true;

        Debug.Log("����� ��������� ������� ��������");
    }
}
