#!/bin/bash
# Build KeeAgent.plgx using Windows MSBuild from WSL2.
# Output: bin/ReleasePlgx/KeeAgent.plgx
#
# Prerequisites:
#   - WSL2 running on Windows, with this repo on the WSL filesystem
#   - Visual Studio / VS Build Tools installed on the Windows side
#   - Mono binfmt handler must be disabled: echo 0 > /proc/sys/fs/binfmt_misc/cli
#     (this script does that for you if needed)
#   - mono-complete installed in WSL (for the .NET Framework 4.6.2 reference
#     assemblies, resolved via FrameworkPathOverride)
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
POWERSHELL="/mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe"

if [ -z "${MONO_REFASM_DIR:-}" ]; then
  # KeeAgent targets .NET Framework 4.6.2; prefer an exact match, then fall
  # back to the highest 4.x reference-assembly dir mono-complete ships.
  MONO_REFASM_DIR="/usr/lib/mono/4.6.2-api"
  [ -d "$MONO_REFASM_DIR" ] || MONO_REFASM_DIR="$(find /usr/lib/mono -maxdepth 1 -name '4.*-api' 2>/dev/null | sort -V | tail -1)"
fi
if [ -z "${MONO_REFASM_DIR:-}" ] || [ ! -d "$MONO_REFASM_DIR" ]; then
  echo "error: could not find mono reference assemblies (install mono-complete, or set MONO_REFASM_DIR)" >&2
  exit 1
fi

# Disable Mono binfmt handler so Windows .exe files run via WSL interop
if [ "$(cat /proc/sys/fs/binfmt_misc/cli 2>/dev/null | head -1)" = "enabled" ]; then
  echo "==> Disabling Mono binfmt handler..."
  echo 0 > /proc/sys/fs/binfmt_misc/cli
fi

echo "==> Locating MSBuild via vswhere..."
MSBUILD_WIN="$("$POWERSHELL" -NoProfile -Command '
  $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
  # -products * is required: without it vswhere only considers full Visual Studio
  # installations and silently ignores standalone Build Tools.
  & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
' | tr -d '\r')"
if [ -z "$MSBUILD_WIN" ]; then
  echo "error: MSBuild.exe not found via vswhere (install Visual Studio or VS Build Tools with the MSBuild component)" >&2
  exit 1
fi
MSBUILD="$(wslpath -u "$MSBUILD_WIN")"
echo "==> Using MSBuild: $MSBUILD_WIN"

# Building directly against the WSL UNC path is blocked by Windows' mark-of-the-web
# zone check, so copy the source to a local Windows temp dir first.
BUILD_DIR_WIN="$("$POWERSHELL" -NoProfile -Command 'Join-Path $env:TEMP "keeagent-build"' | tr -d '\r')"
BUILD_DIR_UNIX="$(wslpath -u "$BUILD_DIR_WIN")"
REPO_WIN="$(wslpath -w "$REPO")"

echo "==> Copying source to local Windows filesystem ($BUILD_DIR_WIN)..."
"$POWERSHELL" -NoProfile -Command "
  if (Test-Path '$BUILD_DIR_WIN') { Remove-Item '$BUILD_DIR_WIN' -Recurse -Force }
  Copy-Item '$REPO_WIN' '$BUILD_DIR_WIN' -Recurse
" 2>&1

MONO_REFASM_DIR_WIN="$(wslpath -w "$MONO_REFASM_DIR")"

echo "==> Building KeeAgent.plgx..."
"$MSBUILD" \
  "${BUILD_DIR_WIN}\\KeeAgent.sln" \
  /property:Configuration=ReleasePlgx \
  /property:FrameworkPathOverride="$MONO_REFASM_DIR_WIN" \
  /restore \
  /t:build \
  /nologo

echo "==> Copying output back..."
mkdir -p "$REPO/bin/ReleasePlgx"
cp "$BUILD_DIR_UNIX/KeeAgent/bin/ReleasePlgx/KeeAgent.plgx" "$REPO/bin/ReleasePlgx/KeeAgent.plgx"

echo "==> Done: $REPO/bin/ReleasePlgx/KeeAgent.plgx"
ls -lh "$REPO/bin/ReleasePlgx/KeeAgent.plgx"
