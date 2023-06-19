#!/bin/sh

slndir="$(dirname "${BASH_SOURCE[0]}")/src"

# build and restore 
dotnet build "$slndir/mkcmp.sln" --nologo || exit

# test
dotnet test "$slndir/mkcmp.Tests" --nologo --no-build
