using System.ComponentModel.DataAnnotations;

public class SchedulePostDto
{
    [Required]
    public DateTime ScheduledAt { get; set; }
}