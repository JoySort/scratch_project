#!/bin/bash
git pull origin master
dotnet build SortSystem.sln  -property:Configuration="release" -property:Configuration="debug"
dotnet vstest LibUnitTest/bin/Debug/net6.0/LibUnitTest.dll