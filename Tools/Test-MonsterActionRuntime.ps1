param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$names=@('MonsterActionRuntime','MonsterActionPolicy','CombatState','PlayerClasses','Subclasses2024','MonsterSpellSequence','MonsterSpellCatalog','DirectSpell','AcidSplash','BreathWeaponRules')
$files=@('Tools/Tests/MonsterActionRuntimeChecks.cs')+$names.ForEach({"Assets/Scripts/$_.cs"})
$out=Join-Path $env:TEMP 'DNDFight_MonsterActionRuntimeChecks.exe'
& $mono $csc -nologo "-out:$out" @files
if($LASTEXITCODE -ne 0){throw 'Monster priority runtime harness compile failed'}
& $mono $out
if($LASTEXITCODE -ne 0){throw 'Monster priority runtime checks failed'}
