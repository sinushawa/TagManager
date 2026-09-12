using System;
using System.Collections.Generic;
using System.Linq;

namespace TagManager
{
    // Geometry priors, not object recognition. Dimensions are world AABB extents in metres.
    // No scene dependencies: usable before the first entity exists, and easy to test offline.
    public static class ArchitecturalSuggestions
    {
        public sealed class Guess
        {
            public string Path;
            public string Reason;
            public double Score;
        }

        public static List<Guess> FromDimensions(double x, double y, double z)
        {
            var result = new List<Guess>();
            if (new[] { x, y, z }.Any(v => double.IsNaN(v) || double.IsInfinity(v) || v < 0)) return result;
            double wide = Math.Max(x, y), narrow = Math.Max(Math.Min(x, y), .001);
            z = Math.Max(z, .001);
            Action<string, double, string> add = (path, score, reason) => result.Add(new Guess { Path = path, Score = score, Reason = reason });
            if (wide < .002 && z < .002) return result;
            if (z <= narrow * .25)
            {
                add("Building_Structure_Slabs", .72, "Broad horizontal form with small thickness; possible slab");
                add("Building_Interiors_Floors", .62, "Thin horizontal surface; possible floor finish");
                add("Site_Hardscape_Paving", .58, "Broad flat volume; possible paving or plaza surface");
                add("Building_Envelope_Roofs", .48, "Flat horizontal element; could be a roof");
                add("Station_Circulation_Platforms", .43, "Flat elongated surface; possible station platform");
            }
            else if (narrow <= wide * .22 && z >= narrow * 2)
            {
                add("Building_Structure_Walls", .72, "Thin vertical volume; possible wall");
                add("Building_Interiors_Partitions", .64, "Thin upright panel; possible interior partition");
                add("Building_Envelope_Glazing", .52, "Upright panel proportions; possible glazing");
                add("Building_Envelope_Doors", .46, "Upright thin element; possible door panel");
            }
            else if (z >= wide * 2.5)
            {
                add("Building_Structure_Columns", .74, "Slender vertical volume; possible structural column");
                add("Building_Envelope_Mullions", .50, "Slender vertical element; possible facade mullion");
                add("Site_Hardscape_Bollards", z < 1.6 ? .62 : .35, "Upright compact section; possible bollard");
            }
            else if (wide >= Math.Max(narrow, z) * 3)
            {
                add("Building_Structure_Beams", .72, "Long horizontal volume with compact cross-section; possible beam");
                add("Building_Circulation_Handrails", .48, "Long narrow element; possible handrail");
                add("Site_Furniture_Benches", .44, "Elongated low volume; possible bench");
            }
            else if (wide > 8 && z > 3)
            {
                add("Building_Massing_Volumes", .65, "Large solid volume; possible building massing");
                add("Building_Spaces_Halls", .48, "Large enclosed extent; possible hall space volume");
                add("Station_Spaces_Concourses", .42, "Large volume; possible concourse space");
                add("Museum_Spaces_Galleries", .42, "Large volume; possible gallery space");
            }
            else
            {
                add("Building_Interiors_Furniture_Cabinets", .43, "Compact solid volume; possible cabinet");
                add("Museum_Exhibitions_Plinths", .42, "Compact solid volume; possible display plinth");
                add("Building_Interiors_Furniture_Desks", .40, "Low solid extent; possible desk or work surface");
                add("Museum_Exhibitions_DisplayCases", .38, "Box-like volume; possible display case");
                add("Building_Massing_Volumes", .30, "Ambiguous volume; unclassified architectural massing");
            }
            return result.OrderByDescending(g => g.Score).ToList();
        }

        public static string[] Starters = {
            "Building_Structure_Slabs", "Building_Structure_Walls", "Building_Structure_Columns",
            "Building_Envelope_Glazing", "Building_Interiors_Furniture_Desks",
            "Museum_Exhibitions_DisplayCases", "Station_Circulation_Platforms", "Site_Hardscape_Paving"
        };
    }
}
