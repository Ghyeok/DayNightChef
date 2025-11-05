using UnityEngine;

[DisallowMultipleComponent]
public class DeathNotifier : MonoBehaviour
{
    [HideInInspector] public Animal animal;
    [HideInInspector] public AnimalSpawner spawner;

    bool _isQuitting;

    void OnApplicationQuit() => _isQuitting = true;

    void OnDestroy()
    {
        if (_isQuitting) return;                 // 에디터 종료 중엔 무시
        if (!spawner || !animal) return;
        spawner.HandleAnimalDestroyed(animal);   // 스포너에 알림
    }
}