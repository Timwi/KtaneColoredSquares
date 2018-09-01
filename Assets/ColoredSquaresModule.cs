using System;
using System.Collections;
using System.Collections.Generic;
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
    public KMRuleSeedable RuleSeedable;

    public KMSelectable[] Buttons;
    public Material[] Materials;
    public Material BlackMaterial;
    public Light LightTemplate;

    private Light[] _lights;
    private SquareColor[] _colors;
    private readonly Color[] _lightColors = new[] { Color.white, Color.red, new Color(131f / 255, 131f / 255, 1f), Color.green, Color.yellow, Color.magenta };

    // Contains the (seeded) rules
    private object[][] _table;

    private HashSet<int> _allowedPresses;
    private HashSet<int> _expectedPresses;
    private object _lastStage;
    private SquareColor _firstStageColor;   // for Souvenir

    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private Coroutine _activeCoroutine;

    static T[] newArray<T>(params T[] array) { return array; }

    void Start()
    {
        _moduleId = _moduleIdCounter++;

        var rnd = RuleSeedable.GetRNG();
        if (rnd.Seed == 1)
        {
            // false = Column; true = Row
            _table = newArray(
                new object[] { SquareColor.Blue, false, SquareColor.Red, SquareColor.Yellow, true, SquareColor.Green, SquareColor.Magenta },
                new object[] { true, SquareColor.Green, SquareColor.Blue, SquareColor.Magenta, SquareColor.Red, false, SquareColor.Yellow },
                new object[] { SquareColor.Yellow, SquareColor.Magenta, SquareColor.Green, true, SquareColor.Blue, SquareColor.Red, false },
                new object[] { SquareColor.Blue, SquareColor.Green, SquareColor.Yellow, false, SquareColor.Red, true, SquareColor.Magenta },
                new object[] { SquareColor.Yellow, true, SquareColor.Blue, SquareColor.Magenta, false, SquareColor.Red, SquareColor.Green },
                new object[] { SquareColor.Magenta, SquareColor.Red, SquareColor.Yellow, SquareColor.Green, false, SquareColor.Blue, true },
                new object[] { SquareColor.Green, true, false, SquareColor.Blue, SquareColor.Magenta, SquareColor.Yellow, SquareColor.Red },
                new object[] { SquareColor.Magenta, SquareColor.Red, SquareColor.Green, SquareColor.Blue, SquareColor.Yellow, false, true },
                new object[] { false, SquareColor.Yellow, SquareColor.Red, SquareColor.Green, true, SquareColor.Magenta, SquareColor.Blue },
                new object[] { SquareColor.Green, false, true, SquareColor.Red, SquareColor.Magenta, SquareColor.Blue, SquareColor.Yellow },
                new object[] { SquareColor.Red, SquareColor.Yellow, true, false, SquareColor.Green, SquareColor.Magenta, SquareColor.Blue },
                new object[] { false, SquareColor.Blue, SquareColor.Magenta, SquareColor.Red, SquareColor.Yellow, true, SquareColor.Green },
                new object[] { true, SquareColor.Magenta, false, SquareColor.Yellow, SquareColor.Blue, SquareColor.Green, SquareColor.Red },
                new object[] { SquareColor.Red, SquareColor.Blue, SquareColor.Magenta, true, SquareColor.Green, SquareColor.Yellow, false },
                new object[] { false, true, false, true, false, true, false }
            );
        }
        else
        {
            var candidates = new object[] { SquareColor.Blue, false, SquareColor.Red, SquareColor.Yellow, true, SquareColor.Green, SquareColor.Magenta };
            _table = new object[15][];
            for (int i = 0; i < 14; i++)
            {
                candidates.Shuffle(rnd);
                _table[i] = candidates.ToArray();
            }
            _table[14] = new object[] { false, true, false, true, false, true, false };
        }

        float scalar = transform.lossyScale.x;
        _lights = new Light[16];
        _colors = new SquareColor[16];

        for (int i = 0; i < 16; i++)
        {
            var j = i;
            Buttons[i].OnInteract += delegate { Pushed(j); return false; };
            Buttons[i].GetComponent<MeshRenderer>().material = BlackMaterial;
            var light = _lights[i] = (i == 0 ? LightTemplate : Instantiate(LightTemplate));
            light.name = "Light" + (i + 1);
            light.transform.parent = Buttons[i].transform;
            light.transform.localPosition = new Vector3(0, 0.08f, 0);
            light.transform.localScale = new Vector3(1, 1, 1);
            light.gameObject.SetActive(false);
            light.range = .1f * scalar;
        }

        SetInitialState();
    }

    private void SetInitialState()
    {
        int[] counts;
        int minCount;
        SquareColor minCountColor;
        do
        {
            counts = new int[5];
            for (int i = 0; i < 16; i++)
            {
                _colors[i] = (SquareColor) Rnd.Range(1, 6);
                counts[(int) _colors[i] - 1]++;
            }
            minCount = counts.Where(c => c > 0).Min();
            minCountColor = (SquareColor) (Array.IndexOf(counts, minCount) + 1);
        }
        while (counts.Count(c => c == minCount) > 1);

        _firstStageColor = minCountColor;
        _lastStage = minCountColor;
        _expectedPresses = new HashSet<int>();
        _allowedPresses = new HashSet<int>();
        for (int i = 0; i < 16; i++)
            if (_colors[i] == minCountColor)
            {
                _allowedPresses.Add(i);
                _expectedPresses.Add(i);
            }
        _activeCoroutine = StartCoroutine(SetSquareColors());
        Debug.LogFormat("[ColoredSquares #{2}] First stage color is {0}; count={1}.", _firstStageColor, minCount, _moduleId);
    }

    private IEnumerator SetSquareColors()
    {
        yield return new WaitForSeconds(1.75f);
        var sequence = shuffle(Enumerable.Range(0, 16).Where(ix => _colors[ix] != SquareColor.White).ToList());
        for (int i = 0; i < sequence.Count; i++)
        {
            SetSquareColor(sequence[i]);
            yield return new WaitForSeconds(.03f);
        }
        _activeCoroutine = null;
    }

    private static IList<T> shuffle<T>(IList<T> list)
    {
        if (list == null)
            throw new ArgumentNullException("list");
        for (int j = list.Count; j >= 1; j--)
        {
            int item = Rnd.Range(0, j);
            if (item < j - 1)
            {
                var t = list[item];
                list[item] = list[j - 1];
                list[j - 1] = t;
            }
        }
        return list;
    }

    void SetSquareColor(int index)
    {
        Buttons[index].GetComponent<MeshRenderer>().material = Materials[(int) _colors[index]];
        _lights[index].color = _lightColors[(int) _colors[index]];
        _lights[index].gameObject.SetActive(true);
    }

    private void SetBlack(int index)
    {
        Buttons[index].GetComponent<MeshRenderer>().material = BlackMaterial;
        _lights[index].gameObject.SetActive(false);
    }

    private void SetAllBlack()
    {
        for (int i = 0; i < 16; i++)
            SetBlack(i);
    }

    void Pushed(int index)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Buttons[index].transform);
        Buttons[index].AddInteractionPunch();

        if (_expectedPresses == null)
            return;

        if (!_allowedPresses.Contains(index))
        {
            Debug.LogFormat(@"[ColoredSquares #{2}] Button #{0} ({1}) was incorrect at this time.", index, _colors[index], _moduleId);
            Module.HandleStrike();
            if (_activeCoroutine != null)
                StopCoroutine(_activeCoroutine);
            SetAllBlack();
            SetInitialState();
        }
        else
        {
            switch (_colors[index])
            {
                case SquareColor.Red:
                    Audio.PlaySoundAtTransform("redlight", Buttons[index].transform);
                    break;
                case SquareColor.Blue:
                    Audio.PlaySoundAtTransform("bluelight", Buttons[index].transform);
                    break;
                case SquareColor.Green:
                    Audio.PlaySoundAtTransform("greenlight", Buttons[index].transform);
                    break;
                case SquareColor.Yellow:
                    Audio.PlaySoundAtTransform("yellowlight", Buttons[index].transform);
                    break;
                case SquareColor.Magenta:
                    Audio.PlaySoundAtTransform("magentalight", Buttons[index].transform);
                    break;
            }

            _expectedPresses.Remove(index);
            _colors[index] = SquareColor.White;
            SetSquareColor(index);
            if (_expectedPresses.Count == 0)
            {
                var whiteCount = _colors.Count(c => c == SquareColor.White);
                if (whiteCount == 16)
                {
                    Debug.LogFormat(@"[ColoredSquares #{0}] Module passed.", _moduleId);
                    SetAllBlack();
                    _expectedPresses = null;
                    _allowedPresses = null;
                    Module.HandlePass();
                }
                else
                {
                    _allowedPresses.Clear();
                    if (_activeCoroutine != null)
                        StopCoroutine(_activeCoroutine);

                    var toBeRecolored = Enumerable.Range(0, 16).Where(i => _colors[i] != SquareColor.White).ToList();
                    foreach (var i in toBeRecolored)
                        SetBlack(i);

                    // Move to next stage.
                    var nextStage = _table[whiteCount - 1][_lastStage is SquareColor ? (int) (SquareColor) _lastStage - 1 : _lastStage.Equals(true) ? 5 : 6];
                    Debug.LogFormat("[ColoredSquares #{2}] {0} lit: next stage is {1}.", whiteCount, nextStage.Equals(true) ? "Row" : nextStage.Equals(false) ? "Column" : ((SquareColor) nextStage).ToString(), _moduleId);
                    if (nextStage.Equals(true))
                    {
                        // Row
                        var firstRow = Enumerable.Range(0, 4).First(row => Enumerable.Range(0, 4).Any(col => _colors[4 * row + col] != SquareColor.White));
                        for (int col = 0; col < 4; col++)
                            if (_colors[4 * firstRow + col] != SquareColor.White)
                            {
                                _allowedPresses.Add(4 * firstRow + col);
                                _expectedPresses.Add(4 * firstRow + col);
                            }
                        foreach (var sq in toBeRecolored)
                            _colors[sq] = (SquareColor) Rnd.Range(1, 6);
                    }
                    else if (nextStage.Equals(false))
                    {
                        // Column
                        var firstCol = Enumerable.Range(0, 4).First(col => Enumerable.Range(0, 4).Any(row => _colors[4 * row + col] != SquareColor.White));
                        for (int row = 0; row < 4; row++)
                            if (_colors[4 * row + firstCol] != SquareColor.White)
                            {
                                _allowedPresses.Add(4 * row + firstCol);
                                _expectedPresses.Add(4 * row + firstCol);
                            }
                        foreach (var sq in toBeRecolored)
                            _colors[sq] = (SquareColor) Rnd.Range(1, 6);
                    }
                    else
                    {
                        // A specific color
                        var color = (SquareColor) nextStage;
                        var ix = Rnd.Range(0, toBeRecolored.Count);
                        _colors[toBeRecolored[ix]] = color;
                        toBeRecolored.RemoveAt(ix);
                        foreach (var sq in toBeRecolored)
                            _colors[sq] = (SquareColor) Rnd.Range(1, 6);
                        for (int i = 0; i < 16; i++)
                            if (_colors[i] == color)
                            {
                                _allowedPresses.Add(i);
                                _expectedPresses.Add(i);
                            }
                    }
                    _lastStage = nextStage;
                    _activeCoroutine = StartCoroutine(SetSquareColors());
                }
            }
        }
    }

