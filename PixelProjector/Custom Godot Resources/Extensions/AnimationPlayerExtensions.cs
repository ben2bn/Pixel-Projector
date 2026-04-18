using Godot;
using System;

public static class AnimationPlayerExtensions
{
    /// <summary>
    /// <para>Adds the <paramref name="animation"/> to the <paramref name="animationPlayer"/>'s global animation library if it is not already inside.</para>
    /// <para>If the <paramref name="animation"/> resource does not have a <see cref="Resource.ResourceName"/> or if its name is already taken, then a random <see langword="string"/> is generated, before adding it to the global animation library with the random <see langword="string"/> as key.</para>
    /// </summary>
    /// <returns>
    /// Returns the existing key to the animation within the <paramref name="animationPlayer"/> or returns the new generated key.
    /// </returns>
    public static string AddAnimation(this AnimationPlayer animationPlayer, Animation animation)
    {
        if (animation == null) return string.Empty;
        StringName key = animationPlayer.FindAnimation(animation);
        if (!key.IsEmpty) return key;

        AnimationLibrary defaultLibrary = animationPlayer.GetAnimationLibrary("");
        if (defaultLibrary == null) return string.Empty;

        Godot.Collections.Array<StringName> keys = defaultLibrary.GetAnimationList();
        if (animation.ResourceName == string.Empty || keys.Contains(animation.ResourceName))
        {
            animation.ResourceName = string.Empty;

            int loopCount = 0;
            while (animation.ResourceName == string.Empty || loopCount > 10)
            {
                loopCount++;

                string randomString = RandomExtensions.GenerateRandomString(8 + (loopCount > 5 ? loopCount : 0));
                if (keys.Contains(randomString)) continue;
                animation.ResourceName = randomString;
            }
        }

        defaultLibrary.AddAnimation(animation.ResourceName, animation);
        return animation.ResourceName;
    }

    /// <summary>
    /// <para>Calls <see cref="AnimationPlayer.Play(StringName, double, float, bool)"/> if <paramref name="animation"/> is found in <paramref name="animationPlayer"/>. Otherwise, the <paramref name="animation"/> is added first through <see cref="AddAnimation(AnimationPlayer, Animation)"/>.</para>
    /// </summary>
    /// <remarks>
    /// Remark: the parameters <paramref name="customBlend"/>, <paramref name="customSpeed"/> and <paramref name="fromEnd"/> are the same as <see cref="AnimationPlayer.Play(StringName, double, float, bool)"/>.
    /// </remarks>
    public static void Play(this AnimationPlayer animationPlayer, Animation animation, double customBlend = -1, float customSpeed = 1, bool fromEnd = false)
    {
        if (animation == null) return;

        StringName key = animationPlayer.FindAnimation(animation);
        key = key == string.Empty ? animationPlayer.AddAnimation(animation) : key;
        if (key.IsEmpty) return;

        animationPlayer.Play(key, customBlend, customSpeed, fromEnd);
    }

    /// <summary>
    /// <para>Calls <see cref="Play(AnimationPlayer, Animation, double, float, bool)"/> with customSpeed set to -1 and fromEnd set to <see langword="true"/>.</para>
    /// <para>Mirrors the functionnality of <see cref="AnimationPlayer.PlayBackwards(StringName, double)"/>, but first adds the <paramref name="animation"/> if not found in <paramref name="animationPlayer"/> by calling <see cref="AddAnimation(AnimationPlayer, Animation)"/>.</para>
    /// </summary>
    /// <remarks>
    /// Remark: the parameter <paramref name="customBlend"/> is the same as <see cref="AnimationPlayer.PlayBackwards(StringName, double)"/>.
    /// </remarks>
    public static void PlayBackwards(this AnimationPlayer animationPlayer, Animation animation, double customBlend = -1)
    {
        animationPlayer.Play(animation, customBlend, customSpeed: -1, fromEnd: true);
    }
}