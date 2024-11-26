using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GodotFrontend.code.Input.InputManager;
using GodotFrontend.code.Input;
using System.Reflection.Metadata;
using Core.Rules;

public class Charge
{
    public UnitGodot chargingUnit;
    public UnitGodot chargedUnit;
    public Node3D arrow;
    public StandardMaterial3D matGreen;
    public StandardMaterial3D matRed;
    public bool validArrow = true;
    public ChargeResponse ChargeResponse;
    public Charge()
    {
        matRed = new StandardMaterial3D();
        matRed.AlbedoColor = new Color(0.9f, 0.1f, 0f);
        matGreen = new StandardMaterial3D();
        matGreen.AlbedoColor = new Color(0f, 0.9f, 0.1f);

        var arrowasset = GD.Load<PackedScene>("res://assets/3d/arrow.glb");
        arrow = arrowasset.Instantiate() as Node3D;
        setValidArrow();
        
    }
    public void isValidCharge(bool valid)
    {
        if (valid != validArrow)
        {
            validArrow = valid;
            if (valid) {
                setValidArrow();
            }
            else
            {
                setInValidArrow();
            }
        }
    }
    public void setInValidArrow()
    {
        setMatArrow(matRed);
    }
    public void setValidArrow()
    {
        setMatArrow(matGreen);
    }
    private void setMatArrow(StandardMaterial3D material)
    {
        arrow.GetChildOrNull<MeshInstance3D>(0).SetSurfaceOverrideMaterial(0, material);
    }
}
public class InputCharge : BaseMove, ISubInputManager
{
    private UnitGodot chargingUnit;
    private UnitGodot chargedUnit;
    public List<Charge> charges = new List<Charge>();
    private Charge currentCharge = new Charge();
    private float arrowModelLenght =0.85f;
    public InputCharge(BattlefieldCursorPosDel battlefieldCursorPosDel) : base(battlefieldCursorPosDel) {
    }
    public void finishChargeSubphase()// finish charge subphase, clear vars and selections
    {
        currentCharge = null;
        foreach (Charge charge in charges)
        {
            charge.arrow.QueueFree();
            charge.chargedUnit.hideChargingResponseBillboard();
        }
        charges.Clear();
    }
    public void selectUnit(UnitGodot unit)
    {
        if (currentCharge.chargingUnit == null )
        {            
            if (SelectOwnUnit(unit) != null && IsFreeOfDeclaredCharges(unit))
            {
                
                currentCharge.chargingUnit = unit;                
                unit.AddChild(currentCharge.arrow);
                currentCharge.arrow.Position = currentCharge.arrow.Position + new Vector3(unit.center.X, unit.center.Y, 0.1f);
            }            
        }
        else {
            if (SelectEnemyUnit(unit) != null)
            {
                if (checkValidCharge(currentCharge.chargingUnit, unit, calcDistanceBetweenUnits(currentCharge.chargingUnit, unit)))
                {
                    currentCharge.chargedUnit = unit;
                    
                    charges.Add(currentCharge);
                    SetArrowCharge(currentCharge);
                    currentCharge = new Charge();
                }
            }
            
        }
    }

    // TODO: for now is a simple euclidian distance, but should incorporate the rotation needed to face the front line
    private float calcDistanceBetweenUnits(Charge charge)
    {
        return calcDistanceBetweenUnits(charge.chargingUnit, charge.chargedUnit);
    }
    private float calcDistanceBetweenUnits(UnitGodot chargingUnit, UnitGodot chargedUnit)
    {
        System.Numerics.Vector2 chargingUnitGlobalCoords = chargingUnit.coreUnit.Transform.localToGlobalTransforms(chargingUnit.center.X, chargingUnit.center.Y);
        System.Numerics.Vector2 chargedUnitGlobalCoords = chargedUnit.coreUnit.Transform.localToGlobalTransforms(chargedUnit.center.X, chargedUnit.center.Y);
        Vector2 betweenUnitsDirectorVector = new Vector2(chargedUnitGlobalCoords.X - chargingUnitGlobalCoords.X, chargedUnitGlobalCoords.Y - chargingUnitGlobalCoords.Y);
        return betweenUnitsDirectorVector.Length();

    }
    private bool IsFreeOfDeclaredCharges(UnitGodot unit)
    {
        bool res = true;
        foreach(Charge charge in charges)
        {
            if (charge.chargingUnit == unit) return false;
        }
        return res;
    }
    // test if the charge is close enough, and charging unit has LOS to objetive
    private bool checkValidCharge(UnitGodot _chargingUnit, UnitGodot _chargedUnit, float dist=0f)
    {
        float maxChargeDist = _chargingUnit.coreUnit.distanceRemaining + _chargingUnit.coreUnit.MaximumChargedm;
        if (dist > maxChargeDist) {
            return false;
        }
        return true;
    }
    private void selectChargingUnit(UnitGodot unit)
    {
        chargingUnit = unit;
    }
    private void selectChargedUnit(UnitGodot unit)
    {
        chargedUnit = unit;
    }
    private void DrawArrow(Vector3 rot, Vector3 scale, Vector3 origin)
    {
        currentCharge.arrow.Rotation = rot;
        currentCharge.arrow.Scale = scale;
        currentCharge.arrow.Position = origin;
    }
    private void SetArrowCharge(Charge charge) {
        SetArrowCharge(charge.chargingUnit, charge.chargedUnit);
    }

