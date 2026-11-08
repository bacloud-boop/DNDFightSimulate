using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Isolated render-texture stage: all dice faces are real meshes, not flat UI icons.
public sealed class TavernDice : MonoBehaviour
{
    public RenderTexture Texture { get; private set; }
    readonly List<GameObject> dice = new List<GameObject>();
    readonly List<Vector3[]> normals = new List<Vector3[]>();
    readonly List<Object> owned = new List<Object>();
    Transform stage;
    Font font;
    public void Initialize(Font textFont)
    {
        font=textFont;
        stage=new GameObject("Dice stage").transform; stage.SetParent(transform); stage.position=new Vector3(0,0,500);
        var camera= new GameObject("Dice camera").AddComponent<Camera>(); camera.transform.SetParent(stage,false);
        camera.transform.localPosition=new Vector3(0,0,-8); camera.clearFlags=CameraClearFlags.SolidColor;
        camera.backgroundColor=Color.clear; camera.cullingMask=1<<30; camera.orthographic=true; camera.orthographicSize=2.5f;
        Texture=new RenderTexture(960,500,24); Texture.antiAliasing=4; camera.targetTexture=Texture;
        var light=new GameObject("Dice light").AddComponent<Light>(); light.transform.SetParent(stage,false);
        light.type=LightType.Directional; light.transform.localRotation=Quaternion.Euler(35,-30,0); light.intensity=1.4f; light.cullingMask=1<<30;
        Show(20,new[]{20});
    }
    Material Material(Color color) { var m=new Material(Resources.Load<Shader>("TavernMiniature"));m.color=color;owned.Add(m);return m; }
    Vector3[] Vertices(int sides)
    {
        var points=new List<Vector3>();
        if(sides==4) points.AddRange(new[]{new Vector3(1,1,1),new Vector3(-1,-1,1),new Vector3(-1,1,-1),new Vector3(1,-1,-1)});
        else if(sides==6) {for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)points.Add(new Vector3(x,y,z));}
        else if(sides==8) points.AddRange(new[]{Vector3.up,Vector3.down,Vector3.left,Vector3.right,Vector3.forward,Vector3.back});
        else if(sides==10) { points.Add(Vector3.up*1.3f);points.Add(Vector3.down*1.3f);for(int i=0;i<5;i++){float a=i*Mathf.PI*2/5;points.Add(new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)));} }
        else {
            float p=(1+Mathf.Sqrt(5))/2;
            for(int a=-1;a<=1;a+=2)for(int b=-1;b<=1;b+=2){points.Add(new Vector3(0,a,b*p));points.Add(new Vector3(a,b*p,0));points.Add(new Vector3(b*p,0,a));}
            if(sides==12) {
                var source=points.ToArray(); var faces=Faces(source);points.Clear();
                foreach(var face in faces){Vector3 center=Vector3.zero;foreach(int i in face)center+=source[i];points.Add(center.normalized);}
            }
        }
        for(int i=0;i<points.Count;i++) points[i]=points[i].normalized;
        return points.ToArray();
    }
    static List<int[]> Faces(Vector3[] v)
    {
        var faces=new List<int[]>();var keys=new HashSet<string>();
        for(int a=0;a<v.Length;a++)for(int b=a+1;b<v.Length;b++)for(int c=b+1;c<v.Length;c++) {
            Vector3 n=Vector3.Cross(v[b]-v[a],v[c]-v[a]);if(n.sqrMagnitude<.00001f)continue;n.Normalize();
            float d=Vector3.Dot(n,v[a]);bool pos=false,neg=false;var indices=new List<int>();
            for(int i=0;i<v.Length;i++){float t=Vector3.Dot(n,v[i])-d;if(t>.001f)pos=true;else if(t<-.001f)neg=true;else indices.Add(i);}
            if(pos&&neg)continue;string key=string.Join(",",indices);if(!keys.Add(key))continue;
            Vector3 center=Vector3.zero;foreach(int i in indices)center+=v[i];center/=indices.Count;n=center.normalized;
            Vector3 u=(v[indices[0]]-center).normalized, w=Vector3.Cross(n,u);
            indices.Sort((i,j)=>Mathf.Atan2(Vector3.Dot(v[i]-center,w),Vector3.Dot(v[i]-center,u)).CompareTo(Mathf.Atan2(Vector3.Dot(v[j]-center,w),Vector3.Dot(v[j]-center,u))));
            faces.Add(indices.ToArray());
        }
        return faces;
    }
    void Show(int sides,int[] values)
    {
        foreach(var die in dice)Destroy(die);dice.Clear();normals.Clear();
        foreach(var asset in owned)Destroy(asset);owned.Clear();
        for(int k=0;k<values.Length;k++) {
            var die=new GameObject("D"+sides);die.transform.SetParent(stage,false);die.layer=30;
            die.transform.localPosition=new Vector3((k-(values.Length-1)/2f)*2.5f,0,0);
            var v=Vertices(sides);var faces=Faces(v);var meshVertices=new List<Vector3>();var triangles=new List<int>();
            var faceNormals=new Vector3[faces.Count];
            for(int f=0;f<faces.Count;f++) {
                var face=faces[f];Vector3 center=Vector3.zero;foreach(int i in face)center+=v[i];center/=face.Length;
                faceNormals[f]=center.normalized;int start=meshVertices.Count;
                foreach(int i in face)meshVertices.Add(v[i]);for(int i=1;i<face.Length-1;i++){triangles.Add(start);triangles.Add(start+i);triangles.Add(start+i+1);}
                var number=new GameObject("Face "+(f+1));number.layer=30;number.transform.SetParent(die.transform,false);
                number.transform.localPosition=center+faceNormals[f]*.016f;
                number.transform.localRotation=Quaternion.LookRotation(-faceNormals[f]);
                var text=number.AddComponent<TextMesh>();text.text=(f+1).ToString();text.font=font;text.fontSize=64;text.characterSize=(sides>=12?.10f:.15f)*.25f;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=new Color(1,.86f,.49f);
                number.GetComponent<MeshRenderer>().sharedMaterial=font.material;
            }
            var mesh=new Mesh();mesh.SetVertices(meshVertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();owned.Add(mesh);
            die.AddComponent<MeshFilter>().sharedMesh=mesh;die.AddComponent<MeshRenderer>().sharedMaterial=Material(new Color(.35f,.055f,.045f));
            dice.Add(die);normals.Add(faceNormals);
            die.transform.localRotation=Quaternion.FromToRotation(faceNormals[values[k]-1],Vector3.back);
        }
    }
    public IEnumerator Roll(int sides,int[] results,float duration,System.Action tick)
    {
        Show(sides,results);var ends=new Quaternion[dice.Count];for(int i=0;i<dice.Count;i++)ends[i]=dice[i].transform.localRotation;
        float elapsed=0,nextTick=0;
        while(elapsed<duration) {
            elapsed+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(elapsed/duration);
            for(int i=0;i<dice.Count;i++) {
                var die=dice[i].transform;float bounce=Mathf.Abs(Mathf.Sin(t*Mathf.PI*4))*(1-t)*.8f;
                die.localPosition=new Vector3((i-(dice.Count-1)/2f)*2.5f+Mathf.Sin(t*12+i)*(1-t),bounce,0);
                die.localRotation=ends[i]*Quaternion.Euler((1-t)*1080,(1-t)*720,(1-t)*360);
            }
            if(elapsed>=nextTick){tick?.Invoke();nextTick+=duration/5;}
            yield return null;
        }
        for(int i=0;i<dice.Count;i++){dice[i].transform.localRotation=ends[i];dice[i].transform.localPosition=new Vector3((i-(dice.Count-1)/2f)*2.5f,0,0);}
    }
    void OnDestroy(){foreach(var asset in owned)if(asset)Destroy(asset);if(stage)Destroy(stage.gameObject);if(Texture){Texture.Release();Destroy(Texture);}}
}
