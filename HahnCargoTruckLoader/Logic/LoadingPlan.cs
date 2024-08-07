using System;
using System.Collections.Generic;
using System.Linq;

public class LoadingPlan
{
    private readonly Dictionary<int, LoadingInstruction> instructions;
    private readonly Truck truck;
    private readonly List<Crate> crates;

    public LoadingPlan(Truck truck, List<Crate> crates)
    {
        this.truck = truck;
        this.crates = crates;
        this.instructions = new Dictionary<int, LoadingInstruction>();
    }

    public Dictionary<int, LoadingInstruction> GetLoadingInstructions()
    {
        var usedSpace = new bool[truck.Width, truck.Height, truck.Length];
        int stepNumber = 0;

        foreach (var crate in crates)
        {
            bool placed = false;

            // Try all orientations for the crate
            foreach (var orientation in GetPossibleOrientations(crate))
            {
                // Attempt to place the crate in every possible position in the truck
                for (int x = 0; x <= truck.Width - orientation.Width; x++)
                {
                    for (int y = 0; y <= truck.Height - orientation.Height; y++)
                    {
                        for (int z = 0; z <= truck.Length - orientation.Length; z++)
                        {
                            if (CanPlaceCrate(usedSpace, x, y, z, orientation))
                            {
                                PlaceCrate(usedSpace, x, y, z, orientation);

                                instructions[crate.CrateID] = new LoadingInstruction
                                {
                                    LoadingStepNumber = stepNumber++,
                                    CrateId = crate.CrateID,
                                    TopLeftX = x,
                                    TopLeftY = y,
                                    TurnHorizontal = orientation.TurnHorizontal,
                                    TurnVertical = orientation.TurnVertical
                                };

                                placed = true;
                                break;
                            }
                        }
                        if (placed) break;
                    }
                    if (placed) break;
                }
                if (placed) break;
            }

            if (!placed)
            {
                throw new Exception($"Unable to place crate with ID {crate.CrateID}");
            }
        }

        return instructions;
    }

    private IEnumerable<Crate> GetPossibleOrientations(Crate crate)
    {
        // Create a new crate instance with the adjusted dimensions for each possible orientation
        yield return CreateOrientedCrate(crate, false, false);
        yield return CreateOrientedCrate(crate, true, false);
        yield return CreateOrientedCrate(crate, false, true);
        yield return CreateOrientedCrate(crate, true, true);
    }

    private Crate CreateOrientedCrate(Crate crate, bool turnHorizontal, bool turnVertical)
    {
        var orientedCrate = new Crate
        {
            CrateID = crate.CrateID,
            Width = crate.Width,
            Height = crate.Height,
            Length = crate.Length
        };

        // Apply the turns based on the LoadingInstruction
        orientedCrate.Turn(new LoadingInstruction
        {
            TurnHorizontal = turnHorizontal,
            TurnVertical = turnVertical
        });

        return new Crate
        {
            CrateID = orientedCrate.CrateID,
            Width = orientedCrate.Width,
            Height = orientedCrate.Height,
            Length = orientedCrate.Length,
            TurnHorizontal = turnHorizontal,
            TurnVertical = turnVertical
        };
    }

    private bool CanPlaceCrate(bool[,,] usedSpace, int x, int y, int z, Crate orientation)
    {
        for (int i = x; i < x + orientation.Width; i++)
        {
            for (int j = y; j < y + orientation.Height; j++)
            {
                for (int k = z; k < z + orientation.Length; k++)
                {
                    if (i >= truck.Width || j >= truck.Height || k >= truck.Length || usedSpace[i, j, k])
                        return false;
                }
            }
        }
        return true;
    }

    private void PlaceCrate(bool[,,] usedSpace, int x, int y, int z, Crate orientation)
    {
        for (int i = x; i < x + orientation.Width; i++)
        {
            for (int j = y; j < y + orientation.Height; j++)
            {
                for (int k = z; k < z + orientation.Length; k++)
                {
                    usedSpace[i, j, k] = true;
                }
            }
        }
    }
}
