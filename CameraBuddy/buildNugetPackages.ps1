nuget pack .\CameraBuddy.nuspec -IncludeReferencedProjects -Prop Configuration=Release
nuget push *.nupkg