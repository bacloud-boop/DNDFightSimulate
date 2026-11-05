let hp=1000,round=1,busy=false,ctx,musicGain,timer;
const $=id=>document.getElementById(id);
function screen(id){audio();document.querySelectorAll('section').forEach(s=>s.hidden=s.id!==id)}
function tone(freq,duration,gain,type='sine'){if(!ctx)return;const o=ctx.createOscillator(),g=ctx.createGain();o.type=type;o.frequency.value=freq;g.gain.setValueAtTime(gain,ctx.currentTime);g.gain.exponentialRampToValueAtTime(.0001,ctx.currentTime+duration);o.connect(g);g.connect(ctx.destination);o.start();o.stop(ctx.currentTime+duration)}
function audio(){if(!ctx){ctx=new AudioContext();let step=0;const notes=[130.81,164.81,196,164.81,146.83,174.61,220,174.61];timer=setInterval(()=>tone(notes[step++%notes.length],1.5,Number($('music').value)/100*.10),650)}ctx.resume()}
function sound(){audio();tone(180+Math.random()*600,.09,Number($('sfx').value)/100*.22,'triangle')}
for(const [id,out,initial] of [['music','mv',35],['sfx','sv',65]]){let v=initial;try{const saved=localStorage.getItem(id);if(saved!==null&&Number.isFinite(Number(saved)))v=Math.max(0,Math.min(100,Number(saved)))}catch{}$(id).value=v;$(out).value=v+'%';$(id).oninput=()=>{audio();$(out).value=$(id).value+'%';try{localStorage.setItem(id,$(id).value)}catch{}}}
function d12(){const a=new Uint32Array(1);do{crypto.getRandomValues(a)}while(a[0]>=4294967292);return a[0]%12+1}
function update(){$('hp').textContent=hp+' / 1000';$('health').value=hp;$('round').textContent='第 '+round+' 回合'}
function log(t){const p=document.createElement('p');p.textContent=t;$('log').prepend(p)}
async function attack(){if(busy)return;busy=true;['attack','end','back'].forEach(id=>$(id).disabled=true);try{for(let i=0;i<9;i++){$('dice').textContent='滚动中 · '+d12();sound();await new Promise(r=>setTimeout(r,100+i*12))}const value=d12();hp=Math.max(0,hp-value);$('dice').textContent='D12 → '+value;log('投掷结果 '+value+'，本次伤害 '+value+'。');update()}finally{busy=false;['attack','end','back'].forEach(id=>$(id).disabled=false)}}
function endRound(){hp=1000;round++;update();$('dice').textContent='D12 · 待投掷';log('回合结束，木桩血量恢复至 1000。')}
async function quit(){if(window.pywebview){clearInterval(timer);if(ctx)await ctx.close();await window.pywebview.api.quit_game()}else alert('请使用 python app.py 启动桌面游戏，以使用退出功能。')}
