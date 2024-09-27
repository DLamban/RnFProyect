using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Core.Units
{
    public class CloseOrder
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Size EnclosedRectangle { get; set; }
        //public List<Point> polygonPoints { get; set; }
        public List<Vector2> enclosedPolygon {  get; set; }
        public BaseTroop[,] formation { get; set; }
        
        public CloseOrder(int width, List<BaseTroop> troops)
        {
            BaseTroop troopExample = troops[0];
            EnclosedRectangle = new Size(width * troopExample.Size.Width, (int)Math.Ceiling((float)troops.Count / width) * troopExample.Size.Height);
            Width = width;
            Height = (int)Math.Ceiling((float)troops.Count / width);
            //ignoramos caracteres por ahora
            // estilo basico, sin locuras de matematicas de matrices
            int x = 0;
            int y = 0;
            formation = new BaseTroop[Height, Width];
            foreach (var troop in troops)
            {
                if (x == width)
                {
                    x = 0;
                    y++;
                }
                formation[y, x] = troop;
                x++;
            }
            // at the end we have the x and y of the last troop, made the list of points of the polygon
            List<Point> points = new List<Point>();
            points.Add(new Point(0, 0));
            points.Add(new Point(0, EnclosedRectangle.Height));
            //anadimos un punto en medio para marcar direccion
            points.Add(new Point(EnclosedRectangle.Width / 2, EnclosedRectangle.Height + 10));
            points.Add(new Point(EnclosedRectangle.Width, EnclosedRectangle.Height));
            if (troops.Count % width == 0)
            {
                // height regular
                points.Add(new Point(EnclosedRectangle.Width, 0));
            }
            else
            {
                // height irregular
                int irregularHeight = troops.Count / width * troopExample.Size.Height;
                points.Add(new Point(EnclosedRectangle.Width, EnclosedRectangle.Height - irregularHeight));
                int irregularWidth = troops.Count % width * troopExample.Size.Width;
                points.Add(new Point(EnclosedRectangle.Width - irregularWidth, EnclosedRectangle.Height - irregularHeight));
                points.Add(new Point(EnclosedRectangle.Width - irregularWidth, 0));
            }
            //polygonPoints = points;
        }
        // TODO: only for close order and open order, for now
        // HEIGHT is in negative, the ranks go backwards
        public List<Vector2> calculateEnclosedPolygondm(int frontlineWidth, List<BaseTroop> troops)
        {
            BaseTroop troopExample = troops[0];
            int troopsCount = troops.Count;
            List<Vector2> polygonPoints = new List<Vector2>();
            // we add in clockwise pattern, doesn't matter, but it's more coherent
            polygonPoints.Add(new Vector2(0, 0));
            // we still have a full line
            if (troopsCount >= frontlineWidth)
            {
                polygonPoints.Add(new Vector2((frontlineWidth * troopExample.Size.Width)/100.0f , 0));
            }else {
                polygonPoints.Add(new Vector2((troopsCount * troopExample.Size.Width)/100.0f, 0));
                polygonPoints.Add(new Vector2((troopsCount * troopExample.Size.Width) / 100.0f,  -troopExample.Size.Height/100.0f));
                polygonPoints.Add(new Vector2(0, -troopExample.Size.Height / 100.0f));

                return polygonPoints;
            }
            if (troopsCount % frontlineWidth == 0) { 
                int ranks = troopsCount / frontlineWidth;
                polygonPoints.Add(new Vector2((frontlineWidth * troopExample.Size.Width) / 100.0f, (ranks * -troopExample.Size.Height) / 100.0f));
                polygonPoints.Add(new Vector2(0, (ranks * -troopExample.Size.Height) / 100.0f));

            }
            else
            {
                int ranksComplete = (troopsCount / frontlineWidth);
                int incompleteRankWidth = (troopsCount % frontlineWidth);
                
                polygonPoints.Add(new Vector2((frontlineWidth * troopExample.Size.Width) / 100.0f, (ranksComplete*-troopExample.Size.Height) /100.0f));
                polygonPoints.Add(new Vector2((incompleteRankWidth * troopExample.Size.Width) / 100.0f, (ranksComplete * -troopExample.Size.Height) / 100.0f));

                int ranksIncomplete = ranksComplete + 1; 
               
                polygonPoints.Add(new Vector2((incompleteRankWidth * troopExample.Size.Width) / 100.0f, (ranksIncomplete * -troopExample.Size.Height) / 100.0f));
                polygonPoints.Add(new Vector2(0, (ranksIncomplete * -troopExample.Size.Height) / 100.0f));

            }   
            return polygonPoints;
        }
        public void printFormation()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (formation[y, x] != null)
                    {
                        Console.Write("#");
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                }
                Console.WriteLine();
            }
        }
    }

}