    private void SetArrowCharge(UnitGodot chargingUnit, UnitGodot chargedUnit)
    {
        System.Numerics.Vector2 chargingUnitGlobalCoords = chargingUnit.coreUnit.Transform.localToGlobalTransforms(chargingUnit.center.X, chargingUnit.center.Y);
        System.Numerics.Vector2 chargedUnitGlobalCoords = chargedUnit.coreUnit.Transform.localToGlobalTransforms(chargedUnit.center.X, chargedUnit.center.Y);
        
        Vector2 betweenUnitsDirectorVector = new Vector2(chargedUnitGlobalCoords.X - chargingUnitGlobalCoords.X, chargedUnitGlobalCoords.Y - chargingUnitGlobalCoords.Y);
        Vector2 unitDirectorVector = new Vector2(chargingUnit.coreUnit.Transform.getVectorDirector().X, chargingUnit.coreUnit.Transform.getVectorDirector().Y);

        float angle = chargingUnit.affTrans.calculateAngle(unitDirectorVector.X, -unitDirectorVector.Y, betweenUnitsDirectorVector.X, betweenUnitsDirectorVector.Y);
        float angleRect = (float)Math.PI / 2;

        float dist = calcDistanceBetweenUnits(chargingUnit,chargedUnit);

        Vector3 rotation = new Vector3(-angle, angleRect, -angleRect);        

        // arrow model lenght is something like 0.82m
        float scaleGrow = betweenUnitsDirectorVector.Length() / arrowModelLenght;
        Vector3 scale = new Vector3(1f, 1f, scaleGrow);

        // move position by offset, should be the lenght * angle
        float xoffset = (float)(scaleGrow / 10f * Math.Cos(angle));
        float yoffset = (float)(scaleGrow / 10f * Math.Sin(angle));
        Vector3 position = new Vector3(chargingUnit.center.X - xoffset, chargingUnit.center.Y - yoffset, 0.1f);

        // final rot, scale and pos
        DrawArrow(rotation, scale, position);
    }
    
    private void AimArrowCursor(Vector3 worldPos, UnitGodot originUnit)
    {
        System.Numerics.Vector2 originGlobalCoords = originUnit.coreUnit.Transform.localToGlobalTransforms(originUnit.center.X, originUnit.center.Y);
        Vector2 worldPosDirectorVector = new Vector2(worldPos.X - originGlobalCoords.X, worldPos.Y- originGlobalCoords.Y);
        Vector2 unitDirectorVector = new Vector2(originUnit.coreUnit.Transform.getVectorDirector().X, originUnit.coreUnit.Transform.getVectorDirector().Y);
        float angle = originUnit.affTrans.calculateAngle(unitDirectorVector.X,-unitDirectorVector.Y, worldPosDirectorVector.X, worldPosDirectorVector.Y);
        float angleRect = (float)Math.PI / 2;
        
        float dist = worldPosDirectorVector.Length();
        currentCharge.isValidCharge( checkValidCharge(originUnit,null,dist));
        
        Vector3 rotation = new Vector3(-angle, angleRect, -angleRect);
        
        // arrow model lenght is something like 0.82m
        float scaleGrow = worldPosDirectorVector.Length()/ arrowModelLenght;
        Vector3 scale = new Vector3(1f,1f,scaleGrow);
        
        // move position by offset, should be the lenght * angle
        float xoffset = (float)(scaleGrow/10f * Math.Cos(angle));
        float yoffset = (float)(scaleGrow/10f * Math.Sin(angle));
        Vector3 position = new Vector3( originUnit.center.X - xoffset, originUnit.center.Y - yoffset, 0.1f);
        
        // final rot, scale and pos
        DrawArrow(rotation, scale, position);
    }
    new public void CustomProcess(double delta)
    {
        
        if (currentCharge.chargingUnit != null) {
            Vector3? worldPosition = battlefieldCursorPosDel();
            AimArrowCursor(worldPosition.Value, currentCharge.chargingUnit);
        }
        
        
    }
}

