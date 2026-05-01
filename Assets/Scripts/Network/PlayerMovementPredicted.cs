using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

namespace Network
{
    public struct MoveData : IReplicateData
    {
        public float Horizontal;
        public float Vertical;
        public Vector3 Forward;

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    public struct ReconcileData : IReconcileData
    {
        public Vector3 Position;
        public Quaternion Rotation;

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovementPredicted : NetworkBehaviour
    {
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _gravity = -9.81f;

        private CharacterController _cc;
        private float _verticalVelocity;
        private Camera _camera;
        private PlayerNetwork _player;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _player = GetComponent<PlayerNetwork>();
        }

        public override void OnStartNetwork()
        {
            base.TimeManager.OnTick += OnTick;

            if (base.Owner.IsLocalClient)
                _camera = Camera.main;
        }

        public override void OnStopNetwork()
        {
            base.TimeManager.OnTick -= OnTick;
        }

        private void OnTick()
        {
            if (_player == null || !_player.IsAlive.Value)
                return;

            MoveData md = default;

            if (base.IsOwner)
            {
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");

                Vector3 forward = Vector3.forward;

                if (_camera != null)
                {
                    forward = _camera.transform.forward;
                    forward.y = 0f;
                    forward.Normalize();
                }

                md = new MoveData
                {
                    Horizontal = h,
                    Vertical = v,
                    Forward = forward
                };
            }

            Replicate(md);
        }

        [Replicate]
        private void Replicate(
            MoveData md,
            ReplicateState state = ReplicateState.Invalid,
            Channel channel = Channel.Unreliable)
        {
            if (_cc == null || !_cc.enabled)
                return;

            if (_player == null || !_player.IsAlive.Value)
                return;

            if (md.Forward.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(md.Forward);

            Vector3 right = transform.right;
            Vector3 move = (md.Forward * md.Vertical + right * md.Horizontal).normalized;

            move *= _speed;

            _verticalVelocity += _gravity * (float)base.TimeManager.TickDelta;
            move.y = _verticalVelocity;

            _cc.Move(move * (float)base.TimeManager.TickDelta);

            if (_cc.isGrounded)
                _verticalVelocity = 0f;

            if (base.IsServerInitialized)
            {
                SendStateObserversRpc(transform.position, transform.rotation);
            }
        }

        [Reconcile]
        private void Reconcile(
            ReconcileData rd,
            Channel channel = Channel.Unreliable)
        {
            if (_cc != null)
                _cc.enabled = false;

            transform.position = rd.Position;
            transform.rotation = rd.Rotation;

            if (_cc != null)
                _cc.enabled = true;
        }

        public override void CreateReconcile()
        {
            // отправляем текущую позицию (как есть)
            ReconcileData rd = new ReconcileData
            {
                Position = transform.position,
                Rotation = transform.rotation
            };

            Reconcile(rd);
        }

        [ObserversRpc]
        private void SendStateObserversRpc(Vector3 pos, Quaternion rot)
        {
            // не трогаем локального владельца
            if (base.IsOwner)
                return;

            if (_cc != null)
                _cc.enabled = false;

            transform.position = pos;
            transform.rotation = rot;

            if (_cc != null)
                _cc.enabled = true;
        }
    }
}