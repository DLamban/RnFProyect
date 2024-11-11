using Godot;
using System;
using System.Collections;
using System.Collections.Generic;


public partial class DataNode2d : Node2D
{

    public enum DrawCommandType
    {
        Rectangle,
        Circle,
        Line,
        Polygon
    }
    
    public class DrawCommand
    {
        public DrawCommandType Type;
        public Vector2 Position;
        public Color Color;
        public Vector2 Size;       
        public float Radius;               
        public Vector2[] polygonPoints;
        
        public DrawCommand(Vector2 position, Color color, Vector2 size)
        {
            Type = DrawCommandType.Rectangle;
            Position = position;
            Color = color;
            Size = size;
        }

        public DrawCommand(Vector2 position, float radius, Color color)
        {
            Type = DrawCommandType.Circle;
            Position = position;
            Radius = radius;
            Color = color;
        }    
        public DrawCommand(List<Vector2> _polygonPoints, Color color)
        {
            Type = DrawCommandType.Polygon;
            polygonPoints = _polygonPoints.ToArray(); 
            Color = color;
        }
    }


    Queue<DrawCommand> commands = new Queue<DrawCommand>();
    public override void _Draw()
    {
        while (commands.Count > 0)
        {
            DrawCommand command = commands.Dequeue();
            switch (command.Type)
            {
                case DrawCommandType.Rectangle:
                    DrawRect(new Rect2(command.Position, command.Size), command.Color);
                    break;

                case DrawCommandType.Circle:
                    DrawCircle(command.Position, command.Radius, command.Color);
                    break;

                case DrawCommandType.Polygon:
                    DrawColoredPolygon(command.polygonPoints,command.Color);
                    break;
            }
        }
           
    }
    public void drawPolygon(List<Vector2> points, Color color)
    {        
        commands.Enqueue(new DrawCommand(points, color));
        QueueRedraw();
    }
    public void drawCircle(Vector2 center, float radius, Color color)
    {
        commands.Enqueue(new DrawCommand(center,radius, color));
        QueueRedraw();
    }

    public void drawRectColor(Rect2I rect, Color color)
    {

        commands.Enqueue(new DrawCommand(rect.Position,color,rect.Size));
        QueueRedraw();
    }
}
