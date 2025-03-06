using Core.GameLoop;
using Core.GeometricEngine;
using Core.Units;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GodotFrontend.code.Input.InputFSM;
using static GodotFrontend.code.Input.InputManager;

namespace GodotFrontend.code.Input
{
    // This class has all the geometry tools needed for moving, charging and etc
    public class BaseMove : ISubInputManager
    {
        public enum DragMode
        {
            left_pivot,
            right_pivot,
            move,
            rotate_center
        }
        
        protected float? offsetDistancePicked;
        protected bool isDragging;
        protected UnitGodot unitDragged;
        protected float distanceMoved;

        // ORIGIN positions when clicking for dragging
        protected DragMode dragMode;
        private Vector2 unitOriginPos;
        protected MatrixAffine currentTransformMat;
        private Vector2 pivotClickPointRect;
        protected Vector2 pivotAnchorPoint;
        protected BattlefieldCursorPosDel battlefieldCursorPosDel;
        private BattleState battleState
        {
            get { return PlayerInfoSingleton.Instance.battleStateManager.currentState; }
        }
        public InputState inputState
        {
            get { return InputFSM.currentState; }
            set
            {
                InputFSM.changeState(value);
            }
        }
        public BaseMove(BattlefieldCursorPosDel _battlefieldCursorPosDel)
        {
            battlefieldCursorPosDel = _battlefieldCursorPosDel;
        }

        protected void restartStateVars()
        {
            offsetDistancePicked = null;
            isDragging = false;
        }
        protected UnitGodot? SelectOwnUnit(UnitGodot unitToSelect)
        {
            if (UnitsClientManager.Instance.canSelectUnit(unitToSelect.coreUnit.Guid, true))
            {
                return unitToSelect;
            }
            return null;
        }
        protected UnitGodot? SelectEnemyUnit(UnitGodot unitToSelect)
        {
            if (UnitsClientManager.Instance.canSelectUnit(unitToSelect.coreUnit.Guid, false))
            {
                return unitToSelect;
            }
            return null;
        }
        #region PIVOTING
        private float intentPivotUnit(float angle, UnitGodot unit, bool isAnchorPointLeft)
        {
            UnitsClientManager.Instance.getCloseUnitInRotation(unit.coreUnit, isAnchorPointLeft);
            return angle;
        }
        // We are passing global vars, but is for formatting purposes
        //TODO: add collision in pivotting
        protected void pivotUnit(Vector3 worldPos, UnitGodot unit, DragMode pivotOrientation, Vector2 anchorPoint)
        {
            intentPivotUnit(0, unit, pivotOrientation == DragMode.left_pivot);
            distanceMoved = 0;
            if (unit.distanceRemaining <= 0) return;
            // remember to copy values and not references
            unit.affTrans.matrixTransform = currentTransformMat.cloneMatrix();
            Vector2 clickPosRect = new Vector2(worldPos.X - anchorPoint.X, worldPos.Y - anchorPoint.Y);
            float angle = unit.affTrans.calculateAngle(pivotClickPointRect.X, pivotClickPointRect.Y, clickPosRect.X, clickPosRect.Y);
            //float angleDegrees = (float)(angle * (360 / Math.Tau));
            float maxAnglePivot = (float)(calcMaxDistancePivot(unit));
            //GD.Print("angle degrees", angleDegrees);
            if (pivotOrientation == DragMode.left_pivot)
            {
                //drawDebugLine(new Vector3(anchorPoint.X,anchorPoint.Y,0.25f), worldPos,Color.Color8(200,0,0));		
                angle = Math.Clamp(angle, -maxAnglePivot, maxAnglePivot * 0.5f);

                unit.pivot(angle, false);
            }
            else if (pivotOrientation == DragMode.right_pivot)
            {

                //drawDebugLine(new Vector3(anchorPoint.X, anchorPoint.Y, 0.25f), worldPos, Color.Color8(0, 200, 0));
                angle = Math.Clamp(angle, maxAnglePivot * -0.5f, maxAnglePivot);
                unit.pivot(angle, true);
            }
            distanceMoved = calculateDistancePivot(unit, angle);
            unit.showDistanceRemaining(distanceMoved);
        }
        private float calculateDistancePivot(UnitGodot unit, float angle)
        {

            float distance = 0;
            float radius = unit.coreUnit.sizeEnclosedRectangledm.X;
            distance = radius * angle;
            // backwards cost double movement
            if (angle < 0)
            {
                distance *= 2;
            }
            return Math.Abs(distance);
        }
        private Vector2 calculatePivotAnchorPoint(UnitGodot unit, bool isLeft)
        {
            //Vector2 pivotAnchorPoint = new Vector2();
            float offsetTroop = (float)unit.coreUnit.Troop.Size.Width / (2 * 100);
            float offsetDistanceY = offsetTroop;
            float offsetDistanceX = 0;
            if (isLeft) // we take right top corner point as anchor
            {
                offsetDistanceX = (float)unit.coreUnit.sizeEnclosedRectangle.Width / 100 - offsetTroop;
            }
            else // we take left top corner point as anchor
            {
                offsetDistanceX = -offsetTroop;
            }
            System.Numerics.Vector2 pivotAnchorPoint = unit.affTrans.localToGlobalTransforms(offsetDistanceX, offsetDistanceY);
            return new Vector2(pivotAnchorPoint.X, pivotAnchorPoint.Y);
        }

