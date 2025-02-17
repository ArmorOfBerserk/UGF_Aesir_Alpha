[System.Serializable]
public class GameData
{
    public int level;
    public int currency;
    public int[] position;

    public GameData(int level, int currency, int[] position)
    {
        this.level = level;
        this.currency = currency;
        this.position = position;
    }
}
