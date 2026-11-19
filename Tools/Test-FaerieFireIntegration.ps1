param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
foreach($check in @('FaerieFireChecks','VisionChecks')){
 $out=Join-Path $env:TEMP ("DNDFight_"+$check+'.exe')
 & $mono $csc -nologo "-out:$out" "Tools/Tests/$check.cs" Assets/Scripts/FaerieFireEffect.cs Assets/Scripts/VisionState.cs
 if($LASTEXITCODE -ne 0){throw "Compile failed: $check"}
 & $mono $out
 if($LASTEXITCODE -ne 0){throw "Check failed: $check"}
}
