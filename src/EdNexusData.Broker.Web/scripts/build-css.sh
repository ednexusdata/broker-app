#!/bin/bash
if command -v mise >/dev/null 2>&1; then
    eval "$(mise activate bash)"
fi

npm run css:build