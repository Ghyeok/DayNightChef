using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ResourceManager
{
    public T Load<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }//Resource폴더를 시작 위치로한 path에 해당하는 T타입의 에셋 파일을 불러오고 리턴
    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject prefab = Load<GameObject>($"Prefabs/{path}");
        if(prefab == null)
        {
            Debug.Log($"Filed to load prefab : {path}");
            if (prefab == null)
            {
                Debug.Log($"Filed to load prefab : {path}");
                return null;
            }
        }
        return Object.Instantiate(prefab, parent);
    }//Load를 사용해 prefab 에 path에 해당하는 gameobject타입의 에셋을 할당

    public void Destory(GameObject go)
    {
        if(go == null)
            return;
        Object.Destroy(go);
    }
}
