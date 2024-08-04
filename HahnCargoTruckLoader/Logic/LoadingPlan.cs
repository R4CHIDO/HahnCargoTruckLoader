using HahnCargoTruckLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HahnCargoTruckLoader.Logic
{
    public class LoadingPlan
    {
        private readonly Dictionary<int, LoadingInstruction> instructions;
        private readonly Truck truck;
        private readonly List<Crate> crates;

        public LoadingPlan(Truck truck, List<Crate> crates)
        {
            this.truck = truck;
            this.crates = crates;
            instructions = new Dictionary<int, LoadingInstruction>();
        }

        public Dictionary<int, LoadingInstruction> GetLoadingInstructions()
        {
            // Initialize occupied space tracker
            bool[,,] cargoSpace = new bool[truck.Width, truck.Height, truck.Length];
            int step = 1;

            // Try placing each crate
            foreach (var crate in crates)
            {
                bool placed = false;

                // Try all orientations
                foreach (var orientation in crate.GetOrientations())
                {
                    if (TryPlaceCrate(crate, orientation, cargoSpace, out var position, out var turns))
                    {
                        // Create and store loading instruction
                        instructions[crate.CrateID] = new LoadingInstruction
                        {
                            LoadingStepNumber = step++,
                            CrateId = crate.CrateID,
                            TopLeftX = position.Item1,
                            TopLeftY = position.Item2,
                            TurnHorizontal = turns.Item1,
                            TurnVertical = turns.Item2
                        };
                        placed = true;
                        break;
                    }
                }

                if (!placed)
                {
                    throw new Exception("Could not place all crates.");
                }
            }

            return instructions;
        }

        private bool TryPlaceCrate(Crate crate, (int width, int height, int length) orientation, bool[,,] cargoSpace, 
                                   out (int, int) position, out (bool, bool) turns)
        {
            turns = (false, false);
            position = (-1, -1);

            var (crateWidth, crateHeight, crateLength) = orientation;

            // Try to fit the crate in every possible position
            for (int x = 0; x <= truck.Width - crateWidth; x++)
            {
                for (int y = 0; y <= truck.Height - crateHeight; y++)
                {
                    for (int z = 0; z <= truck.Length - crateLength; z++)
                    {
                        if (CanPlace(crateWidth, crateHeight, crateLength, x, y, z, cargoSpace))
                        {
                            // Mark the crate as placed
                            PlaceCrate(crateWidth, crateHeight, crateLength, x, y, z, cargoSpace);
                            position = (x, y);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool CanPlace(int crateWidth, int crateHeight, int crateLength, int startX, int startY, int startZ, bool[,,] cargoSpace)
        {
            for (int x = startX; x < startX + crateWidth; x++)
            {
                for (int y = startY; y < startY + crateHeight; y++)
                {
                    for (int z = startZ; z < startZ + crateLength; z++)
                    {
                        if (cargoSpace[x, y, z])
                        {
                            return false; // Space is already occupied
                        }
                    }
                }
            }
            return true;
        }

        private void PlaceCrate(int crateWidth, int crateHeight, int crateLength, int startX, int startY, int startZ, bool[,,] cargoSpace)
        {
            for (int x = startX; x < startX + crateWidth; x++)
            {
                for (int y = startY; y < startY + crateHeight; y++)
                {
                    for (int z = startZ; z < startZ + crateLength; z++)
                    {
                        cargoSpace[x, y, z] = true;
                    }
                }
            }
        }
    }
}
