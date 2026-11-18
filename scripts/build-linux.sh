#!/bin/bash
# Build KeeAgent on Linux using the dotnet CLI (not mono/xbuild).
#
# Prerequisites:
#   - dotnet SDK (https://dotnet.microsoft.com/download)
#   - mono-complete (provides the .NET Framework 4.6.2 reference assemblies used
#     for compiling, and the Mono.Posix / libMonoPosixHelper runtime bits KeeAgent
#     needs for Unix domain sockets)
#
# Env overrides:
#   CONFIGURATION    Build configuration (default: Debug)
#   MONO_REFASM_DIR  Mono reference-assembly dir (default: auto-detected)
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO="$(cd "$SCRIPT_DIR/.." && pwd)"

CONFIGURATION="${CONFIGURATION:-Debug}"

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

if ! command -v dotnet >/dev/null 2>&1; then
  echo "error: dotnet CLI not found (install the .NET SDK)" >&2
  exit 1
fi

echo "==> Building KeeAgent.sln (Configuration=$CONFIGURATION, FrameworkPathOverride=$MONO_REFASM_DIR)..."
dotnet build "$REPO/KeeAgent.sln" \
  /property:Configuration="$CONFIGURATION" \
  /property:FrameworkPathOverride="$MONO_REFASM_DIR" \
  /nologo

echo "==> Done: $REPO/KeeAgent/bin/$CONFIGURATION/KeeAgent.dll"
