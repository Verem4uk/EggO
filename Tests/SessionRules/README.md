# Session completion rules

Run from the repository root with the .NET 9 SDK:

```powershell
dotnet run --project Tests/SessionRules/SessionRules.csproj
```

The harness compiles the actual session, level, question-generation, persistence
and analytics classes. It loads the relevant fields from the seven checked-in
Unity level assets and their referenced blocks and collections. Minimal Unity
substitutes keep it independent of the editor and prevent network requests.
Rendering, Unity serialization, scene bindings and actual Addressables loading
still need a Unity 6000.1.6f1 editor/WebGL smoke test.

## Configured rules

`Level.CompletionRequirements` references level elements rather than generated
question IDs. `MinimumQuestions: 0` means every question actually prepared for
that element; a positive value means at least that many first-time openings.
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
already prepared final image card, then the closing reflection question.
Already opened closing elements are skipped; repeated finish confirmations do
not advance the sequence. Early exit cannot award credit for skipped Important
questions.

Progress is evaluated and the session-complete event is emitted once when the
closing reflection question is first opened (or the sequence is exhausted).
Analytics consistently uses the same one-based level number for start and end.
Existing saved progress is never lowered.

## Editor smoke test

- Open MainScene and check that all seven levels retain their configured
  CompletionRequirements and FinishElements references.
- On each level, finish immediately before and at its milestone; check the map.
- On level 7, finish before the Important questions: instruction and image must
  appear, with no level credit. Repeat after all four Important questions:
  credit is granted after completing the closing sequence.
- Finish from the final image and from the closing reflection; revisit history
  and press finish repeatedly. No image should be regenerated or skipped.
- In a WebGL build, check unchanged preloading and one start/end analytics event
  with matching level numbers.
