rm *.nupkg
nuget pack .\CameraBuddy.nuspec -IncludeReferencedProjects -Prop Configuration=Release
nuget push -source https://www.nuget.org -NonInteractive *.nupkg