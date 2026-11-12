import json,re,html,urllib.parse
from pathlib import Path
clean=lambda s:html.unescape(re.sub(r'<[^>]*>','',s)).strip()
norm=lambda s:re.sub('[^a-z0-9]','',s.lower())
index=json.loads(Path('Docs/ChineseSpellSource2024.json').read_text(encoding='utf-8'))['spells']
old=json.loads(Path('Assets/Resources/Rules2024/Spells.json').read_text(encoding='utf-8'))['spells']; prior={norm(s['name']):s for s in old}
chapters={}
for p in Path('Temp').glob('chinese-spells-*.html'):
 text=p.read_text(encoding='utf-8-sig'); heads=list(re.finditer(r'<h4\b[^>]*id="([^"]+)"[^>]*>(.*?)</h4>',text,re.I|re.S))
 for i,h in enumerate(heads):chapters[h[1]]=(clean(h[2]),text[h.end():heads[i+1].start() if i+1<len(heads) else len(text)])
classes={'诗':'Bard','牧':'Cleric','德':'Druid','帕':'Paladin','软':'Ranger','术':'Sorcerer','法':'Wizard','锁':'Warlock'}
levels={'戏法':0,'一环':1,'二环':2,'三环':3,'四环':4,'五环':5,'六环':6,'七环':7,'八环':8,'九环':9}
spells=[]; matched=0
for entry in index:
 anchor=urllib.parse.unquote(entry['href'].split('#')[1])
 if anchor not in chapters:
  candidates=[k for k,(title,_) in chapters.items() if norm(re.sub(r'^[^A-Za-z]+','',entry['name']))==norm(re.split(r'[｜|]',title)[-1])]
  if len(candidates)!=1: raise ValueError('Unresolved source anchor: '+ascii(entry['name']))
  anchor=candidates[0]
 title,body=chapters[anchor]
 parts=re.split(r'[｜|]',title,maxsplit=1); chinese=parts[0].strip();english=parts[1].strip() if len(parts)>1 else re.sub(r'^[^A-Za-z]+','',entry['name'])
 previous=prior.get(norm(english)); matched+=previous is not None
 field=lambda label: clean((re.search(r'<strong>\s*'+label+r'[:：]\s*</strong>(.*?)(?:<br\s*/?>|</p>)',body,re.I|re.S) or [None,''])[1])
 timing=re.sub(r'或?\s*仪式','',field('施法时间')).strip()
 record={'name':previous['name'] if previous else english,'chineseName':chinese,'level':levels[entry['levelLabel']],'school':entry['school'],'classes':[classes[c] for c in entry['classes'].split() if c in classes],'castingTime':timing,'range':field('施法距离'),'components':entry['components'],'duration':field('持续时间'),'concentration':entry['concentration'],'source':urllib.parse.urljoin('https://5echm.kagangtuya.top/topics/速查/法术速查/5E万法大全.html',entry['href'].split('#')[0]+'#'+anchor),'implementation':previous['implementation'] if previous else 'not_implemented'}
 assert record['range'] and record['castingTime'] and record['duration'],english
 if record['name'] in set(re.findall(r'new DirectSpell\("([^"]+)"',Path('Assets/Scripts/DirectSpell.cs').read_text(encoding='utf-8-sig')))|{'Acid Splash','Shield'}:record['implementation']='combat_implemented'
 spells.append(record)
assert len(spells)==391 and len({norm(s['name']) for s in spells})==391
result={'count':len(spells),'scope':'PHB2024 source metadata; implementation status is explicit.','spells':spells}
for p in ['Assets/Resources/Rules2024/Spells.json','Docs/FreeSpellInventory.json']:Path(p).write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
print(f'Imported {len(spells)} PHB2024 metadata entries; matched {matched} existing entries; {len(spells)-matched} additional entries.')
