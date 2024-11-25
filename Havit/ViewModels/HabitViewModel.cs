public class HabitViewModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string Frequency { get; set; } = "daily";
     public int TimesComplete { get; set; } = 0;
    public DateTime? LastCompletedAt { get; set; }
    public IFormFile? ImageFile { get; set; }

    public bool IsCompletedToday()
    {
        if (LastCompletedAt == null) return false;
        
        return Frequency switch
        {
            "daily" => LastCompletedAt?.Date == DateTime.Today,
            "weekly" => LastCompletedAt?.Date >= DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek),
            "monthly" => LastCompletedAt?.Month == DateTime.Today.Month && LastCompletedAt?.Year == DateTime.Today.Year,
            _ => false
        };
    }
}