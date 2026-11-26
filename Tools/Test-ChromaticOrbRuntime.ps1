param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$out=Join-Path $env:TEMP 'DNDFight_ChromaticOrbRuntimeChecks.exe'
& $mono $csc -nologo -main:ChromaticOrbChecksMain "-out:$out" Tools/Tests/SpiritualWeaponRuntimeChecks.cs Tools/Tests/ChromaticOrbRuntimeChecks.cs Tools/Tests/MapValueStubs.cs Assets/Scripts/SpiritualWeaponEffect.cs Assets/Scripts/SpiritualWeaponSpell.cs Assets/Scripts/RangedSpellAttack.cs Assets/Scripts/CreatureCover.cs Assets/Scripts/AcidArrowResidue.cs Assets/Scripts/DamageTypes.cs Assets/Scripts/ChromaticOrbRules.cs Assets/Scripts/ChromaticOrbSpell.cs
if($LASTEXITCODE -ne 0){throw 'Chromatic Orb runtime harness compile failed'}
& $mono $out
if($LASTEXITCODE -ne 0){throw 'Chromatic Orb runtime checks failed'}
