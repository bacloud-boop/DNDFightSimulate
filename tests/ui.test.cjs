const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const elements = new Map();
const element = id => { if (!elements.has(id)) elements.set(id, {id,value:0, textContent:'', disabled:false, children:[], prepend(p){this.children.unshift(p)}}); return elements.get(id); };
const saved = new Map(); let closed=false;
class AudioContext { resume(){} close(){closed=true} createOscillator(){return {frequency:{},connect(){},start(){},stop(){}}} createGain(){return {gain:{setValueAtTime(){},exponentialRampToValueAtTime(){}},connect(){}}} }
let quit=false;
const context = vm.createContext({console, Uint32Array, Math, Number, AudioContext, crypto:{getRandomValues(a){a[0]=11}}, document:{getElementById:element,querySelectorAll(){return ['home','battle','settings'].map(element)},createElement(){return {}}}, localStorage:{getItem:k=>saved.get(k)??null,setItem:(k,v)=>saved.set(k,v)},setInterval(){return 1},clearInterval(){},setTimeout(fn){fn()},window:{pywebview:{api:{quit_game(){quit=true}}}},alert(){throw Error('unexpected alert')}});
vm.runInContext(fs.readFileSync('ui/app.js','utf8'),context);
(async()=>{
 assert.equal(vm.runInContext('d12()',context),12);
 vm.runInContext("screen('battle')",context); assert.equal(element('battle').hidden,false); assert.equal(element('home').hidden,true);
 const first=vm.runInContext('attack()',context); await vm.runInContext('attack()',context); await first;
 assert.equal(element('hp').textContent,'988 / 1000'); assert.equal(element('log').children.length,1); assert.equal(element('attack').disabled,false);
 vm.runInContext('endRound()',context); assert.equal(element('hp').textContent,'1000 / 1000'); assert.equal(element('round').textContent,'第 2 回合');
 element('music').value=0; element('music').oninput(); assert.equal(saved.get('music'),0); assert.equal(element('mv').value,'0%');
 await vm.runInContext('quit()',context); assert.equal(quit,true); assert.equal(closed,true);
 console.log('PASS: navigation, D12, single attack during animation, damage, round reset, volume storage, audio shutdown and desktop exit bridge');
})().catch(e=>{console.error(e);process.exitCode=1});

