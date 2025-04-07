using UnityEngine;


/* 게임 전체에서 공통적으로 사용할 enum 타입들 모아놓음
 * 일종의 중앙관리 시스템이 되어 규모가 커질수록 유용하다
 *  ex) UIType, SoundType, SceneType...
 * 
 *  특정 클래스에서만 사용되는 enum 타입은 그와 관련된 매니저에서 선언
 */
public class Define
{
    public enum UIEvent
    {
        Click,
        Drag,
        DragEnd,
    }

    public enum UIType
    {
        Button,
        Image,
        Text,
    }

    public enum TouchEvent
    {
        Press,
        Click,
    }

    public enum CameraMode
    {
        Day,
        Night,
    }

    public enum Sounds
    {
        BGM,
        SFX,
        MaxCount,
    }

    // enum 추가....
}
