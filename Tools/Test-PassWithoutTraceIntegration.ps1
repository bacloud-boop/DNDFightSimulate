param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$output=Join-Path $env:TEMP 'DNDFight_PassWithoutTraceChecks.exe'
& "$UnityData/MonoBleedingEdge/bin/mono.exe" "$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe" -nologo "-out:$output" Tools/Tests/PassWithoutTraceChecks.cs Assets/Scripts/PassWithoutTraceEffect.cs
if($LASTEXITCODE -ne 0){throw 'Pass without Trace compile failed'}
& "$UnityData/MonoBleedingEdge/bin/mono.exe" $output
if($LASTEXITCODE -ne 0){throw 'Pass without Trace checks failed'}
