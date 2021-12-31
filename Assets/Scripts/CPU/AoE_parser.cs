using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AoE_input_parser
{
    public static SimulationState state;

    static string input;
    static string[] lines;

    public static void LoadInput()
    {
        // Loat input.txt
        input = System.IO.File.ReadAllText(@"C:\Users\Public\Documents\input13.txt");
        // Split input string into an array of strings, separated by empty lines and " "
        //lines = new List<string>(input.Split(new string[] { "\r\n", "\n"}, System.StringSplitOptions.RemoveEmptyEntries));
    }


    public static Cell[] parse_input(string input)
    {
        string[] input_array = input.Split(' ');
        Cell[] cells = new Cell[input_array.Length];
        for (int i = 0; i < input_array.Length; i++)
        {
            //cells[i] = new Cell(input_array[i]);
        }
        return cells;
    }
}
