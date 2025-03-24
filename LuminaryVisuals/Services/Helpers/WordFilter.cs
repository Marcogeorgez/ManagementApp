using LuminaryVisuals.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace LuminaryVisuals.Services.Helpers;

public class WordFilter
{
    private readonly IDbContextFactory<ApplicationDbContext> contextFactory;
    private readonly IMemoryCache _cache;
    private readonly string _cacheKey = "BlockedWords";
    private readonly TimeSpan _cacheDuration = TimeSpan.FromDays(7);
    private readonly Regex _wordSplitter;

    public WordFilter(IDbContextFactory<ApplicationDbContext> contextFactory, IMemoryCache memoryCache)
    {
        this.contextFactory = contextFactory;
        _cache = memoryCache;
        _wordSplitter = new Regex(@"[^\s]+", RegexOptions.Compiled);

        // Ensure the cache is populated
        GetBlockedWordsFromCacheOrDb();
    }

    private HashSet<string> GetBlockedWordsFromCacheOrDb()
    {
        if (!_cache.TryGetValue(_cacheKey, out HashSet<string> blockedWords))
        {
            using var context = contextFactory.CreateDbContext();
            // Cache miss, load from database
            blockedWords = new HashSet<string>(
                context.BlockedWords.Select(w => w.Word),
                StringComparer.OrdinalIgnoreCase
            );

            // Store in cache
            _cache.Set(_cacheKey, blockedWords, _cacheDuration);
        }

        return blockedWords;
    }

    public bool IsMessageAllowed(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return true;

        var blockedWords = GetBlockedWordsFromCacheOrDb();

        if (blockedWords.Count == 0)
            return true;

        // Use the compiled regex to extract words with word boundaries
        var matches = _wordSplitter.Matches(message);

        // For short messages, check each word directly
        if (matches.Count <= 10)
        {
            foreach (Match match in matches)
            {
                if (blockedWords.Contains(match.Value))
                {
                    return false;
                }
            }
            return true;
        }

        // For longer messages, use more efficient set operations
        var messageWords = new HashSet<string>(
            matches.Cast<Match>().Select(m => m.Value),
            StringComparer.OrdinalIgnoreCase);

        bool isAllowed = !messageWords.Overlaps(blockedWords);

        return isAllowed;
    }

    public void InvalidateCache()
    {
        _cache.Remove(_cacheKey);
        Console.WriteLine("Cache is invalidated.");
    }
}
