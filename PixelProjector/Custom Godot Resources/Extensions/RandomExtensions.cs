using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class RandomExtensions
{
    /// <summary>
    /// Instance of <see cref="RandomNumberGenerator"/>, used to generate random numbers through its built-in methods.
    /// </summary>
    public static RandomNumberGenerator RandomGenerator { get; private set; } = new RandomNumberGenerator();

    /// <summary>
    /// Generates a random sequence of string of length <paramref name="stringLength"/>. The string consists only of characters included in <paramref name="include"/>.
    /// </summary>
    /// <remarks>
    /// If <paramref name="include"/> is null or empty, then the default character list includes only the alphabet in lower and upper case.
    /// </remarks>
    public static string GenerateRandomString(int stringLength, List<char> include = null)
    {
        if (include == null || include.Count == 0)
        {
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            include = alphabet.ToList<char>();
            include.AddRange(alphabet.ToLower().ToList<char>());
        }

        string newString = string.Empty;
        for (int i = 0; i < stringLength; i++)
        {
            newString += include[RandomGenerator.RandiRange(0, include.Count)];
        }

        return newString;
    }
}