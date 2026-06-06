# philips-display-controller (pdc)

A small Windows command‑line utility that adjusts **display brightness** over
DDC/CI — primarily for **Philips** monitors, with a fallback for other displays.
It also ships an **MCP server** so AI agents (e.g. Claude) can read and change
brightness as a tool.

## How it works

- **External monitors (e.g. Philips)** are controlled via the Windows Monitor
  Configuration API (`dxva2.dll`), writing the standard DDC/CI **VCP code `0x10`
  (Luminance)**. Philips panels are detected by their `PHL` PnP manufacturer id.
- **Other displays** (such as internal laptop panels that don't speak DDC/CI)
  fall back to the OS backlight control via WMI (`WmiMonitorBrightnessMethods`).

No vendor drivers or native DLLs are required — everything uses built‑in Windows
APIs. The app targets **.NET 10** and runs as **64‑bit** (no 32‑bit lock‑in).

## Requirements

- Windows 10 / 11, x64
- A monitor that supports DDC/CI (enable "DDC/CI" in the monitor's OSD if needed)
- To **build** from source: [.NET 10 SDK](https://dotnet.microsoft.com/download)
- To **run** a self‑contained build: nothing — the runtime is bundled

## Installation

> All options below need only the **.NET 10 SDK** — no Visual Studio.

### Option A — build/install scripts (recommended, no Visual Studio)

Two helper scripts are included:

| Script        | What it does                                                                                  |
|---------------|----------------------------------------------------------------------------------------------|
| `build.cmd`   | Builds a self‑contained, single‑file `pdc.exe`. Makes no system changes.                      |
| `install.cmd` | Builds, then copies `pdc.exe` to `%LOCALAPPDATA%\Programs\pdc` and adds it to your user `PATH`. |

```bat
:: just build
build.cmd

:: build and install so `pdc` works from any new terminal
install.cmd
```

Both verify a .NET 10 SDK is present and print a download link if it's missing.
After `install.cmd`, open a **new** terminal so the updated `PATH` takes effect,
then run `pdc --info`.

To uninstall: delete `%LOCALAPPDATA%\Programs\pdc` and remove that folder from
your user `PATH` (Settings → Environment Variables).

### Option B — manual publish (single file)

```powershell
dotnet publish pdc/pdc.csproj -c Release
```

The single file is produced at:

```
pdc\bin\Release\net10.0-windows\win-x64\publish\pdc.exe
```

Copy `pdc.exe` anywhere (optionally add its folder to your `PATH`).

### Option C — build and run from source

```powershell
dotnet build pdc.sln -c Release
dotnet run --project pdc -- --info
```

## Usage

```
pdc <command> [parameter]
```

Only **one** command is processed per invocation, and its parameter (if any) is
the next token.

### Commands / parameters

| Short | Long           | Parameter        | Description                                                        |
|-------|----------------|------------------|-------------------------------------------------------------------|
| `-h`  | `--help`       | —                | Show the list of available commands.                              |
| `-v`  | `--version`    | —                | Show the application version.                                     |
| `-i`  | `--info`       | —                | List connected displays with their **index** and name.           |
| `-b`  | `--brightness` | `<spec>`         | Set brightness. See the spec format below.                       |

### Brightness spec

The brightness parameter is a single token (no spaces) in one of these forms:

- **All displays:** `all:<level>`
- **Specific display(s):** `<index>:<level>` — combine multiple with `;`

Where:

- `<index>` is a display index from `pdc --info`
- `<level>` is a brightness percentage, **0–100**
- `:` separates index/`all` from the level (`KeyValueDelimiter`)
- `;` separates multiple display entries (`ParameterDelimiter`)

### Examples

```powershell
# List displays and their indexes
pdc --info
#   Display index: 0 | Display name: ...
#   Display index: 1 | Display name: ... PHL...

# Set ALL displays to 50%
pdc --brightness all:50

# Set display 1 to 20%
pdc --brightness 1:20

# Set display 1 to 20% and display 2 to 80% in one call
pdc --brightness 1:20;2:80

# Short forms
pdc -b all:100
pdc -i
pdc -v
```

> Tip: if your shell treats `;` specially, quote the parameter, e.g.
> `pdc -b "1:20;2:80"`.

## Quick presets (.cmd files)

For quick access without typing commands, the repo includes ready‑to‑run batch
files. Double‑click them, or right‑click → *Send to → Desktop (create shortcut)*
and pin them to the taskbar.

They locate `pdc.exe` the same way: they prefer the published build next to the
script, and fall back to `pdc.exe` on your `PATH` (so they work anywhere after
`install.cmd`).

Each of these sets **all** displays to a fixed level and exits immediately:

| File          | Sets all displays to |
|---------------|----------------------|
| `pdc-0.cmd`   | 0%                   |
| `pdc-10.cmd`  | 10%                  |
| `pdc-30.cmd`  | 30%                  |
| `pdc-50.cmd`  | 50%                  |
| `pdc-90.cmd`  | 90%                  |
| `pdc-100.cmd` | 100%                 |

> Note: `pdc-0.cmd` turns the backlight nearly off on most monitors — use a
> higher preset (or the monitor's OSD) to recover.

## MCP server (pdc-mcp)

[`pdc-mcp`](pdc-mcp/) is a [Model Context Protocol](https://modelcontextprotocol.io)
server that exposes brightness control as **tools an AI agent can call**. It
reuses the same brightness logic as the CLI and communicates over **stdio**.

### Tools

| Tool                 | Parameters                         | Description                                                  |
|----------------------|------------------------------------|--------------------------------------------------------------|
| `list_displays`      | —                                  | List displays with their index and name.                     |
| `set_brightness`     | `displayIndex` (int), `brightness` (0–100) | Set brightness of one display by index.              |
| `set_all_brightness` | `brightness` (0–100)               | Set the same brightness on all displays at once.             |

Brightness values are clamped to the 0–100 range.

### Build

```powershell
dotnet publish pdc-mcp/pdc-mcp.csproj -c Release
```

Produces a self‑contained server at:

```
pdc-mcp\bin\Release\net10.0-windows\win-x64\publish\pdc-mcp.exe
```

### Register with Claude Code (CLI)

```powershell
claude mcp add pdc-brightness --scope user -- "C:\path\to\pdc-mcp\bin\Release\net10.0-windows\win-x64\publish\pdc-mcp.exe"
```

Verify it connected:

```powershell
claude mcp list
#   pdc-brightness: ...\pdc-mcp.exe - Connected
```

Remove it later with:

```powershell
claude mcp remove pdc-brightness --scope user
```

### Register with Claude Desktop

Add an entry to `%APPDATA%\Claude\claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "pdc-brightness": {
      "command": "C:\\path\\to\\pdc-mcp\\bin\\Release\\net10.0-windows\\win-x64\\publish\\pdc-mcp.exe"
    }
  }
}
```

Then restart Claude Desktop.

### Using it

After registering (and restarting the client), just ask in natural language:

- "List my displays."
- "Set my Philips monitor to 20%."
- "Dim all my screens to 40%."

The agent picks the right tool and calls it. Brightness‑changing tools will
prompt for permission the first time, like any other tool.

## Project structure

| Project / file        | Description                                                        |
|-----------------------|-------------------------------------------------------------------|
| `pdc/`                | The CLI app and DDC/CI + WMI brightness logic (`DisplayApiService`). |
| `pdc-mcp/`            | MCP server exposing brightness control as agent tools.             |
| `ArgumentParser/`     | Minimal command‑line argument parser used by the CLI.              |
| `build.cmd`           | Build a self‑contained `pdc.exe` (no Visual Studio).               |
| `install.cmd`         | Build and install `pdc.exe` to `%LOCALAPPDATA%\Programs\pdc` + PATH. |
| `pdc-0/10/30/50/90/100.cmd` | One‑click presets: set all displays to a fixed brightness.  |

## Notes & limitations

- DDC/CI must be enabled on the monitor; some displays accept brightness writes
  slowly or only intermittently.
- GPU/driver support for DDC/CI varies (AMD/Intel are generally reliable).
- Windows‑only by design (uses Windows display APIs).

## License

See [LICENSE](LICENSE).
