# Portrait phone and tablet interface

MainScene uses the existing Nothing Phone profile (1080 x 2400) as its visual
reference. Every root UI canvas uses Scale With Screen Size / Expand with an
800 x 1400 reference. Narrow, tall phones retain width-based scaling; portrait
tablets gain side margins instead of stretching the content across the display.

PortraitCanvas applies Screen.safeArea to its SafeArea child and limits the
PortraitContent child to 800 UI units. Backgrounds remain full-screen. It reacts
to viewport and safe-area changes, including changes to the browser viewport.
Native player orientation is locked to portrait.

## Questions and images

ResponsiveSessionLayout reserves separate header, question, image and navigation
areas. It preserves the original image-size ceilings on Nothing Phone and fits
the cells into the remaining space on smaller screens:

- One image: one large, aspect-preserving image.
- Two images: one column, one image above the other.
- Four images: exactly two columns and two rows.

All text is displayed at once. PortraitTextArea selects the largest font size
that fits the fixed area, using the actual font's wrapping and line spacing.
Each complete word must also fit horizontally, including punctuation and phrases
joined with non-breaking spaces. Oversized words reduce the font instead of
falling back to character wrapping. Questions leave an 85-unit gap below the
header buttons, including when the text fills its available height.
There are no ScrollRect, text masks or scrollbar controls on question, warning
or coffee text. The maximum font size is retained where space permits; compact
image questions also have a lower font-size ceiling before fitting. Text is
recalculated when the question, language or available space changes.

PortraitScreenLayout handles the menu, shared controls, map, coffee and warning
screens. Map positions are adjusted through a parent MapArt container, keeping
levitation and pulsation independent of responsive positioning. Expanded images
use the safe area and preserve their aspect ratio.

## Checks

```powershell
dotnet run --project Tests/SessionRules/SessionRules.csproj
& 'D:/Programs/6000.1.6f1/Editor/Unity.exe' -batchmode -force-d3d11 -projectPath 'D:/Repositories/EggO' -executeMethod PortraitLayoutSmokeTests.Run -logFile 'D:/Repositories/EggO/Logs/portrait-layout-tests.log'
```

The Unity suite uses 1080x2400, 1080x1920, 320x568, 390x844, 768x1024,
1536x2048 and 1200x1920 portrait profiles, with and without safe-area insets.
It tests the longest real English, Polish and Russian questions, complete text
fitting, 1/2/4 image layouts, aspect preservation and separation from navigation.
It also runs the existing session-view and navigation/audio regression checks.

To make rendering deterministic, the editor test uses the real scene's content
with world-space canvases sized to the logical dimensions produced by the
configured scaler. It writes rendered previews to Logs/PortraitLayout and does
not save its test scene or write player progress. Real browser/device safe areas
and touch interaction should additionally be checked in the built player.

PortraitLayoutInstaller.Run is the idempotent editor migration that saves the
responsive MainScene hierarchy. Existing object references and button events
are retained; inactive text containers are included when upgrading an existing
scene. PortraitLayoutAudit.Run exports hierarchy and Game View profiles to Logs.
