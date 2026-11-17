using System;
// Searches complete legal footprints, preferring no displacement of the fallen carrier.
public static class RescueReleasePlanner
{
    public struct Plan {public int CarrierX,CarrierY,BodyX,BodyY;}
    static long Distance2(int x,int y,int a,int b){long dx=x-a,dy=y-b;return dx*dx+dy*dy;}
    public static bool TryFind(int size,int x,int y,int carrierSpaces,int bodySpaces,Func<int,int,int,int,bool> legal,out Plan result)
    {
        result=default(Plan);if(size<1||carrierSpaces<1||bodySpaces<1||carrierSpaces>size||bodySpaces>size||legal==null)return false;
        long bestCarrier=long.MaxValue,bestBody=long.MaxValue;bool found=false;
        for(int cy=0;cy<=size-carrierSpaces;cy++)for(int cx=0;cx<=size-carrierSpaces;cx++){
            long carrierDistance=Distance2(cx,cy,x,y);if(carrierDistance>bestCarrier)continue;
            for(int by=0;by<=size-bodySpaces;by++)for(int bx=0;bx<=size-bodySpaces;bx++){
                if(cx<bx+bodySpaces&&cx+carrierSpaces>bx&&cy<by+bodySpaces&&cy+carrierSpaces>by)continue;
                long bodyDistance=Distance2(bx,by,x,y);if(carrierDistance==bestCarrier&&bodyDistance>=bestBody)continue;
                if(!legal(cx,cy,bx,by))continue;
                found=true;bestCarrier=carrierDistance;bestBody=bodyDistance;result=new Plan{CarrierX=cx,CarrierY=cy,BodyX=bx,BodyY=by};
            }
        }return found;
    }
}
