#!/bin/sh
dotnet build
dotnet test ./mkcmp.Tests/mkcmp.Tests.csproj
