#!/usr/bin/env bash
# Package the existing 0.I-1 tiles binary into a portable Linux tar.gz.
# Does not rebuild, does not run the game, and does not copy saves, config,
# memorials, or the playtest world. Same license as the rest of this
# repository (CC BY-SA 3.0).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
BIN="${CDDA_BIN:-$ROOT/cataclysm-tiles}"
OUTDIR="${OUTDIR:-/opt/cursor/artifacts}"
NAME="cataclysmdda-0.I-1-linux64-tiles"
DOC="$ROOT/doc/PORTABLE-LINUX-0.I-1.md"

if [[ ! -x "$BIN" ]]; then
  echo "No tiles binary at: $BIN" >&2
  echo "Rebuild required. From a 0.I-1 tree:" >&2
  echo "  tools/vm-baseline/install-deps-ubuntu.sh" >&2
  echo "  tools/vm-baseline/build-tiles.sh" >&2
  echo "  tools/vm-baseline/package-linux-tiles.sh" >&2
  exit 2
fi

# grep -q closes the pipe early. With pipefail that looks like a missing version string.
set +o pipefail
if ! strings "$BIN" | grep -F -q '0.I-1 7b2efa5cea'; then
  set -o pipefail
  echo "Binary does not contain the 0.I-1 version string '0.I-1 7b2efa5cea'." >&2
  echo "Refusing to package a different build. Rebuild from this baseline or point CDDA_BIN at the 0.I-1 binary." >&2
  exit 2
fi
set -o pipefail

mkdir -p "$OUTDIR"
STAGE="$(mktemp -d)"
TREE="$STAGE/$NAME"
cleanup() { rm -rf "$STAGE"; }
trap cleanup EXIT

mkdir -p "$TREE"
install -m 0755 "$BIN" "$TREE/cataclysm-tiles"
strip --strip-unneeded "$TREE/cataclysm-tiles"

install -m 0755 "$ROOT/cataclysm-launcher" "$TREE/cataclysm-launcher"
install -m 0755 "$ROOT/tools/vm-baseline/run-portable.sh" "$TREE/run-portable.sh"
install -m 0644 "$ROOT/LICENSE.txt" "$TREE/LICENSE.txt"
install -m 0644 "$ROOT/LICENSE-OFL-Terminus-Font.txt" "$TREE/LICENSE-OFL-Terminus-Font.txt"
install -m 0644 "$ROOT/LICENSE-Apache-Robot-Font.txt" "$TREE/LICENSE-Apache-Robot-Font.txt"
if [[ -f "$DOC" ]]; then
  install -m 0644 "$DOC" "$TREE/README-PORTABLE.txt"
fi

# Game content only. data/cache is generated at runtime and is gitignored.
mkdir -p "$TREE/data" "$TREE/gfx"
cp -a "$ROOT/data/." "$TREE/data/"
rm -rf "$TREE/data/cache"
rm -f "$TREE/data/mods/user-default-mods.json"
cp -a "$ROOT/gfx/." "$TREE/gfx/"

BUILD_ID="$(readelf -n "$TREE/cataclysm-tiles" | awk '/Build ID/ {print $3}')"
{
  echo "name: Cataclysm DDA 0.I-1 (Ito-1) Stable Release"
  echo "upstream_commit: 7b2efa5cea38e4d4d97dd0e63b28b9148623da59"
  echo "upstream_tag: 0.I-1"
  echo "version_string: 0.I-1 7b2efa5cea"
  echo "build_id: ${BUILD_ID}"
  echo "stripped: yes (--strip-unneeded). Build ID matches the unstripped compiler output."
  echo "rebuild: no. This archive packages the binary already produced by build-tiles.sh."
  echo "compile: Ubuntu 24.04, g++ 13.3.0, make TILES=1 SOUND=1 RELEASE=1 LOCALIZE=0 CCACHE=1"
  echo "localization: English only (LOCALIZE=0). lang/ is not included."
  echo "tests: tests/cata_test was compiled and linked earlier and is not in this archive. The Catch suite was not run."
  echo "excluded: save, config, memorial, graveyard, data/cache, playtest world, object files, git metadata"
} > "$TREE/VERSION.txt"

# Mod data ships options.json files. Those are content, not a player config.
# Reject user saves, the runtime cache, and a config directory at the install root.
if [[ -d "$TREE/save" || -d "$TREE/config" || -d "$TREE/memorial" || -d "$TREE/graveyard" || -d "$TREE/user-data" || -d "$TREE/data/cache" ]]; then
  echo "Staged tree contains a save, config, memorial, or data cache directory." >&2
  exit 1
fi
bad="$(find "$TREE" \( -name '*.sav' -o -name '*.sav.zzip' -o -name '*credential*' -o -path '*/data/cache/*' \) -print -quit)"
if [[ -n "$bad" ]]; then
  echo "Staged tree contains save files, credentials, or cache files: $bad" >&2
  exit 1
fi

TAR="$OUTDIR/${NAME}.tar.gz"
rm -f "$TAR" "$TAR.sha256"
tar -C "$STAGE" -czf "$TAR" "$NAME"
SHA="$(sha256sum "$TAR" | awk '{print $1}')"
echo "$SHA  ${NAME}.tar.gz" > "$TAR.sha256"
BIN_SHA="$(sha256sum "$TREE/cataclysm-tiles" | awk '{print $1}')"
echo "binary_sha256: $BIN_SHA" >> "$TREE/VERSION.txt"
# Re-pack so VERSION.txt includes the binary checksum. The outer checksum is rewritten after.
tar -C "$STAGE" -czf "$TAR" "$NAME"
SHA="$(sha256sum "$TAR" | awk '{print $1}')"
echo "$SHA  ${NAME}.tar.gz" > "$TAR.sha256"

echo "ARCHIVE $TAR"
echo "SHA256 $SHA"
echo "BYTES $(wc -c < "$TAR")"
echo "BUILD_ID $BUILD_ID"
echo "BINARY_SHA256 $BIN_SHA"
ls -lh "$TAR" "$TAR.sha256"
