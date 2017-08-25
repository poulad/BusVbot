#!/usr/bin/env bash

rootdir="`pwd`"
# echo "Working dir => `pwd`"

#exit if any command fails
set -e


echo "## Start test.."

dotnet test "$rootdir/test/BusVbot.Tests/BusVbot.Tests.csproj" --configuration Release --list-tests
dotnet test "$rootdir/test/BusVbot.Tests/BusVbot.Tests.csproj" --configuration Release

echo "#> test completed"
