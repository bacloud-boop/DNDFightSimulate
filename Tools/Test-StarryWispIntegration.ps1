param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$out=Join-Path $env:TEMP 'DNDFight_StarryWispChecks.exe'
& $mono $csc -nologo "-out:$out" Tools/Tests/StarryWispChecks.cs Assets/Scripts/StarryWispEffect.cs Assets/Scripts/VisionState.cs Assets/Scripts/SenseEffects.cs Assets/Scripts/DirectSpell.cs Assets/Scripts/AcidSplash.cs Assets/Scripts/MonsterSpellSequence.cs
if($LASTEXITCODE -ne 0){throw 'Starry Wisp model compile failed'}
& $mono $out
if($LASTEXITCODE -ne 0){throw 'Starry Wisp checks failed'}
