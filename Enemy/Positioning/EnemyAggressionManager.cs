using System.Collections.Generic;
using Extensions;
using UnityEngine;

namespace Enemy.Positioning {
    // NO Instance needed, communication is via Event Channels
    public class EnemyAggressionManager : Singleton<EnemyAggressionManager> {
        [SerializeField] PositioningGrid positioningGrid;
        
        Transform _playerTransform;
        TargetEntitiesUnregisteredChannel _targetEntityUnregisteredChannel;
        TargetEntitiesUnregisteredChannel.TargetEntitiesUnregisteredChannelEventHandler _handler;
        bool _isPlayerUnregistered;
        
        // Enemies
        readonly List<EnemyData> _inactiveEnemyDatas = new(); // Sleeping
        EnemyData _activeEnemyData; // Active

        async void Start() {
            await EntityManager.Instance.WaitTillInitialized();
            _playerTransform = EntityManager.Instance.GetEntityOfType(EntityType.Player, out _targetEntityUnregisteredChannel).transform;
            
            if (_targetEntityUnregisteredChannel != null) {
                // Create a delegate instance and subscribe to the event
                _handler = HandlePlayerUnregistered;
                _targetEntityUnregisteredChannel.RegisterListener(_handler);
            }
            
            positioningGrid.OnPlayerGridPositionChanged += ReevaluateEnemyBehavior;
        }

        void HandlePlayerUnregistered() {
            positioningGrid.OnPlayerGridPositionChanged -= ReevaluateEnemyBehavior;
            _isPlayerUnregistered = true;
        } 
        void HandlePlayerUnregisteredWarning() => Debug.LogWarning("Player is unregistered, no need to register the enemy");
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
                FindBestFittingCell(aggressiveEnemyData);
            }
        }
        // Note: When Calling this for reregistering an enemy to be inactive, make sure [_activeEnemyData] is null, otherwise it will be set as active enemy
        public void RegisterEnemy(GameObject enemy, OptimalPositionChanged optimalPositionChangedChannel, ChangeAggressionChannel changeAggressionChannel) {
            if(_isPlayerUnregistered) {
                HandlePlayerUnregisteredWarning();
                return;
            }
            
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
                    
                    // Enemy that is trying to register is not the active enemy
                    
                    var distanceFromActiveEnemyToPlayer = (_activeEnemyData.Enemy.transform.position - _playerTransform.position).sqrMagnitude;
                    
                    // Should the new Enemy get Agressive?
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
                        // Give it a cell
                        
                        if (FindBestFittingCell(enemyData)) {
                            // Add it to the List of inactive enemies
                            _inactiveEnemyDatas.Add(enemyData);
                            enemyData.ChangeAggressionChannel.SendEventMessage(false);
                        }
                        else {
                            // Fallback, can't find a cell for the Enemy, just go into Attack Mode...
                            _inactiveEnemyDatas.Remove(enemyData);
                            enemyData.ChangeAggressionChannel.SendEventMessage(true);
                        }
                    }
                }
            } else {
                // If the entry exists, send a signal to the enemy in case he missed the signal, while being in another state.
                var enemyData = _inactiveEnemyDatas.Find(e => e.Enemy.name == enemy.name);
                enemyData.ChangeAggressionChannel.SendEventMessage(false);
                enemyData.OptimalPositionChangedChannel.SendEventMessage(enemyData.CurrentCell.NavMeshSamplePosition);
            }
        }
        public void UnregisterEnemy(GameObject enemy) {
            if(_isPlayerUnregistered) {
                HandlePlayerUnregisteredWarning();
                return;
            }
            
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
        
        bool FindBestFittingCell(EnemyData enemyData) {
            var optimalCell = positioningGrid.GetClosestGridObjectWithinMinMaxRange(enemyData.Enemy, enemyData.CurrentCell);
            if(optimalCell == null) {
                Debug.LogWarning("No optimal cell found for " + enemyData.Enemy.name);
                return false;
            }
            // First Call
            if (enemyData.CurrentCell == null) {
                enemyData.CurrentCell = optimalCell;
                positioningGrid.OccupyCell(enemyData.CurrentCell.X, enemyData.CurrentCell.Z, true);
                enemyData.OptimalPositionChangedChannel.SendEventMessage(enemyData.CurrentCell.NavMeshSamplePosition);
                return true;
            }
            
            // Enemy is already in the optimal cell
            if (optimalCell == enemyData.CurrentCell) {
                return true;
            } 

            // Unoccupy the old cell
            positioningGrid.OccupyCell(enemyData.CurrentCell.X, enemyData.CurrentCell.Z, false);

            // Set the new optimal cell
            enemyData.CurrentCell = optimalCell;

            // Occupy the new cell
            positioningGrid.OccupyCell(enemyData.CurrentCell.X, enemyData.CurrentCell.Z, true);

            // Inform the enemy about the new optimal position
            enemyData.OptimalPositionChangedChannel.SendEventMessage(enemyData.CurrentCell.NavMeshSamplePosition);
            return true;
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
            return (enemy.transform.position - _playerTransform.position)
                .sqrMagnitude < distanceToCompareWith;
        }

        void OnDestroy() {
            if (_targetEntityUnregisteredChannel != null && _handler != null) {
                _targetEntityUnregisteredChannel.UnregisterListener(_handler);
            }
            
            positioningGrid.OnPlayerGridPositionChanged -= ReevaluateEnemyBehavior;
        }

        class EnemyData {
            public GameObject Enemy;
            public PositioningGridObject CurrentCell;
            public OptimalPositionChanged OptimalPositionChangedChannel; // TODO: Rename Channel
            public ChangeAggressionChannel ChangeAggressionChannel; // TODO: Rename Channel
        }
    }
}
