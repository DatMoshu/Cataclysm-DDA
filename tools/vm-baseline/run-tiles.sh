#!/usr/bin/env bash
# Launch the tiles binary with an isolated user directory.
# Refuses $HOME and any path containing .cataclysm-dda unless
# CDDA_ALLOW_HOME_USERDIR=1 is set. Never pass USE_HOME_DIR to make.
# Same license as the rest of this repository (CC BY-SA 3.0).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
BIN="${CDDA_BIN:-$ROOT/cataclysm-tiles}"
USERDIR="${CDDA_USERDIR:-/tmp/cdda-0i-1-userdir}"

if [[ ! -x "$BIN" ]]; then
  echo "Tiles binary not found at: $BIN" >&2
  echo "Build it with: tools/vm-baseline/build-tiles.sh" >&2
  exit 1
fi

case "$USERDIR" in
  "$HOME"|"$HOME"/*|*cataclysm-dda*)
    if [[ "${CDDA_ALLOW_HOME_USERDIR:-}" != "1" ]]; then
      echo "Refusing user directory '$USERDIR'." >&2
      echo "The default is /tmp/cdda-0i-1-userdir so an existing player save is not opened." >&2
      echo "Set CDDA_USERDIR to an empty disposable directory, or CDDA_ALLOW_HOME_USERDIR=1 to override." >&2
      exit 1
    fi
    ;;
esac

mkdir -p "$USERDIR"
echo "Binary: $BIN" >&2
echo "User directory (config, saves, memorials): $USERDIR" >&2
echo "This process is local. It does not open a network port." >&2

exec "$BIN" --userdir "$USERDIR" "$@"
