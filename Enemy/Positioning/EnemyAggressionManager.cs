using System.Collections.Generic;
using UnityEngine;

namespace Enemy.Positioning {
    // NO Instance needed, communication is via Event Channels
    public class EnemyAggressionManager : MonoBehaviour {
        public static EnemyAggressionManager Instance;
        [SerializeField] PositioningGrid positioningGrid;
        Transform _playerTransform;
        // Enemies
        readonly List<EnemyData> _inactiveEnemyDatas = new(); // Sleeping
        EnemyData _activeEnemyData; // Active

        void Awake() {
            if (Instance == null) {
                Instance = this;
            }
            else {
                Destroy(gameObject);
            }
        }

        void Start() {
            _playerTransform = positioningGrid.positioningAnker.transform;
            positioningGrid.OnPlayerGridPositionChanged += ReevaluateEnemyBehavior;
        }
        
        void ReevaluateEnemyBehavior() {
            // Swap out the active enemy if there is a closer inactive enemy
            var closestEnemyToPlayer = GetClosestInactiveEnemyToPlayer();
            if (closestEnemyToPlayer != null) {
                // There is a closest inactive enemy
                if (_activeEnemyData == null) {
                    // There is no active enemy
                    
                    // Set the closest inactive enemy as active enemy
                    _inactiveEnemyDatas.Remove(closestEnemyToPlayer);
                    positioningGrid.OccupyCell(closestEnemyToPlayer.CurrentCell.X, closestEnemyToPlayer.CurrentCell.Z, false);
                    
                    Debug.LogWarning("Setting " + closestEnemyToPlayer.Enemy.name + " as active enemy");
                    
                    _activeEnemyData = closestEnemyToPlayer;
                    closestEnemyToPlayer.ChangeAggressionChannel.SendEventMessage(true);
                }
                else {
                    // We already have an active enemy
                    var distanceFromActiveEnemyToPlayer = (_activeEnemyData.Enemy.transform.position - _playerTransform.position).sqrMagnitude;
                    if (IsDistanceToPlayerCloser(closestEnemyToPlayer.Enemy, distanceFromActiveEnemyToPlayer)) {
                        // Handle ex-active enemy
                        var exActiveEnemyData = _activeEnemyData;
                        // STOP-ATTACK Event for ex-active enemy
                        exActiveEnemyData.ChangeAggressionChannel.SendEventMessage(false);
                        
                        // ORDER IMPORTANT:
                        // Set active enemy with the closest enemy, if we dont do it here, the recursive call will set self to active enemy again, since _activeEnemyData is null
                        _activeEnemyData = closestEnemyToPlayer;
                        
                        // Reregister the ex-active enemy
                        RegisterEnemy(exActiveEnemyData.Enemy, exActiveEnemyData.OptimalPositionChangedChannel, exActiveEnemyData.ChangeAggressionChannel);
                        
                        // Set the closest inactive enemy as active enemy
                        _inactiveEnemyDatas.Remove(closestEnemyToPlayer);
                        positioningGrid.OccupyCell(closestEnemyToPlayer.CurrentCell.X, closestEnemyToPlayer.CurrentCell.Z, false);
                        _activeEnemyData.ChangeAggressionChannel.SendEventMessage(true);
                    }
                }
            }
            else {
                // NOOP, closestEnemyToPlayer is null
            }

            foreach (var aggressiveEnemyData in _inactiveEnemyDatas) {
                CheckIfCellIsStillOptimal(aggressiveEnemyData);
            }
        }
        // Note: When Calling this for reregistering an enemy to be inactive, make sure [_activeEnemyData] is null, otherwise it will be set as active enemy
        public void RegisterEnemy(GameObject enemy, OptimalPositionChanged optimalPositionChangedChannel, ChangeAggressionChannel changeAggressionChannel) {
           if (!_inactiveEnemyDatas.Exists(e => e.Enemy.name == enemy.name)) {
               // Create DataObject for the enemy
                var enemyData = new EnemyData {
                    Enemy = enemy,
                    CurrentCell = null,
                    OptimalPositionChangedChannel = optimalPositionChangedChannel,
                    ChangeAggressionChannel = changeAggressionChannel
                };
                
                if (_activeEnemyData == null) {
                    // No Active Enemy
                    
                    // Set active enemy with new registered enemy
                    _activeEnemyData = enemyData;
    
                    enemyData.ChangeAggressionChannel.SendEventMessage(true);
                }
                else {
                    if (_activeEnemyData.Enemy.name == enemy.name) {
                        // Enemy that is trying to register is the active enemy
                        _activeEnemyData.ChangeAggressionChannel.SendEventMessage(true);
                        return;
                    }
                    
                    var distanceFromActiveEnemyToPlayer = (_activeEnemyData.Enemy.transform.position - _playerTransform.position).sqrMagnitude;
                    if(IsDistanceToPlayerCloser(enemyData.Enemy, distanceFromActiveEnemyToPlayer)) {
                        // Reposition the old active enemy
                        var exActiveEnemyData = _activeEnemyData;
                        // STOP-ATTACK Event
                        exActiveEnemyData.ChangeAggressionChannel.SendEventMessage(false);
                        
                        // ORDER IMPORTANT:
                        // Set active enemy with the closest enemy, if we dont do it here, the recursive call will set self to active enemy again, since _activeEnemyData is null
                        _activeEnemyData = enemyData;
                        
                        // Reregister the ex-active enemy
                        RegisterEnemy(exActiveEnemyData.Enemy, exActiveEnemyData.OptimalPositionChangedChannel, exActiveEnemyData.ChangeAggressionChannel);
                        
                        _activeEnemyData = enemyData;
                        
                        // ATTACK Event for the new active enemy
                        _activeEnemyData.ChangeAggressionChannel.SendEventMessage(true);
                    }
                    else {
                        // Add it to the List of inactive enemies
                        _inactiveEnemyDatas.Add(enemyData);
                        // Give it a cell
                        CheckIfCellIsStillOptimal(enemyData);
                        enemyData.ChangeAggressionChannel.SendEventMessage(false);
                    }
                }
           }
           else {
                // If the entry exists, send a signal to the enemy in case he missed the signal, while being in another state.
                var enemyData = _inactiveEnemyDatas.Find(e => e.Enemy.name == enemy.name);
                enemyData.ChangeAggressionChannel.SendEventMessage(false);
                enemyData.OptimalPositionChangedChannel.SendEventMessage(enemyData.CurrentCell.NavMeshSamplePosition);
           }
        }
        public void UnregisterEnemy(GameObject enemy) {
            // TODO: HANDLE: _activeEnemyData.Enemy
            if (_activeEnemyData != null && _activeEnemyData.Enemy != null && _activeEnemyData.Enemy.name == enemy.name) {
                // This enemy was the active enemy

                // Reset this enemy from active enemy
                _activeEnemyData = null;
                
                var closestEnemyToPlayer = GetClosestInactiveEnemyToPlayer();
                if (closestEnemyToPlayer != null) {
                    // Set the closest inactive enemy as active enemy, no comparison needed, there is no active enemy
                    positioningGrid.OccupyCell(closestEnemyToPlayer.CurrentCell.X, closestEnemyToPlayer.CurrentCell.Z, false);
                    _inactiveEnemyDatas.Remove(closestEnemyToPlayer);
                    _activeEnemyData = closestEnemyToPlayer;
                    _activeEnemyData.ChangeAggressionChannel.SendEventMessage(true);
                }
            }
            else {
                // Enemy was not the active enemy
                
                var enemyData = _inactiveEnemyDatas.Find(e => e.Enemy.name == enemy.name);
                if (enemyData == null) {
                    // Enemy was killed before it was registered
                    // No Action needed
                }
                else {
                    // Enemy was registered and inactive
                    
                    positioningGrid.OccupyCell(enemyData.CurrentCell.X, enemyData.CurrentCell.Z, false);
                    _inactiveEnemyDatas.Remove(enemyData);
                }
            }
        }
        void CheckIfCellIsStillOptimal(EnemyData enemyData) {
            var optimalCell = positioningGrid.GetClosestGridObjectWithinMinMaxRange(enemyData.Enemy, enemyData.CurrentCell);
            // First Call
            if (enemyData.CurrentCell == null) {
                enemyData.CurrentCell = optimalCell;
                positioningGrid.OccupyCell(enemyData.CurrentCell.X, enemyData.CurrentCell.Z, true);
                enemyData.OptimalPositionChangedChannel.SendEventMessage(enemyData.CurrentCell.NavMeshSamplePosition);
                return;
            }
            
            // Enemy is already in the optimal cell
            if (optimalCell == enemyData.CurrentCell) {
                return;
            } 

            // Unoccupy the old cell
            positioningGrid.OccupyCell(enemyData.CurrentCell.X, enemyData.CurrentCell.Z, false);

            // Set the new optimal cell
            enemyData.CurrentCell = optimalCell;

            // Occupy the new cell
            positioningGrid.OccupyCell(enemyData.CurrentCell.X, enemyData.CurrentCell.Z, true);

            // Inform the enemy about the new optimal position
            enemyData.OptimalPositionChangedChannel.SendEventMessage(enemyData.CurrentCell.NavMeshSamplePosition);
        }
        
        EnemyData GetClosestInactiveEnemyToPlayer() {
            EnemyData closestEnemy = null;
            var closestDistance = float.MaxValue;
            foreach (var enemyData in _inactiveEnemyDatas) {
                var distance = (enemyData.Enemy.transform.position - _playerTransform.position).sqrMagnitude;
                
                if (!(distance < closestDistance)) { continue; }
                
                closestDistance = distance;
                closestEnemy = enemyData;
            }
            return closestEnemy;
        }
        
        // Checks the distance between [enemy] and the player and see if its smaller than the parameter [distanceToCompareWith]
        bool IsDistanceToPlayerCloser(GameObject enemy, float distanceToCompareWith) {
            return (enemy.transform.position - _playerTransform.position).sqrMagnitude < distanceToCompareWith;
        }
        class EnemyData {
            public GameObject Enemy;
            public PositioningGridObject CurrentCell;
            public OptimalPositionChanged OptimalPositionChangedChannel; // TODO: Rename Channel
            public ChangeAggressionChannel ChangeAggressionChannel; // TODO: Rename Channel
        }
    }
}
