namespace SceneLoadingSystem;

using Godot;
using Godot.Collections;
using System;

/// <summary>
/// <para>Responsible for transitioning between two scenes with a loading screen in between (<see cref="PackedLoadingScreen"/>'s UID must be changed when added to a new project).</para>
/// <para>Behaves as a singleton, therefore the script must be added to the project's autoload.</para>
/// <para>Uses threading to transition between scenes, but can be disable by setting <see cref="UseThreads"/> to <see langword="false"/>.</para>
/// <para>The <see cref="PackedLoadingScreen"/> can be modified on run-time by calling <see cref="SetPackedLoadingScreen(string)"/>. This will change the <see cref="PackedScene"/> only if it is a valid <see cref="LoadingScreen"/> type.</para>
/// </summary>
public partial class SceneLoader : Node
{
    /// <summary>
    /// Static reference to the single instance of the class.
    /// </summary>
    private static SceneLoader Instance { get; set; } = null;

    /// <summary>
    /// Emitted when the loading progress has changed, sending the progress as a <see cref="float"/> value from 0 to 1.
    /// </summary>
    [Signal]
    public delegate void OnProgressChangedEventHandler(float progress);

    /// <summary>
    /// Emitted when the scene has successfully finished loading.
    /// </summary>
    [Signal]
    public delegate void OnLoadFinishedEventHandler();

    /// <summary>
    /// <para><see cref="PackedScene"/> reference of the <see cref="LoadingScreen"/> used between transitions of scenes.</para>
    /// <para>Should be modified appropriately to the correct UID of the <see cref="LoadingScreen"/> scene when added to a new project</para>
    /// <para>Can be modified on run-time through <see cref="SetPackedLoadingScreen(string)"/> given that it is a <see cref="LoadingScreen"/>.</para>
    /// </summary>
    public static PackedScene PackedLoadingScreen { get; private set; } = GD.Load<PackedScene>("uid://dpnxca3dc0y7n");

    /// <summary>
    /// Determines whether the loading of the next scene is using multiple threads.
    /// </summary>
    public static bool UseThreads { get; set; } = true;

    /// <summary>
    /// The path of the scene currently loading.
    /// </summary>
    private static string scenePath;

    /// <summary>
    /// Progress information modified by <see cref="ResourceLoader.LoadThreadedGetStatus(string, Godot.Collections.Array)"/>.
    /// </summary>
    private static Godot.Collections.Array progress = new Godot.Collections.Array();

    /// <summary>
    /// <see cref="PackedScene"/> of the loaded scene. Used for instantiation as the new current scene.
    /// </summary>
    private static PackedScene loadedResource;

    /// <summary>
    /// Changes the <see cref="PackedScene"/> reference to the <see cref="LoadingScreen"/> only if the given <paramref name="packedScenePath"/> references a <see cref="LoadingScreen"/> in <see cref="PackedScene"/> format.
    /// </summary>
    public static void SetPackedLoadingScreen(string packedScenePath)
    {
        PackedScene newPackedLoadingScreen = GD.Load<PackedScene>(packedScenePath);

        try
        {
            PackedLoadingScreen.Instantiate<LoadingScreen>()?.QueueFree();
        }
        catch (InvalidCastException exception)
        {
            GD.PushError("SetPackedLoadingScreen error: ", exception);
            return;
        }

        PackedLoadingScreen = newPackedLoadingScreen ?? PackedLoadingScreen;
    }

    /// <summary>
    /// Insures that only one instance of the class is running.
    /// </summary>
    public override void _EnterTree()
    {
        if (Instance.IsValidInstance()) { QueueFree(); return; }
        Instance = this;
        SetProcess(false);
    }

    /// <summary>
    /// Starts loading the scene given by <paramref name="newScenePath"/> if found. Instantiates a new <see cref="LoadingScreen"/> as a child and attaches the signals <see cref="OnProgressChanged"/> and <see cref="OnLoadFinished"/>.
    /// </summary>
    public static async void LoadScene(string newScenePath)
    {
        scenePath = newScenePath;

        LoadingScreen newLoadingScreen = PackedLoadingScreen.Instantiate<LoadingScreen>();
        Instance.AddChild(newLoadingScreen);

        Instance.OnProgressChanged += newLoadingScreen.OnProgressChanged;
        Instance.OnLoadFinished += newLoadingScreen.OnLoadFinished;

        await Instance.ToSignal(newLoadingScreen, LoadingScreen.SignalName.OnLoadingScreenReady);

        StartLoad();
    }

    /// <summary>
    /// Starts load if the <see cref="ResourceLoader.LoadThreadedRequest(string, string, bool, ResourceLoader.CacheMode)"/> is successful.
    /// </summary>
    private static void StartLoad()
    {
        Error error = ResourceLoader.LoadThreadedRequest(scenePath, useSubThreads: UseThreads);
        if (error == Error.Ok)
        {
            Instance.SetProcess(true);
        }
    }

    /// <summary>
    /// <para>Updates the loading process every frame emitting the <see cref="OnProgressChanged"/> signal to the <see cref="LoadingScreen"/>.</para>
    /// <para>When the loading is finished, stores the loaded scene to <see cref="loadedResource"/> and changes current scene to <see cref="loadedResource"/>, then emits the signal <see cref="OnLoadFinished"/> which destroys the <see cref="LoadingScreen"/>.</para>
    /// </summary>
    public override void _Process(double delta)
    {
        ResourceLoader.ThreadLoadStatus status = ResourceLoader.LoadThreadedGetStatus(scenePath, progress);
        Instance.EmitSignal(SignalName.OnProgressChanged, progress[0]);

        switch (status)
        {
            case ResourceLoader.ThreadLoadStatus.InvalidResource or
                    ResourceLoader.ThreadLoadStatus.Failed:
                Instance.SetProcess(false);
                break;
            case ResourceLoader.ThreadLoadStatus.Loaded:
                loadedResource = ResourceLoader.LoadThreadedGet(scenePath) as PackedScene;
                Instance.GetTree().ChangeSceneToPacked(loadedResource);
                Instance.EmitSignal(SignalName.OnLoadFinished);
                Instance.SetProcess(false);
                break;
        }
    }
}