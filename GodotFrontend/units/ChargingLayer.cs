using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFrontend.units
{
    internal class ChargingLayer
    {
       
        // We will need a information layer to show max charge distance
        // maybe a hud, or meybe a texture
        private CanvasLayer canvas;
        public Sprite3D sprite;
        private SubViewport subViewPort;
        private Label label;
        internal ChargingLayer(UnitGodot unit)
        {
            sprite = new Sprite3D();
            subViewPort = new SubViewport();
            sprite.Position = new Vector3(0, 0, 0.6f);
            sprite.AddChild(subViewPort);
            sprite.Texture = subViewPort.GetTexture();            
            subViewPort.Size = new Vector2I(100, 500);

            canvas = new CanvasLayer();
            subViewPort.AddChild(canvas);
            label = new Label();
            label.Text = "Charge";
            
            canvas.AddChild(label);
                       
            adjustChargingLayer(unit);
        }
        private void adjustChargingLayer(UnitGodot unit)
        {
            //sprite.Scale = new Vector3(unit.coreUnit.sizeEnclosedRectangledm.X, unit.coreUnit.MaximumCharge, 1);
            sprite.Scale = new Vector3(unit.coreUnit.sizeEnclosedRectangledm.X, unit.coreUnit.MaximumChargedm, 1);

            Vector3 calcPosition = Vector3.Zero;
            float movementOffset = unit.affTrans.ForwardVec.Y * unit.coreUnit.temporalCombatVars.distanceRemaining;
            calcPosition.X = unit.center.X;
            calcPosition.Y = -unit.center.Y + movementOffset;
            calcPosition.Z = 0.30f;

            sprite.Position = calcPosition;
           
            
        }
    }
}
