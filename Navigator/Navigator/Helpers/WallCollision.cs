using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigator.Helpers
{
    public interface IGetPixel
    {
        int GetPixel(int x, int y);
    }


    public class WallCollision
    {
        private bool[,] isWall;
        private List<int> wallColors = new List<int>()
        {
            Color.FromArgb(255,255,255,255).ToArgb(),
            Color.FromArgb(0,0,0,0).ToArgb()
        };

        Func<int,int,int> pixelMethod;

        public WallCollision(Func<int,int,int> getPixel)
        {
            pixelMethod = getPixel;
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
                    if (wallColors.Contains(pixelMethod(x1, y)))
                    {
                        return false;
                    }
                }
                return true;
            case CheckDirection.Down:
                for (int y = y1; y <= y2; y++)
                {
                    if (wallColors.Contains(pixelMethod(x1, y)))
                    {
                        return false;
                    }
                }
                return true;
            case CheckDirection.Left:
                for (int x = x1; x >= x2; x--)
                {
                    if (wallColors.Contains(pixelMethod(x, y1)))
                    {
                        return false;
                    }
                }
                return true;
            case CheckDirection.Right:
                for (int x = x1; x <= x2; x++)
                {
                    if (wallColors.Contains(pixelMethod(x, y1)))
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