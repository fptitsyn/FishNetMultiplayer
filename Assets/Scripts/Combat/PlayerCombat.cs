using FishNet.Object;
using Network;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Combat
{
    public class PlayerCombat : NetworkBehaviour
    {
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _firePoint;

        [SerializeField] private float _cooldown = 0.4f;
        [SerializeField] private int _maxAmmo = 30;

        private float _lastShotTime;

        private InputAction _attackAction;

        private PlayerNetwork _playerNetwork;

        private Camera _cam;

        public override void OnStartClient()
        {
            _playerNetwork = GetComponent<PlayerNetwork>();
            _attackAction = InputSystem.actions.FindAction("Attack");
            _cam = Camera.main;
        
            if (IsServerInitialized)
            {
                _playerNetwork.Ammo.Value = _maxAmmo;
            }
        }

        private void Update()
        {
            if (!_playerNetwork.IsAlive.Value) return;

            if (!IsOwner) return;

            if (_attackAction.triggered)
            {
                Vector3 dir = _cam.transform.forward;
                dir.y = 0f;
                dir.Normalize();

                ShootServerRpc(_firePoint.position, dir);
            }
        }

        [ServerRpc]
        private void ShootServerRpc(Vector3 pos, Vector3 dir)
        {
            if (_playerNetwork.HP.Value <= 0) return;
            if (_playerNetwork.Ammo.Value <= 0) return;
            if (Time.time < _lastShotTime + _cooldown) return;

            _lastShotTime = Time.time;
            _playerNetwork.Ammo.Value--;

            var go = Instantiate(
                _projectilePrefab,
                pos + dir * 1.2f, 
                Quaternion.LookRotation(dir)
            );
    
            if (!go.TryGetComponent<NetworkObject>(out var networkObject)) return;
            ServerManager.Spawn(networkObject, Owner);
        }
    }
}