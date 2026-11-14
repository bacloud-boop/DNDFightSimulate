using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame {
 readonly MirrorImageEffect mirrorImage=new MirrorImageEffect();bool defenseOrderPending;int defenseOrder;bool mirrorIntercepted;
 void DrawDefenseOrder(){if(!defenseOrderPending)return;Fill(new Rect(0,0,1600,900),new Color(0,0,0,.94f));Surface(new Rect(465,245,670,380),paperTexture);GUI.Label(new Rect(495,270,610,60),"攻击命中 · 选择防御顺序",heading);GUI.Label(new Rect(495,345,610,130),"镜像术与护盾术均可触发。选择顺序后逐项处理；此时不公开敌方命中骰值与总值。",label);if(Btn(new Rect(495,535,285,55),"先镜像术"))defenseOrder=1;if(Btn(new Rect(820,535,285,55),"先护盾术"))defenseOrder=2;}
 IEnumerator InterceptMirror(bool attackerSees,bool bypass){
  mirrorIntercepted=false;int count=mirrorImage.Count(worldRounds);if(count==0||!attackerSees||bypass)yield break;
  int[] rolls=new int[count];for(int i=0;i<count;i++)rolls[i]=Random.Range(1,7);
  mirrorIntercepted=mirrorImage.Intercept(worldRounds,rolls);dice="镜像术 · 替身判定";yield return PresentRoll(6,rolls,mirrorIntercepted?"替身承受本次攻击并消失；剩余 "+mirrorImage.Count(worldRounds):"所有骰值均低于3，本次未能拦截");history.Insert(0,mirrorIntercepted?"镜像术替身承受攻击，本体不受此次命中效果。":"镜像术未拦截攻击。");
 }
 IEnumerator ResolvePlayerHitDefense(int die,long total,bool attackerSees,bool bypass){
  mirrorIntercepted=false;bool mirror=mirrorImage.Count(worldRounds)>0&&attackerSees&&!bypass;int order=1;
  if(mirror&&CanOfferShield){defenseOrder=0;defenseOrderPending=true;dice="攻击已命中 · 等待防御顺序选择";while(defenseOrder==0)yield return null;defenseOrderPending=false;order=defenseOrder;}
  if(order==1){yield return InterceptMirror(attackerSees,bypass);if(!mirrorIntercepted)yield return OfferShield(true);}
  else {yield return OfferShield(true);if(die==20||total>=combat.PlayerArmorClass)yield return InterceptMirror(attackerSees,bypass);}
 }
}
