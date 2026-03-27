using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace LoseMenu
{
    public class LoseMenuManager : MonoBehaviour
    {
        private Button btnMainMenu;
        private Button btnPlayAgain;

        private VisualElement menuContainer;

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;

            menuContainer = root.Q<VisualElement>("MenuContainer");

            btnMainMenu = root.Q<Button>("BtnMainMenu");
            btnPlayAgain = root.Q<Button>("BtnPlayAgain");

            if (btnMainMenu != null) btnMainMenu.clicked += OnMainMenuClicked;
            if (btnPlayAgain != null) btnPlayAgain.clicked += OnPlayAgainClicked;

            // --- LÓGICA DE ROLES AL INICIAR EL MENÚ ---
            if (NetworkManager.Singleton != null)
            {
                if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
                {
                    // Si es un cliente puro (no es el Host), ocultamos el botón de volver a jugar
                    // porque un cliente no puede obligar al servidor a reiniciar la partida.
                    if (btnPlayAgain != null)
                    {
                        btnPlayAgain.style.display = DisplayStyle.None;
                    }
                }
            }
        }

        private void OnDisable()
        {
            if (btnMainMenu != null) btnMainMenu.clicked -= OnMainMenuClicked;
            if (btnPlayAgain != null) btnPlayAgain.clicked -= OnPlayAgainClicked;
        }

        private void OnMainMenuClicked()
        {
            // Verificamos si hay una partida activa
            if (NetworkManager.Singleton != null)
            {
                if (NetworkManager.Singleton.IsHost)
                {
                    Debug.Log("Soy Host: Cerrando el servidor y volviendo al menú.");
                }
                else if (NetworkManager.Singleton.IsClient)
                {
                    Debug.Log("Soy Cliente: Desconectándome y volviendo al menú.");
                }

                // 1. Apagamos la red (desconecta a todos o te saca de la partida)
                NetworkManager.Singleton.Shutdown();

                // 2. Destruimos el NetworkManager para que el IntroMenu cree uno nuevo y limpio
                Destroy(NetworkManager.Singleton.gameObject);
            }

            // 3. Cargamos la escena de IntroMenu de forma local, no por red
            SceneManager.LoadScene("IntroMenu");
        }

        private void OnPlayAgainClicked()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
            {
                Debug.Log("Soy Host: Reiniciando la partida para todos.");
                // Usamos el SceneManager de Netcode para llevar a todo el grupo de vuelta al mapa
                NetworkManager.Singleton.SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
            }
        }
    }
}