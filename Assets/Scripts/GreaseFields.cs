using System;
using System.Collections.Generic;
public sealed class GreaseField
{
 public readonly long Id,Dc,Begins,Expires;public readonly int X,Y,Land;
 internal GreaseField(long id,int x,int y,int land,long dc,long now){Id=id;X=x;Y=y;Land=land;Dc=dc;Begins=now;Expires=now+10;}
 public bool Active(long now)=>now>=Begins&&now<Expires;
 public bool Cell(int x,int y)=>x>=X&&x<X+2&&y>=Y&&y<Y+2&&(Land&(1<<((y-Y)*2+x-X)))!=0;
 public bool Intersects(int x,int y,int spaces){for(int j=0;j<spaces;j++)for(int i=0;i<spaces;i++)if(Cell(x+i,y+j))return true;return false;}
}
public sealed class GreaseFields
{
 readonly List<GreaseField> fields=new List<GreaseField>();long sequence;
 public bool CanAdd(long now)=>now>=0&&now<=long.MaxValue-10&&sequence<long.MaxValue;
 public GreaseField Add(int x,int y,int land,long dc,long now){if(!CanAdd(now)||land<0||land>15)throw new ArgumentException();fields.RemoveAll(f=>!f.Active(now));var added=new GreaseField(++sequence,x,y,land,dc,now);fields.Add(added);return added;}
 public GreaseField Current(int x,int y,int spaces,long now){GreaseField best=null;foreach(var f in fields)if(f.Active(now)&&f.Intersects(x,y,spaces)&&(best==null||f.Dc>best.Dc||f.Dc==best.Dc&&f.Id>best.Id))best=f;return best;}
 public bool Cell(int x,int y,long now){foreach(var f in fields)if(f.Active(now)&&f.Cell(x,y))return true;return false;}
 public bool Any(long now){foreach(var f in fields)if(f.Active(now)&&f.Land!=0)return true;return false;}
 public void Clear(){fields.Clear();sequence=0;}
}