        // Inverted calc, so it will be angle = dist/r
        private float calcMaxDistancePivot(UnitGodot unit)
        {
            return unit.distanceRemaining / unit.coreUnit.sizeEnclosedRectangledm.X;
        }
        #endregion
        public void onArrowClick(Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeIdx, Node collider)
        {
            if (isDragging == false)
            {
                Node arrow = collider.GetParent().GetParent();
                isDragging = true;

                unitDragged = (UnitGodot)arrow.GetParent().GetParent();

                currentTransformMat = unitDragged.affTrans.copyMatrixTransformValues();
                unitOriginPos = unitDragged.position2D();
                Vector3? worldPosNullable = battlefieldCursorPosDel();

                switch (arrow.Name)
                {
                    case "middle_movement_arrow":
                        dragMode = DragMode.move;
                        break;
                    case "left_movement_arrow":
                        dragMode = DragMode.left_pivot;
                        pivotAnchorPoint = calculatePivotAnchorPoint(unitDragged, true);
                        pivotClickPointRect = new Vector2(worldPosNullable.Value.X - pivotAnchorPoint.X, worldPosNullable.Value.Y - pivotAnchorPoint.Y);
                        break;
                    case "right_movement_arrow":
                        dragMode = DragMode.right_pivot;
                        pivotAnchorPoint = calculatePivotAnchorPoint(unitDragged, false);
                        pivotClickPointRect = new Vector2(worldPosNullable.Value.X - pivotAnchorPoint.X, worldPosNullable.Value.Y - pivotAnchorPoint.Y);
                        break;
                    case "rotate_arrow":
                        dragMode = DragMode.rotate_center;
                        break;
                    default:
                        GD.Print("SOMETHING WENT WRONG IN ARROW SELECTION");
                        break;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="pivotAnchorPoint">where is the point to rotate so we get the rect from there to the click pos</param>
        /// <returns>angle in radians</returns>
        private float calculateAngle(UnitGodot unit, Vector2 pivotAnchorPoint, Vector3 clickWorldPos)
        {

            // we need the vector that form anchor point and clickpos

            Vector2 rectClick = new Vector2(pivotAnchorPoint.X - clickWorldPos.X, pivotAnchorPoint.Y - clickWorldPos.Y);

            float angle = unit.affTrans.calculateAngleToTransform(rectClick.X, rectClick.Y);

            return angle;


        }
        private float? calculateInitialAngle(UnitGodot unit, Vector2 pivotAnchorPoint)
        {
            Vector3? clickPos = battlefieldCursorPosDel();

            if (clickPos.HasValue)
            {
                return calculateAngle(unit, pivotAnchorPoint, clickPos.Value);
            }
            return null;
        }

        public void CustomProcess(double delta)
        {
        }
    }
}
