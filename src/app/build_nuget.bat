nuget pack ZBit.SharpBrake.2.3.0.nuspec -Prop Configuration=Release -Symbols
pause
nuget push ZBit.SharpBrake.2.3.0.nupkg -Source https://www.nuget.org/api/v2/package
pause
#nuget push ZBit.SharpBrake.2.3.0.symbols.nupkg
