namespace Saga.Domain.Dtos.Organizations;

public class TitleQualificationDto
{
    public Guid? Key { get; set; }
    public Guid? TitleKey { get; set; } = Guid.Empty;
    public Guid? EducationKey { get; set; } = Guid.Empty;
    public List<Guid> SkillKeys { get; set; } = new List<Guid>();
    public List<Guid> LanguageKeys { get; set; } = new List<Guid>();
    public Guid? PositionKey { get; set; } = Guid.Empty;
    public int? MinExperience { get; set; } = 0;
}
