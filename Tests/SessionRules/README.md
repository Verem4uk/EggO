# Session rules and resource lifecycle

Run the standalone checks from the repository root with the .NET 9 SDK:

```powershell
dotnet run --project Tests/SessionRules/SessionRules.csproj
```

The harness compiles the actual plan generation, session, image-resource,
Addressables adapter, coordinator, persistence and analytics classes. It loads
the relevant fields from the seven checked-in Unity level assets and their
referenced blocks and collections. Minimal Unity substitutes prevent network
requests. It covers five random seeds, completion boundaries, early exit,
history, concurrent sessions using the same assets, short/empty pools, failed
loads, retries, late arrivals after disposal and exactly-once external effects.

## Runtime responsibilities

- `Level`, `Question`, `ImageQuestion`, the blocks and collections are content
  definitions. They store no cursors, generated pools, loaded sprites or handles.
- `SessionFactory` creates a separate `SessionPlan` using an injected random
  source. Blocks sharing a collection use one pool within that plan, avoiding
  repeats. The sequence, counters and required counts are fixed when generated.
- `Session` navigates that plan and its history. `ConfirmCurrentQuestion()`
  records a first display only after the view has text and all required images.
  Requesting a question without confirming it does not earn progress.
  `SessionResult` reports completion eligibility and normal/early termination.
  The session never reads Root, PlayerPrefs, analytics or a clock.
- `SessionImages` starts preloading images selected for this plan. Requests and
  sprite leases are reused only within that session, including retry and
  history. There is no application-wide image cache. Failed partial loads
  release their data; disposal releases existing leases and late results.
- `AddressableSessionImageLoader` owns each texture handle and generated sprite.
  It awaits the Completed callback rather than handle.Task, so the code works
  with the WebGL Addressables API as well.
- `SessionCoordinator` starts analytics, observes the single session result,
  saves eligible progress and records duration through an injected clock.
  Disposal unsubscribes it and releases images. Leaving for the menu before
  completion does not grant credit or synthesize a session-complete event.
- `Controller` owns the active coordinator and clears the view before disposal.
  `SessionView` rejects obsolete async results after Back, language changes,
  replacement or exit. Pending images block Next; a failed image offers retry
  through Next without advancing. Early exit remains available, with level 7's
  mandatory final-card route preserved.

Navigation and localization retain their existing architecture. The new loading
and retry messages support the existing three languages.

## Configured rules

`Level.CompletionRequirements` references level elements rather than generated
question IDs. `MinimumQuestions: 0` means every question actually selected for
that element; a positive value means at least that many first-time displays.
An empty required block does not grant completion. History replay does not add
progress, and navigating back does not revoke progress already reached.

| Level | Required milestone |
| --- | --- |
| 1 | First random question after the introduction |
| 2 | Every generated StoryTell card |
| 3 | Every TestBlock entry, including the last image explanation |
| 4 | Every generated Metafors card |
| 5 | Every generated ScaryBlock card |
| 6 | Both entries of OvereatTasty and both entries of LazyBalance |
| 7 | All four Important questions and the final generated image card |

`Level.FinishElements` specifies the early-exit sequence. Levels 1-6 go to the
closing reflection question. Level 7 goes through the image instruction, the
already selected final image card, then the closing reflection question.
Already displayed closing entries are skipped; repeated finish confirmations do
not advance the sequence. Early exit cannot award credit for skipped Important
questions, and a failed final-card download cannot stand in for displaying it.

Progress is evaluated and the session-complete event is emitted once when the
closing reflection question is confirmed (or the sequence is exhausted).
Analytics uses the same one-based level number for start and end.
Existing saved progress is never lowered.

## Unity view smoke test

With Unity 6000.1.6f1 installed, run this batch entry point without `-quit`;
the test exits the editor after its asynchronous operations complete:

```powershell
& 'D:/Programs/6000.1.6f1/Editor/Unity.exe' -batchmode -nographics -projectPath 'D:/Repositories/EggO' -executeMethod SessionViewSmokeTests.Run -logFile 'D:/Repositories/EggO/Logs/session-view-smoke-tests.log'
```

The editor-only test uses the real MainScene bindings with controlled image
downloads. It verifies loading guards, retry without advancement, stale-result
rejection after Back, and release of late results after exit. It does not save
the scene, edit assets or write user progress.

Live server downloads, visual layout and a built WebGL player still need a
manual smoke test. The standalone substitutes cannot validate these.
