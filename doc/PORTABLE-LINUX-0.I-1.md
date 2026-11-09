# Cataclysm DDA 0.I-1 portable Linux tiles build

English tiles build of upstream `0.I-1` commit `7b2efa5cea38e4d4d97dd0e63b28b9148623da59`.
The in-game version string is `0.I-1 (Ito-1) Stable Release`.

This archive is the game binary plus the data, graphics, and fonts it needs.
It is not a GitHub Release. It does not contain a save, a config directory, a memorial, or the VM playtest world.
The Catch test binary was compiled and linked during the baseline build and is not in this archive. That suite was not run.

The build VM is ephemeral. This file is the copy to keep. Unpacking it does not give you that VM's desktop.

## What you need

- 64-bit Linux, x86-64
- glibc 2.38 or newer. The binary uses `GLIBC_2.38`. Ubuntu 24.04 has glibc 2.39 and is the system this binary was built on. Ubuntu 22.04 has glibc 2.35 and will not load it.
- SDL2, SDL2_ttf, SDL2_image, SDL2_mixer, freetype, and zlib as shared libraries
- A display you already have (X11 or Wayland). No sound device is required. If sound startup complains, run with `SDL_AUDIODRIVER=dummy`.

Known-good library versions from the build machine (Ubuntu 24.04.5):

| Package | Version |
| --- | --- |
| libc6 | 2.39-0ubuntu8.9 |
| libsdl2-2.0-0 | 2.30.0+dfsg-1ubuntu3.1 |
| libsdl2-ttf-2.0-0 | 2.22.0+dfsg-1 |
| libsdl2-image-2.0-0 | 2.8.2+dfsg-1build2 |
| libsdl2-mixer-2.0-0 | 2.8.0+dfsg-1build3 |
| libfreetype6 | 2.13.2+dfsg-1ubuntu0.2 |
| zlib1g | 1:1.3.dfsg-3.1ubuntu2.2 |

## Fresh Ubuntu 24.04

```bash
sudo apt-get update
sudo apt-get install -y \
  libsdl2-2.0-0 \
  libsdl2-ttf-2.0-0 \
  libsdl2-image-2.0-0 \
  libsdl2-mixer-2.0-0 \
  libfreetype6 \
  zlib1g

tar -xzf cataclysmdda-0.I-1-linux64-tiles.tar.gz
cd cataclysmdda-0.I-1-linux64-tiles
./run-portable.sh
```

`run-portable.sh` stores config and saves in `./user-data` next to the binary. That directory is created when you play. It is not in the archive. To put them somewhere else:

```bash
CDDA_USERDIR="$HOME/.local/share/cataclysm-dda-0.I-1" ./run-portable.sh
```

`./cataclysm-launcher` is the upstream launcher. Without `--userdir` it follows the game's default and writes `save/` and `config/` in the install directory.

## Check the archive

```bash
sha256sum -c cataclysmdda-0.I-1-linux64-tiles.tar.gz.sha256
```

`VERSION.txt` inside the archive records the upstream commit, the ELF Build ID, and the SHA-256 of `cataclysm-tiles`. The packaged binary is the compiler output with debug symbols removed (`strip --strip-unneeded`). The Build ID is unchanged. This package was not a second compile.

## Build it again

From a checkout that contains commit `7b2efa5cea38e4d4d97dd0e63b28b9148623da59` (tag `0.I-1`) plus the `tools/vm-baseline` scripts:

```bash
tools/vm-baseline/install-deps-ubuntu.sh
tools/vm-baseline/build-tiles.sh
OUTDIR=/opt/cursor/artifacts tools/vm-baseline/package-linux-tiles.sh
```

`install-deps-ubuntu.sh` installs the compilers and the SDL development packages. The runtime command above is only the shared libraries. `build-tiles.sh` runs:

```bash
make -j"$(nproc)" TILES=1 SOUND=1 RELEASE=1 LOCALIZE=0 LTO=0 CCACHE=1
```

`package-linux-tiles.sh` refuses to run if `cataclysm-tiles` is missing or does not contain the version string `0.I-1 7b2efa5cea`. It does not delete the unstripped binary in the build tree.

## License

Cataclysm is CC BY-SA 3.0. See `LICENSE.txt`. The bundled fonts have their own notices: `LICENSE-OFL-Terminus-Font.txt` and `LICENSE-Apache-Robot-Font.txt`. Do not strip those files out of the archive.
