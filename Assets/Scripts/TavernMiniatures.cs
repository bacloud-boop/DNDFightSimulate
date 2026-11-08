using System.Collections.Generic;
using UnityEngine;

public sealed class TavernMiniatures : MonoBehaviour
{
    public RenderTexture Player {get;private set;}
    public RenderTexture Monster {get;private set;}
    readonly List<Object> owned=new List<Object>();
    readonly List<GameObject> roots=new List<GameObject>();
    Material Mat(Color color){var m=new Material(Resources.Load<Shader>("TavernMiniature"));m.color=color;owned.Add(m);return m;}
    void Part(Transform parent,PrimitiveType type,Vector3 position,Vector3 scale,Material material,Quaternion rotation=default(Quaternion)){
        var obj=GameObject.CreatePrimitive(type);obj.name="Miniature part";obj.layer=29;obj.transform.SetParent(parent,false);obj.transform.localPosition=position;obj.transform.localScale=scale;
        if(rotation!=default(Quaternion))obj.transform.localRotation=rotation;
        obj.GetComponent<Renderer>().sharedMaterial=material;Destroy(obj.GetComponent<Collider>());
    }
    public void Initialize(){Player=Create(false,600);Monster=Create(true,700);}
    RenderTexture Create(bool dummy,float depth)
    {
        var root=new GameObject(dummy?"Wooden dummy miniature":"Hero miniature");roots.Add(root);root.transform.SetParent(transform);root.transform.position=new Vector3(0,0,depth);
        var brass=Mat(new Color(.5f,.31f,.11f));var dark=Mat(new Color(.16f,.09f,.055f));var red=Mat(new Color(.45f,.07f,.045f));var steel=Mat(new Color(.52f,.51f,.44f));var wood=Mat(new Color(.38f,.20f,.08f));
        Part(root.transform,PrimitiveType.Cylinder,new Vector3(0,-.9f,0),new Vector3(1.9f,.12f,1.9f),brass);
        Part(root.transform,PrimitiveType.Cylinder,new Vector3(0,-.74f,0),new Vector3(1.65f,.06f,1.65f),dark);
        if(dummy){
            Part(root.transform,PrimitiveType.Cylinder,new Vector3(0,.1f,0),new Vector3(.55f,.9f,.55f),wood);
            Part(root.transform,PrimitiveType.Cylinder,new Vector3(0,.65f,0),new Vector3(.25f,.8f,.25f),wood,Quaternion.Euler(0,0,90));
            Part(root.transform,PrimitiveType.Sphere,new Vector3(0,1.28f,0),Vector3.one*.65f,wood);
            Part(root.transform,PrimitiveType.Cylinder,new Vector3(0,.1f,-.31f),new Vector3(.55f,.025f,.55f),red,Quaternion.Euler(90,0,0));
            Part(root.transform,PrimitiveType.Cylinder,new Vector3(0,.1f,-.35f),new Vector3(.28f,.025f,.28f),brass,Quaternion.Euler(90,0,0));
        }else{
            Part(root.transform,PrimitiveType.Capsule,new Vector3(-.22f,-.22f,0),new Vector3(.3f,.5f,.3f),dark);
            Part(root.transform,PrimitiveType.Capsule,new Vector3(.22f,-.22f,0),new Vector3(.3f,.5f,.3f),dark);
            Part(root.transform,PrimitiveType.Capsule,new Vector3(0,.5f,0),new Vector3(.85f,.65f,.5f),red);
            Part(root.transform,PrimitiveType.Sphere,new Vector3(0,1.28f,0),new Vector3(.6f,.65f,.6f),steel);
            Part(root.transform,PrimitiveType.Cube,new Vector3(0,1.28f,-.30f),new Vector3(.42f,.08f,.04f),dark);
            Part(root.transform,PrimitiveType.Capsule,new Vector3(-.54f,.53f,0),new Vector3(.27f,.48f,.28f),steel,Quaternion.Euler(0,0,-20));
            Part(root.transform,PrimitiveType.Cube,new Vector3(-.75f,.85f,-.15f),new Vector3(.10f,1.25f,.09f),steel,Quaternion.Euler(0,0,-12));
            Part(root.transform,PrimitiveType.Cube,new Vector3(-.65f,.26f,-.15f),new Vector3(.4f,.08f,.12f),brass);
            Part(root.transform,PrimitiveType.Cylinder,new Vector3(.56f,.48f,-.22f),new Vector3(.85f,.07f,.85f),brass,Quaternion.Euler(90,0,0));
            Part(root.transform,PrimitiveType.Cylinder,new Vector3(.56f,.48f,-.31f),new Vector3(.64f,.07f,.64f),red,Quaternion.Euler(90,0,0));
        }
        var camera=new GameObject("Miniature camera").AddComponent<Camera>();camera.transform.SetParent(root.transform,false);camera.transform.localPosition=new Vector3(3,2.1f,-5);camera.transform.LookAt(root.transform.position+Vector3.up*.3f);
        camera.farClipPlane=20;camera.orthographic=true;camera.orthographicSize=1.65f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.cullingMask=1<<29;
        var texture=new RenderTexture(400,400,24);texture.antiAliasing=4;camera.targetTexture=texture;owned.Add(texture);
        var light=new GameObject("Miniature light").AddComponent<Light>();light.transform.SetParent(root.transform,false);light.type=LightType.Directional;light.transform.localRotation=Quaternion.Euler(30,-30,0);light.intensity=1.2f;light.cullingMask=1<<29;
        return texture;
    }
    void OnDestroy(){foreach(var root in roots)if(root)Destroy(root);foreach(var obj in owned){if(obj is RenderTexture rt)rt.Release();if(obj)Destroy(obj);}}
}
