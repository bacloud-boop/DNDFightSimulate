param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$names=@('AcidSplash','DirectSpell','MonsterSpellCatalog','MonsterSpellSequence','CounterspellRules','BreathWeaponRules','CombatState','PlayerClasses','Subclasses2024','FighterFeatures','DamageTypes')
foreach($check in @('BurningHandsChecks','MonsterSpellChecks','MonsterSpellCatalogChecks','TypedDamageChecks')){
 $files=@("Tools/Tests/$check.cs")+$names.ForEach({"Assets/Scripts/$_.cs"})
 if($check -eq 'TypedDamageChecks'){$files+='Assets/Scripts/MonsterDamage.cs'}
 $out=Join-Path $env:TEMP ("DNDFight_"+$check+'.exe')
 & $mono $csc -nologo "-out:$out" @files
 if($LASTEXITCODE -ne 0){throw "Model compile failed: $check"}
 & $mono $out
 if($LASTEXITCODE -ne 0){throw "Checks failed: $check"}
}
