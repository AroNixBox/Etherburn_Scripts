using System;
using UnityEngine;
using UnityEngine.Events;

namespace Extensions {
    public class MoveInDirectionAndDestroy : MonoBehaviour  {
        [SerializeField] float speed = 5f;
        [SerializeField] float destroyTime = 2f;
        [SerializeField] Direction direction;
        [SerializeField] UnityEvent beforeDestroyEvent;
        
        Vector3 _moveDirection;
        bool _enableMoving;

        public void StartMove() {
            Invoke(nameof(DestroySelf), destroyTime);
            
            _moveDirection = direction switch {
                Direction.Up => Vector3.up,
                Direction.Down => Vector3.down,
                Direction.Left => Vector3.left,
                Direction.Right => Vector3.right,
                Direction.Forward => Vector3.forward,
                Direction.Backward => Vector3.back,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            _enableMoving = true;
        }

        void Update() {
            if (!_enableMoving) return;
            transform.Translate(_moveDirection * (speed * Time.deltaTime));
        }
        
        void DestroySelf() {
            beforeDestroyEvent?.Invoke();
            Destroy(gameObject);
        }

        enum Direction { Up, Down, Left, Right, Forward, Backward }
    }
}
