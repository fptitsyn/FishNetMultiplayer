using System;
using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UI;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Network
{
    public class PlayerNetwork : NetworkBehaviour
    {
        [SerializeField] private GameObject model;

        public Action PlayerDied;
    
        public readonly SyncVar<string> Nickname = new();
        public readonly SyncVar<int> HP = new(100);
        public readonly SyncVar<int> Ammo = new(30);
        public readonly SyncVar<bool> IsAlive = new(true);

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            if (Owner.IsLocalClient)
            {
                SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
            }
            
            if (IsServerInitialized)
            {
                IsAlive.Value = true;
                transform.position = new Vector3(
                    Random.Range(-3f, 3f),
                    0.4f,
                    0
                );
            }

            HP.OnChange += OnHpChanged;
            IsAlive.OnChange += OnIsAliveChanged;
        }

        public override void OnStopNetwork()
        {
            HP.OnChange -= OnHpChanged;
            IsAlive.OnChange -= OnIsAliveChanged;
        }
        
        private void OnHpChanged(int oldValue, int newValue, bool asServer)
        {
            if (!asServer) return;
            if (newValue <= 0 && IsAlive.Value)
            {
                IsAlive.Value = false;
                PlayerDied?.Invoke();
                StartCoroutine(RespawnRoutine());
            }
        }
    
        [ServerRpc(RequireOwnership = false)]
        private void SubmitNicknameServerRpc(string nickname)
        {
            string safeValue = string.IsNullOrWhiteSpace(nickname) ? $"Player_{OwnerId}" : nickname.Trim();
            Nickname.Value = safeValue;
        }
    
        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(3);

            Vector3 spawnPos = PlayerSpawner.Instance.GetSpawnPosition();

            transform.position = spawnPos;

            HP.Value = 100;
            Ammo.Value = 30;
            yield return null;
            IsAlive.Value = true;
        }
    
        private void OnIsAliveChanged(bool prev, bool next, bool asServer)
        {
            if (model == null)
            {
                Debug.LogError("Model not assigned!");
                return;
            }

            model.SetActive(next);
        }
    }
}