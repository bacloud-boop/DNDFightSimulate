param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$out=Join-Path $env:TEMP 'DNDFight_SpiritualWeaponRuntimeChecks.exe'
& $mono $csc -nologo "-out:$out" Tools/Tests/SpiritualWeaponRuntimeChecks.cs Tools/Tests/MapValueStubs.cs Assets/Scripts/SpiritualWeaponEffect.cs Assets/Scripts/SpiritualWeaponSpell.cs Assets/Scripts/RangedSpellAttack.cs Assets/Scripts/CreatureCover.cs Assets/Scripts/AcidArrowResidue.cs
if($LASTEXITCODE -ne 0){throw 'Spiritual Weapon runtime harness compile failed'}
& $mono $out
if($LASTEXITCODE -ne 0){throw 'Spiritual Weapon runtime checks failed'}
