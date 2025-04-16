using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.ViewModels.Employees;

namespace Saga.Domain.ViewModels.Organizations;

public class TitleQualificationItem
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? TitleKey { get; set; } = Guid.Empty;
    public string? Code { get; set; } = String.Empty;
    public string? Name { get; set; } = String.Empty;
    public string? CompanyName { get; set; } = String.Empty;
}

public class TitleQualificationForm
{
    public Guid Key { get; set; }
    public Guid? TitleKey { get; set; } = Guid.Empty;
    public Guid? EducationKey { get; set; } = Guid.Empty;
    public List<Guid> SkillKeys { get; set; } = new List<Guid>();
    public List<Guid> LanguageKeys { get; set; } = new List<Guid>();
    public Guid? PositionKey { get; set; } = Guid.Empty;
    public int? MinExperience { get; set; } = 0;

    //properties for dropdown
    public List<SelectListItem> Titles { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Educations { get; set; } = new List<SelectListItem>();
    public MultiSelectList SkillTags { get; set; } = new MultiSelectList(new List<SelectListItem>());
    public MultiSelectList LanguageTags { get; set; } = new MultiSelectList(new List<SelectListItem>());
    public List<SelectListItem> Positions { get; set; } = new List<SelectListItem>();

    public TitleForm? Title { get; set; }
    public EducationForm? Education { get; set; }
    public PositionForm? Position { get; set; }

    //For displaying multiple select of Skills and Languages
    public IEnumerable<SkillForm>? Skills { get; set; } = Enumerable.Empty<SkillForm>();
    public IEnumerable<LanguageForm>? Languages { get; set; } = Enumerable.Empty<LanguageForm>();
}
