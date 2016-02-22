@echo off

.nuget\nuget restore GitAspx.sln
msbuild GitAspx.sln 

PAUSE