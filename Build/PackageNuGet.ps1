param($scriptRoot)

$msBuild = "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
$nuGet = "$scriptRoot..\tools\NuGet.exe"
$solution = "$scriptRoot\..\Rainbow.Tfs.sln"

& $nuGet restore $solution
& $msBuild $solution /p:Configuration=Release /t:Rebuild /m

$rainbowTfsAssembly = Get-Item "$scriptRoot\..\src\Rainbow.Tfs\bin\Release\Rainbow.Tfs.dll" | Select-Object -ExpandProperty VersionInfo
$targetAssemblyVersion = $rainbowTfsAssembly.ProductVersion

& $nuGet pack "$scriptRoot\Rainbow.Tfs.nuget\Rainbow.Tfs.nuspec" -version $targetAssemblyVersion
& $nuGet pack "$scriptRoot\..\src\Rainbow.Tfs\Rainbow.Tfs.csproj" -Symbols -Prop Configuration=Release