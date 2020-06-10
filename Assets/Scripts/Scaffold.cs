using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ColoredSquares;
using UnityEngine;

using Rnd = UnityEngine.Random;

public sealed class Scaffold : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMRuleSeedable RuleSeedable;

    public KMSelectable[] Buttons;
    public Material[] Materials;
    public Material[] MaterialsCB;
    public Light[] Lights;

    private MeshRenderer[] _buttonRenderers;
    private Coroutine _activeCoroutine;
    private static T[] newArray<T>(params T[] array) { return array; }
    private static readonly Color[] _lightColors = newArray<Color>(
        Color.black, Color.white, Color.red, new Color32(0x83, 0x83, 0xff, 0xff), Color.green, Color.yellow, Color.magenta,
        new Color32(0xfe, 0x97, 0x00, 0xff), new Color32(0x00, 0xfe, 0xff, 0xff), new Color32(0x85, 0x16, 0xca, 0xff),
        new Color32(0x93, 0x04, 0x00, 0xff), new Color32(0xb1, 0x61, 0x10, 0xff), new Color32(0xe0, 0xa9, 0xfe, 0xff),
        new Color32(0x28, 0x75, 0xfe, 0xff), new Color32(0x87, 0xed, 0x8d, 0xff), new Color32(0x00, 0x2b, 0x14, 0xff),
        new Color32(0xb4, 0xb4, 0xb4, 0xff));

    public bool IsColorblind { get; private set; }
    public bool IsCoroutineActive { get { return _activeCoroutine != null; } }

    private void Awake()
    {
        _buttonRenderers = Buttons.Select(b => b.GetComponent<MeshRenderer>()).ToArray();
    }

    public void FixLightSizes(float scalar)
    {
        for (int i = 0; i < 16; i++)
            Lights[i].range = .1f * scalar;
    }

    public void SetColorblind(KMColorblindMode cb)
    {
        IsColorblind = cb.ColorblindModeActive;
    }

    public void SetColorblind(SquareColor[] colors)
    {
        IsColorblind = true;
        StartSquareColorsCoroutine(colors);
    }

    public void SetButtonBlack(int index)
    {
        _buttonRenderers[index].sharedMaterial = Materials[(int) SquareColor.Black];
        Lights[index].gameObject.SetActive(false);
    }

    public void SetAllButtonsBlack()
    {
        for (int i = 0; i < 16; i++)
            SetButtonBlack(i);
    }

    public void SetButtonColor(int ix, SquareColor color)
    {
        if (color == SquareColor.Black)
            SetButtonBlack(ix);
        else
        {
            _buttonRenderers[ix].sharedMaterial = IsColorblind ? MaterialsCB[(int) color] ?? Materials[(int) color] : Materials[(int) color];
            Lights[ix].color = _lightColors[(int) color];
            Lights[ix].gameObject.SetActive(true);
        }
    }

    public void ModuleSolved()
    {
        if (_activeCoroutine != null)
        {
            StopCoroutine(_activeCoroutine);
            _activeCoroutine = null;
        }
        SetAllButtonsBlack();
    }

    public void StartSquareColorsCoroutine(SquareColor[] colors, SquaresToRecolor behaviour = SquaresToRecolor.All, bool delay = false, bool unshuffled = false)
    {
        var indexes = new List<int?>((behaviour == SquaresToRecolor.NonwhiteOnly
            ? Enumerable.Range(0, 16).Where(ix => colors[ix] != SquareColor.White)
            : Enumerable.Range(0, 16)).Select(i => (int?) i));
        if (!unshuffled)
            indexes.Shuffle();
        if (delay)
            indexes.Insert(0, null);
        StartSquareColorsCoroutine(colors, indexes.ToArray());
    }

    /// <summary>
    /// Starts a coroutine that re-colors some of the squares.
    /// </summary>
    /// <param name="colors">The colors to set the relevant squares to.</param>
    /// <param name="indexes">Specifies which squares to recolor and in which order. Insert a <c>null</c> value to add a delay.</param>
    public void StartSquareColorsCoroutine(SquareColor[] colors, int?[] indexes)
    {
        if (_activeCoroutine != null)
            StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(SetSquareColorsCoroutine(colors, indexes));
    }

    private IEnumerator SetSquareColorsCoroutine(SquareColor[] colors, int?[] indexes)
    {
        foreach (var i in indexes)
        {
            if (i == null)
                yield return new WaitForSeconds(Rnd.Range(1.5f, 2f));
            else
            {
                SetButtonColor(i.Value, colors[i.Value]);
                yield return new WaitForSeconds(.03f);
            }
        }
        _activeCoroutine = null;
    }
}
