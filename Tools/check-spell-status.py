import json,re
from pathlib import Path
from collections import Counter
runtime=set(re.findall(r'new DirectSpell\("([^"\n]+)"',Path('Assets/Scripts/DirectSpell.cs').read_text(encoding='utf-8-sig')))|{'Acid Splash','Shield','Counterspell','Hellish Rebuke'}
files=['Assets/Resources/Rules2024/Spells.json','Docs/FreeSpellInventory.json']
data=[json.loads(Path(p).read_text(encoding='utf-8-sig')) for p in files]
assert data[0]==data[1], 'Runtime and documented inventory differ'
records=data[0]['spells'];names={s['name'] for s in records}
assert len(names)==len(records)==data[0]['count']==391
assert not runtime-names, 'Runtime names missing from metadata: '+repr(runtime-names)
implemented={s['name'] for s in records if s['implementation']=='combat_implemented'}
assert implemented==runtime, 'Implementation flags differ from runtime catalog: '+repr(implemented^runtime)
assert all('Artificer' not in s['classes'] and set(s['components'])<= {'声音','姿势'} for s in records)
print('PASS: exact runtime spell names, implementation flags, synchronized inventories and removed classes/components',dict(Counter(s['implementation'] for s in records)))

learning_source=Path("Assets/Scripts/SpellLibrary.cs").read_text(encoding="utf-8-sig").split("static bool IsLearningOnly",1)[1].split(";",1)[0]
learning_names=set(re.findall(r'name=="([^"\n]+)"',learning_source))
assert learning_names=={s["name"] for s in records if s["implementation"]=="learning_only"}, "Learning-only UI and metadata differ"
print("PASS: learning-only spells have actual learning UI entries")