#pragma warning disable 414
    private string TwitchHelpMessage = @"Press the desired squares with “!{0} red”, “!{0} green”, “!{0} blue”, “!{0} yellow”, “!{0} magenta”, “!{0} row”, or “!{0} col”.";
#pragma warning restore 414

    KMSelectable[] ProcessTwitchCommand(string command)
    {
        var colors = Enum.GetValues(typeof(SquareColor));
        foreach (SquareColor col in colors)
            if (command.Equals(col.ToString(), StringComparison.OrdinalIgnoreCase))
                return Enumerable.Range(0, 16).Where(i => _colors[i] == col).Select(i => Buttons[i]).ToArray();

        if (command.Equals("row", StringComparison.OrdinalIgnoreCase))
        {
            var applicableRow = Enumerable.Range(0, 5).First(row => row == 4 || Enumerable.Range(0, 4).Any(col => _colors[4 * row + col] != SquareColor.White));
            if (applicableRow == 4)
                return null;
            return Enumerable.Range(0, 4).Where(col => _colors[4 * applicableRow + col] != SquareColor.White).Select(col => Buttons[4 * applicableRow + col]).ToArray();
        }

        if (command.Equals("col", StringComparison.OrdinalIgnoreCase) || command.Equals("column", StringComparison.OrdinalIgnoreCase))
        {
            var applicableCol = Enumerable.Range(0, 5).First(col => col == 4 || Enumerable.Range(0, 4).Any(row => _colors[4 * row + col] != SquareColor.White));
            if (applicableCol == 4)
                return null;
            return Enumerable.Range(0, 4).Where(row => _colors[4 * row + applicableCol] != SquareColor.White).Select(row => Buttons[4 * row + applicableCol]).ToArray();
        }

        return null;
    }
}
