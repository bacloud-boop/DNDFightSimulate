param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$out=Join-Path $env:TEMP 'DNDFight_FlameBladeRuntimeChecks.exe'
$source=Get-Content Assets/Scripts/ArmorRuntime.cs -Raw -Encoding utf8
$begin=$source.IndexOf('    bool ArmorSpellAllowed(')
$end=$source.IndexOf('    sealed class ArmorSaveRoll',$begin)
if($begin -lt 0 -or $end -lt 0){throw 'Production hand gates not found'}
$gate=Join-Path $env:TEMP 'DNDFight_FlameBladeHandGates.cs'
Set-Content -LiteralPath $gate -Encoding utf8 -Value ('using System;using UnityEngine; public sealed partial class TrainingGame {'+$source.Substring($begin,$end-$begin)+'}')
& $mono $csc -nologo -main:FlameBladeChecksMain "-out:$out" Tools/Tests/SpiritualWeaponRuntimeChecks.cs Tools/Tests/FlameBladeRuntimeChecks.cs Tools/Tests/MapValueStubs.cs Assets/Scripts/SpiritualWeaponEffect.cs Assets/Scripts/SpiritualWeaponSpell.cs Assets/Scripts/RangedSpellAttack.cs Assets/Scripts/CreatureCover.cs Assets/Scripts/AcidArrowResidue.cs Assets/Scripts/VisionState.cs Assets/Scripts/WeaponGripState.cs Assets/Scripts/AmmunitionState.cs Assets/Scripts/FlameBladeEffect.cs Assets/Scripts/FlameBladeSpell.cs $gate
if($LASTEXITCODE -ne 0){throw 'Flame Blade production harness compile failed'}
& $mono $out
if($LASTEXITCODE -ne 0){throw 'Flame Blade runtime checks failed'}
