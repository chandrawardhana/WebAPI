using Saga.Domain.Entities.Organizations;
using Saga.Domain.Enums;

namespace Saga.DomainShared.Interfaces;

public interface IDocumentGenerator
{
    Task<byte[]> GenerateDocumentAsync<T>(string entityName, T model, DocumentGeneratorFormat format);
    Task<string> GetCompanyPolicyReportHTML<T>(T model);
    Task<byte[]> GenerateCompanyPolicyReportPDF(string htmlContent);
    Task<string> GetCurriculumVitaeReportHTML<T>(T model);
    Task<byte[]> GenerateCurriculumVitaeReportPDF(string htmlContent);
    Task<byte[]> GenerateAttendanceDailyReportXlsx<T>(T model);
    Task<byte[]> GenerateAttendanceWeeklyReportXlsx<T>(T model);
    Task<byte[]> GenerateAttendanceMonthlyReportXlsx<T>(T model);
    Task<byte[]> GenerateAttendanceRecapitulationReportXlsx<T>(T model);
    Task<byte[]> GenerateAttendanceLateDetailReportXlsx<T>(T model);
    Task<byte[]> GenerateAttendanceLeaveDetailReportXlsx<T>(T model);
    Task<byte[]> GenerateAttendanceEarlyOutDetailReportXlsx<T>(T model);
    Task<byte[]> GenerateAttendanceOutPermitDetailReportXlsx<T>(T model);
    Task<byte[]> GenerateShiftScheduleDetailReportXlsx<T>(T model);
    Task<byte[]> GenerateOvertimeLetterDetailReportXlsx<T>(T model);
}
