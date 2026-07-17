namespace DSOCompanion.Models;

public sealed class VarnokPlan
{
    public List<VarnokItem> Items { get; set; } = [];

    public int DreamRequired => Items.Sum(x => x.DreamRequired);
    public int DreamOwned => Items.Sum(x => x.DreamOwned);
    public int DreamMissing => Items.Sum(x => x.DreamMissing);

    public int StarRequired => Items.Sum(x => x.StarRequired);
    public int StarOwned => Items.Sum(x => x.StarOwned);
    public int StarMissing => Items.Sum(x => x.StarMissing);

    public int CompletedItems => Items.Count(x => x.IsComplete);
    public int TotalItems => Items.Count;

    public double Progress
    {
        get
        {
            int required = DreamRequired + StarRequired;
            if (required <= 0)
                return 100;

            int counted =
                Items.Sum(x => Math.Min(x.DreamOwned, x.DreamRequired)) +
                Items.Sum(x => Math.Min(x.StarOwned, x.StarRequired));

            return Math.Min(100, counted * 100.0 / required);
        }
    }

    public VarnokItem? NextPriority =>
        Items
            .Where(x => !x.IsComplete)
            .OrderByDescending(x => x.Progress)
            .ThenBy(x => x.DreamMissing + x.StarMissing)
            .FirstOrDefault();
}
