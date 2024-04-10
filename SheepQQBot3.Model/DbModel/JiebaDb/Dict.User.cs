namespace SheepQQBot3.DbModel.JiebaDb;

public partial class Dict
{
    public Dict()
    { }

    public Dict(string word)
        : this(word, 100, null)
    { }

    public Dict(string word, int freq, string tag, bool isDefault = false)
    {
        Word = word;
        Freq = freq;
        Tag = tag;
        IsDefault = isDefault ? 1 : 0;
    }
}