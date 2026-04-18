using Godot;
using System;
using System.Collections.Generic;

public partial class QuestMaker : Node
{
    [Export]
    public float MinQuestTimer { get; private set; } = 5f;
    [Export]
    public float MaxQuestTimer { get; private set; } = 20f;

    public enum ColorIndex
    {
        Red = 0,
        Green = 1,
        Blue = 2,
        Cyan = 3,
        Yellow = 4,
        Magenta = 5,
    }

    public int GameScore
    {
        get => gameScore;
        private set
        {
            if (value < 0) value = 0;
            gameScore = value;
            UpdateGameScoreCounter();
        }
    }
    private int gameScore = 0;

    [Export]
    public Label GameScoreCounter { get; private set; }

	public int RedQuest
    {
        get => redQuest;
        private set
        {
            redQuest = value;
            UpdateRedCounter();
        }
    }
    private int redQuest = 0;
    public int BlueQuest
    {
        get => blueQuest;
        private set
        {
            blueQuest = value;
            UpdateBlueCounter();
        }
    }
    private int blueQuest = 0;
    public int GreenQuest
    {
        get => greenQuest;
        private set
        {
            greenQuest = value;
            UpdateGreenCounter();
        }
    }
    private int greenQuest = 0;
    public int CyanQuest
    {
        get => cyanQuest;
        private set
        {
            cyanQuest = value;
            UpdateCyanCounter();
        }
    }
    private int cyanQuest = 0;
    public int YellowQuest
    {
        get => yellowQuest;
        private set
        {
            yellowQuest = value;
            UpdateYellowCounter();
        }
    }
    private int yellowQuest = 0;
    public int MagentaQuest
    {
        get => magentaQuest;
        private set
        {
            magentaQuest = value;
            UpdateMagentaCounter();
        }
    }
    private int magentaQuest = 0;

    [Export]
    public Label RedCounter { get; private set; }
    [Export]
    public Label BlueCounter { get; private set; }
    [Export]
    public Label GreenCounter { get; private set; }
    [Export]
    public Label CyanCounter { get; private set; }
    [Export]
    public Label YellowCounter { get; private set; }
    [Export]
    public Label MagentaCounter { get; private set; }

    private Timer timer;

    public override void _Ready()
    {
        if (!RedCounter.IsValidInstance()) GD.PushWarning($"{PropertyName.RedCounter} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (!BlueCounter.IsValidInstance()) GD.PushWarning($"{PropertyName.BlueCounter} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (!GreenCounter.IsValidInstance()) GD.PushWarning($"{PropertyName.GreenCounter} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (!CyanCounter.IsValidInstance()) GD.PushWarning($"{PropertyName.CyanCounter} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (!YellowCounter.IsValidInstance()) GD.PushWarning($"{PropertyName.YellowCounter} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (!MagentaCounter.IsValidInstance()) GD.PushWarning($"{PropertyName.MagentaCounter} at {GetPath()} should be defined explicitely in editor before running if available.");
        if (!GameScoreCounter.IsValidInstance()) GD.PushWarning($"{PropertyName.GameScoreCounter} at {GetPath()} should be defined explicitely in editor before running if available.");

        SetTimer();
    }

    public void SetTimer()
    {
        if (timer == null)
        {
            timer = new Timer();
        }

        timer.Autostart = true;
        timer.OneShot = false;
        timer.WaitTime = RandomExtensions.RandomGenerator.RandfRange(MinQuestTimer, MaxQuestTimer);

        if (!timer.IsInsideTree())
        {
            timer.Timeout += AddRandomQuest;
            AddChild(timer);
        }
    }

    public void AddRandomQuest()
	{
		ColorIndex colorIndex = (ColorIndex)RandomExtensions.RandomGenerator.RandiRange(0, 5);

        switch(colorIndex)
        {
            case ColorIndex.Red:
                RedQuest++;
                break;
            case ColorIndex.Green:
                GreenQuest++;
                break;
            case ColorIndex.Blue:
                BlueQuest++;
                break;
            case ColorIndex.Cyan:
                CyanQuest++;
                break;
            case ColorIndex.Yellow:
                YellowQuest++;
                break;
            case ColorIndex.Magenta:
                MagentaQuest++;
                break;
        }

        timer.WaitTime = RandomExtensions.RandomGenerator.RandfRange(MinQuestTimer, MaxQuestTimer);
    }

    public void OnReceiveColor(Color color)
    {
        switch(color)
        {
            case Color c when c == new Color(1, 0, 0)://red
                RedQuest -= 1;
                if (RedQuest < 0) RedQuest = 0;
                else GameScore++;
                    break;
            case Color c when c == new Color(0, 1, 0)://green
                GreenQuest -= 1;
                if (GreenQuest < 0) GreenQuest = 0;
                else GameScore++;
                break;
            case Color c when c == new Color(0, 0, 1)://blue
                BlueQuest -= 1;
                if (BlueQuest < 0) BlueQuest = 0;
                else GameScore++;
                break;
            case Color c when c == new Color(0, 1, 1)://cyan
                CyanQuest -= 1;
                if (CyanQuest < 0) CyanQuest = 0;
                else GameScore++;
                break;
            case Color c when c == new Color(1, 1, 0)://yellow
                YellowQuest -= 1;
                if (YellowQuest < 0) YellowQuest = 0;
                else GameScore++;
                break;
            case Color c when c == new Color(1, 0, 1)://magenta
                MagentaQuest -= 1;
                if (MagentaQuest < 0) MagentaQuest = 0;
                else GameScore++;
                break;
        }
    }

    public void UpdateRedCounter()
    {
        RedCounter.Text = RedQuest.ToString();
    }
    public void UpdateGreenCounter()
    {
        GreenCounter.Text = GreenQuest.ToString();
    }
    public void UpdateBlueCounter()
    {
        BlueCounter.Text = BlueQuest.ToString();
    }
    public void UpdateCyanCounter()
    {
        CyanCounter.Text = CyanQuest.ToString();
    }
    public void UpdateYellowCounter()
    {
        YellowCounter.Text = YellowQuest.ToString();
    }
    public void UpdateMagentaCounter()
    {
        MagentaCounter.Text = MagentaQuest.ToString();
    }

    public void UpdateGameScoreCounter()
    {
        GameScoreCounter.Text = GameScore.ToString();
    }

    public string GetHTTPColorQuests()
    {
        string colors = "";
        if (RedQuest > 0)
        {
            colors += "red+";
        }
        if (BlueQuest > 0)
        {
            colors += "blue+";
        }
        if (GreenQuest > 0)
        {
            colors += "green+";
        }
        if (CyanQuest > 0)
        {
            colors += "cyan+";
        }
        if (YellowQuest > 0)
        {
            colors += "yellow+";
        }
        if (MagentaQuest > 0)
        {
            colors += "magenta+";
        }

        if (colors.Length > 0)
        {
            colors = colors.Remove(colors.Length - 1);
        }

        return colors;
    }
}
