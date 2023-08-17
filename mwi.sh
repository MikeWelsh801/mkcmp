#!/bin/sh

proj_dir="$(dirname "${BASH_SOURCE[0]}")/src/mwi/mwi.csproj"

dotnet run --project $proj_dir -v q
