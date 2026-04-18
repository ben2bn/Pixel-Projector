namespace ComponentSystem;

using Godot;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;


[GlobalClass]
public partial class MixerAiComponent : Node, IComponent
{
	public struct AiTask
	{
		public string Name;
		public string[] Args;
	}

	// [Export]
	public AiTask[] TaskQueue { get; set; } = [];

	[Export]
	public PathfindingComponent PathComponent { get; set; }

	[Export]
	public StaticBody2D RedBox { get; set; }

	[Export]
	public StaticBody2D BlueBox { get; set; }

	[Export]
	public StaticBody2D GreenBox { get; set; }

	[Export]
	public InteractComponent InteractComp { get; set; }

	[Export]
	public HoldPixelComponent HoldPixelComp { get; set; }

	[Export]
	public StaticBody2D Mixer { get; set; }

	[Export]
	public StaticBody2D DepositBox { get; set; }

	[Signal]
	public delegate void StartAiLoopEventHandler();

	[Export]
	public QuestMaker QuestMaker { get; private set; }


	// void IHasComponentDependency.TryUpdateComponentDependencies(ComponentManager componentManager)
	// {
	// 	if (!A.IsValidInstance())
	// 	{
	// 		A = (Type)componentManager.GetComponentMatch(typeof(Type)) ?? A;
	// 	}
	// }

	public override void _Ready()
	{
		GetTasks();
	}


	public void GetTasks()
	{
		var httpRequest = new HttpRequest();
		AddChild(httpRequest);
		httpRequest.RequestCompleted += HttpRequestCompleted;

		// Perform a GET request. The URL below returns JSON as of writing.
		GD.Print("Performing HTTP request...");
		string colorQuests = QuestMaker.GetHTTPColorQuests();
		Error error = httpRequest.Request("http://10.20.17.103:5000/mixer?color=" + colorQuests + "&owned=0+0+0+0+0+0&available=0+0+0+0+0+0");
		GD.Print("HTTP request sent.");
		if (error != Error.Ok)
		{
			GD.PushError("An error occurred in the HTTP request.");
		}
	}

	private void HttpRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
	{
		GD.Print($"HTTP request completed with result: {result}, response code: {responseCode}");
		var json = new Json();
		json.Parse(body.GetStringFromUtf8());
		var tasks = json.GetData().AsGodotDictionary()["result"].AsGodotArray();
		// GD.Print($"Parsed {tasks.Count} tasks from the response.");
		// GD.Print(tasks);

		for (int i = 0; i < tasks.Count; i++)
		{
			var task = tasks[i].AsGodotDictionary();
			var taskName = task["name"].AsString();
			Godot.Collections.Array<string> args = [];
			// GD.Print($"Processing task {i}: {taskName} with args: {task["args"]}");
			if (task["args"].AsGodotDictionary().Count == 1)
			{
				// GD.Print($"Task {i} has 1 argument: {task["args"].AsGodotDictionary()["color"]}");
				args.Add(task["args"].AsGodotDictionary()["color"].ToString());
			}
			else if (task["args"].AsGodotDictionary().Count == 2)
			{
				args.Add(task["args"].AsGodotDictionary()["color_1"].ToString());
				args.Add(task["args"].AsGodotDictionary()["color_2"].ToString());
			}
			// GD.Print(args);
			// GD.Print($"Appending task {i}: {taskName} with args len: {args..}");
			TaskQueue = TaskQueue.Append(new AiTask { Name = taskName, Args = args.ToArray() }).ToArray();
		}

		StartAiLoopFn();
	}

	private async Task StartAiLoopFn()
	{
		foreach (var task in TaskQueue)
		{
			if (task.Name == "go_collect_paint")
			{
				GD.Print($"Processing task: {task.Name} with args: {task.Args[0]}");
				var color = task.Args[0];
				Node2D target = null;
				switch (color)
				{
					case "red":
						target = RedBox;
						break;
					case "blue":
						target = BlueBox;
						break;
					case "green":
						target = GreenBox;
						break;
				}
				PathComponent.TargetDestination = target;
				await ToSignal(PathComponent, "OnReachTargetPosition");
				InteractComp.InteractCurrentInteractable();
				while (HoldPixelComp.PixelColor == null)
				{
					await ToSignal(GetTree().CreateTimer(0.5), "timeout");
					InteractComp.InteractCurrentInteractable();
					// GD.Print("Waiting to hold pixel...");
					// GD.Print($"Current held pixel color: {HoldPixelComp.PixelColor}");
				}
				PathComponent.TargetDestination = Mixer;
				await ToSignal(PathComponent, "OnReachTargetPosition");
				InteractComp.InteractCurrentInteractable();
			}
			else if (task.Name == "mix_paints")
			{
				GD.Print($"Processing task: {task.Name} with args: {task.Args[0]}, {task.Args[1]}");
				// PathComponent.TargetDestination = Mixer;
				// InteractComp.InteractCurrentInteractable();
				while (HoldPixelComp.PixelColor == null)
				{
					await ToSignal(GetTree().CreateTimer(0.5), "timeout");
					InteractComp.InteractCurrentInteractable();
				}
				PathComponent.TargetDestination = DepositBox;
				await ToSignal(PathComponent, "OnReachTargetPosition");
				InteractComp.InteractCurrentInteractable();
				while (HoldPixelComp.PixelColor == null)
				{
					await ToSignal(GetTree().CreateTimer(0.5), "timeout");
					InteractComp.InteractCurrentInteractable();
				}
			}
		}
	}
}
