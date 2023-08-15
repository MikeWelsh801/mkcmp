#!/bin/sh

proj_dir="$(dirname "${BASH_SOURCE[0]}")/src/mc/mc.csproj"

dotnet run --project $proj_dir -- "$@"

