const fs = require('fs');
const html = fs.readFileSync(fs.existsSync('.cache/Rules2024/creatures.html')?'.cache/Rules2024/creatures.html':'Temp/creatures-2024.html', 'utf8');
const clean = value => value.replace(/<[^>]+>/g, '').replace(/&amp;/g, '&').replace(/&nbsp;/g, ' ').replace(/&#39;|&rsquo;/g, "'").replace(/−/g, '-').replace(/\s+/g, ' ').trim();
const headings = [...html.matchAll(/<h[34]\b[^>]*id="([^"]+)"[^>]*>([\s\S]*?)<\/h[34]>/g)];
const entries = [];
for (let i=0;i<headings.length;i++) {
 const heading=headings[i];const block=html.slice(heading.index+heading[0].length,i+1<headings.length?headings[i+1].index:html.indexOf('<!-- FOOTER -->',heading.index));
 if(!/^\s*<p[^>]*>[^<]*\bBeast,/.test(block))continue;
 const text=clean(block);const cr=text.match(/\bCR ([\d/]+)/);if(!cr)throw Error('Missing CR '+clean(heading[2]));
 const challenge=cr[1].includes('/')?Number(cr[1].split('/')[0])/Number(cr[1].split('/')[1]):Number(cr[1]);if(challenge>1)continue;
 const field=key=>{const match=block.match(new RegExp('<strong>'+key+'<\\/strong>([\\s\\S]*?)<\\/p>'));return match?clean(match[1]):'';};
 const speed=field('Speed');const attributeNames=['Str','Dex','Con','Wis','Int','Cha'];const abilities=[],saves=[];
 for(const name of attributeNames){const row=block.match(new RegExp('<th>'+name+'<\\/th>\\s*<td>([^<]+)<\\/td>\\s*<td>([^<]+)<\\/td>\\s*<td>([^<]+)<\\/td>'));if(!row)throw Error('Missing '+name);abilities.push(Number(clean(row[1])));saves.push(Number(clean(row[3])));}
 const actionNames=[...block.matchAll(/<strong><em>([^<]+)<\/em><\/strong>/g)].map(x=>clean(x[1]).replace(/\.$/,''));
 entries.push({name:clean(heading[2]),size:text.split(' ')[0],challenge,challengeLabel:cr[1],armor:Number(field('AC').match(/^\d+/)[0]),hp:Number(field('HP').match(/^\d+/)[0]),walk:Number((speed.match(/^\d+/)||['0'])[0]),climb:Number((speed.match(/Climb (\d+)/)||[0,0])[1]),swim:Number((speed.match(/Swim (\d+)/)||[0,0])[1]),fly:Number((speed.match(/Fly (\d+)/)||[0,0])[1]),burrow:Number((speed.match(/Burrow (\d+)/)||[0,0])[1]),darkvision:Number((field('Senses').match(/Darkvision (\d+)/)||[0,0])[1]),blindsight:Number((field('Senses').match(/Blindsight (\d+)/)||[0,0])[1]),abilities,saves,skillModifiers:['Athletics','Acrobatics','Sleight of Hand','Stealth','Arcana','History','Investigation','Nature','Religion','Animal Handling','Insight','Medicine','Perception','Survival','Deception','Intimidation','Performance','Persuasion'].map(name=>{const m=field('Skills').match(new RegExp('(?:^|, )'+name+' ([+-]?\\d+)'));return m?Number(m[1]):-999;}),actionNames,source:'https://www.dndbeyond.com/sources/dnd/br-2024/creature-stat-blocks#'+heading[1]});
}
if(new Set(entries.map(x=>x.name)).size!==entries.length)throw Error('Duplicate beast');
fs.writeFileSync('Assets/Resources/Rules2024/Beasts.json',JSON.stringify({description:'Mechanical stat facts for free 2024 individual Beasts with CR at most 1. This file stores numerical metadata and trait/action names. Ten forms have current ground-combat effects in BeastAttack.cs; other forms and off-map terrain effects are pending. Flight is deferred.',entries},null,2));
console.log('Imported '+entries.length+' beasts, '+entries.filter(x=>x.fly===0).length+' without flight.');
