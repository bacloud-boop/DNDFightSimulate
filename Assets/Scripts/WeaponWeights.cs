using System;
using System.Collections.Generic;
// PHB2024 free equipment table. Values are integer ounces, including the quarter-pound dart.
public static class WeaponWeights
{
 static readonly Dictionary<string,int> weights=new Dictionary<string,int>{
 {"长剑",48},{"短剑",32},{"匕首",16},{"刺剑",32},{"弯刀",48},{"鞭",48},
 {"短棒",32},{"巨棒",160},{"手斧",32},{"标枪",32},{"轻锤",32},{"硬头锤",64},{"长棍",64},{"镰刀",32},{"矛",48},
 {"战斧",64},{"链枷",32},{"长柄刀",96},{"巨斧",112},{"巨剑",96},{"戟",96},{"骑枪",96},{"巨锤",160},{"钉头锤",64},{"长矛",288},{"三叉戟",64},{"战镐",32},{"战锤",80},
 {"长弓",32},{"飞镖",4},{"轻弩",80},{"短弓",32},{"投石索",0},{"吹箭筒",16},{"手弩",48},{"重弩",288},{"火铳",160},{"手铳",48}};
 public static string Key(string name)=>name=="长剑（双手）"||name=="长剑（单手）"?"长剑":name;
 public static int Ounces(string name){int value;if(name==null||!weights.TryGetValue(Key(name),out value))throw new ArgumentException("Unknown weapon weight",nameof(name));return value;}
}
