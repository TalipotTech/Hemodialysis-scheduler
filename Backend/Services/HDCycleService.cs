using HDScheduler.API.Models;

namespace HDScheduler.API.Services;

/// <summary>
/// Service for calculating next dialysis dates based on patient HD cycles
/// </summary>
public interface IHDCycleService
{
    /// <summary>
    /// Calculate the next dialysis date based on patient's HD cycle
    /// </summary>
    DateTime? CalculateNextDialysisDate(string hdCycle, DateTime lastSessionDate);
    
    /// <summary>
    /// Get all future dates for a patient based on their HD cycle (up to specified days ahead)
    /// </summary>
    List<DateTime> GetUpcomingDialysisDates(string hdCycle, DateTime lastSessionDate, int daysAhead = 30);
    
    /// <summary>
    /// Check if a patient should have dialysis on a specific date based on their HD cycle
    /// </summary>
    bool ShouldHaveDialysisOnDate(string hdCycle, DateTime sessionDate, DateTime hdStartDate);
    
    /// <summary>
    /// Parse HD cycle string and return number of days between sessions
    /// </summary>
    int? GetDaysBetweenSessions(string hdCycle);
}

public class HDCycleService : IHDCycleService
{
    public DateTime? CalculateNextDialysisDate(string hdCycle, DateTime lastSessionDate)
    {
        if (string.IsNullOrEmpty(hdCycle))
            return null;

        var cycle = hdCycle.Trim().ToLower();
        
        // Handle "Every X days" pattern
        if (cycle.Contains("every") && cycle.Contains("day"))
        {
            var daysBetween = ExtractNumberFromCycle(cycle);
            if (daysBetween.HasValue)
            {
                return lastSessionDate.AddDays(daysBetween.Value);
            }
        }
        
        // Handle "X times per week" or "X/week" pattern
        if (cycle.Contains("/week") || cycle.Contains("per week") || cycle.Contains("x/week"))
        {
            var sessionsPerWeek = ExtractNumberFromCycle(cycle);
            if (sessionsPerWeek.HasValue && sessionsPerWeek.Value > 0)
            {
                // Calculate average days between sessions
                int daysBetween = 7 / sessionsPerWeek.Value;
                return lastSessionDate.AddDays(daysBetween);
            }
        }
        
        // Handle specific day patterns (MWF, TTS, etc.)
        if (cycle.Contains("mwf") || cycle == "monday wednesday friday")
        {
            return GetNextMWFDate(lastSessionDate);
        }
        else if (cycle.Contains("tts") || cycle == "tuesday thursday saturday")
        {
            return GetNextTTSDate(lastSessionDate);
        }
        else if (cycle == "daily" || cycle == "everyday")
        {
            return lastSessionDate.AddDays(1);
        }
        else if (cycle.Contains("alternate") || cycle.Contains("every other day"))
        {
            return lastSessionDate.AddDays(2);
        }
        
        // Default: try to extract number of days
        var days = ExtractNumberFromCycle(cycle);
        if (days.HasValue)
        {
            return lastSessionDate.AddDays(days.Value);
        }
        
        return null;
    }

    public List<DateTime> GetUpcomingDialysisDates(string hdCycle, DateTime lastSessionDate, int daysAhead = 30)
    {
        var dates = new List<DateTime>();
        var currentDate = lastSessionDate;
        var endDate = DateTime.Today.AddDays(daysAhead);
        
        int maxIterations = 100; // Safety limit
        int iterations = 0;
        
        while (currentDate < endDate && iterations < maxIterations)
        {
            var nextDate = CalculateNextDialysisDate(hdCycle, currentDate);
            if (!nextDate.HasValue || nextDate.Value > endDate)
                break;
                
            dates.Add(nextDate.Value);
            currentDate = nextDate.Value;
            iterations++;
        }
        
        return dates;
    }

    public bool ShouldHaveDialysisOnDate(string hdCycle, DateTime sessionDate, DateTime hdStartDate)
    {
        if (string.IsNullOrEmpty(hdCycle))
            return false;

        var cycle = hdCycle.Trim().ToLower();
        
        // Daily cycles
        if (cycle == "daily" || cycle == "everyday")
        {
            return true;
        }
        
        // Alternate day cycle
        if (cycle.Contains("alternate") || cycle.Contains("every other day") || cycle == "every 2 days")
        {
            var daysDiff = (sessionDate.Date - hdStartDate.Date).Days;
            return daysDiff % 2 == 0;
        }
        
        // Every X days
        var daysBetween = GetDaysBetweenSessions(cycle);
        if (daysBetween.HasValue)
        {
            var daysDiff = (sessionDate.Date - hdStartDate.Date).Days;
            return daysDiff % daysBetween.Value == 0;
        }
        
        // Specific day patterns
        if (cycle.Contains("mwf"))
        {
            var dow = sessionDate.DayOfWeek;
            return dow == DayOfWeek.Monday || dow == DayOfWeek.Wednesday || dow == DayOfWeek.Friday;
        }
        else if (cycle.Contains("tts"))
        {
            var dow = sessionDate.DayOfWeek;
            return dow == DayOfWeek.Tuesday || dow == DayOfWeek.Thursday || dow == DayOfWeek.Saturday;
        }
        
        return false;
    }

    public int? GetDaysBetweenSessions(string hdCycle)
    {
        if (string.IsNullOrEmpty(hdCycle))
            return null;

        var cycle = hdCycle.Trim().ToLower();
        
        if (cycle == "daily" || cycle == "everyday")
            return 1;
            
        if (cycle.Contains("every") && cycle.Contains("day"))
            return ExtractNumberFromCycle(cycle);
            
        if (cycle.Contains("alternate") || cycle.Contains("every other day"))
            return 2;
            
        // Handle sessions per week (calculate average)
        if (cycle.Contains("/week") || cycle.Contains("per week"))
        {
            var sessionsPerWeek = ExtractNumberFromCycle(cycle);
            if (sessionsPerWeek.HasValue && sessionsPerWeek.Value > 0)
            {
                return 7 / sessionsPerWeek.Value;
            }
        }
        
        return ExtractNumberFromCycle(cycle);
    }

    private int? ExtractNumberFromCycle(string cycle)
    {
        // Extract first number from cycle string
        var numbers = System.Text.RegularExpressions.Regex.Matches(cycle, @"\d+");
        if (numbers.Count > 0 && int.TryParse(numbers[0].Value, out int result))
        {
            return result;
        }
        return null;
    }

    private DateTime GetNextMWFDate(DateTime lastDate)
    {
        // Monday, Wednesday, Friday cycle
        var nextDate = lastDate.AddDays(1);
        while (nextDate.DayOfWeek != DayOfWeek.Monday && 
               nextDate.DayOfWeek != DayOfWeek.Wednesday && 
               nextDate.DayOfWeek != DayOfWeek.Friday)
        {
            nextDate = nextDate.AddDays(1);
        }
        return nextDate;
    }

    private DateTime GetNextTTSDate(DateTime lastDate)
    {
        // Tuesday, Thursday, Saturday cycle
        var nextDate = lastDate.AddDays(1);
        while (nextDate.DayOfWeek != DayOfWeek.Tuesday && 
               nextDate.DayOfWeek != DayOfWeek.Thursday && 
               nextDate.DayOfWeek != DayOfWeek.Saturday)
        {
            nextDate = nextDate.AddDays(1);
        }
        return nextDate;
    }
}
