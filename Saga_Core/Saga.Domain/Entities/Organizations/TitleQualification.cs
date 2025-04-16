namespace Saga.Domain.Entities.Organizations;

[Table("tbttitlequalification", Schema = "Organization")]
public class TitleQualification : AuditTrail
{
    [Required]
    public Guid TitleKey { get; set; }

    [Required]
    public Guid EducationKey { get; set; }

    [Column("Skills", TypeName = "text")]
    public List<Guid> SkillKeys { get; set; } = new List<Guid>();

    [Column("Languages", TypeName = "text")]
    public List<Guid> LanguageKeys { get; set; } = new List<Guid>();

    [Required]
    public Guid PositionKey { get; set; }
    [Required]
    public int MinExperience { get; set; }

    [NotMapped]
    public Title? Title { get; set; }

    [NotMapped]
    public Education? Education { get; set; }

    [NotMapped]
    public Position? Position { get; set; }
}
