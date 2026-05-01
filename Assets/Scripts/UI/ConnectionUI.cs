using System;
using FishNet;
using FishNet.Managing;
using TMPro;
using FishNet.Object;
using UnityEngine;

namespace UI
{
    public class ConnectionUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nicknameInput;
    
        // Сохраняем ник локально до появления сетевого объекта игрока.
        public static string PlayerNickname { get; private set; } = "Player";

        public static Action HostStarted;
        [SerializeField] private NetworkManager networkManager;

        private void Start()
        {
            if (!networkManager) networkManager = InstanceFinder.NetworkManager;
        }

        public void StartAsHost()
        {
            SaveNickname();
            // Хост одновременно является сервером и клиентом.
            networkManager.ServerManager.StartConnection();
            HostStarted?.Invoke();
            // gameObject.SetActive(false);
        }

        public void StartAsClient()
        {
            SaveNickname();
            // Клиент только подключается к уже запущенному хосту/серверу.
            networkManager.ClientManager.StartConnection();
            gameObject.SetActive(false);
        }

        private void SaveNickname()
        {
            // Нормализуем ввод, чтобы сервер не получил пустую строку.
            string rawValue = nicknameInput != null ? nicknameInput.text : string.Empty;
            PlayerNickname = string.IsNullOrWhiteSpace(rawValue) ? "Player" : rawValue.Trim();
            nicknameInput.text = "";
        }
    }
}
