#!/usr/bin/env bash
# Copies the built Angular Elements bundle into the MAUI app's wwwroot.
# Run after scripts/bootstrap.sh. Windows users can run this with Git Bash or WSL.
set -euo pipefail

PANEL_OUT="src/rework-panel/dist/rework-panel"
TARGET="src/Rework.Desktop/wwwroot/angular"

if [ ! -f "$PANEL_OUT/main.js" ]; then
  echo "Panel build not found at $PANEL_OUT. Run: (cd src/rework-panel && npm run build)" >&2
  exit 1
fi

mkdir -p "$TARGET"
cp "$PANEL_OUT/main.js" "$TARGET/main.js"
cp "$PANEL_OUT/polyfills.js" "$TARGET/polyfills.js"
echo "Copied panel bundle to $TARGET"
