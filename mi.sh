#!/bin/sh

proj_dir="$(dirname "${BASH_SOURCE[0]}")/src/mi"

dotnet run --project $proj_dir -v q
