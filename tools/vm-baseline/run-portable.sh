#!/bin/sh
# Launch the portable tiles build with a user directory that is created at
# runtime and is not part of the archive. Same license as the rest of this
# repository (CC BY-SA 3.0).
set -eu

ROOT=$(CDPATH= cd -- "$(dirname "$0")" && pwd)
USERDIR=${CDDA_USERDIR:-"$ROOT/user-data"}

mkdir -p "$USERDIR"
echo "Game directory: $ROOT" >&2
echo "User directory (created on use, not shipped): $USERDIR" >&2

exec "$ROOT/cataclysm-launcher" --userdir "$USERDIR" "$@"
