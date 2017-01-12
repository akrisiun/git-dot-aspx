@echo off

.nuget\nuget restore GitAspx.sln

@set msbuild="%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"
@if not exist %MSBuild% @set msbuild="%ProgramFiles%\MSBuild\14.0\Bin\MSBuild.exe"
@if not exist %MSBuild% @set msbuild="%ProgramFiles(x86)%\MSBuild\12.0\Bin\MSBuild.exe"
@if not exist %MSBuild% @set msbuild="%ProgramFiles%\MSBuild\12.0\Bin\MSBuild.exe"

%msbuild% /p:Configuration=Debug /v:m  GitAspx.sln 

PAUSE