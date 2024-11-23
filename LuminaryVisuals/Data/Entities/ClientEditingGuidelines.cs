using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuminaryVisuals.Data.Entities
{
    public class ClientEditingGuidelines
    {
        [Key]
        public int Id { get; set; } 
        [Required]
        public string UserId { get; set; } 

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } 
        public string? WebsiteLink { get; set; }
        public string? VideoStructure { get; set; } 
        public string? CrossFades { get; set; } 
        public string? FadeToBlack { get; set; } 
        public string? BlackAndWhite { get; set; } 
        public string? DoubleExposure { get; set; } 
        public string? MaskingTransitions { get; set; } 
        public string? LensFlares { get; set; } 
        public string? OldFilmLook { get; set; } 
        public string? PictureInPicture { get; set; }
        public string? OtherTransitions { get; set; } 
        public string? TransitionComments { get; set; } 
        public string? UseSpeeches { get; set; } 
        public string? SpeechComments { get; set; }
        public string? SoundDesignEmphasis { get; set; } 
        public string? SoundDesignComments { get; set; } 
        public string? musicLicensingSites { get; set; }
        public string? SongSamples { get; set; }
        public string? ColorReferences { get; set; } 
        public string? FilmReferences { get; set; } 
        public string? ClientSamples { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
