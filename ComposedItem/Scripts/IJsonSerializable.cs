public interface IJsonSerializable
{
    public string Save();
    public void Load(string json);
}
