using UnityEngine;

namespace IAV26.G09.P3
{
    public class CameraCycler : MonoBehaviour
    {
        private Unity.FPS.Gameplay.PlayerInputHandler m_PIH;
        private Camera[] m_Cameras;
        int m_CamId = 0;
        [SerializeField] private Camera firstCamera;

        void Start()
        {
            m_PIH = GetComponent<Unity.FPS.Gameplay.PlayerInputHandler>();

            GameObject[] cameraObjects = GameObject.FindGameObjectsWithTag("MainCamera");
            m_Cameras = new Camera[cameraObjects.Length];
            for (int i = 0; i < cameraObjects.Length; i++)
            {
                m_Cameras[i] = cameraObjects[i].GetComponent<Camera>();
                m_Cameras[i].enabled = false;
            }

            if (firstCamera != null)
                firstCamera.enabled = true;
        }

        private void CycleCamera()
        {
            m_Cameras[m_CamId].enabled = false;
            m_CamId = (m_CamId + 1) % m_Cameras.Length;
            m_Cameras[m_CamId].enabled = true;
        }

        void Update()
        {
            // gestiona el ciclo de camara
            if (m_PIH && m_PIH.GetChangeCameraDown() && m_Cameras.Length > 0)
                CycleCamera();
        }
    }
}