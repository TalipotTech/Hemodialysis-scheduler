namespace HDScheduler.API.DTOs;

public class SessionPhaseStatusDTO
{
    public string SessionPhase { get; set; } = "PRE_DIALYSIS";
    public bool IsPreDialysisLocked { get; set; }
    public bool IsIntraDialysisLocked { get; set; }
    public DateTime? PreDialysisCompletedAt { get; set; }
    public DateTime? IntraDialysisStartedAt { get; set; }
    public DateTime? PostDialysisStartedAt { get; set; }
}
