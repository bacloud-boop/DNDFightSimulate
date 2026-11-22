using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    MonsterActionPolicy monsterActions=new MonsterActionPolicy(),monsterActionDraft=new MonsterActionPolicy();
    int SpellChaseReach(DirectSpell spell)=>MonsterSpellCatalog.IsCone(spell)?(spell.Name=="Conjure Barrage"||spell.Name=="Cone of Cold"?60:15):MonsterSpellCatalog.IsLine(spell)?100:Mathf.Max(spell.Range,spell.EmanationRadius);
    IEnumerator RunMonsterPriorityTurn()
    {
        foreach(int kind in monsterActions.Order){
            if(!monsterActions.Enabled(kind))continue;
            if(!combat.MonsterCanAct||combat.BattleFinished||page!="battle"||!map.Locked)yield break;
            if(kind==(int)MonsterActionKind.Species){
                // Species actions share their original resources; placement comes from this policy.
                bool before=monsterSpecies.BonusPriority==0;
                yield return TryMonsterSpeciesMagic(before,true);yield return TryMonsterSpeciesBonus(before,true);
                if(BreathReady(-1)){yield return ChasePlayer(30,false);if(combat.MonsterCanAct)yield return TryMonsterBreath(before,true);}
            }
            else if(kind==(int)MonsterActionKind.SpellCycle){
                var entry=monsterSpells.Next;var spell=entry==null?null:MonsterSpellCatalog.Find(entry.Name);
                if(spell!=null&&MonsterSpellReady&&(spell.Bonus?combat.MonsterBonusActionAvailable:combat.MonsterActionAvailable)){
                    if(!spell.Healing)yield return ChasePlayer(SpellChaseReach(spell),false);
                    var result=new MonsterSpellTurn();yield return TryMonsterTurnSpell(result);
                }
            }
            else if(kind==(int)MonsterActionKind.DragonBreath){
                if(DragonBreathReady(-1)){yield return ChasePlayer(15,false);BreathTemplate plan;if(DragonBreathReady(-1)&&TryPlanDragonBreath(-1,out plan))yield return UseDragonBreath(-1,plan,true);}
            }
            else if(kind==(int)MonsterActionKind.CustomAttack&&combat.MonsterActionAvailable){
                yield return ChasePlayer(MonsterAttackReach);
                if(combat.MonsterCanAct&&MonsterCanCastAttack&&combat.MonsterActionAvailable){int victim=map.NearestEnemyInReach(!combat.PlayerDead,MonsterAttackReach);
                    if(victim>=0&&combat.TryUseMonsterAction()){if(victim>0)yield return ResolveMonsterSpiritAttack(FindSummon(victim));else yield return Counterattack(false,true);}
                }
            }
            rolling=true;
        }
    }
    void DrawMonsterActionPriorities()
    {
        GUILayout.Label("自动行动优先级（由上至下）",heading);
        GUILayout.Label("依次尝试，动作与附赠动作分别共享额度；不可用时跳过。法术循环每回合最多尝试一项。束缚时仍优先按已有规则脱困。",small);
        for(int i=0;i<4;i++){
            int kind=monsterActionDraft.Order[i];monsterActionDraft.SetEnabled(kind,GUILayout.Toggle(monsterActionDraft.Enabled(kind),MonsterActionPolicy.Names[kind],label));
            GUILayout.BeginHorizontal();if(GUILayout.Button("上移",button))monsterActionDraft.Move(i,-1);if(GUILayout.Button("下移",button))monsterActionDraft.Move(i,1);GUILayout.EndHorizontal();
        }
    }
}
