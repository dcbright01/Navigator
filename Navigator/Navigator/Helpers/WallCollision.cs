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

            int tempX1 = x1,
                tempY1 = y1;
            CheckDirection mode = CheckDirection.None;

            while (tempX1 != x2 || tempY1 != y2)
            {
                int differenceX = Math.Abs(tempX1 - x2);
                int differenceY = Math.Abs(tempY1 - y2);

                if (differenceX > differenceY)
                {
                    // We approach on X
                    if (tempX1 > x2)
                    {
                        // Left
                        mode = CheckDirection.Left;
                    }
                    else if (x2 > tempX1)
                    {
                        // right
                        mode = CheckDirection.Right;
                    }
                }
                else
                {
                    // We approach on Y
                    if (tempY1 > y2)
                    {
                        // Up
                        mode = CheckDirection.Up;
                    }
                    else if (y2 > tempY1)
                    {
                        // Down
                        mode = CheckDirection.Down;
                    }
                }
                switch (mode)
                {
                    case CheckDirection.Up:
                        tempY1--;
                        if (wallColors.Contains(pixelMethod(tempX1, tempY1)))
                            return false;
                        break;
                    case CheckDirection.Down:
                        tempY1++;
                        if (wallColors.Contains(pixelMethod(tempX1, tempY1)))
                            return false;
                        break;
                    case CheckDirection.Left:
                        tempX1--;
                        if (wallColors.Contains(pixelMethod(tempX1, tempY1)))
                            return false;
                        break;
                    case CheckDirection.Right:
                        tempX1++;
                        if (wallColors.Contains(pixelMethod(tempX1, tempY1)))
                            return false;
                        break;
                }
            }
            
            return true;
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