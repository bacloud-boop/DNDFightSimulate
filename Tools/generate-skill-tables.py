import json
from pathlib import Path
d=json.loads(Path('Assets/Resources/Rules2024/Skills.json').read_text(encoding='utf-8'))
arr=lambda xs:'new[]{'+','.join(map(str,xs))+'}'
strings=lambda xs:'new[]{'+','.join(json.dumps(x,ensure_ascii=False) for x in xs)+'}'
s='// Generated mechanical tables. Run Tools/generate-skill-tables.py after editing Skills.json.\npublic static class SkillSourceTables\n{\n'
s+=' public static readonly string[] BackgroundNames='+strings([x['name'] for x in d['backgrounds']])+';\n'
s+=' public static readonly string[] ToolNames='+strings([x['name'] for x in d['tools']])+';\n'
s+=' public static readonly int[] ToolAbilities='+arr([x['ability'] for x in d['tools']])+';\n'
s+=' public static readonly int[] ClassCounts='+arr([x['initialCount'] for x in d['classes']])+';\n'
for name,values in [('ClassOptions',[x['skills'] for x in d['classes']]),('BackgroundSkills',[x['skills'] for x in d['backgrounds']]),('BackgroundTools',[x['toolOptions'] for x in d['backgrounds']])]:s+=' public static readonly int[][] '+name+'={'+','.join(arr(v) for v in values)+'};\n'
s+=' public static readonly string[] BackgroundFeats='+strings([x['proficiencyFeat'] for x in d['backgrounds']])+';\n}\n'
Path('Assets/Scripts/SkillSourceTables.cs').write_text(s,encoding='utf-8')
