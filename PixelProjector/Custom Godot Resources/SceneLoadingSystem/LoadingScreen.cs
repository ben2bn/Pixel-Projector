namespace SceneLoadingSystem;

using Godot;
using System;

[GlobalClass]
public partial class LoadingScreen : CanvasLayer
{
    [Signal]
    public delegate void OnLoadingScreenReadyEventHandler();

    [Export]
    private AnimationPlayer animationPlayer;
    [Export]
    private ProgressBar progressBar;
    [Export]
    private Animation transition;

    public override async void _Ready()
    {
        Layer = (int)RenderingServer.CanvasLayerMax;

        //run transition into loading screen
        if (animationPlayer.IsValidInstance() && animationPlayer.IsPlaying())
        {
            await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
        }

        EmitSignal(SignalName.OnLoadingScreenReady);
    }

    public void OnProgressChanged(float progress)
    {
        //run progress logic i.e. progress bar update
        progressBar.Value = progress;
    }

    public async void OnLoadFinished()
    {
        //run transition out of loading screen
        if (animationPlayer.IsValidInstance())
        {
            animationPlayer.PlayBackwards(transition);
            if (animationPlayer.IsPlaying()) await ToSignal(animationPlayer, AnimationMixer.SignalName.AnimationFinished);
        }

        this.SafeQueueFree();
    }
}