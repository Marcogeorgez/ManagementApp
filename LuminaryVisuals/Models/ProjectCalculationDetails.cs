namespace LuminaryVisuals.Models
{
    public class ProjectCalculationDetails
    {
        public decimal ClientDiscount { get; set; } = 0;
        public decimal? HighlightsDifficulty { get; set; }
        public decimal? PrePartsPrecentage { get; set; }
        public decimal? Resolution { get; set; }
        public decimal? FootageQuality { get; set; }
        public string? CameraNumber { get; set; } = "0";
        public decimal FootageSize { get; set; } = 0;
        public decimal Misc { get; set; } = 0;
        public string? PrePartsDuration { get; set; } = "0";
        public string? DocumentaryMulticameraDuration { get; set; } = "0";
        public string? DocumentaryMulticameraDurationHours { get; set; } = "0";
        public string? HighlightsDuration { get; set; } = "0";
        public string? SocialMediaDuration { get; set; } = "0";
        public decimal FinalProjectBillableHours { get; set; } = 0;

    }
}
