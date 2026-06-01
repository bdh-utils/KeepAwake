# KeepAwake

A tiny Windows tray utility that stops your PC from going to sleep. Start it
when you need the machine to stay awake — a long download, a presentation, a
build — and stop it when you're done. It lives quietly in the system tray and
gets out of your way.

## Features

- **Two ways to stay awake** — pick whichever your machine respects:
  - **Block sleep** — asks Windows to stay awake via
    `SetThreadExecutionState`. The cleanest option, and it can keep the
    **display** on too.
  - **Wiggle the mouse** — nudges the cursor a single pixel on a
    configurable interval (5–3600 seconds, default 30) so Windows registers
    activity. Handy on managed/corporate machines where the sleep request is
    overridden by policy.
- **Live switching** — change method or the keep-display-on option while
  running and it takes effect immediately.
- **System tray** — closing the window minimises to the tray rather than
  quitting. Right-click the tray icon to start/stop, open, or exit.
- **Brand-themed UI** — styled to the bdh-utils palette, with an About page.

## Usage

1. Launch **KeepAwake**.
2. Choose a method (**Block sleep** or **Wiggle the mouse**). With *Block
   sleep* you can also tick **Keep the display awake too**; with *Wiggle the
   mouse* you can set how often it wiggles.
3. Click **Start**. The status dot turns to the accent colour and the tray
   tooltip reads *Running*.
4. Click **Stop** (or use the tray menu) when you're finished. KeepAwake drops
   the request so the machine can sleep normally again.

Closing the window keeps KeepAwake running in the tray. Use **Exit** from the
tray menu to quit fully — this always releases the keep-awake request first.

## Building

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download) on Windows.

```powershell
# Build the app
dotnet build KeepAwake/KeepAwake.csproj

# Run it
dotnet run --project KeepAwake/KeepAwake.csproj

# Run the unit tests
dotnet test
```

## How it's built

The keep-awake logic lives in `KeepAwakeController`, a WPF-free class that
decides which OS calls to make based on the chosen mode and state. All
operating-system interaction sits behind the `ISystemActivity` interface, with
`Win32SystemActivity` providing the real `SetThreadExecutionState` /
`mouse_event` calls. The WPF `MainWindow` is a thin shell over the controller,
and the controller is covered by unit tests in `KeepAwake.Tests` that exercise
it against a fake `ISystemActivity`.

## About bdh-utils

KeepAwake is part of the **bdh-utils** collection of small, free Windows
utilities: <https://github.com/bdh-utils>

---

KeepAwake was developed entirely with AI assistance. It is free and
no-nonsense — no ads, no tracking, no upsell — and released as free,
open-source software under the [Apache License 2.0](LICENSE).
