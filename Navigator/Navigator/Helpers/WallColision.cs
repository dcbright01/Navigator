using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Splat;

namespace Navigator.Helpers
{
    /// <summary>
    /// Class that should be able to calculate path colision between two vertices (currently a single step for testing)
    /// </summary>
    public class WallColision
    {

        private bool[,] isWall;
        private List<Color> wallColors = new List<Color>()
        {
            Color.FromArgb(255,255,255,255),
            Color.FromArgb(0,0,0,0)
        };

        public IBitmap visualized;

        public WallColision(bool[,] isWallArray)
        {
            isWall = isWallArray;
        }

        public bool IsValidStep(int x1, int y1, int x2, int y2)
        {
            CheckDirection mode = CheckDirection.None;
            // calculate differences 
            if (x1 > x2)
            {
                // Left
                mode = CheckDirection.Left;
            }
            else if (x2 > x1)
            {
                // right
                mode = CheckDirection.Right;
            }
            else // X1 == X2
            {
                // Check changes on y
                if (y1 > y2)
                {
                    // Up
                    mode = CheckDirection.Up;
                }
                else if (y2 > y1)
                {
                    // Down
                    mode = CheckDirection.Down;
                }
            }
            switch (mode)
            {
                case CheckDirection.Up:
                    for (int y = y1; y >= y2; y--)
                    {
                        if (isWall[x1, y])
                        {
                            return false;
                        }
                    }
                    return true;
                case CheckDirection.Down:
                    for (int y = y1; y <= y2; y++)
                    {
                        if (isWall[x1, y])
                        {
                            return false;
                        }
                    }
                    return true;
                case CheckDirection.Left:
                    for (int x = x1; x >= x2; x--)
                    {
                        if (isWall[x, y1])
                        {
                            return false;
                        }
                    }
                    return true;
                case CheckDirection.Right:
                    for (int x = x1; x <= x2; x++)
                    {
                        if (isWall[x, y1])
                        {
                            return false;
                        }
                    }
                    return true;
            }
            return false;
        }

        enum CheckDirection
        {
            Up,
            Down,
            Right,
            Left,
            None
        }
    }
}
