using HDScheduler.API.Data;
using Microsoft.Data.SqlClient;
using Dapper;

namespace HDScheduler.API.Services;

public interface IBedFormatterService
{
    Task<string> FormatBedNumberAsync(int bedNumber);
    Task<BedNamingConfiguration> GetConfigurationAsync();
}

public class BedFormatterService : IBedFormatterService
{
    private readonly DapperContext _context;
    private readonly ILogger<BedFormatterService> _logger;
    private BedNamingConfiguration? _cachedConfig;
    private DateTime _cacheExpiry = DateTime.MinValue;

    public BedFormatterService(DapperContext context, ILogger<BedFormatterService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<BedNamingConfiguration> GetConfigurationAsync()
    {
        // Cache for 5 minutes to avoid constant DB queries
        if (_cachedConfig != null && DateTime.Now < _cacheExpiry)
        {
            return _cachedConfig;
        }

        using var connection = _context.CreateConnection();
        var configs = await connection.QueryAsync<ConfigItem>(
            "SELECT ConfigKey, ConfigValue FROM HospitalConfiguration WHERE ConfigKey LIKE 'Bed%'");

        var configDict = configs.ToDictionary(c => c.ConfigKey, c => c.ConfigValue);

        _cachedConfig = new BedNamingConfiguration
        {
            Pattern = configDict.GetValueOrDefault("BedNamingPattern", "NUMERIC"),
            Prefix = configDict.GetValueOrDefault("BedPrefix", "Bed"),
            BedsPerGroup = int.TryParse(configDict.GetValueOrDefault("BedsPerGroup", "5"), out var bpg) ? bpg : 5,
            CustomFormat = configDict.GetValueOrDefault("BedCustomFormat", "Bed {n}")
        };

        _cacheExpiry = DateTime.Now.AddMinutes(5);
        return _cachedConfig;
    }

    public async Task<string> FormatBedNumberAsync(int bedNumber)
    {
        var config = await GetConfigurationAsync();
        return FormatBedNumber(bedNumber, config);
    }

    public static string FormatBedNumber(int bedNumber, BedNamingConfiguration config)
    {
        return config.Pattern switch
        {
            "NUMERIC" => bedNumber.ToString(),
            "PREFIXED_NUMERIC" => $"{config.Prefix} {bedNumber}",
            "ALPHA_NUMERIC" => FormatAlphaNumeric(bedNumber, config.BedsPerGroup),
            "ALPHABETIC" => FormatAlphabetic(bedNumber),
            "CUSTOM" => ApplyCustomFormat(bedNumber, config),
            _ => $"Bed {bedNumber}"
        };
    }

    private static string FormatAlphaNumeric(int bedNumber, int bedsPerGroup)
    {
        // Example: bedsPerGroup=5 → 1-5=A1-A5, 6-10=B1-B5, 11-15=C1-C5
        char letter = (char)(65 + ((bedNumber - 1) / bedsPerGroup)); // A, B, C...
        int number = ((bedNumber - 1) % bedsPerGroup) + 1;
        return $"{letter}{number}";
    }

    private static string FormatAlphabetic(int bedNumber)
    {
        // Simple: 1=A, 2=B, 3=C...
        if (bedNumber <= 26)
        {
            return ((char)(64 + bedNumber)).ToString();
        }
        // For > 26: 27=AA, 28=AB, etc.
        int first = (bedNumber - 1) / 26;
        int second = (bedNumber - 1) % 26;
        return $"{(char)(65 + first)}{(char)(65 + second)}";
    }

    private static string ApplyCustomFormat(int bedNumber, BedNamingConfiguration config)
    {
        var format = config.CustomFormat;
        
        // If format is just a single letter or ends with a letter, auto-append bed number
        // Examples: "W" -> "W1", "ICU-D" -> "ICU-D1", "A" -> "A1"
        if (!string.IsNullOrEmpty(format) && !format.Contains("{"))
        {
            // Check if format ends with a letter (case-insensitive)
            char lastChar = format[format.Length - 1];
            if (char.IsLetter(lastChar))
            {
                return $"{format}{bedNumber}";
            }
        }
        
        // Replace placeholders
        format = format.Replace("{n}", bedNumber.ToString());
        format = format.Replace("{N}", bedNumber.ToString("D2")); // Zero-padded
        
        // Letter placeholder
        char letter = (char)(65 + ((bedNumber - 1) / config.BedsPerGroup));
        format = format.Replace("{a}", letter.ToString());
        format = format.Replace("{A}", letter.ToString());
        
        // Group number
        int groupNum = ((bedNumber - 1) % config.BedsPerGroup) + 1;
        format = format.Replace("{g}", groupNum.ToString());
        
        return format;
    }

    public void ClearCache()
    {
        _cachedConfig = null;
        _cacheExpiry = DateTime.MinValue;
    }
}

public class BedNamingConfiguration
{
    public string Pattern { get; set; } = "NUMERIC";
    public string Prefix { get; set; } = "Bed";
    public int BedsPerGroup { get; set; } = 5;
    public string CustomFormat { get; set; } = "Bed {n}";
}

public class ConfigItem
{
    public string ConfigKey { get; set; } = string.Empty;
    public string ConfigValue { get; set; } = string.Empty;
}
