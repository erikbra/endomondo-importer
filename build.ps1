#!/usr/bin/env pwsh

$runtimes = "linux-x64","win-x64";

$runtimes | % {
    dotnet publish -c release -p:PublishSingleFile=true --self-contained true -r $_ -o publish/$_
}