
namespace MyGameEnums
{
    // 게임에서 사용하는 tag들
    public enum TagName
    {
        Untagged,
        Player,
        Platform,
        SpawnPoint,
        DeadZone,
        Spike,
        Monster,
        Heal,
        Sword,
        Mace,
        CrumblePlatform,
        SavePoint
    }
    // 게임에 존재하는 씬
    public enum SceneName
    {
        TestScene,
        MainMenuScene,
        GameOverScene,
        OptionScene,
        TutorialStage,
        StageScene1,
        StageScene2,
        StageScene3,
        StageScene4,
        StageScene5
    }

    public static class TagNameToString
    {
        // 태그의 String형을 반환
        public static string GetTag(this TagName tagName)
        {
            return tagName switch
            {
                TagName.Untagged => "Untagged",
                TagName.Player => "Player",
                TagName.Platform => "Platform",
                TagName.SpawnPoint => "SpawnPoint",
                TagName.DeadZone => "DeadZone",
                TagName.Spike => "Spike",
                TagName.Monster => "Monster",
                TagName.Heal => "Heal",
                TagName.Sword => "Sword",
                TagName.Mace => "Mace",
                TagName.CrumblePlatform => "CrumblePlatform",
                TagName.SavePoint => "SavePoint",
                _ => LogAndReturn(tagName.ToString(), "Untagged")
            };
        }

        // 씬의 String형을 반환
        public static string GetScene(this SceneName scenesName)
        {
            return scenesName switch
            {
                SceneName.TestScene => "TestScene",
                SceneName.MainMenuScene => "MainMenuScene",
                SceneName.GameOverScene => "GameOverScene",
                SceneName.OptionScene => "OptionScene",
                SceneName.TutorialStage => "TutorialStage",
                SceneName.StageScene1 => "StageScene1",
                SceneName.StageScene2 => "StageScene2",
                SceneName.StageScene3 => "StageScene3",
                SceneName.StageScene4 => "StageScene4",
                SceneName.StageScene5 => "StageScene5",
                _ => LogAndReturn(scenesName.ToString(), "UnknownScene")
            };
        }

        // 로그를 출력하고 기본값을 반환하는 함수
        private static string LogAndReturn(string enumValue, string defaultValue)
        {
            UnityEngine.Debug.LogWarning($"[Enum Error] 정의되지 않은 Enum 값({enumValue})이 들어왔습니다. 기본값({defaultValue})을 반환합니다.");
            return defaultValue;
        }
    }
}
