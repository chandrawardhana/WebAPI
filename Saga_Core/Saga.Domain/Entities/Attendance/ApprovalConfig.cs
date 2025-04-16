
using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance
{
    [Table("tbmapprovalconfig", Schema = "Attendance")]
    public class ApprovalConfig : AuditTrail
    {
        [Required]
        public Guid OrganizationKey { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        [NotMapped]
        public Organization? Organization { get; set; }
        [NotMapped]
        public IEnumerable<Approver>? Approvers { get; set; }

        public ApprovalConfigListItem ConvertToListItem(Company company)
        {
            return new ApprovalConfigListItem
            {
                Key = this.Key,
                OrganizationKey = this.OrganizationKey,
                CompanyCode = company.Code,
                CompanyName = company.Name,
                OrganizationName = this.Organization.Name,
                Description = this.Description,
                Organization = this.Organization
            };
        }

        public ApprovalConfigForm ConvertToForm()
        {
            return new ApprovalConfigForm
            {
                Key = this.Key,
                OrganizationKey = this.OrganizationKey,
                Description = this.Description,
                Organization = this.Organization,
                Approvers = this.Approvers?.Select(x => x.ConvertToApproverListItem())
            };
        }
    }
}
