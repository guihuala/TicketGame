using UnityEngine;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Game/Schedule/LevelDatabase")]
public class LevelDatabase : ScriptableObject
{
    public DaySchedule[] levels;
}
