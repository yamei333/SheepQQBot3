using System;

namespace SheepQQBot3.DbModel.JiebaDb;

public partial class Idf
{
    public Idf()
    { }

    public Idf(string word, decimal weight)
    {
        Word = word;
        Weight = weight;
    }
}