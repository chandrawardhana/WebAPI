namespace Saga.Domain.Dtos.Employees;

public class EmployeeLanguageDto
{
    public Guid? Key { get; set; }
    public Guid EmployeeKey { get; set; }
    public Guid? LanguageKey { get; set; } = Guid.Empty;
    public Level SpeakLevel { get; set; }
    public Level ListenLevel { get; set; }

    public EmployeeLanguage ConvertToEntity()
    {
        return new EmployeeLanguage
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey,
            LanguageKey = this.LanguageKey ?? Guid.Empty,
            SpeakLevel = this.SpeakLevel,
            ListenLevel = this.ListenLevel
        };
    }
}
