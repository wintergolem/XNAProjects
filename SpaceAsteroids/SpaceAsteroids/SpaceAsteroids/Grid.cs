using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace SpaceAsteroids
{
    class Cell
    {
        Vector4 v4Coord;
        List<Gameobject> lGameobjects;
        List<Cell> neighborCells;

        public void InitPos(Vector4 a_v4Coord)
        {
            v4Coord = a_v4Coord;
            lGameobjects = new List<Gameobject>();
        }
        public void InitNeighors ( List<Cell> a_neighors)
        {
            neighborCells = a_neighors;
        }

        public void Clear()
        {
            lGameobjects = new List<Gameobject>();
        }

        public void Add(Gameobject a_go)
        {
            lGameobjects.Add(a_go);
        }

        public int CheckIfShouldAdd(Gameobject a_go)//returns 0 = added , 1 = close smaller , 2 = close larger , 3 = far small , 4 = far larger
        {
            //check x coordinates
            if (a_go.v2Position.X >= v4Coord.X && a_go.v2Position.X <= v4Coord.Z)
            {
                //check y coordinate
                if (a_go.v2Position.Y >= v4Coord.Y && a_go.v2Position.Y <= v4Coord.W)
                {
                    Add(a_go);
                    return 0;
                }
                else if (a_go.v2Position.Y < v4Coord.Y)
                {
                    //x is right but y is smaller so check smaller index
                    return 1;
                }
                else
                {
                    //x is right but y is too big, so check larger index
                    return 2;
                }
            }
            else if (a_go.v2Position.X < v4Coord.X)
            {
                //x is smaller so check a lower index
                return 3;
            }
            //x must be larger, so check a bigger index
            return 4;
        }

        public void CheckCollisions()
        {
            foreach (Gameobject g in lGameobjects)
            {
                foreach (Gameobject g2 in lGameobjects)
                {
                    if (g != g2)
                    {
                        if ( ((g.v2Position.X < g2.v2Position.X + g2.fCircleSize) && (g.v2Position.X > g2.v2Position.X)) ||
                                ((g.v2Position.X + g.fCircleSize < g2.v2Position.X + g2.fCircleSize) && (g.v2Position.X + g.fCircleSize > g2.v2Position.X)) )
                        {
                            //x coordinate in colliding, check y
                            if (((g.v2Position.Y < g2.v2Position.Y + g2.fCircleSize) && (g.v2Position.Y > g2.v2Position.Y)) ||
                                ((g.v2Position.Y + g.fCircleSize < g2.v2Position.Y + g2.fCircleSize) && (g.v2Position.Y + g.fCircleSize > g2.v2Position.Y)))
                            {
                                //collided
                                g.Collide();
                                g2.Collide();
                            }
                        }
                    }
                }
            }
        }
    }

    class Grid
    {
        //variables
        float fGridSize;
        int iCellCountwidth;
        int iCellCountHeight;
        GameManager manager;
        Cell[] aCells;

        public Grid() { }
        public Grid(float a_ifGridSize)
        {
            fGridSize = a_ifGridSize;
            iCellCountwidth = (int)( Math.Ceiling( (manager.viewport.Width / fGridSize) ) );
            iCellCountHeight = (int)(Math.Ceiling((manager.viewport.Height / fGridSize) ) );
            aCells = new Cell[iCellCountHeight * iCellCountwidth];

            //get all cells memory and coord
            int iIndex = 0;
            for( int i = 0 ; i < iCellCountHeight; i++ )
                for (int j = 0; j < iCellCountwidth; j++ )
                {
                    aCells[ iIndex ] = new Cell();
                    aCells[ iIndex ].InitPos(new Vector4(i * a_ifGridSize, j * a_ifGridSize, i * a_ifGridSize + a_ifGridSize, j * a_ifGridSize + a_ifGridSize));
                    iIndex++;
                }
        }

        public void AddToGrid(Gameobject a_go)
        {
            int iIndex = (int)(Math.Ceiling((double)(aCells.Length/2) ));
            int result = aCells[iIndex].CheckIfShouldAdd( a_go );

            while (result != 0)
            {
                switch (result)
                {
                    case 1:
                        //close smaller                         
                        iIndex = (int)MathHelper.Clamp( CalculateIndexLowerClose(iIndex) , 0, aCells.Length);
                        break;
                    case 2:
                        //close large                           (current index divided by iCellcountheight times icellcountheight do get upper bounds of column
                        iIndex = (int)MathHelper.Clamp( CalculateIndexUpperClose(iIndex) , 0, aCells.Length);
                        break;
                    case 3:
                        //far small
                        iIndex = (int)MathHelper.Clamp( iIndex / 2, 0, aCells.Length);
                        break;

                    case 4:
                        //far big                           find remaining amount of cells, half that and add it the current index
                        iIndex = (int)MathHelper.Clamp( iIndex + (aCells.Length - iIndex) / 2, 0, aCells.Length);
                        break;
                }
                result = aCells[iIndex].CheckIfShouldAdd(a_go);
            }
        }

        int CalculateIndexLowerClose(int a_iIndex)
        {
            //(current index divided by iCellCount height) times (iCellcountheight) plus one to get lower bounds of column
            int iLowerBounds = (int)(1 + (iCellCountHeight) * (Math.Floor((double)(a_iIndex / iCellCountHeight))));
            //add half the difference between the lower bounds and current index to the lower bounds to get next index
            return iLowerBounds + (a_iIndex - iLowerBounds) / 2;
        }

        int CalculateIndexUpperClose(int a_iIndex)
        {
            //(current index divided by iCellCount height) times (iCellcountheight) plus one to get lower bounds of column
            int iUpperBounds = (int)((iCellCountHeight) * (Math.Ceiling((double)(a_iIndex / iCellCountHeight))));
            //add half the difference between the lower bounds and current index to the lower bounds to get next index
            return a_iIndex + (int) MathHelper.Clamp( (iUpperBounds - a_iIndex) / 2 , 1 , 100000000000);
        }
    }
}
