using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ColoredSquares;
using UnityEngine;

using Rnd = UnityEngine.Random;

/// <summary>
/// On the Subject of Colored Squares
/// Created by TheAuthorOfOZ, implemented by Timwi
/// </summary>
public class ColoredSquaresModule : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMAudio Audio;

    public KMSelectable[] Buttons;

    void Start()
    {
        Debug.Log("[ColoredSquares] Started.");
        Module.OnActivate += ActivateModule;

        for (int i = 0; i < 16; i++)
        {
            var j = i;
            Buttons[i].OnInteract += delegate { Pushed(j); return false; };
        }
    }

    void ActivateModule()
    {
    }

    void Pushed(int index)
    {
        Debug.LogFormat(@"[ColoredSquares] You pushed button #{0}.", index);
    }
}
