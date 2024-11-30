using System.Collections.Generic;
using UnityEngine;

namespace Enemy.Positioning {
    // NO Instance needed, communication is via Event Channels
    public class EnemyAggressionManager : MonoBehaviour {
        public static EnemyAggressionManager Instance;
        [SerializeField] PositioningGrid positioningGrid;
        // Enemies
        public List<EnemyData> _aggressiveEnemyDatas = new(); // Sleeping

        void Awake() {
            if (Instance == null) {
                Instance = this;
            }
            else {
                Destroy(gameObject);
            }
        }

        void Start() {
            positioningGrid.OnPlayerGridPositionChanged += ReevaluateEnemyBehavior;
        }

        void ReevaluateEnemyBehavior() {
            CheckAllEnemiesForOptimalCell();
        }

        public void RegisterEnemy(GameObject enemy, OptimalPositionChanged optimalPositionChangedChannel) {
            if (!_aggressiveEnemyDatas.Exists(e => e.Enemy.name == enemy.name)) {
                var enemyData = new EnemyData {
                    Enemy = enemy,
                    CurrentCell = null,
                    OptimalPositionChangedChannel = optimalPositionChangedChannel
                };
                _aggressiveEnemyDatas.Add(enemyData);

                CheckIfCellIsStillOptimal(enemyData);
            }
            else {
                Debug.LogError("Enemy already registered");
            }
        }

        public void UnregisterEnemy(GameObject enemy) {
            var enemyData = _aggressiveEnemyDatas.Find(e => e.Enemy.name == enemy.name);
            if (enemyData == null) {
                return;
            }

            positioningGrid.OccupyCell(enemyData.CurrentCell.X, enemyData.CurrentCell.Z, false);
            _aggressiveEnemyDatas.Remove(enemyData);
        }

        void CheckAllEnemiesForOptimalCell() {
            foreach (var aggressiveEnemyData in _aggressiveEnemyDatas) {
                CheckIfCellIsStillOptimal(aggressiveEnemyData);
            }
        }

        // FIXED!!
        void CheckIfCellIsStillOptimal(EnemyData enemyData) {
            if (_aggressiveEnemyDatas.Count == 0) {
                return;
            }

            var optimalCell = positioningGrid.GetClosestGridObjectWithinMinMaxRange(enemyData.Enemy);
            // First Time
            if (enemyData.CurrentCell == null) {
                enemyData.CurrentCell = optimalCell;
                positioningGrid.OccupyCell(enemyData.CurrentCell.X, enemyData.CurrentCell.Z, true);
                enemyData.OptimalPositionChangedChannel.SendEventMessage(enemyData.CurrentCell.NavMeshSamplePosition);
                return;
            }

            if (optimalCell == enemyData.CurrentCell) {
                return;
            } // Optimal Cell is still the same

            // Unoccupy the current cell
            positioningGrid.OccupyCell(enemyData.CurrentCell.X, enemyData.CurrentCell.Z, false);

            //Request a new cell
            enemyData.CurrentCell = optimalCell;

            // Occupy the new cell
            positioningGrid.OccupyCell(enemyData.CurrentCell.X, enemyData.CurrentCell.Z, true);

            enemyData.OptimalPositionChangedChannel.SendEventMessage(enemyData.CurrentCell.NavMeshSamplePosition);
        }

        [System.Serializable]
        public class EnemyData {
            public GameObject Enemy;
            public PositioningGridObject CurrentCell;
            public OptimalPositionChanged OptimalPositionChangedChannel; // TODO: Rename Channel
        }
    }
}
