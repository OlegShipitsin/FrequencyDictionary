namespace FrequencyDictionary.DictionaryBuilder;

public interface IDictionaryBuilder
{
    Dictionary<string, int> GetDictionary(Stream inStream);
}