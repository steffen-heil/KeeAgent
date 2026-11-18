# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

KeeAgent is a KeePass 2.x plugin (C# / .NET 4.6.2) that acts as an SSH agent, exposing SSH keys stored in KeePass to other programs via standard agent protocols (Pageant on Windows, Unix domain socket on Linux/macOS). It depends on the **SshAgentLib** git submodule for all SSH/agent protocol implementation.

## Building

### Prerequisites

- Clone with submodules: `git clone --recursive` (SshAgentLib is a required submodule at `SshAgentLib/`)
- Restore NuGet packages before first build

### Linux/macOS (Mono)

```bash
mono nuget.exe restore
xbuild KeeAgent.sln                                         # Debug build
xbuild /property:Configuration=ReleasePlgx KeeAgent.sln    # Release .plgx file
```

Output: `bin/ReleasePlgx/KeeAgent.plgx`

### Windows (msbuild / VS Code)

```powershell
./scripts/msbuild.ps1 /t:build KeeAgent.sln                                         # Debug
./scripts/msbuild.ps1 /property:Configuration=ReleasePlgx /t:build KeeAgent.sln     # Release .plgx
```

### Build Configurations

| Configuration | Output | Purpose |
|---|---|---|
| `Debug` | `bin/Debug/KeeAgent.dll` | Development, runs with KeePass.exe |
| `Release` | `bin/Release/KeeAgent.dll` | Optimized |
| `ReleaseDll` | `bin/ReleaseDll/KeeAgent.dll` | ILRepack merged single DLL |
| `ReleasePlgx` | `bin/ReleasePlgx/KeeAgent.plgx` | Distributable plugin package |

### Running for Debugging (Windows)

Configure KeePass as the startup executable in project debug properties:
- External program: `bin/Debug/KeePass.exe`
- Arguments: `--debug --pw:test Test.kdbx`

### Tests

Tests use NUnit 2.6.2 (located in `lib/NUnit/`). The test project (`KeeAgentTestProject/`) also requires `lib/IOProtocolExt/IOProtocolExt.dll`. Run via your NUnit test runner pointing at the built `KeeAgentTestProject.dll`.

## Architecture

### Core Components

- **`KeeAgent/KeeAgentExt.cs`** — The KeePass plugin entry point. Implements `Plugin` (KeePass plugin API). Initializes the SSH agent on startup, hooks KeePass events (file open/close), adds menu items, registers SPR placeholders.

- **`KeeAgent/Options.cs`** — Global plugin settings (agent mode, socket paths, notification behavior). Serialized to KeePass's XML config store via custom key names prefixed with `"KeeAgent."`.

- **`KeeAgent/EntrySettings.cs`** — Per-entry SSH key settings, XML-serialized and stored in each KeePass entry's custom data. Specifies whether the key comes from an attachment or an external file path.

- **`KeeAgent/DatabaseSettings.cs`** — Per-database settings stored in the database's custom data.

- **`KeeAgent/UI/`** — WinForms dialogs and panels: `OptionsPanel` (plugin settings UI), `EntryPanel` (per-entry key config), `DatabaseSettingsPanel`, `ManageDialog` (loaded keys), `PasswordDialog`, `HostKeysDialog`, etc.

- **`SshAgentLib/`** (submodule) — All SSH protocol handling: key parsing (OpenSSH, PuTTY PPK), agent message protocol, `PageantAgent`/`UnixAgent`/`PageantClient`/`UnixClient` implementations.

### Agent Mode

KeeAgent operates in three modes (configured via `Options.AgentMode`):
- **Server** — KeeAgent itself acts as the SSH agent (creates socket/Pageant window)
- **Client** — KeeAgent forwards keys to an existing external agent
- **Auto** — Server if no external agent detected, otherwise Client

On Windows, `PageantAgent` (Pageant-compatible window message protocol) is used; it can additionally expose Cygwin, MSYS, WSL sockets, and the Windows OpenSSH named pipe. On Unix, `UnixAgent` creates a Unix domain socket.

### Key Loading Flow

When a KeePass database opens (`MainForm_FileOpened`), `KeeAgentExt` iterates entries and loads SSH keys configured in `EntrySettings` into the active `IAgent`. Keys can be stored as KeePass entry attachments or as external files. The plugin also supports SPR (String Provider) placeholders `{KEEAGENT:KEYFILEPATH}` and `{KEEAGENT:IDENTFILEOPT}` for use in KeePass entry URLs/commands.

### Settings Serialization

- Global options: serialized to KeePass's application config via `IPluginHost.CustomConfig`, keyed as `"KeeAgent.<PropertyName>"`.
- Entry settings: XML-serialized into a KeePass entry's custom string field.
- Database settings: stored in the database's custom data.

### Language Version

The C# code is intentionally restricted to LangVersion 4 (C# 4.0) in the project files for Mono compatibility. Avoid using language features beyond C# 4.
