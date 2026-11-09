# 0.I-1 VM baseline

Scripts for a tiles (SDL2) build of this tree. They do not change game rules.

The game stays under the repository license (CC BY-SA 3.0). These scripts are covered by that same license.

```bash
tools/vm-baseline/install-deps-ubuntu.sh
tools/vm-baseline/build-tiles.sh
tools/vm-baseline/run-tiles.sh
```

`run-tiles.sh` defaults to `--userdir /tmp/cdda-0i-1-userdir` and refuses `$HOME` and any path containing `cataclysm-dda` unless `CDDA_ALLOW_HOME_USERDIR=1`. Do not pass `USE_HOME_DIR=1` to make.

Details, the verified commit, and the playtest notes are in `doc/BASELINE-0.I-1.md`. The quality-of-life audit is in `doc/QOL-AUDIT-0.I-1.md`. No gameplay changes are included.
