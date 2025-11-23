using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("프리팹 할당")]
    [SerializeField]
    private GameObject loadingScreenPrefab; // 1. 프리팹을 참조
    private UI_LoadingCanvas loadingScreenInstance; // 2. 생성된 인스턴스를 저장

    public enum LoadType
    {
        NewGame,
        Continue,
    }

    public LoadType CurrentLoadType = LoadType.NewGame;
    public void SetLoadType(LoadType loadType) {  CurrentLoadType = loadType; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 3. 프리팹을 씬에 생성
            if (loadingScreenPrefab != null)
            {
                GameObject instance = Instantiate(loadingScreenPrefab);
                loadingScreenInstance = instance.GetComponent<UI_LoadingCanvas>();

                // 4. 생성된 로딩 스크린도 파괴되지 않도록 설정
                DontDestroyOnLoad(instance);

                // 5. 생성 직후에는 숨김
                instance.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName, LoadSceneMode mode, Action onLoadComplete = null)
    {
        if (loadingScreenInstance == null)
        {
            Debug.LogError("LoadingScreenPrefab이 할당되지 않았습니다!");
            return;
        }
        StartCoroutine(LoadSceneCoroutine(sceneName, mode, onLoadComplete));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, LoadSceneMode mode, Action onLoadComplete)
    {
        //  1. 로딩창 켜기
        loadingScreenInstance.gameObject.SetActive(true);
        loadingScreenInstance.SetProgress(0); // LoadingScreenUI 스크립트의 함수 호출

        // 2. 새 씬 로드 시작
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, mode);
        op.allowSceneActivation = false;

        // 3. 로딩 진행률 로직
        float timer = 0f;
        while (!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;

            float progressValue = 0f;
            if (op.progress < 0.9f)
            {
                progressValue = Mathf.Clamp01(op.progress / 0.9f);
            }
            else
            {
                // 100%로 부드럽게 채우기
                progressValue = Mathf.Lerp(loadingScreenInstance.GetComponentInChildren<Slider>().value, 1f, timer * 0.5f);

                if (progressValue >= 0.99f)
                {
                    op.allowSceneActivation = true;
                    yield return new WaitForSeconds(0.5f);
                    break;
                }
            }

            loadingScreenInstance.SetProgress(progressValue);
        }

        // 4. 로드 완료 처리
        if (mode == LoadSceneMode.Additive)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }
        onLoadComplete?.Invoke();

        // 5. 로딩창 끄기
        loadingScreenInstance.gameObject.SetActive(false);
    }
}