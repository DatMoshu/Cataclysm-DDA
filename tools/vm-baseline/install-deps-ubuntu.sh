#!/usr/bin/env bash
# Install Ubuntu packages for a 0.I-1 tiles (SDL2) build.
# Same license as the rest of this repository (CC BY-SA 3.0).
set -euo pipefail

if [[ "$(id -u)" -eq 0 ]]; then
  APT=(apt-get)
else
  APT=(sudo DEBIAN_FRONTEND=noninteractive apt-get)
fi

"${APT[@]}" update
"${APT[@]}" install -y \
  build-essential \
  ccache \
  pkg-config \
  libsdl2-dev \
  libsdl2-ttf-dev \
  libsdl2-image-dev \
  libsdl2-mixer-dev \
  libfreetype6-dev \
  gettext \
  libncurses-dev
