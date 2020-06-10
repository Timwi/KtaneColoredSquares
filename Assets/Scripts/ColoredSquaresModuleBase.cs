using System;
using System.Collections;
using System.Collections.Generic;
using ColoredSquares;
using UnityEngine;

public abstract class ColoredSquaresModuleBase : MonoBehaviour
{
    public Scaffold ScaffoldPrefab;
    protected Scaffold Scaffold;
    public KMColorblindMode ColorblindMode;

    private KMBombModule _module;

    protected SquareColor[] _colors = new SquareColor[16];

    public abstract string Name { get; }

    private static readonly Dictionary<string, int> _moduleIdCounters = new Dictionary<string, int>();
    private int _moduleId;
    protected bool _isSolved = false;

    private void Awake()
    {
        if (!_moduleIdCounters.ContainsKey(Name))
            _moduleIdCounters[Name] = 1;

        _moduleId = _moduleIdCounters[Name]++;
        _module = GetComponent<KMBombModule>();

        Scaffold = Instantiate(ScaffoldPrefab, transform);
        Scaffold.SetColorblind(ColorblindMode);
        var moduleSelectable = GetComponent<KMSelectable>();
        foreach (var btn in Scaffold.Buttons)
            btn.Parent = moduleSelectable;
        moduleSelectable.Children = Scaffold.Buttons;
        moduleSelectable.UpdateChildren();
        for (int i = 0; i < 16; i++)
            Scaffold.Buttons[i].OnInteract = MakeButtonHandler(i);
        Scaffold.SetAllButtonsBlack();
        Scaffold.FixLightSizes(_module.transform.lossyScale.x);
    }

    private KMSelectable.OnInteractHandler MakeButtonHandler(int index)
    {
        return delegate
        {
            Scaffold.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Scaffold.Buttons[index].transform);
            Scaffold.Buttons[index].AddInteractionPunch();
            if (!_isSolved)
                ButtonPressed(index);
            return false;
        };
    }

    protected void Log(string format, params object[] args)
    {
        Debug.LogFormat(@"[{0} #{1}] {2}", Name, _moduleId, string.Format(format, args));
    }

    protected void LogDebug(string format, params object[] args)
    {
        Debug.LogFormat(@"<{0} #{1}> {2}", Name, _moduleId, string.Format(format, args));
    }

    protected abstract void ButtonPressed(int index);

    protected void ModulePassed()
    {
        Scaffold.ModuleSolved();
        Log("Module solved.");
        _module.HandlePass();
        _isSolved = true;
    }

    protected void Strike()
    {
        _module.HandleStrike();
        Scaffold.SetAllButtonsBlack();
    }

    protected void PlaySound(int index)
    {
        switch (_colors[index])
        {
            case SquareColor.Red:
            case SquareColor.Black:
            case SquareColor.White:
            case SquareColor.Forest:
                Scaffold.Audio.PlaySoundAtTransform("redlight", Scaffold.Buttons[index].transform);
                break;
            case SquareColor.Blue:
            case SquareColor.Orange:
            case SquareColor.Brown:
            case SquareColor.Gray:
                Scaffold.Audio.PlaySoundAtTransform("bluelight", Scaffold.Buttons[index].transform);
                break;
            case SquareColor.Green:
            case SquareColor.Cyan:
            case SquareColor.Mauve:
                Scaffold.Audio.PlaySoundAtTransform("greenlight", Scaffold.Buttons[index].transform);
                break;
            case SquareColor.Yellow:
            case SquareColor.Purple:
            case SquareColor.Azure:
                Scaffold.Audio.PlaySoundAtTransform("yellowlight", Scaffold.Buttons[index].transform);
                break;
            case SquareColor.Magenta:
            case SquareColor.Chestnut:
            case SquareColor.Jade:
                Scaffold.Audio.PlaySoundAtTransform("magentalight", Scaffold.Buttons[index].transform);
                break;
        }
    }

#pragma warning disable 414
    protected readonly string TwitchHelpMessage = @"!{0} A1 A2 A3 B3 [specify column as letter, then row as number] | !{0} colorblind";
#pragma warning restore 414

    protected IEnumerator ProcessTwitchCommand(string command)
    {
        if (command.Trim().Equals("colorblind", StringComparison.InvariantCultureIgnoreCase))
        {
            Scaffold.SetColorblind(_colors);
            yield return null;
            yield break;
        }

        var buttons = new List<KMSelectable>();
        foreach (var piece in command.ToLowerInvariant().Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (piece.Length != 2 || piece[0] < 'a' || piece[0] > 'd' || piece[1] < '1' || piece[1] > '4')
                yield break;
            buttons.Add(Scaffold.Buttons[(piece[0] - 'a') + 4 * (piece[1] - '1')]);
        }

        yield return null;
        yield return "solve";
        yield return "strike";
        yield return buttons;
    }
}