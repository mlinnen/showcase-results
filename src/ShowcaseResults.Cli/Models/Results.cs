namespace ShowcaseResults.Models;

public record EventInfo(string Name, int Year);

public record PlaceEntry(int Place, int CarverId, string Winner, int EntryNumber);

public record SpecialPrize(int Order, string Name, int CarverId, string Winner, int EntryNumber, string Prize);

public record OverallCategory(string Category, List<PlaceEntry> Places);

public record DivisionCategory(string Name, string? Style, List<PlaceEntry> Places);

public record DivisionResult(string Division, List<DivisionCategory> Categories);

public record Competitor(int CarverId, string FirstName, string LastName);

public record ShowcaseResultsData(
    EventInfo Event,
    List<SpecialPrize> SpecialPrizes,
    List<OverallCategory> OverallResults,
    List<DivisionResult> DivisionResults,
    List<Competitor> Competitors);
