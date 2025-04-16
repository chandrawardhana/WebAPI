namespace Saga.Domain.ViewModels.Employees;

public class EmployeeLanguageList
{
    public IEnumerable<EmployeeLanguage> EmployeeLanguages { get; set; } = Enumerable.Empty<EmployeeLanguage>();
}

public class EmployeeLanguageForm
{
    public Guid Key { get; set; }
    public Guid EmployeeKey { get; set; }
    public Guid? LanguageKey { get; set; } = Guid.Empty;
    public Level? SpeakLevel { get; set; } = null;
    public Level? ListenLevel { get; set; } = null;
    public Employee? Employee { get; set; }
    public Language? Language { get; set; }

    //Additional No, string LanguageName, SpeakLevel Text, and ListenLevel Text for array JsonEmployeeLanguages
    public int? No { get; set; } = 0;
    public string? LanguageName { get; set; } = String.Empty;
    public string? SpeakLevelName { get; set; } = String.Empty;
    public string? ListenLevelName { get; set; } = String.Empty;
}
