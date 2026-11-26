param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$out=Join-Path $env:TEMP 'DNDFight_ElementalWeaponRuntimeChecks.exe'
& $mono $csc -nologo "-out:$out" Tools/Tests/ElementalWeaponRuntimeChecks.cs Assets/Scripts/ElementalWeaponEffect.cs Assets/Scripts/ElementalWeaponSpell.cs Assets/Scripts/EquipmentObjects.cs Assets/Scripts/TimedSpellBuffs.cs Assets/Scripts/DamageTypes.cs Assets/Scripts/CombatState.cs Assets/Scripts/PlayerClasses.cs Assets/Scripts/Subclasses2024.cs
if($LASTEXITCODE -ne 0){throw 'Elemental Weapon runtime harness compile failed'}
& $mono $out
if($LASTEXITCODE -ne 0){throw 'Elemental Weapon runtime checks failed'}
