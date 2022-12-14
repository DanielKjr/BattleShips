using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleShips
{

    public class PlacingShips : IState
    {
        public Ship[] shipList = new Ship[5];
        public MouseState mstate { get; set; }
        public bool MReleased { get; set; }
        public int currentShip;
        public bool notEnoughRoom = false;
        private static PlacingShips instance;
        public bool spaceReleased = true;
        public float ChatRotation = 0;
        public static PlacingShips Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlacingShips();
                }
                return instance;
            }
        }

        private PlacingShips()
        {
            shipList[0] = new Destroyer();
            shipList[1] = new Submarine();
            shipList[2] = new Cruiser();
            shipList[3] = new Battleship();
            shipList[4] = new Carrier();
            currentShip = 0;
            ChatRotation = 0;

        }
        public void Enter()
        {
            
            
        }

        public void Execute()
        {
            ShowCurrentShip();
            RotateShip();
            PlaceShip();
            if (currentShip == 5)
            {
                GameStateController.Instance.ChangeGameState(YourTurn.Instance);
            }
            
        }

        public void Exit()
        {

            notEnoughRoom = false;

            //send board to server, not sure if it should be here
            User user = GameWorld.Instance.Player.GetComponent<User>() as User;
            if (user != null)
            {              
                GameWorld.Instance._networkHandler.SendMessageToServer(new SendBoard()
                {
                    Name = user.Name,
                    Board = GameWorld.Instance.lowerB.cells

                }, MessageType.sendBoard);
            }        

        }

        public void PlaceShip()
        {
            
            mstate = Mouse.GetState();
            if (mstate.LeftButton == ButtonState.Pressed && MReleased == true && currentShip < 5 && GameWorld.Instance.LowerBoard.BoardSpace.Contains(new Vector2(mstate.X, mstate.Y)))
            {
                
                MReleased = false;
                if (CheckShip)
                {
                    notEnoughRoom = false;
                    if (checkedCells.All(x => !x.IsOccupied))
                    {
                        foreach (var cell in checkedCells)
                        {
                            cell.IsOccupied = true;
                            Point point = new Point((int)(cell.Position.X + 0.5f), (int)(cell.Position.Y + 0.5f - 10));
                            GameWorld.Instance.lowerB.cells[point].IsOccupied = true;
                        }
                        Cell selectedCell = GameWorld.Instance.LowerBoard.board.Find(x => x.isHovering == true);
                        GameObject thisShip = new GameObject();
                        Type shipType = shipList[currentShip].GetType();                       
                        Ship shippy = (Ship)Activator.CreateInstance(shipType);                        
                        SpriteRenderer SSR = new SpriteRenderer();
                        SSR.SetSprite(shippy.spriteString);
                        thisShip.AddComponent(shippy);
                        thisShip.AddComponent(SSR);
                        thisShip.Transform.Position = selectedCell.cellVector;
                        
                        switch (shipList[currentShip].shipdir)
                        {
                            case SHIPDIRECTION.UP:
                                SSR.OffSetY = shippy.OffSetY;
                                SSR.Rotation = 0;
                                break;
                            case SHIPDIRECTION.RIGHT:
                                SSR.OffSetY = 12;
                                SSR.OffSetX = shippy.OffSetX;
                                SSR.Rotation = (float)1.57f;
                                break;
                        }
                     
                        GameWorld.Instance.Instantiate(thisShip);
                        currentShip++;
                    }
                    if (currentShip == 5)
                    {
                       
                        GameStateController.Instance.ChangeGameState(YourTurn.Instance);
                    }
                }
                else
                {
                    notEnoughRoom = true;
                    checkedCells.Clear();
                }
            }


            if (mstate.LeftButton != ButtonState.Pressed)
            {
                MReleased = true;
            }
        }
        public void RotateShip()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && spaceReleased && currentShip < 5)
            {
                spaceReleased = false;
                switch (shipList[currentShip].shipdir)
                {
                    case SHIPDIRECTION.UP:
                        shipList[currentShip].shipdir = SHIPDIRECTION.RIGHT;
                        break;
                    case SHIPDIRECTION.RIGHT:
                        shipList[currentShip].shipdir = SHIPDIRECTION.UP;
                        break;
                }
            }
            
            if (Keyboard.GetState().IsKeyUp(Keys.Space))
            { 
                spaceReleased = true;
            }
        }
        public void ShowCurrentShip()
        {
            if (currentShip < 5)
            {
                switch (shipList[currentShip].shipdir)
                {
                    case SHIPDIRECTION.UP:
                        ChatRotation = 0;
                        break;
                    case SHIPDIRECTION.RIGHT:

                        ChatRotation = 1.57f;
                        break;
                }
            }
           


        }
            

        public bool CheckShip
        {

            get
            {

                Cell selectedCell = GameWorld.Instance.LowerBoard.board.First(x => x.isHovering == true);
                if (selectedCell != null)
                {
                    Point selectedPosition = selectedCell.Position;
                    List<Cell> checkedCells = new List<Cell>();
                    for (int i = 0; i < shipList[currentShip].CellSpan; i++)
                    {
                        if (shipList[currentShip].shipdir == SHIPDIRECTION.UP)
                        {

                            if (GameWorld.Instance.LowerBoard.board.Exists(x => x.Position == new Point(selectedCell.Position.X, selectedCell.Position.Y - i)))
                            {
                                Cell currentCell = GameWorld.Instance.LowerBoard.board.Find(x => x.Position == new Point(selectedCell.Position.X, selectedCell.Position.Y - i));
                                if (currentCell.IsOccupied)
                                {
                                    checkedCells.Add(currentCell);
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else if (shipList[currentShip].shipdir == SHIPDIRECTION.RIGHT)
                        {
                            if (GameWorld.Instance.LowerBoard.board.Exists(x => x.Position == new Point(selectedCell.Position.X + i, selectedCell.Position.Y)))
                            {
                                Cell currentCell = GameWorld.Instance.LowerBoard.board.Find(x => x.Position == new Point(selectedCell.Position.X + i, selectedCell.Position.Y));
                                if (currentCell.IsOccupied)
                                {
                                    checkedCells.Add(currentCell);
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    if (checkedCells.All(x => !x.IsOccupied))
                    {
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
        }

        public List<Cell> checkedCells
        {
            get
            {
                Cell selectedCell = GameWorld.Instance.LowerBoard.board.First(x => x.isHovering == true);
                Point selectedPosition = selectedCell.Position;
                List<Cell> checkedCells = new List<Cell>();
                for (int i = 0; i < shipList[currentShip].CellSpan; i++)
                {
                    if (shipList[currentShip].shipdir == SHIPDIRECTION.UP)
                    {
                        Cell currentCell = GameWorld.Instance.LowerBoard.board.Find(x => x.Position == new Point(selectedCell.Position.X, selectedCell.Position.Y - i));
                        checkedCells.Add(currentCell);
                    }
                    else
                    {
                        Cell currentCell = GameWorld.Instance.LowerBoard.board.Find(x => x.Position == new Point(selectedCell.Position.X + i, selectedCell.Position.Y));
                        checkedCells.Add(currentCell);
                    }
                }
                return checkedCells;
            }
        }
    }
}
