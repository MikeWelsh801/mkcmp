#!/bin/sh

proj_dir="$(dirname "${BASH_SOURCE[0]}")/src/mwc/mwc.csproj"

dotnet run --project $proj_dir -- "$@"

