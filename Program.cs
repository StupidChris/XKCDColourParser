﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static System.Console;
using static System.Math;

namespace XKCDColourParser
{
    public class Program
    {
        /// <summary>
        /// Colour support structure 
        /// </summary>
        public struct Colour : IComparable<Colour>
        {
            #region Fields
            public readonly string name;    //Name
            public readonly float r;        //Red
            public readonly float g;        //Green
            public readonly float b;        //Blue
            #endregion

            #region Constructor
            /// <summary>
            /// Creates a new Colour
            /// </summary>
            /// <param name="name">Name of the colour</param>
            /// <param name="r">Red component (0 to 1)</param>
            /// <param name="g">Green component (0 to 1)</param>
            /// <param name="b">Blue component (0 to 1)</param>
            public Colour(string name, float r, float g, float b)
            {
                this.name = name;
                this.r = Clamp01(r);
                this.g = Clamp01(g);
                this.b = Clamp01(b);
            }
            #endregion

            #region Methods
            /// <summary>
            /// Compares two colours, using their string name
            /// </summary>
            /// <param name="other">Colour to compare to</param>
            /// <returns>-1 if this colour comes before, 1 if it comes after, and 0 if they are equal</returns>
            public int CompareTo(Colour other) => string.Compare(this.name, other.name, StringComparison.Ordinal);
            #endregion

            #region Static methods
            /// <summary>
            /// Clamps a float value between 0 and 1
            /// </summary>
            /// <param name="f">Float to clamp</param>
            /// <returns>The clamped <paramref name="f"/></returns>
            public static float Clamp01(float f) => Max(0, Min(1, f));
            #endregion
        }

        #region Constants
        private const string path = @"D:\Chris\Desktop\rgb.txt";        //File path
        private const string sPath = @"D:\Chris\Desktop\formatted.txt"; //Save path
        private const bool forCSharp6 = true;                           //Switch to false if for usage in a non C#6.0 environment
        #endregion

        #region Main
        private static void Main()
        {
            WriteLine("Starting XKCD colour parser");
            WriteLine("Loading from " + path);
            Stopwatch watch = Stopwatch.StartNew();
            string[] lines = File.ReadAllLines(path);
            WriteLine($"Loaded, {lines.Length} lines to parse");
            
            List<Colour> colours = new List<Colour>(lines.Length);
            int i;
            for (i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line[0] == '#') { continue; } //Empty or comment

                string[] splits = line.Split(' ');
                if (splits.Length != 2) { continue; }                           //Badly formatted

                string color = splits[1];
                if (color.Length != 6) { continue; }                            //Badly formatted

                colours.Add(new Colour(splits[0], ToFloat(color, 0), ToFloat(color, 2), ToFloat(color, 4)));
            }

            watch.Stop();
            WriteLine($"Detected {colours.Count} correctly formatted colours, in {watch.ElapsedMilliseconds}ms");
            WriteLine("Pressed any key to proceed with parsing");
            ReadLine();

            watch.Restart();
            colours.Sort();
            colours = new List<Colour>(colours.Distinct());
            string[] formatted = new string[colours.Count * 5];
            i = 0;
            foreach (Colour colour in colours)
            {
                WriteLine($"Parsing colour {colour.name} with RGB code of ({colour.r}, {colour.g}, {colour.b})");
                formatted[i++] =  "/// <summary>";
                formatted[i++] = $"/// A formatted XKCD survey colour ({colour.r}, {colour.g}, {colour.b})";
                formatted[i++] =  "/// </summary>";
                formatted[i++] = $"public static{(forCSharp6 ? string.Empty : " readonly")} Color {colour.name}{(forCSharp6 ? " { get; }" : string.Empty)} = new Color({colour.r}{IsFloat(colour.r)}, {colour.g}{IsFloat(colour.g)}, {colour.b}{IsFloat(colour.b)});";
                formatted[i++] = string.Empty;
            }

            WriteLine("Saving to " + sPath);
            File.WriteAllLines(sPath, formatted);
            watch.Stop();
            WriteLine($"Parsed in  {watch.ElapsedMilliseconds}ms");
            WriteLine("Press any key to close");
            ReadLine();
        }
        #endregion

        #region Static methods
        /// <summary>
        /// Converts a hexadecimal byte number into a 0 to 1 float
        /// </summary>
        /// <param name="hex">Hex number to convert</param>
        /// <param name="start">Starting index in the string to fetch the two hex chars for</param>
        /// <returns>The float version of this hex number</returns>
        private static float ToFloat(string hex, int start) => Convert.ToByte(hex.Substring(start, 2), 16) / 255f;

        /// <summary>
        /// Returns the f character if necessary to display the float correctly for the passed number
        /// </summary>
        /// <param name="f">Number to check</param>
        /// <returns>string.Empty if <paramref name="f"/> is 0 or 1, else returns "f"</returns>
        private static string IsFloat(float f) => f == 0 || f == 1 ? string.Empty : "f";
        #endregion
    }
}