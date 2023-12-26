namespace CommonLibrary
{
    //public static class NSFWExtensions
    //{
    //    private static readonly NsfwSpy _nsfwSpy = new NsfwSpy();
    //    [Obsolete("Obsolete")]
    //    private static readonly WebClient _webClient = new WebClient();

    //    public static NFSWResult CheckImage(string imagePath)
    //        => new(_nsfwSpy.ClassifyImage(imagePath));

    //    public static NFSWResult CheckWebImage(string url)
    //        => new(_nsfwSpy.ClassifyImage(new Uri(url), _webClient));
    //}

    //public class NFSWResult
    //{
    //    public float Pornography { get; }
    //    public float Sexy { get; }
    //    public float Hentai { get; }
    //    public string PornographyPercent { get; }
    //    public string SexyPercent { get; }
    //    public string HentaiPercent { get; }
    //    public bool IsNsfw { get; }
    //    public string PredictedLabel { get; }

    //    public NFSWResult(NsfwSpyResult nsfwSpyResult)
    //    {
    //        Pornography = nsfwSpyResult.Pornography;
    //        Sexy = nsfwSpyResult.Sexy;
    //        Hentai = nsfwSpyResult.Hentai;
    //        PredictedLabel = nsfwSpyResult.PredictedLabel;
    //        PornographyPercent = Pornography.ToString("#0.##%");
    //        SexyPercent = Sexy.ToString("#0.##%");
    //        HentaiPercent = Hentai.ToString("#0.##%");
    //        IsNsfw = nsfwSpyResult.IsNsfw;
    //    }
    //}
}