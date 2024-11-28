using UnityEngine;

namespace Enemy.Positioning {
    // NO Instance needed, communication is via Event Channels
    public class EnemyAggressionManager : MonoBehaviour {
        [SerializeField] PositioningGrid positioningGrid;
        [SerializeField] Transform testEnemy;
        PositioningGridObject _currentCell;

        void Start() {
            positioningGrid.OnPlayerGridPositionChanged += CheckIfCellIsStillOptimal;
        }
        
        void RequestCellForEnemy() {
            _currentCell = positioningGrid.GetClosestGridObjectWithinMinMaxRange(testEnemy);
            positioningGrid.OccupyCell(_currentCell, true);
            testEnemy.position = _currentCell.NavMeshSamplePosition;
        }
        
        void CheckIfCellIsStillOptimal() {
            // If the player has Moved, check each occupied
            if (_currentCell == null) {
                RequestCellForEnemy();
                return;
            }
            
            var optimalCell = positioningGrid.GetClosestGridObjectWithinMinMaxRange(testEnemy);
            if (optimalCell == _currentCell) { return; }
            positioningGrid.OccupyCell(_currentCell, false);
            RequestCellForEnemy();
        }
        
        // TODOS:
        // Evaluate Distance for each aggressive enemy to player
        // Attacking Enemy:
        // On Attack: Throw AttackStarted Event and AttackEnded Event. In between these two, no reevaluation of attack permission can be done
        // If no event is thrown, each time OnPlayerGridPositionChanged the reevaluation of attack permission is done.
    }
}
