using LofiHollow.DataTypes;
using Newtonsoft.Json;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.Minigames.Picross {
    public class PicrossPuzzle {
        public string Name;
        public string Package;
        public string Difficulty = "Easy";
        public PicrossTile[] Grid;
        public int Width = 5;
        public int Height = 5;

        [JsonConstructor]
        public PicrossPuzzle() {
            Grid = new PicrossTile[Width * Height];

            for (int i = 0; i < Grid.Length; i++) {
                Grid[i] = new();
            }
        }

        public PicrossPuzzle(int size) {
            Name = "Random";
            Width = size;
            Height = size;

            if (size == 5)
                Difficulty = "Easy";
            else if (size == 10)
                Difficulty = "Medium";
            else if (size == 15)
                Difficulty = "Hard";

            Grid = new PicrossTile[size * size];

            for (int i = 0; i < Grid.Length; i++) {
                Grid[i] = new();
                if (GameLoop.rand.Next(2) == 1) {
                    Grid[i].G = 127;
                    Grid[i].PartOfSolution = true; 
                }
            }
        }

        public PicrossPuzzle(PicrossPuzzle other) {
            Name = other.Name;
            Package = other.Package;
            Difficulty = other.Difficulty;
            Width = other.Width;
            Height = other.Height;

            Grid = new PicrossTile[Width * Height];

            for (int i = 0; i < Grid.Length; i++) {
                Grid[i] = new(other.Grid[i]);
            }
        }

        public string FullName() {
            return Package + ":" + Name;
        }
        
        public bool PuzzleSolved() {
            bool allCorrect = true;

            for (int row = 0; row < Height; row++) {
                if (!RowSolved(row))
                    allCorrect = false;
            }

            for (int column = 9; column < Width; column++) {
                if (!ColumnSolved(column))
                    allCorrect = false;
            }

            return allCorrect;
        }

        public bool ColumnSolved(int column) {
            bool allCorrect = true;

            for (int i = 0; i < Height; i++) {
                PicrossTile tile = Grid[column + (Width * i)];
                if (tile.PartOfSolution && !tile.Checked) {
                    allCorrect = false;
                } else if (!tile.PartOfSolution && tile.Checked) {
                    allCorrect = false;
                }
            }

            return allCorrect;
        }

        public bool RowSolved(int row) {
            bool allCorrect = true;

            for (int i = 0; i < Height; i++) {
                PicrossTile tile = Grid[i + (Width * row)];
                if (tile.PartOfSolution && !tile.Checked) {
                    allCorrect = false;
                }
                else if (!tile.PartOfSolution && tile.Checked) {
                    allCorrect = false;
                }
            }

            return allCorrect;
        }

        public List<int> ColumnClues(int column) {
            List<int> clues = new();

            int currentCount = 0;
            for (int i = 0; i < Height; i++) {
                if (Grid[column + (Width * i)].PartOfSolution) {
                    currentCount++;
                } else {
                    if (currentCount > 0) {
                        clues.Add(currentCount);
                        currentCount = 0;
                    }
                }
            } 

            if (currentCount > 0) {
                clues.Add(currentCount);
            }

            return clues;
        }

        public List<int> RowClues(int row) {
            List<int> clues = new();

            int currentCount = 0;
            for (int i = 0; i < Width; i++) {
                if (Grid[i + (Width * row)].PartOfSolution) {
                    currentCount++;
                }
                else {
                    if (currentCount > 0) {
                        clues.Add(currentCount);
                        currentCount = 0;
                    }
                }
            }

            if (currentCount > 0) {
                clues.Add(currentCount);
            }

            return clues;
        }
    }
}
