using System;

namespace SheepQQBot3.DbModel.JiebaDb;

public partial class StopWord
{
    public StopWord()
    { }

    public StopWord(string word)
    {
        Word = word ?? throw new ArgumentNullException(nameof(word));
    }
}