using UnityEngine;

// Code-native heraldic silhouettes, kept as transparent textures for crisp scaled UI.
public static class HeraldryArt
{
    sealed class Canvas
    {
        public readonly Texture2D Texture=new Texture2D(256,256,TextureFormat.RGBA32,false);
        readonly Color gold=new Color(.94f,.72f,.34f);
        readonly Color shade=new Color(.45f,.25f,.09f);
        public Canvas(){Texture.SetPixels(new Color[256*256]);Texture.wrapMode=TextureWrapMode.Clamp;}
        void Pixel(int x,int y,Color color){if(x>=0&&x<256&&y>=0&&y<256)Texture.SetPixel(x,255-y,color);}
        public void Line(float ax,float ay,float bx,float by,float width=8){
            var a=new Vector2(ax,ay);var b=new Vector2(bx,by);var ab=b-a;
            for(int y=Mathf.Max(0,(int)(Mathf.Min(ay,by)-width));y<Mathf.Min(256,Mathf.Max(ay,by)+width+1);y++)
            for(int x=Mathf.Max(0,(int)(Mathf.Min(ax,bx)-width));x<Mathf.Min(256,Mathf.Max(ax,bx)+width+1);x++){
                float t=ab.sqrMagnitude==0?0:Mathf.Clamp01(Vector2.Dot(new Vector2(x,y)-a,ab)/ab.sqrMagnitude);
                if(Vector2.Distance(new Vector2(x,y),a+t*ab)<=width/2)Pixel(x,y,gold);
            }
        }
        public void Ring(float cx,float cy,float radius,float width=7){for(int y=0;y<256;y++)for(int x=0;x<256;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(cx,cy));if(Mathf.Abs(d-radius)<=width/2)Pixel(x,y,gold);}}
        public void Polygon(params Vector2[] v){
            for(int y=0;y<256;y++)for(int x=0;x<256;x++){
                bool inside=false;for(int i=0,j=v.Length-1;i<v.Length;j=i++)if((v[i].y>y)!=(v[j].y>y)&&x<(v[j].x-v[i].x)*(y-v[i].y)/(v[j].y-v[i].y)+v[i].x)inside=!inside;
                if(inside)Pixel(x,y,x<128?gold:Color.Lerp(gold,shade,.3f));
            }
        }
        public void Sword(float x=128){Polygon(new Vector2(x,22),new Vector2(x+12,48),new Vector2(x+9,166),new Vector2(x-9,166),new Vector2(x-12,48));Line(x-34,172,x+34,172,10);Line(x,176,x,218,13);Ring(x,224,9,7);}
        public Texture2D Finish(){Texture.Apply();return Texture;}
    }
    public static Texture2D Create(int role)
    {
        var c=new Canvas();
        switch(role){
            case 0: // Sword and shield
                c.Polygon(new Vector2(42,95),new Vector2(113,77),new Vector2(174,95),new Vector2(165,176),new Vector2(108,225),new Vector2(50,176));
                c.Line(108,99,108,191,6);c.Line(70,131,144,131,6);c.Sword(184);break;
            case 1: // Spellbook and staff
                c.Line(191,61,191,223,11);c.Ring(191,39,20,9);
                c.Polygon(new Vector2(27,105),new Vector2(85,115),new Vector2(85,221),new Vector2(27,210));
                c.Polygon(new Vector2(96,115),new Vector2(158,103),new Vector2(158,209),new Vector2(96,221));
                c.Line(90,118,90,222,4);for(int y=132;y<192;y+=22){c.Line(42,y,69,y+4,4);c.Line(111,y,141,y-5,4);}break;
            case 2: // Bow, string and quiver
                for(int i=0;i<30;i++){float a=-1.15f+i*2.3f/30,b=-1.15f+(i+1)*2.3f/30;c.Line(54+95*Mathf.Cos(a),128+99*Mathf.Sin(a),54+95*Mathf.Cos(b),128+99*Mathf.Sin(b),9);}
                c.Line(93,37,93,219,3);c.Line(64,128,165,128,5);c.Line(165,128,153,118,5);c.Line(165,128,153,138,5);
                c.Polygon(new Vector2(178,106),new Vector2(216,106),new Vector2(216,224),new Vector2(178,224));for(int x=181;x<=213;x+=16){c.Line(x,47,x,108,4);c.Line(x-6,43,x+5,59,5);}break;
            case 3: // Crystal
                c.Polygon(new Vector2(128,24),new Vector2(179,79),new Vector2(167,175),new Vector2(128,222),new Vector2(88,175),new Vector2(76,79));
                c.Line(76,79,179,79,3);c.Line(102,79,128,222,3);c.Line(153,79,128,222,3);c.Ring(128,132,95,3);break;
            case 4: // Pact scroll and seal
                c.Line(49,61,198,61,24);c.Line(49,199,198,199,24);c.Line(65,61,65,199,7);c.Line(188,61,188,199,7);
                c.Line(87,93,162,93,5);c.Line(87,117,154,117,5);c.Line(87,140,144,140,5);c.Ring(151,174,17,9);c.Line(140,189,133,219,9);c.Line(160,189,168,219,9);break;
            case 5: // Lute
                c.Polygon(new Vector2(165,27),new Vector2(183,34),new Vector2(119,145),new Vector2(99,132));c.Ring(91,164,47,27);c.Ring(91,164,14,4);
                c.Line(91,164,172,38,3);c.Line(98,171,180,42,3);c.Line(78,194,107,202,7);c.Line(162,42,189,55,5);c.Line(153,57,179,70,5);break;
            case 6: // Holy symbol
                c.Ring(128,128,70,8);for(int i=0;i<12;i++){float a=i*Mathf.PI/6;c.Line(128+82*Mathf.Cos(a),128+82*Mathf.Sin(a),128+101*Mathf.Cos(a),128+101*Mathf.Sin(a),6);}
                c.Line(128,74,128,190,17);c.Line(90,110,166,110,17);break;
            case 7: // Dagger
                c.Polygon(new Vector2(128,48),new Vector2(147,89),new Vector2(139,154),new Vector2(117,154),new Vector2(109,89));c.Line(91,161,165,161,10);c.Line(128,168,128,216,16);c.Ring(128,224,10,7);break;
            case 8: // Prayer beads
                for(int i=0;i<14;i++){float a=i*Mathf.PI*2/14;c.Ring(128+63*Mathf.Cos(a),108+67*Mathf.Sin(a),11,10);}c.Line(128,176,128,200,6);c.Ring(128,198,8,7);for(int i=-2;i<=2;i++)c.Line(128+i*4,207,128+i*8,239,4);break;
            case 9: // Holy sword
                c.Ring(128,77,48,5);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;c.Line(128+58*Mathf.Cos(a),77+58*Mathf.Sin(a),128+72*Mathf.Cos(a),77+72*Mathf.Sin(a),4);}c.Sword();break;
            case 10: // Double-headed axe
                c.Line(128,34,128,230,15);c.Polygon(new Vector2(115,70),new Vector2(55,38),new Vector2(35,65),new Vector2(35,123),new Vector2(59,147),new Vector2(115,114));
                c.Polygon(new Vector2(141,70),new Vector2(201,38),new Vector2(221,65),new Vector2(221,123),new Vector2(197,147),new Vector2(141,114));break;
            case 11: // Druid oak staff and leaf
                c.Line(109,48,109,232,12);c.Ring(109,51,28,9);
                c.Polygon(new Vector2(123,132),new Vector2(149,72),new Vector2(208,48),new Vector2(196,111),new Vector2(158,146));
                c.Line(125,139,195,64,5);c.Line(146,115,143,91,4);c.Line(163,96,184,99,4);break;
            default: // Wooden target
                c.Line(128,34,128,236,17);c.Line(41,102,216,102,18);c.Ring(128,134,66,12);c.Ring(128,134,40,9);c.Ring(128,134,12,18);break;
        }
        return c.Finish();
    }
    public static Texture2D Gem(){var c=new Canvas();c.Polygon(new Vector2(128,10),new Vector2(222,79),new Vector2(205,183),new Vector2(128,246),new Vector2(51,183),new Vector2(34,79));c.Line(34,79,222,79,5);c.Line(34,79,128,246,5);c.Line(222,79,128,246,5);return c.Finish();}
}
