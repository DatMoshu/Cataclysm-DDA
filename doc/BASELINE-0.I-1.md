# 0.I-1 development baseline

This branch is a known-good, playable build of upstream stable **0.I-1 "Ito"** (the in-game string is `0.I-1 (Ito-1) Stable Release`). Gameplay is unchanged. The only additions are build/run scripts and this audit.

## Commit

| | |
| --- | --- |
| Upstream tag | `0.I-1` (annotated) |
| Tag object | `0fb5d5b67811b68ad15798cda13d2b5ec07526cd` |
| Tagged commit | `7b2efa5cea38e4d4d97dd0e63b28b9148623da59` |
| Tag message | `0.I-1 "Ito" Release` |
| Tagger | Antti Riikonen, 2026-09-20 |
| Commit | Merge pull request #88726, changelog and in-game version for 0.I-1, 2026-09-19 |
| Generated `src/version.h` | `#define VERSION "0.I-1 7b2efa5cea"` |
| Binary string | `Version: 0.I-1 (Ito-1) Stable Release` |

The tag was fetched from the public remote `https://github.com/CleverRaven/Cataclysm-DDA.git`. Nothing was pushed there.

## What was already in the fork

`master` was not moved. It is still `f87b7c42d46b90e8a0ac0f88238c3bf5beeaa66c` (2026-06-21 experimental snapshot, tag `cdda-experimental-2026-06-21-1551` on the fork). That commit is an upstream merge. There are no DatMoshu commits on `master`.

`master` is not an ancestor of 0.I-1, so a fast-forward is impossible.

| | |
| --- | --- |
| Merge base | `348e64ab8bf624492859711e9131f3966b05fb5d` (2025-07-08) |
| Commits on `master` not in 0.I-1 | 6501 |
| Commits on 0.I-1 not in `master` | 374 |

0.I-1 is the stable line. Fork `master` is later experimental history from a different line. Merging them would not produce 0.I-1.

### Custom branch left untouched

`csharp-mapgen` (`ae92f1c1287ffcc143d2ff4c846273fe3b871831`) has three commits by Moshu `<kaiseer01@gmail.com>` dated 2026-01-10:

1. `69f64a5899` Add initial C# port of CDDA map generation
2. `2aa9af9e12` Add overmap specials generation for cities and roads
3. `ae92f1c128` Add JSON mapgen loader and executor with palette support

They add `CSharpPort/` (about 4600 lines) on top of merge-base `5f3e1f5c87` and are 8973 commits behind `master`. This is a separate C# map generator, not a patch to the C++ game. It was not merged: the civilization/settlements direction was not selected, and rebasing or merging that branch would rewrite or mix history that is already pushed. The branch is unchanged.

## Build

Ubuntu 24.04, g++ 13.3.0. No source changes were required.

```bash
tools/vm-baseline/install-deps-ubuntu.sh
tools/vm-baseline/build-tiles.sh
```

The command that produced the binary:

```text
make -j4 TILES=1 SOUND=1 RELEASE=1 LOCALIZE=0 CCACHE=1
```

`LOCALIZE=0` builds English only and skips gettext. Link-time optimization was not enabled. `USE_HOME_DIR` was not set, so the binary does not write into `$HOME`.

Recorded result from `/tmp/cdda-build-logs/tiles-release.log` on the build VM:

```text
START 2026-11-09T01:19:14Z commit 7b2efa5cea38e4d4d97dd0e63b28b9148623da59
#define VERSION "0.I-1 7b2efa5cea"
EXIT 0 2026-11-09T01:31:55Z
```

Zero `error:` lines. Make printed `Cannot run an astyle check, your system either does not have astyle, or it is too old.` and continued. That check is not part of the game binary.

| Artifact | Result |
| --- | --- |
| `cataclysm-tiles` | 182 MB, linked to SDL2, SDL2_ttf, SDL2_image, SDL2_mixer |
| `tests/cata_test` | 225 MB, compiled and linked. The Catch suite was not executed. |

The full compiler log is about 2.3 MB of command lines and lived at `/tmp/cdda-build-logs/tiles-release.log` on that VM. It is not in git. The binary is gitignored and is not a download from this pull request.

## Run

```bash
tools/vm-baseline/run-tiles.sh
```

Default user directory: `/tmp/cdda-0i-1-userdir`. The playtest used `/tmp/cdda-playtest-1`. The script refuses a home directory or any path containing `cataclysm-dda` unless `CDDA_ALLOW_HOME_USERDIR=1`.

The VM had no `/dev/snd`. The playtest set `SDL_AUDIODRIVER=dummy` so SDL mixer did not wait on a missing device. Sound support is still compiled in.

The process is local. It does not open a port.

## Playtest

Disposable world only. No path under `$HOME` or `./save` was created.

| Step | Evidence |
| --- | --- |
| Title | Window title `Cataclysm: Dark Days Ahead - 0.I-1 7b2efa5cea`. Menu text `Version: 0.I-1 (Ito-1) Stable Release`. |
| New game | Keys `n`, `d` (Play Now, default scenario). No world-name confirmation. |
| World | `Kennard Corner`, character Drema Hutson, evac shelter, Spring, day 61, year 1. |
| Move | Right moved from inside the shelter onto the dirt east of it. Later steps printed `Moving onto this bench is slow.` |
| Inventory | `i` showed the smartphone, wallet, ear plugs, and worn clothes. Escape closed it. |
| Drop | `d`, `c` (ear plugs), Return. Log: `You drop your pair of ear plugs on the bench.` |
| Pickup | `g` opened the pickup list with no direction key (only the bench had items). `a`, Return. Log: `You pick up 1 pair of ear plugs`. |
| Save | Shift+S, then `Y` on `Save and quit? (Case Sensitive)`. Returned to the main menu. Process still running. |
| Quit | `q`, then `Y`. Process exit code 0. |
| Reload | New process, same `--userdir`. Menu showed `Kennard Corner (1)`. Return, then `1` (the character row's key). Same shelter, same log lines, inventory still had the ear plugs. |

The key log, including two load-menu attempts that did not load the character, is `tools/vm-baseline/playtest-keys.txt`. The successful reload keys were Return and `1`.

## Portable archive

`tools/vm-baseline/package-linux-tiles.sh` packs the existing `cataclysm-tiles` binary with `data/`, `gfx/`, fonts, and the license files. It does not rebuild, and it does not include saves, config, `data/cache`, or the playtest world. Runtime requirements are in `doc/PORTABLE-LINUX-0.I-1.md`.

The archive is not stored in git. The copy for this run is:

- `/opt/cursor/artifacts/cataclysmdda-0.I-1-linux64-tiles.tar.gz`
- `/opt/cursor/artifacts/cataclysmdda-0.I-1-linux64-tiles.tar.gz.sha256`

## What this VM does not keep

The cloud VM disk and the full compiler log are not a durable download and are not published on a port. The portable archive above is the file to keep. It does not include a desktop session. Saves from the playtest were disposable and are not in the archive.
