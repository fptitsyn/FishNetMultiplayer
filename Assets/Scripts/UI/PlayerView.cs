using System.Collections;
using Network;
using TMPro;
using Unity.Collections;
using FishNet.Object;
using UnityEngine;

namespace UI
{
    public class PlayerView : NetworkBehaviour
    {
        [SerializeField] private PlayerNetwork playerNetwork;
        [SerializeField] private TMP_Text nicknameText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text ammoText;
        [SerializeField] private TMP_Text respawnTimerText;
        
        public override void OnStartClient()
        {
            playerNetwork.Nickname.OnValueChanged += OnNicknameChanged;
            playerNetwork.HP.OnValueChanged += OnHpChanged;
            playerNetwork.Ammo.OnValueChanged += OnAmmoChanged;
            playerNetwork.PlayerDied += HandleRespawnTimer;

            OnNicknameChanged(default, playerNetwork.Nickname.Value);
            OnHpChanged(0, playerNetwork.HP.Value);
            OnAmmoChanged(0, playerNetwork.Ammo.Value);
        }

        public override void OnNetworkDespawn()
        {
            playerNetwork.Nickname.OnValueChanged -= OnNicknameChanged;
            playerNetwork.HP.OnValueChanged -= OnHpChanged;
            playerNetwork.Ammo.OnValueChanged -= OnAmmoChanged;
            playerNetwork.PlayerDied -= HandleRespawnTimer;
        }

        private void OnNicknameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
        {
            nicknameText.text = newValue.ToString();
        }

        private void OnHpChanged(int oldValue, int newValue)
        {
            hpText.text = $"HP: {newValue}";
        }

        private void OnAmmoChanged(int oldValue, int newValue)
        {
            ammoText.text = $"Ammo: {newValue}";
        }

        private void HandleRespawnTimer()
        {
            StartCoroutine(CountRespawnTime());
        }

        private IEnumerator CountRespawnTime()
        {
            respawnTimerText.enabled = true;
            for (int i = 3; i > 0; i--)
            {
                respawnTimerText.text = $"Respawn in: {i}...";
                yield return new WaitForSeconds(1f);
            }
            respawnTimerText.enabled = false;
        }
    }
}