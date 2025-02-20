using Core.DB.Models;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GodotFrontend.code.UIOverlay
{
    internal class BattlefieldOverlay
    {
        //singleton
        private static BattlefieldOverlay instance;

        public static BattlefieldOverlay Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BattlefieldOverlay();
                }
                return instance;
            }
        }
        private Node3D battlefield;
        
        // init the battlefield overlay
        public void vinculateBattlefield(Node3D _battlefield)
        {
            battlefield = _battlefield;
            createOverlaySprite();
        }
        private void createOverlaySprite()
        {
            var sprite = new Sprite3D();
            var subViewPort = new SubViewport();
            sprite.Position = new Vector3(0, 0, 0.6f);
            sprite.AddChild(subViewPort);
            sprite.Texture = subViewPort.GetTexture();
            subViewPort.Size = new Vector2I(100, 500);

            //var canvas = new CanvasLayer();
            //subViewPort.AddChild(canvas);
            //var label = new Godot.Label();
            //label.Text = "Charge";

            //canvas.AddChild(label);
            battlefield.AddChild(sprite);


        }
        public void drawShootLine(UnitGodot selectedUnit)
        {
            //TODO


        }
    }
}
