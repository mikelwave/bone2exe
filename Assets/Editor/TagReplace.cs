using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class TagReplace {
    static TagReplace()
    {
        EditorSceneManager.sceneOpened += Replace;
    }
    static void Replace(Scene scene, OpenSceneMode mode)
    {
        
        //GameObject[] objs = GameObject.FindGameObjectsWithTag("MainMap");
        /*
        GameObject[] objs = GameObject.FindGameObjectsWithTag("SSMap");
        if(objs.Length != 0)
        {
            foreach(GameObject obj in objs)
            {
                CompositeCollider2D composite = obj.GetComponent<CompositeCollider2D>();
                composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
                composite.offsetDistance = 0.00005f;

                //objs[0].GetComponent<PlatformEffector2D>().surfaceArc = 175;

                //if(obj.name == "MainMap") continue;
                Transform t = obj.transform;

                //Debug.Log("Old tag of object "+t.name+": "+t.tag);
                //t.tag = "Untagged";
                //Debug.Log("New tag of object "+t.name+": "+t.tag);
                EditorSceneManager.MarkSceneDirty(scene);
            }
        }*/
    }
}