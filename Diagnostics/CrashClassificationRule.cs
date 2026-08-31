namespace AkengMauiCrashReporter.Diagnostics
{
    internal sealed class CrashClassificationRule
    {
        public CrashCategory Category { get; }

        public string Title { get; }

        public string Description { get; }

        public string? LikelyCause { get; }

        public string? Recommendation { get; }

        public bool IsReleaseSensitive { get; }

        public Func<string, string, string?, CrashRuleMatch?> Match { get; }

        public CrashClassificationRule(CrashCategory category, string title, string description, string? likelyCause, string? recommendation, bool isReleaseSensitive, Func<string, string, string?, CrashRuleMatch?> match)
        {
            Category = category;
            Title = title;
            Description = description;
            LikelyCause = likelyCause;
            Recommendation = recommendation;
            IsReleaseSensitive = isReleaseSensitive;
            Match = match;
        }
    }
}
