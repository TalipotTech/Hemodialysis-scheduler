using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HDScheduler.API.Data;
using HDScheduler.API.Models;
using HDScheduler.API.Services;
using HDScheduler.API.DTOs;
using Microsoft.Data.SqlClient;
using Dapper;

namespace HDScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfigurationController : ControllerBase
{
    private readonly DapperContext _context;
    private readonly IBedFormatterService _bedFormatter;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(
        DapperContext context, 
        IBedFormatterService bedFormatter,
        ILogger<ConfigurationController> logger)
    {
        _context = context;
        _bedFormatter = bedFormatter;
        _logger = logger;
    }

    /// <summary>
    /// Get bed naming configuration
    /// </summary>
    [HttpGet("bed-naming")]
    public async Task<ActionResult<ApiResponse<BedNamingConfigDto>>> GetBedNamingConfiguration()
    {
        try
        {
            var config = await _bedFormatter.GetConfigurationAsync();
            
            var dto = new BedNamingConfigDto
            {
                Pattern = config.Pattern,
                Prefix = config.Prefix,
                BedsPerGroup = config.BedsPerGroup,
                CustomFormat = config.CustomFormat,
                AvailablePatterns = GetAvailablePatterns(),
                PreviewSamples = GeneratePreviewSamples(config)
            };

            return Ok(ApiResponse<BedNamingConfigDto>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bed naming configuration");
            return StatusCode(500, ApiResponse<BedNamingConfigDto>.ErrorResponse("Error retrieving configuration"));
        }
    }

    /// <summary>
    /// Update bed naming configuration
    /// </summary>
    [HttpPut("bed-naming")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateBedNamingConfiguration([FromBody] UpdateBedNamingConfigRequest request)
    {
        try
        {
            using var connection = _context.CreateConnection();

            // Update configuration values
            await connection.ExecuteAsync(@"
                UPDATE HospitalConfiguration 
                SET ConfigValue = @Value, UpdatedAt = @Now 
                WHERE ConfigKey = @Key",
                new[]
                {
                    new { Key = "BedNamingPattern", Value = request.Pattern, Now = DateTime.Now },
                    new { Key = "BedPrefix", Value = request.Prefix, Now = DateTime.Now },
                    new { Key = "BedsPerGroup", Value = request.BedsPerGroup.ToString(), Now = DateTime.Now },
                    new { Key = "BedCustomFormat", Value = request.CustomFormat, Now = DateTime.Now }
                });

            // Clear cache
            if (_bedFormatter is BedFormatterService service)
            {
                service.ClearCache();
            }

            _logger.LogInformation("✅ Bed naming configuration updated to pattern: {Pattern}", request.Pattern);

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Bed naming configuration updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating bed naming configuration");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error updating configuration"));
        }
    }

    /// <summary>
    /// Preview bed names with given configuration
    /// </summary>
    [HttpPost("bed-naming/preview")]
    public ActionResult<ApiResponse<List<string>>> PreviewBedNames([FromBody] BedNamingConfiguration config)
    {
        try
        {
            var samples = GeneratePreviewSamples(config);
            return Ok(ApiResponse<List<string>>.SuccessResponse(samples));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating preview");
            return StatusCode(500, ApiResponse<List<string>>.ErrorResponse("Error generating preview"));
        }
    }

    private List<BedPatternOption> GetAvailablePatterns()
    {
        return new List<BedPatternOption>
        {
            new BedPatternOption
            {
                Value = "NUMERIC",
                Label = "Numeric (1, 2, 3, 4...)",
                Description = "Simple numeric bed numbers",
                Example = "1, 2, 3, 4, 5, 6, 7, 8, 9, 10"
            },
            new BedPatternOption
            {
                Value = "PREFIXED_NUMERIC",
                Label = "Prefixed Numeric (Bed 1, Bed 2...)",
                Description = "Numeric with customizable prefix",
                Example = "Bed 1, Bed 2, Bed 3, Bed 4, Bed 5"
            },
            new BedPatternOption
            {
                Value = "ALPHA_NUMERIC",
                Label = "Alpha-Numeric (A1, A2, B1, B2...)",
                Description = "Letter groups with numbers (configurable beds per group)",
                Example = "A1, A2, A3, A4, A5, B1, B2, B3, B4, B5"
            },
            new BedPatternOption
            {
                Value = "ALPHABETIC",
                Label = "Alphabetic (A, B, C, D...)",
                Description = "Letter-only bed identifiers",
                Example = "A, B, C, D, E, F, G, H, I, J"
            },
            new BedPatternOption
            {
                Value = "CUSTOM",
                Label = "Custom Format",
                Description = "Define your own format using {n}=number, {a}=letter, {g}=group",
                Example = "Room-{n}, Ward-{a}{g}, ICU-{N}"
            }
        };
    }

    private List<string> GeneratePreviewSamples(BedNamingConfiguration config)
    {
        var samples = new List<string>();
        for (int i = 1; i <= 10; i++)
        {
            samples.Add(BedFormatterService.FormatBedNumber(i, config));
        }
        return samples;
    }
}

// DTOs
public class BedNamingConfigDto
{
    public string Pattern { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public int BedsPerGroup { get; set; }
    public string CustomFormat { get; set; } = string.Empty;
    public List<BedPatternOption> AvailablePatterns { get; set; } = new();
    public List<string> PreviewSamples { get; set; } = new();
}

public class BedPatternOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Example { get; set; } = string.Empty;
}

public class UpdateBedNamingConfigRequest
{
    public string Pattern { get; set; } = "NUMERIC";
    public string Prefix { get; set; } = "Bed";
    public int BedsPerGroup { get; set; } = 5;
    public string CustomFormat { get; set; } = "Bed {n}";
}
