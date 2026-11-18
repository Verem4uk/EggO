# Screen transitions and audio

Controller owns one DOTween sequence and an explicit screen state. Repeated
open/play commands are ignored during a transition. Return to menu, coffee,
disable and destruction cancel the previous transition before changing screens.
The map's point animations are nested in that same sequence, so cancellation
cannot leave child coroutines running. Session activation waits for the actual
map hide sequence rather than an estimated delay.

CanvasGroupAutoFadeWithImages retains automatic show/hide for standalone views.
Controller-managed screens use its sequence builders instead. Faders only touch
their own local graphics, preserve active/inactive content, and exclude map
points, which have their own reveal animations. Shared controls use a separate
CanvasGroup owned by Controller. Serialized field names and existing prefab
OnClick methods, including PointMap.Disable, are preserved.

Menu exit/pause, shared-button fades, canvas/image fades, point fade/stagger and
the map reveal delay remain configurable in the Inspector. Screen transitions,
background changes and audio fades use unscaled time, so pausing gameplay does
not freeze navigation. Standalone canvas fades retain their useUnscaledTime
setting. Levitation and pulsation restore their original position/scale on
disable; pulsation preserves Z and handles equal minimum/maximum scales.

AudioController owns one volume envelope and at most one download. User volume
is multiplied by the envelope and is never overwritten by fade completion.
Replacing/stopping audio aborts and disposes its request; version checks discard
late results. Downloaded clips are released on replacement/stop, while the
serialized default clip stays owned by the project. An empty track also cancels
pending work. FadeScreen cancels competing fades and quits only after the exit
fade completes.

## Automated checks

Run in Unity 6000.1.6f1 without -quit. The asynchronous entry point exits the
editor itself:

```powershell
& 'D:/Programs/6000.1.6f1/Editor/Unity.exe' -batchmode -nographics -projectPath 'D:/Repositories/EggO' -executeMethod AnimationSmokeTests.Run -logFile 'D:/Repositories/EggO/Logs/animation-smoke-tests.log'
```

The test uses real MainScene references, manually advances DOTween's editor
manager, and substitutes text-only session content and controlled audio
requests. It checks duplicate commands, interrupted and completed transitions,
input recovery, session disposal, shared graphic ownership, point reveal timing,
fade reversal, late audio results, volume preservation, looping effects and
exit-fade cancellation. It does not save the scene or write player progress.

The session rules regression suite is documented in
[SessionRules/README.md](SessionRules/README.md). Visual pacing, live downloads
and the built WebGL player still need a manual smoke test.
