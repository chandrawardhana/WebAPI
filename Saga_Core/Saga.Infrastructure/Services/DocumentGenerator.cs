using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Saga.Domain.Enums;
using Saga.DomainShared.Interfaces;
using System.Data;
using ClosedXML.Excel;
using Saga.Domain.ViewModels.Employees;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Saga.Domain.ViewModels.Organizations;
using DinkToPdf.Contracts;
using DinkToPdf;
using DocumentFormat.OpenXml.Spreadsheet;
using Saga.Domain.Entities.Organizations;
using System.Reflection;
using System.Text;
using Saga.Domain.ViewModels.Attendances;
using Saga.Domain.Entities.Attendance;
using DocumentFormat.OpenXml.Wordprocessing;
using MediatR;
using Saga.Mediator.Attendances.CutOffMediator;
using Saga.DomainShared.Helpers;
namespace Saga.Infrastructure.Services;

public class DocumentGenerator(
    ICompositeViewEngine _viewEngine,
    ITempDataProvider _tempDataProvider,
    IServiceProvider _serviceProvider,
    ILogger<DocumentGenerator> _logger,
    IWebHostEnvironment _webHostEnvironment,
    IConverter _converter,
    IRazorRendererHelper _razorRenderer,
    IMediator _mediator
) : IDocumentGenerator
{
    private static string basePathFastReportTemplates = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
            "Resources",
            "FastReportTemplates"
        );
    
    public async Task<byte[]> GenerateDocumentAsync<T>(string entityName, T model, DocumentGeneratorFormat format)
    {
        try
        {
            return format switch
            {
                DocumentGeneratorFormat.Pdf => await GeneratePdf(entityName, model),
                DocumentGeneratorFormat.Xlsx => GenerateXlsx(model),
                _ => throw new ArgumentException("Unsupported document format", nameof(format))
            };
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private async Task<byte[]> GeneratePdf<T>(string entityName, T model)
    {
        try
        {
            // Initialize FastReport WebReport
            var webReport = new FastReport.Web.WebReport();

            // Load report template
            var templatePath = Path.Combine(basePathFastReportTemplates, $"{entityName}Report.frx");
            if (!File.Exists(templatePath))
            {
                _logger.LogError("Report template not found at {TemplatePath}", templatePath);
                throw new FileNotFoundException($"Report template not found: {templatePath}");
            }

            _logger.LogInformation("Loading report template from {TemplatePath}", templatePath);
            webReport.Report.Load(templatePath);

            // Handle different model types
            if (model is CompanyPolicyReport companyPolicyReport)
            {
                HandleCompanyPolicyReport(webReport, companyPolicyReport);
            } else if (model is CurriculumVitaeReport curriculumVitaeReport)
            {
                HandleCurriculumVitaeReport(webReport, curriculumVitaeReport);
            }
            else
            {
                _logger.LogWarning("Unsupported model type: {Type}", typeof(T).Name);
                throw new ArgumentException($"Unsupported model type: {typeof(T).Name}");
            }

            // Export to PDF
            using var ms = new MemoryStream();
            webReport.Report.Export(new FastReport.Export.PdfSimple.PDFSimpleExport(), ms);
            _logger.LogInformation("PDF export completed. Generated size: {Size} bytes", ms.Length);

            return ms.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF: {Error}", ex.Message);
            throw;
        }
    }

    private byte[] GenerateXlsx<T>(T model)
    {
        if (model is EmployeeReport employeeReportData)
        {
            return GenerateEmployeeReportXlsx(employeeReportData);
        }

        // Fallback to existing logic for other types
        var dataTable = ConvertModelToDataTable(model);
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Sheet1");
            worksheet.Cell(1, 1).InsertTable(dataTable);

            using (var memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }

    private DataTable ConvertModelToDataTable<T>(T model)
    {
        var dataTable = new DataTable();

        //Add coloumns based on model properties
        var properties = typeof(T).GetProperties();
        foreach (var property in properties)
        {
            dataTable.Columns.Add(property.Name, property.PropertyType);
        }

        //Add row with model data
        var row = dataTable.NewRow();
        foreach (var property in properties)
        {
            row[property.Name] = property.GetValue(model) ?? DBNull.Value;
        }
        dataTable.Rows.Add(row);

        return dataTable;
    }

    private byte[] GenerateEmployeeReportXlsx(EmployeeReport reportData)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Employee Report");

            int currentRow = 1;

            // Employee Basic Information
            worksheet.Range(currentRow, 1, currentRow, 21).Merge();
            worksheet.Cell(currentRow, 1).Value = "Employee Information";
            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(currentRow, 1).Style.Font.SetFontSize(12).Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            currentRow++;

            // Basic Employee Headers
            var basicHeaders = new Dictionary<int, string>
            {
                {1, "Employee Code"}, {2, "Employee Name"}, {3, "Company"}, {4, "Branch"},
                {5, "Organization"}, {6, "Position"}, {7, "Title"}, {8, "Grade"},
                {9, "Status"}, {10, "Direct Supervisor"}, {11, "Tax Number"},
                {12, "Tax Registered"}, {13, "Tax Status"}, {14, "BPJS Health Number"},
                {15, "BPJS Health Registered"}, {16, "BPJS Labor Number"},
                {17, "BPJS Labor Registered"}, {18, "BPJS Pension Number"},
                {19, "BPJS Pension Registered"}, {20, "Age"}, {21, "Long Of Join"}
            };

            foreach (var header in basicHeaders)
            {
                worksheet.Cell(currentRow, header.Key).Value = header.Value;
                worksheet.Cell(currentRow, header.Key).Style.Font.SetFontSize(12).Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            }
            currentRow++;

            // Employee Basic Data
            foreach (var item in reportData.EmployeesData)
            {
                worksheet.Cell(currentRow, 1).Value = item.Code;
                worksheet.Cell(currentRow, 2).Value = $"{item.FirstName ?? ""} {item.LastName ?? ""}".Trim();
                worksheet.Cell(currentRow, 3).Value = item.Company?.Name;
                worksheet.Cell(currentRow, 4).Value = item.Branch?.Name;
                worksheet.Cell(currentRow, 5).Value = item.Organization?.Name;
                worksheet.Cell(currentRow, 6).Value = item.Position?.Name;
                worksheet.Cell(currentRow, 7).Value = item.Title?.Name;
                worksheet.Cell(currentRow, 8).Value = item.Grade?.Name;
                worksheet.Cell(currentRow, 9).Value = item.Status.ToString();
                worksheet.Cell(currentRow, 10).Value = $"{item.DirectSupervisor?.FirstName} {item.DirectSupervisor?.LastName}".Trim();
                worksheet.Cell(currentRow, 11).Value = item.EmployeePayroll?.TaxNumber;
                worksheet.Cell(currentRow, 12).Value = item.EmployeePayroll.TaxRegistered.HasValue ? Convert.ToDateTime(item.EmployeePayroll.TaxRegistered).ToString("dd/MM/yyyy") : "";
                worksheet.Cell(currentRow, 13).Value = item.EmployeePayroll?.TaxStatus.ToString();
                worksheet.Cell(currentRow, 14).Value = item.EmployeePayroll?.HealthNationalityInsuranceNumber;
                worksheet.Cell(currentRow, 15).Value = item.EmployeePayroll.HealthNationalityInsuranceRegistered.HasValue ? Convert.ToDateTime(item.EmployeePayroll.HealthNationalityInsuranceRegistered).ToString("dd/MM/yyyy") : "";
                worksheet.Cell(currentRow, 16).Value = item.EmployeePayroll?.LaborNationalityInsuranceNumber;
                worksheet.Cell(currentRow, 17).Value = item.EmployeePayroll.LaborNationalityInsuranceRegistered.HasValue ? Convert.ToDateTime(item.EmployeePayroll.LaborNationalityInsuranceRegistered).ToString("dd/MM/yyyy") : "";
                worksheet.Cell(currentRow, 18).Value = item.EmployeePayroll?.PensionNationalityInsuranceNumber;
                worksheet.Cell(currentRow, 19).Value = item.EmployeePayroll.PensionNationalityInsuranceRegistered.HasValue ? Convert.ToDateTime(item.EmployeePayroll.PensionNationalityInsuranceRegistered).ToString("dd/MM/yyyy") : "";
                worksheet.Cell(currentRow, 20).Value = item.Age.ToString();
                worksheet.Cell(currentRow, 21).Value = item.LongOfJoin.ToString();
                currentRow++;
            }

            currentRow += 2; // Add space between sections

            // Employee Personal Information
            worksheet.Range(currentRow, 1, currentRow, 20).Merge();
            worksheet.Cell(currentRow, 1).Value = "Personal Information";
            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(currentRow, 1).Style.Font.SetFontSize(12).Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            currentRow++;

            var personalHeaders = new Dictionary<int, string>
            {
                {1, "Employee Code"}, {2, "Nationality Number"}, {3, "Place of Birth"},
                {4, "Date of Birth"}, {5, "Gender"}, {6, "Religion"}, {7, "Marital Status"},
                {8, "Address"}, {9, "Country"}, {10, "Province"}, {11, "City"},
                {12, "Postal Code"}, {13, "Phone Number"}, {14, "Email"},
                {15, "Social Media"}, {16, "Ethnic"}, {17, "Weight"},
                {18, "Height"}, {19, "Blood"}, {20, "Number of Children"}
            };

            foreach (var header in personalHeaders)
            {
                worksheet.Cell(currentRow, header.Key).Value = header.Value;
                worksheet.Cell(currentRow, header.Key).Style.Font.SetFontSize(12).Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            }
            currentRow++;

            foreach (var item in reportData.EmployeesData)
            {
                if (item.EmployeePersonal != null)
                {
                    worksheet.Cell(currentRow, 1).Value = item.Code;
                    worksheet.Cell(currentRow, 2).Value = item.EmployeePersonal.NationalityNumber;
                    worksheet.Cell(currentRow, 3).Value = item.EmployeePersonal.PlaceOfBirth;
                    worksheet.Cell(currentRow, 4).Value = item.EmployeePersonal.DateOfBirth?.ToString("dd/MM/yyyy");
                    worksheet.Cell(currentRow, 5).Value = item.EmployeePersonal.Gender.ToString();
                    worksheet.Cell(currentRow, 6).Value = item.EmployeePersonal.Religion?.Name;
                    worksheet.Cell(currentRow, 7).Value = item.EmployeePersonal.MaritalStatus.ToString();
                    worksheet.Cell(currentRow, 8).Value = item.EmployeePersonal.Address;
                    worksheet.Cell(currentRow, 9).Value = item.EmployeePersonal.Country?.Name;
                    worksheet.Cell(currentRow, 10).Value = item.EmployeePersonal.Province?.Name;
                    worksheet.Cell(currentRow, 11).Value = item.EmployeePersonal.City?.Name;
                    worksheet.Cell(currentRow, 12).Value = item.EmployeePersonal.PostalCode;
                    worksheet.Cell(currentRow, 13).Value = item.EmployeePersonal.PhoneNumber;
                    worksheet.Cell(currentRow, 14).Value = item.EmployeePersonal.Email;
                    worksheet.Cell(currentRow, 15).Value = item.EmployeePersonal.SocialMedia;
                    worksheet.Cell(currentRow, 16).Value = item.EmployeePersonal.Ethnic?.Name;
                    worksheet.Cell(currentRow, 17).Value = item.EmployeePersonal.Weight;
                    worksheet.Cell(currentRow, 18).Value = item.EmployeePersonal.Height;
                    worksheet.Cell(currentRow, 19).Value = item.EmployeePersonal.Blood.ToString();
                    worksheet.Cell(currentRow, 20).Value = item.EmployeePersonal.NumOfChild;
                    currentRow++;
                }
            }

            currentRow += 2;

            // Employee Education
            worksheet.Range(currentRow, 1, currentRow, 5).Merge();
            worksheet.Cell(currentRow, 1).Value = "Education Information";
            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(currentRow, 1).Style.Font.SetFontSize(12).Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            currentRow++;

            var educationHeaders = new Dictionary<int, string>
            {
                {1, "Employee Code"}, {2, "Education Level"}, {3, "Graduated Year"},
                {4, "Score"}, {5, "Is Certificated"}
            };

            foreach (var header in educationHeaders)
            {
                worksheet.Cell(currentRow, header.Key).Value = header.Value;
                worksheet.Cell(currentRow, header.Key).Style.Font.SetFontSize(12).Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            }
            currentRow++;

            foreach (var item in reportData.EmployeesData)
            {
                foreach (var edu in item.EmployeeEducations)
                {
                    worksheet.Cell(currentRow, 1).Value = item.Code;
                    worksheet.Cell(currentRow, 2).Value = edu.Education?.Name;
                    worksheet.Cell(currentRow, 3).Value = edu.GraduatedYear;
                    worksheet.Cell(currentRow, 4).Value = edu.Score;
                    worksheet.Cell(currentRow, 5).Value = edu.IsCertificated.ToString();
                    currentRow++;
                }
            }

            currentRow += 2;

            // Employee Experience
            worksheet.Range(currentRow, 1, currentRow, 5).Merge();
            worksheet.Cell(currentRow, 1).Value = "Experience Information";
            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(currentRow, 1).Style.Font.SetFontSize(12).Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            currentRow++;

            var experienceHeaders = new Dictionary<int, string>
            {
                {1, "Employee Code"}, {2, "Company Name"}, {3, "Position"},
                {4, "Year Start"}, {5, "Year End"}
            };

            foreach (var header in experienceHeaders)
            {
                worksheet.Cell(currentRow, header.Key).Value = header.Value;
                worksheet.Cell(currentRow, header.Key).Style.Font.SetFontSize(12).Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            }
            currentRow++;

            foreach (var item in reportData.EmployeesData)
            {
                foreach (var exp in item.EmployeeExperiences)
                {
                    worksheet.Cell(currentRow, 1).Value = item.Code;
                    worksheet.Cell(currentRow, 2).Value = exp.CompanyName;
                    worksheet.Cell(currentRow, 3).Value = exp.Position?.Name;
                    worksheet.Cell(currentRow, 4).Value = exp.YearStart;
                    worksheet.Cell(currentRow, 5).Value = exp.YearEnd;
                    currentRow++;
                }
            }

            currentRow += 2;

            // Employee Family
            worksheet.Range(currentRow, 1, currentRow, 6).Merge();
            worksheet.Cell(currentRow, 1).Value = "Family Information";
            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(currentRow, 1).Style.Font.SetFontSize(12).Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            currentRow++;

            var familyHeaders = new Dictionary<int, string>
            {
                {1, "Employee Code"}, {2, "Name"}, {3, "Gender"},
                {4, "Date of Birth"}, {5, "Relationship"}, {6, "Phone Number"}
            };

            foreach (var header in familyHeaders)
            {
                worksheet.Cell(currentRow, header.Key).Value = header.Value;
                worksheet.Cell(currentRow, header.Key).Style.Font.SetFontSize(12).Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            }
            currentRow++;

            foreach (var item in reportData.EmployeesData)
            {
                foreach (var family in item.EmployeeFamilies)
                {
                    worksheet.Cell(currentRow, 1).Value = item.Code;
                    worksheet.Cell(currentRow, 2).Value = family.Name;
                    worksheet.Cell(currentRow, 3).Value = family.GenderName;
                    worksheet.Cell(currentRow, 4).Value = family.BoD?.ToString("dd/MM/yyyy");
                    worksheet.Cell(currentRow, 5).Value = family.RelationshipName;
                    worksheet.Cell(currentRow, 6).Value = family.PhoneNumber;
                    currentRow++;
                }
            }

            currentRow += 2;

            // Employee Hobby
            worksheet.Range(currentRow, 1, currentRow, 4).Merge();
            worksheet.Cell(currentRow, 1).Value = "Hobby Information";
            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(currentRow, 1).Style.Font.SetFontSize(12).Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            currentRow++;

            var hobbyHeaders = new Dictionary<int, string>
            {
                {1, "Employee Code"}, {2, "Hobby"}, {3, "Level"}, {4, "Description"}
            };

            foreach (var header in hobbyHeaders)
            {
                worksheet.Cell(currentRow, header.Key).Value = header.Value;
                worksheet.Cell(currentRow, header.Key).Style.Font.SetFontSize(12).Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            }
            currentRow++;

            foreach (var item in reportData.EmployeesData)
            {
                foreach (var hobby in item.EmployeeHobbies)
                {
                    worksheet.Cell(currentRow, 1).Value = item.Code;
                    worksheet.Cell(currentRow, 2).Value = hobby.Hobby?.Name;
                    worksheet.Cell(currentRow, 3).Value = hobby.LevelName;
                    worksheet.Cell(currentRow, 4).Value = hobby.Hobby?.Description;
                    currentRow++;
                }
            }

            currentRow += 2;

            // Employee Language
            worksheet.Range(currentRow, 1, currentRow, 4).Merge();
            worksheet.Cell(currentRow, 1).Value = "Language Information";
            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(currentRow, 1).Style.Font.SetFontSize(12).Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            currentRow++;

            var languageHeaders = new Dictionary<int, string>
            {
                {1, "Employee Code"}, {2, "Language"}, {3, "Speaking Level"},
                {4, "Listening Level"}
            };

            foreach (var header in languageHeaders)
            {
                worksheet.Cell(currentRow, header.Key).Value = header.Value;
                worksheet.Cell(currentRow, header.Key).Style.Font.SetFontSize(12).Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            }
            currentRow++;

            foreach (var item in reportData.EmployeesData)
            {
                foreach (var lang in item.EmployeeLanguages)
                {
                    worksheet.Cell(currentRow, 1).Value = item.Code;
                    worksheet.Cell(currentRow, 2).Value = lang.Language?.Name;
                    worksheet.Cell(currentRow, 3).Value = lang.SpeakLevelName;
                    worksheet.Cell(currentRow, 4).Value = lang.ListenLevelName;
                    currentRow++;
                }
            }

            currentRow += 2;

            // Employee Skills
            worksheet.Range(currentRow, 1, currentRow, 4).Merge();
            worksheet.Cell(currentRow, 1).Value = "Skills Information";
            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(currentRow, 1).Style.Font.SetFontSize(12).Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            currentRow++;

            var skillHeaders = new Dictionary<int, string>
            {
                {1, "Employee Code"}, {2, "Skill"}, {3, "Level"},
                {4, "Is Certificated"}
            };

            foreach (var header in skillHeaders)
            {
                worksheet.Cell(currentRow, header.Key).Value = header.Value;
                worksheet.Cell(currentRow, header.Key).Style.Font.SetFontSize(12).Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            }
            currentRow++;

            foreach (var item in reportData.EmployeesData)
            {
                foreach (var skill in item.EmployeeSkills)
                {
                    worksheet.Cell(currentRow, 1).Value = item.Code;
                    worksheet.Cell(currentRow, 2).Value = skill.Skill?.Name;
                    worksheet.Cell(currentRow, 3).Value = skill.LevelName;
                    worksheet.Cell(currentRow, 4).Value = skill.IsCertificated.ToString();
                }
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using (var memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }

    private void HandleCompanyPolicyReport(FastReport.Web.WebReport webReport, CompanyPolicyReport model)
    {
        try
        {
            _logger.LogInformation("Starting HandleCompanyPolicyReport with {Count} policies",
                model.CompanyPolicies?.Count() ?? 0);

            if (model.CompanyPolicies != null && model.CompanyPolicies.Any())
            {
                // Create a new DataSet
                var dataSet = new System.Data.DataSet("CompanyPoliciesData");

                // Create DataTable with proper structure
                var dataTable = new System.Data.DataTable("CompanyPolicies");

                // Define columns explicitly
                dataTable.Columns.AddRange(new System.Data.DataColumn[]
                {
                    new System.Data.DataColumn("CompanyName", typeof(string)),
                    new System.Data.DataColumn("OrganizationName", typeof(string)),
                    new System.Data.DataColumn("EffectiveDate", typeof(string)),
                    new System.Data.DataColumn("ExpiredDate", typeof(string)),
                    new System.Data.DataColumn("Policy", typeof(string))
                });

                _logger.LogInformation("Created DataTable with {Count} columns", dataTable.Columns.Count);

                // Add data rows
                foreach (var policy in model.CompanyPolicies)
                {
                    var row = dataTable.NewRow();
                    row["CompanyName"] = policy.CompanyName ?? string.Empty;
                    row["OrganizationName"] = policy.OrganizationName ?? string.Empty;

                    var culture = new System.Globalization.CultureInfo("id-ID");
                    row["EffectiveDate"] = policy.EffectiveDate.HasValue
                        ? policy.EffectiveDate.Value.ToString("dd MMMM yyyy", culture)
                        : string.Empty;
                    row["ExpiredDate"] = policy.ExpiredDate.HasValue
                        ? policy.ExpiredDate.Value.ToString("dd MMMM yyyy", culture)
                        : string.Empty;

                    row["Policy"] = policy.Policy?.Replace("\r\n", "\n").Replace("\r", "\n") ?? string.Empty;

                    dataTable.Rows.Add(row);
                }

                _logger.LogInformation("Added {Count} rows to DataTable", dataTable.Rows.Count);

                // Add the DataTable to DataSet
                dataSet.Tables.Add(dataTable);

                // Clear any existing data sources
                webReport.Report.Dictionary.Clear();

                // Register the data with FastReport
                webReport.Report.RegisterData(dataSet);

                // Configure data source
                var dataSource = webReport.Report.GetDataSource("CompanyPolicies");
                if (dataSource != null)
                {
                    dataSource.Enabled = true;

                    // Ensure the data source is properly bound to the table
                    var tableDataSource = (FastReport.Data.TableDataSource)dataSource;
                    tableDataSource.Table = dataSet.Tables["CompanyPolicies"];
                }
                else
                {
                    _logger.LogError("Failed to get data source 'CompanyPolicies' from report");
                    throw new InvalidOperationException("Data source 'CompanyPolicies' not found in report");
                }

                // Configure the report for multiple pages
                var dataBand = webReport.Report.FindObject("Data1") as FastReport.DataBand;
                if (dataBand != null)
                {
                    // Set the band to start new page for each record
                    dataBand.StartNewPage = true;

                    // Ensure the data band is properly bound
                    dataBand.DataSource = dataSource;
                }

                webReport.Width = "100%";

                webReport.Report.Prepare();

            }
            else
            {
                _logger.LogWarning("No company policies data available for the report");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HandleCompanyPolicyReport: {Error}", ex.Message);
            throw;
        }
    }

    private void HandleCurriculumVitaeReport(FastReport.Web.WebReport webReport, CurriculumVitaeReport model)
    {
        try
        {
            _logger.LogInformation("Starting HandleCurriculumVitaeReport with {Count} employees",
                model.CurriculumVitae?.Count() ?? 0);

            if (model.CurriculumVitae != null && model.CurriculumVitae.Any())
            {
                // Create DataSet to hold employee information
                var ds = new System.Data.DataSet("ReportData");
                var dt = new System.Data.DataTable("Employees");

                // Define columns for employee data
                dt.Columns.AddRange(new System.Data.DataColumn[]
                {
                new System.Data.DataColumn("FullName", typeof(string)),
                new System.Data.DataColumn("Position", typeof(string)),
                new System.Data.DataColumn("PlaceOfBirth", typeof(string)),
                new System.Data.DataColumn("Gender", typeof(string)),
                new System.Data.DataColumn("Religion", typeof(string)),
                new System.Data.DataColumn("MaritalStatus", typeof(string)),
                new System.Data.DataColumn("Weight", typeof(decimal)),
                new System.Data.DataColumn("Height", typeof(decimal)),
                new System.Data.DataColumn("PhoneNumber", typeof(string)),
                new System.Data.DataColumn("SocialMedia", typeof(string)),
                new System.Data.DataColumn("Email", typeof(string)),
                new System.Data.DataColumn("Address", typeof(string)),
                new System.Data.DataColumn("City", typeof(string)),
                new System.Data.DataColumn("Province", typeof(string)),
                new System.Data.DataColumn("Country", typeof(string)),
                new System.Data.DataColumn("PostalCode", typeof(string)),
                new System.Data.DataColumn("Skills", typeof(string)),
                new System.Data.DataColumn("Languages", typeof(string)),
                new System.Data.DataColumn("Hobbies", typeof(string)),
                new System.Data.DataColumn("ImagePath", typeof(string)),
                // Education columns (4 sets)
                new System.Data.DataColumn("Education1", typeof(string)),
                new System.Data.DataColumn("GraduatedYear1", typeof(int)),
                new System.Data.DataColumn("Score1", typeof(decimal)),
                new System.Data.DataColumn("Education2", typeof(string)),
                new System.Data.DataColumn("GraduatedYear2", typeof(int)),
                new System.Data.DataColumn("Score2", typeof(decimal)),
                new System.Data.DataColumn("Education3", typeof(string)),
                new System.Data.DataColumn("GraduatedYear3", typeof(int)),
                new System.Data.DataColumn("Score3", typeof(decimal)),
                new System.Data.DataColumn("Education4", typeof(string)),
                new System.Data.DataColumn("GraduatedYear4", typeof(int)),
                new System.Data.DataColumn("Score4", typeof(decimal)),
                // Experience columns (4 sets)
                new System.Data.DataColumn("Position1", typeof(string)),
                new System.Data.DataColumn("Company1", typeof(string)),
                new System.Data.DataColumn("YearStart1", typeof(int)),
                new System.Data.DataColumn("YearEnd1", typeof(int)),
                new System.Data.DataColumn("Position2", typeof(string)),
                new System.Data.DataColumn("Company2", typeof(string)),
                new System.Data.DataColumn("YearStart2", typeof(int)),
                new System.Data.DataColumn("YearEnd2", typeof(int)),
                new System.Data.DataColumn("Position3", typeof(string)),
                new System.Data.DataColumn("Company3", typeof(string)),
                new System.Data.DataColumn("YearStart3", typeof(int)),
                new System.Data.DataColumn("YearEnd3", typeof(int)),
                new System.Data.DataColumn("Position4", typeof(string)),
                new System.Data.DataColumn("Company4", typeof(string)),
                new System.Data.DataColumn("YearStart4", typeof(int)),
                new System.Data.DataColumn("YearEnd4", typeof(int))
                });

                ds.Tables.Add(dt);

                // Add data for each employee
                foreach (var employee in model.CurriculumVitae)
                {
                    var row = dt.NewRow();

                    // Basic information
                    row["FullName"] = $"{employee.FirstName} {employee.LastName}".Trim();
                    row["Position"] = employee.Position?.Name ?? string.Empty;

                    if (employee.EmployeePersonal != null)
                    {
                        row["PlaceOfBirth"] = employee.EmployeePersonal.PlaceOfBirth ?? string.Empty;
                        row["Gender"] = employee.EmployeePersonal.Gender.ToString();
                        row["Religion"] = employee.EmployeePersonal.Religion?.Name ?? string.Empty;
                        row["MaritalStatus"] = employee.EmployeePersonal.MaritalStatus.ToString();
                        row["Weight"] = employee.EmployeePersonal.Weight ?? 0;
                        row["Height"] = employee.EmployeePersonal.Height ?? 0;
                        row["PhoneNumber"] = employee.EmployeePersonal.PhoneNumber ?? string.Empty;
                        row["SocialMedia"] = employee.EmployeePersonal.SocialMedia ?? string.Empty;
                        row["Email"] = employee.EmployeePersonal.Email ?? string.Empty;
                        row["Address"] = employee.EmployeePersonal.Address ?? string.Empty;
                        row["City"] = employee.EmployeePersonal.City?.Name ?? string.Empty;
                        row["Province"] = employee.EmployeePersonal.Province?.Name ?? string.Empty;
                        row["Country"] = employee.EmployeePersonal.Country?.Name ?? string.Empty;
                        row["PostalCode"] = employee.EmployeePersonal.PostalCode ?? string.Empty;
                    }

                    // Skills, Languages, and Hobbies
                    row["Skills"] = string.Join("\n", employee.EmployeeSkills.Select(s => $"{s.Skill?.Name ?? string.Empty} - {s.Level}"));
                    row["Languages"] = string.Join("\n", employee.EmployeeLanguages.Select(l => $"{l.Language?.Name ?? string.Empty} - {l.SpeakLevel}"));
                    row["Hobbies"] = string.Join("\n", employee.EmployeeHobbies.Select(h => h.Hobby?.Name ?? string.Empty));

                    // Education entries
                    var educations = employee.EmployeeEducations.Take(4).OrderBy(e => e.GraduatedYear).ToList();
                    for (int i = 0; i < 4; i++)
                    {
                        var index = i + 1;
                        if (i < educations.Count)
                        {
                            row[$"Education{index}"] = educations[i].Education?.Name ?? string.Empty;
                            row[$"GraduatedYear{index}"] = educations[i].GraduatedYear ?? 0;
                            row[$"Score{index}"] = educations[i].Score ?? 0;
                        }
                    }

                    // Experience entries
                    var experiences = employee.EmployeeExperiences.Take(4).OrderBy(e => e.YearStart).ToList();
                    for (int i = 0; i < 4; i++)
                    {
                        var index = i + 1;
                        if (i < experiences.Count)
                        {
                            row[$"Position{index}"] = experiences[i].Position?.Name ?? string.Empty;
                            row[$"Company{index}"] = experiences[i].CompanyName ?? string.Empty;
                            row[$"YearStart{index}"] = experiences[i].YearStart ?? 0;
                            row[$"YearEnd{index}"] = experiences[i].YearEnd ?? 0;
                        }
                    }

                    // Handle employee photo
                    if (employee.Asset?.FilePath != null)
                    {
                        var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath,
                            employee.Asset.FilePath.TrimStart('/'));
                        row["ImagePath"] = File.Exists(physicalPath) ? physicalPath : string.Empty;
                    }

                    dt.Rows.Add(row);
                }

                // Clear any existing data sources
                webReport.Report.Dictionary.Clear();

                // Register the dataset with the report
                webReport.Report.RegisterData(ds);

                // Configure data source
                var dataSource = webReport.Report.GetDataSource("Employees");
                if (dataSource != null)
                {
                    dataSource.Enabled = true;

                    // Ensure the data source is properly bound to the table
                    var tableDataSource = (FastReport.Data.TableDataSource)dataSource;
                    tableDataSource.Table = ds.Tables["Employees"];
                }
                else
                {
                    _logger.LogError("Failed to get data source 'Employees' from report");
                    throw new InvalidOperationException("Data source 'Employees' not found in report");
                }

                // Configure the report for multiple pages
                var dataBand = webReport.Report.FindObject("Data1") as FastReport.DataBand;
                if (dataBand != null)
                {
                    // Set the band to start new page for each record
                    dataBand.StartNewPage = true;

                    // Ensure the data band is properly bound
                    dataBand.DataSource = dataSource;
                }

                // Prepare and render the report
                webReport.Report.Prepare();

                _logger.LogInformation("Successfully prepared report for {Count} employees", model.CurriculumVitae.Count());
            }
            else
            {
                _logger.LogWarning("No curriculum vitae data available for the report");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HandleCurriculumVitaeReport: {Error}", ex.Message);
            throw;
        }
    }


    public Task<byte[]> GenerateCompanyPolicyReportPDF(string htmlContent)
    {
        var globalSettings = new GlobalSettings
        {
            ColorMode = ColorMode.Color,
            Orientation = Orientation.Portrait,
            PaperSize = PaperKind.A4,
            Margins = new MarginSettings { Top = 20, Bottom = 10, Left = 30, Right = 30 },
            DocumentTitle = "User",

        };

        var objectSettings = new ObjectSettings
        {
            PagesCount = true,
            HtmlContent = htmlContent,
            WebSettings = { DefaultEncoding = "utf-8" },
            //HeaderSettings = { FontSize = 8, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 },
            //FooterSettings = { FontSize = 8, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 },
            FooterSettings = {FontSize = 8, Right = " [page]", Line = false, Spacing = 2.812 }
        };

        var document = new HtmlToPdfDocument()
        {
            GlobalSettings = globalSettings,
            Objects = { objectSettings }
        };

        return Task.FromResult(_converter.Convert(document));
    }

    public async Task<string> GetCompanyPolicyReportHTML<T>(T model)
    {
        var report = model as CompanyPolicyReport;
        if (report == null || report.CompanyPolicies == null || !report.CompanyPolicies.Any())
            return string.Empty;

        // Concatenate HTML for each policy
        var htmlBuilder = new StringBuilder();
        var policyList = report.CompanyPolicies.ToList();
        for (int i = 0; i < policyList.Count; i++)
        {
            var policy = policyList[i];
            var singlePolicyModel = new CompanyPolicyReport
            {
                CompanyPolicies = new List<CompanyPolicyItemReport> { policy },
                OrganizationKey = report.OrganizationKey,
                EffectiveDate = report.EffectiveDate,
                DocumentGeneratorFormat = report.DocumentGeneratorFormat,
                Organizations = report.Organizations
            };

            string htmlTemplate = await _razorRenderer.RenderViewToString(
                "~/Views/PdfDocumentTemplate/CompanyPolicy.cshtml",
                singlePolicyModel
            );

            htmlBuilder.Append(htmlTemplate);

            // Add page break after each policy except the last one
            if (i < policyList.Count - 1)
            {
                htmlBuilder.Append("<div class='page-break'></div>");
            }
        }

        return htmlBuilder.ToString();
    }

    public async Task<string> GetCurriculumVitaeReportHTML<T>(T model)
    {
        var report = model as CurriculumVitaeReport;
        if (report == null || report.CurriculumVitae == null || !report.CurriculumVitae.Any())
            return string.Empty;

        // Concatenate HTML for each employee's CV
        var htmlBuilder = new StringBuilder();
        var employeeList = report.CurriculumVitae.ToList();
        for (int i = 0; i < employeeList.Count; i++)
        {
            var employee = employeeList[i];
            var singleEmployeeModel = new CurriculumVitaeReport
            {
                CurriculumVitae = new List<EmployeeForm> { employee },
                CompanyKey = report.CompanyKey,
                OrganizationKey = report.OrganizationKey,
                DocumentGeneratorFormat = report.DocumentGeneratorFormat
            };

            string htmlTemplate = await _razorRenderer.RenderViewToString(
                "~/Views/PdfDocumentTemplate/CurriculumVitae.cshtml",
                singleEmployeeModel
            );

            htmlBuilder.Append(htmlTemplate);

            // Add page break after each CV except the last one
            if (i < employeeList.Count - 1)
            {
                htmlBuilder.Append("<div class='page-break'></div>");
            }
        }

        return htmlBuilder.ToString();
    }

    public Task<byte[]> GenerateCurriculumVitaeReportPDF(string htmlContent)
    {
        var globalSettings = new GlobalSettings
        {
            ColorMode = ColorMode.Color,
            Orientation = Orientation.Portrait,
            PaperSize = PaperKind.A4,
            Margins = new MarginSettings { Top = 20, Bottom = 10, Left = 30, Right = 30 },
            DocumentTitle = "User",

        };

        var objectSettings = new ObjectSettings
        {
            PagesCount = true,
            HtmlContent = htmlContent,
            WebSettings = { DefaultEncoding = "utf-8" },
            //HeaderSettings = { FontSize = 8, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 },
            FooterSettings = { FontSize = 8, Right = "Page [page] of [toPage]", Line = false, Spacing = 2.812 },
        };

        var document = new HtmlToPdfDocument()
        {
            GlobalSettings = globalSettings,
            Objects = { objectSettings }
        };

        return Task.FromResult(_converter.Convert(document));
    }

    public Task<byte[]> GenerateAttendanceDailyReportXlsx<T>(T model)
    {
        var report = model as AttendanceDailyReport;

        if (report == null) throw new ArgumentException("Invalid model type");

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Attendance Daily Report");
            int currentRow = 1;
            
            // Set all headers
            var headers = new Dictionary<int, string>
            {
                {1, "No"}, {2, "NIK"}, {3, "Name"}, {4, "Company"}, {5, "Organization"}, {6, "Position"}, {7, "Title"},
                {8, "Date"}, {9, "Day"}, {10, "Shift"}, {11, "IN"}, {12, "OUT"}, {13, "Working Hour"}, {14, "Description"}
            };

            foreach (var header in headers)
            {
                worksheet.Cell(currentRow, header.Key).Value = header.Value;
                worksheet.Cell(currentRow, header.Key).Style.Font.SetFontSize(12)
                        .Font.SetFontColor(XLColor.White)
                        .Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            }

            currentRow++;

            int rowIndex = 1;
            foreach (var item in report.AttendancesData)
            {
                worksheet.Cell(currentRow, 1).Value = rowIndex;
                worksheet.Cell(currentRow, 2).Value = item.EmployeeID;
                worksheet.Cell(currentRow, 3).Value = item.EmployeeName;
                worksheet.Cell(currentRow, 4).Value = item.CompanyName;
                worksheet.Cell(currentRow, 5).Value = item.OrganizationName;
                worksheet.Cell(currentRow, 6).Value = item.PositionName;
                worksheet.Cell(currentRow, 7).Value = item.TitleName;
                worksheet.Cell(currentRow, 8).Value = item.AttendanceDate?.ToString("dd-MM-yyyy");
                worksheet.Cell(currentRow, 9).Value = item.AttendanceDay;
                worksheet.Cell(currentRow, 10).Value = item.ShiftName;

                var timeInCell = worksheet.Cell(currentRow, 11);
                timeInCell.Value = item.In?.ToString("hh\\:mm");
                if (item.IsLateDocument.HasValue && item.AttendanceStatus == AttendanceStatus.Late)
                {
                    timeInCell.Style.Font.FontColor = item.IsLateDocument.Value
                            ? XLColor.LimeGreen  // Document exists
                            : XLColor.Red;   // No document
                }

                worksheet.Cell(currentRow, 12).Value = item.Out?.ToString("hh\\:mm");
                worksheet.Cell(currentRow, 13).Value = item.WorkingHour?.ToString("hh\\:mm");
                worksheet.Cell(currentRow, 14).Value = item.Description;

                currentRow++;
                rowIndex++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Add borders to all cells
            var dataRange = worksheet.Range(1, 1, currentRow - 1, 14);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            using (var memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                return Task.FromResult(memoryStream.ToArray());
            }
        }
    }

    public Task<byte[]> GenerateAttendanceWeeklyReportXlsx<T>(T model)
    {
        var report = model as AttendanceWeeklyReport;

        if (report == null) throw new ArgumentException("Invalid model type");

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Attendance Weekly Report");

            worksheet.Column(1).Width = 10; //No
            worksheet.Column(2).Width = 15; //NIK
            worksheet.Column(3).Width = 25; //Name
            worksheet.Column(4).Width = 20; //Company
            worksheet.Column(5).Width = 20; //Organization
            worksheet.Column(6).Width = 20; //Position
            worksheet.Column(7).Width = 20; //Title

            int startColumn = 8;

            var headerStyle = worksheet.Style;
            headerStyle.Font.Bold = true;
            headerStyle.Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Cell(1, 1).Value = "No";
            worksheet.Cell(1, 2).Value = "NIK";
            worksheet.Cell(1, 3).Value = "Name";
            worksheet.Cell(1, 4).Value = "Company";
            worksheet.Cell(1, 5).Value = "Organization";
            worksheet.Cell(1, 6).Value = "Position";
            worksheet.Cell(1, 7).Value = "Title";

            if (!report.StartDate.HasValue)
                throw new ArgumentException("Start date is required");

            var startDate = report.StartDate.Value;

            for (int i = 0; i < 7; i++)
            {
                var currentDate = startDate.AddDays(i);
                var dateColumn = startColumn + (i * 2);

                //Date header
                worksheet.Range(1, dateColumn, 1, dateColumn + 1).Merge();
                worksheet.Cell(1, dateColumn).Value = currentDate.ToString("dd-MM-yyyy");

                //IN Out header
                worksheet.Cell(2, dateColumn).Value = "IN";
                worksheet.Cell(3, dateColumn).Value = "OUT";
            }

            // Apply header styling
            worksheet.Range(1, 1, 2, startColumn + 14).Style = headerStyle;

            // Starting row for data
            int currentRow = 3;
            int rowIndex = 1;
            foreach (var attendance in report.AttendancesWeeklyData)
            {
                worksheet.Cell(currentRow, 1).Value = rowIndex;
                worksheet.Cell(currentRow, 2).Value = attendance.NIK;
                worksheet.Cell(currentRow, 3).Value = attendance.EmployeeName;
                worksheet.Cell(currentRow, 4).Value = attendance.CompanyName;
                worksheet.Cell(currentRow, 5).Value = attendance.OrganizationName;
                worksheet.Cell(currentRow, 6).Value = attendance.PositionName;
                worksheet.Cell(currentRow, 7).Value = attendance.TitleName;

                if (attendance.DailyAttendances != null)
                {
                    foreach (var dailyAttendance in attendance.DailyAttendances.OrderBy(x => x.AttendanceDate))
                    {
                        //Calculate column position based on date difference
                        int dayDiff = (int)((dailyAttendance.AttendanceDate.ToDateTime(TimeOnly.MinValue)) - startDate.ToDateTime(TimeOnly.MinValue)).TotalDays;

                        if (dayDiff >= 0 && dayDiff < 7)
                        {
                            int dateColumn = startColumn + (dayDiff * 2);

                            var timeInCell = worksheet.Cell(currentRow, dateColumn);
                            timeInCell.Value = dailyAttendance.In.ToString("hh\\:mm");
                            if (dailyAttendance.IsLateDocument.HasValue && dailyAttendance.Status == AttendanceStatus.Late)
                            {
                                timeInCell.Style.Font.FontColor = dailyAttendance.IsLateDocument.Value
                                        ? XLColor.LimeGreen  // Document exists
                                        : XLColor.Red;   // No document
                            }

                            worksheet.Cell(currentRow, dateColumn + 1).Value = dailyAttendance.Out.ToString("hh\\:mm");
                        }
                    }
                }

                currentRow++;
                rowIndex++;
            }

            // Apply borders to data
            var dataRange = worksheet.Range(3, 1, currentRow - 1, startColumn + 14);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Center align the time values
            var timeRange = worksheet.Range(3, startColumn, currentRow - 1, startColumn + 14);
            timeRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return Task.FromResult(stream.ToArray());
            }
        }
    }

    public async Task<byte[]> GenerateAttendanceMonthlyReportXlsx<T>(T model)
    {
        var report = model as AttendanceMonthlyReport;

        if (report == null) throw new ArgumentException("Invalid model type");

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Attendance Monthly Report");

            worksheet.Column(1).Width = 10; //No
            worksheet.Column(2).Width = 15; //NIK
            worksheet.Column(3).Width = 25; //Name
            worksheet.Column(4).Width = 20; //Company
            worksheet.Column(5).Width = 20; //Organization
            worksheet.Column(6).Width = 20; //Position
            worksheet.Column(7).Width = 20; //Title

            int startColumn = 8;

            var headerStyle = worksheet.Style;
            headerStyle.Font.Bold = true;
            headerStyle.Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var subHeaderStyle = worksheet.Style;
            subHeaderStyle.Font.Bold = true;
            subHeaderStyle.Fill.SetBackgroundColor(XLColor.FromHtml("#f4f4f4"));
            subHeaderStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            subHeaderStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Cell(1, 1).Value = "No";
            worksheet.Cell(1, 2).Value = "NIK";
            worksheet.Cell(1, 3).Value = "Name";
            worksheet.Cell(1, 4).Value = "Company";
            worksheet.Cell(1, 5).Value = "Organization";
            worksheet.Cell(1, 6).Value = "Position";
            worksheet.Cell(1, 7).Value = "Title";

            if (!report.SelectedMonth.HasValue || !report.SelectedYear.HasValue || (!report.CompanyKey.HasValue && report.CompanyKey != Guid.Empty))
                throw new ArgumentException("Month, year and company are required for monthly report.");

            //Calculate date range for the monthly report
            var cutoff = await GetCutOffConfiguration(report.CompanyKey.Value, report.SelectedYear.Value);
            var (startDay, endDay) = CutOffDateRange.GetCutOffDays(cutoff, report.SelectedMonth.Value);
            var (startDate, endDate) = CutOffDateRange.CalculateCutOffDateRange(report.SelectedMonth.Value, report.SelectedYear.Value, startDay, endDay);
            int totalDays = endDate.DayNumber - startDate.DayNumber + 1; // Add 1 to include both start and end dates

            for (var i = 0; i < totalDays; i++)
            {
                var currentDate = startDate.AddDays(i);
                var dateColumn = startColumn + (i * 2);

                worksheet.Column(dateColumn).Width = 8;
                worksheet.Column(dateColumn + 1).Width = 8;

                var dateCell = worksheet.Cell(1, dateColumn);
                dateCell.Value = currentDate.Day.ToString();
                worksheet.Range(1, dateColumn, 1, dateColumn + 1).Merge();
                worksheet.Range(1, dateColumn, 1, dateColumn + 1).Style = headerStyle;

                worksheet.Cell(2, dateColumn).Value = "IN";
                worksheet.Cell(2, dateColumn + 1).Value = "OUT";
                worksheet.Range(2, dateColumn, 2, dateColumn + 1).Style = subHeaderStyle;
            }

            int currentRow = 3;
            int rowIndex = 1;
            foreach (var attendance in report.AttendancesMonthlyData)
            {
                worksheet.Cell(currentRow, 1).Value = rowIndex;
                worksheet.Cell(currentRow, 2).Value = attendance.NIK;
                worksheet.Cell(currentRow, 3).Value = attendance.EmployeeName;
                worksheet.Cell(currentRow, 4).Value = attendance.CompanyName;
                worksheet.Cell(currentRow, 5).Value = attendance.OrganizationName;
                worksheet.Cell(currentRow, 6).Value = attendance.PositionName;
                worksheet.Cell(currentRow, 7).Value = attendance.TitleName;

                if (attendance.DailyAttendances != null)
                {
                    foreach (var monthlyAttendance in attendance.DailyAttendances)
                    {
                        var daysDifference = monthlyAttendance.AttendanceDate.DayNumber - startDate.DayNumber;
                        if (daysDifference >= 0 && daysDifference < totalDays)
                        {
                            var dateColumn = startColumn + (daysDifference * 2);

                            var timeInCell = worksheet.Cell(currentRow, dateColumn);
                            timeInCell.Value = monthlyAttendance.In.ToString("hh\\:mm");
                            if (monthlyAttendance.IsLateDocument.HasValue && monthlyAttendance.Status == AttendanceStatus.Late)
                            {
                                timeInCell.Style.Font.FontColor = monthlyAttendance.IsLateDocument.Value
                                        ? XLColor.LimeGreen  // Document exists
                                        : XLColor.Red;   // No document
                            }

                            worksheet.Cell(currentRow, dateColumn + 1).Value = monthlyAttendance.Out.ToString("hh\\:mm");
                        }

                    }
                }

                currentRow++;
                rowIndex++;
            }

            // Apply borders to all used cells
            var usedRange = worksheet.RangeUsed();
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Convert workbook to byte array
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return await Task.FromResult(stream.ToArray());
            }
        }
    }

    public async Task<byte[]> GenerateAttendanceRecapitulationReportXlsx<T>(T model)
    {
        var report = model as AttendanceRecapitulationReport;

        if (report == null) throw new ArgumentException("Invalid model type");

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Attendance Recapitulation Report");

            worksheet.Column(1).Width = 10;
            worksheet.Column(2).Width = 15; 
            worksheet.Column(3).Width = 25;  
            worksheet.Column(4).Width = 20; 
            worksheet.Column(5).Width = 20; 
            worksheet.Column(6).Width = 20; 
            worksheet.Column(7).Width = 20;

            var headerStyle = workbook.Style;
            headerStyle.Font.Bold = true;
            headerStyle.Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;

            int currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "No";
            worksheet.Cell(currentRow, 2).Value = "NIK";
            worksheet.Cell(currentRow, 3).Value = "Name";
            worksheet.Cell(currentRow, 4).Value = "Company";
            worksheet.Cell(currentRow, 5).Value = "Organization";
            worksheet.Cell(currentRow, 6).Value = "Position";
            worksheet.Cell(currentRow, 7).Value = "Title";

            if (!report.SelectedMonth.HasValue || !report.SelectedYear.HasValue || (!report.CompanyKey.HasValue && report.CompanyKey != Guid.Empty))
                throw new ArgumentException("Month, year and company are required for recapitulation report.");

            //Calculate date range for the monthly report
            var cutoff = await GetCutOffConfiguration(report.CompanyKey.Value, report.SelectedYear.Value);
            var (startDay, endDay) = CutOffDateRange.GetCutOffDays(cutoff, report.SelectedMonth.Value);
            var (startDate, endDate) = CutOffDateRange.CalculateCutOffDateRange(report.SelectedMonth.Value, report.SelectedYear.Value, startDay, endDay);

            int dateColumn = 8;
            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                worksheet.Cell(currentRow, dateColumn).Value = currentDate.Day;
                dateColumn++;
                currentDate = currentDate.AddDays(1);
            }

            //Standard totals
            int totalColumn = dateColumn;
            worksheet.Cell(currentRow, totalColumn).Value = "IN";
            worksheet.Cell(currentRow, totalColumn + 1).Value = "A";
            worksheet.Cell(currentRow, totalColumn + 2).Value = "WH";

            var leaveCodes = report.AttendancesRecapitulationData
                                   .SelectMany(x => x.Totals.LeaveTotals.Keys)
                                   .Distinct()
                                   .OrderBy(x => x)
                                   .ToList();

            //Leave totals
            int leaveColoumn = totalColumn + 3;
            foreach(var leaveCode in leaveCodes)
            {
                worksheet.Cell(currentRow, leaveColoumn).Value = leaveCode;
                leaveColoumn++;
            }

            worksheet.Range(1, 1, 1, leaveColoumn - 1).Style = headerStyle;

            currentRow = 2;
            int rowIndex = 1;
            foreach (var item in report.AttendancesRecapitulationData)
            {
                worksheet.Cell(currentRow, 1).Value = rowIndex;
                worksheet.Cell(currentRow, 2).Value = item.NIK;
                worksheet.Cell(currentRow, 3).Value = item.EmployeeName;
                worksheet.Cell(currentRow, 4).Value = item.CompanyName;
                worksheet.Cell(currentRow, 5).Value = item.OrganizationName;
                worksheet.Cell(currentRow, 6).Value = item.PositionName;
                worksheet.Cell(currentRow, 7).Value = item.TitleName;

                //Daily attendance codes
                dateColumn = 8;
                foreach (var dailyRecap in item.DailyRecaps)
                {
                    worksheet.Cell(currentRow, dateColumn).Value = dailyRecap.Code;
                    worksheet.Cell(currentRow, dateColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    dateColumn++;
                }

                //Standard totals
                worksheet.Cell(currentRow, totalColumn).Value = item.Totals.WorkEntries;
                worksheet.Cell(currentRow, totalColumn + 1).Value = item.Totals.Alphas;
                worksheet.Cell(currentRow, totalColumn + 2).Value = item.Totals.WorkingHours;

                //Leave totals
                leaveColoumn = leaveColoumn + 3;
                foreach (var leaveCode in leaveCodes)
                {
                    var leaveTotal = item.Totals.LeaveTotals.GetValueOrDefault(leaveCode, 0);
                    worksheet.Cell(currentRow, leaveColoumn).Value = leaveTotal;
                    leaveColoumn++;
                }

                currentRow++;
                rowIndex++;
            }

            //Add borders to all cells
            var dateRange = worksheet.Range(1, 1, currentRow - 1, leaveColoumn - 1);
            dateRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dateRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            worksheet.Rows().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return await Task.FromResult(stream.ToArray());
            }
        }
    }

    public Task<byte[]> GenerateAttendanceLateDetailReportXlsx<T>(T model)
    {
        var report = model as LateDetailReport;

        if (report == null) throw new ArgumentException("Invalid model type");

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Attendance Late Detail Report");

            worksheet.Column(1).Width = 10; //No
            worksheet.Column(2).Width = 15; //NIK
            worksheet.Column(3).Width = 25; //Name
            worksheet.Column(4).Width = 20; //Company
            worksheet.Column(5).Width = 20; //Organization
            worksheet.Column(6).Width = 20; //Position
            worksheet.Column(7).Width = 20; //Title
            worksheet.Column(8).Width = 30; //Date
            worksheet.Column(9).Width = 20; //TimeIn
            worksheet.Column(10).Width = 20; //TimeOut
            worksheet.Column(11).Width = 20; //Late
            worksheet.Column(12).Width = 20; //WorkingHour

            var headerStyle = worksheet.Style;
            headerStyle.Font.Bold = true;
            headerStyle.Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Cell(1, 1).Value = "No";
            worksheet.Cell(1, 2).Value = "NIK";
            worksheet.Cell(1, 3).Value = "Name";
            worksheet.Cell(1, 4).Value = "Company";
            worksheet.Cell(1, 5).Value = "Organization";
            worksheet.Cell(1, 6).Value = "Position";
            worksheet.Cell(1, 7).Value = "Title";
            worksheet.Cell(1, 8).Value = "Date";
            worksheet.Cell(1, 9).Value = "TimeIn";
            worksheet.Cell(1, 10).Value = "TimeOut";
            worksheet.Cell(1, 11).Value = "Late";
            worksheet.Cell(1, 12).Value = "WorkingHour";

            worksheet.Range(1, 1, 1, 12).Style = headerStyle;

            if (!report.SelectedMonth.HasValue || !report.SelectedYear.HasValue)
                throw new ArgumentException("Month and year are required for late detail report.");

            int currentRow = 3;
            int rowIndex = 1;
            foreach (var item in report.LatePermitReportDetails)
            {
                worksheet.Cell(currentRow, 1).Value = rowIndex;
                worksheet.Cell(currentRow, 2).Value = item.NIK;
                worksheet.Cell(currentRow, 3).Value = item.EmployeeName;
                worksheet.Cell(currentRow, 4).Value = item.CompanyName;
                worksheet.Cell(currentRow, 5).Value = item.OrganizationName;
                worksheet.Cell(currentRow, 6).Value = item.PositionName;
                worksheet.Cell(currentRow, 7).Value = item.TitleName;
                worksheet.Cell(currentRow, 8).Value = item.Date.ToString("mm/dd/yyyy");
                worksheet.Cell(currentRow, 9).Value = item.TimeIn?.ToString("HH:mm");
                worksheet.Cell(currentRow, 10).Value = item.TimeOut.ToString("HH:mm");
                var timeCell = worksheet.Cell(currentRow, 11);
                timeCell.Value = item.Late.ToString("HH:mm");
                if (item.IsLateDocument.HasValue)
                {
                    timeCell.Style.Font.FontColor = item.IsLateDocument.Value
                        ? XLColor.LimeGreen  // Document exists
                        : XLColor.Red;   // No document
                }
                worksheet.Cell(currentRow, 12).Value = item.WorkingHour.ToString("HH:mm");

                currentRow++;
                rowIndex++;
            }

            // Apply borders to all used cells
            var usedRange = worksheet.RangeUsed();
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Convert workbook to byte array
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return Task.FromResult(stream.ToArray());
            }
        }
    }

    public async Task<byte[]> GenerateAttendanceLeaveDetailReportXlsx<T>(T model)
    {
        var report = model as LeaveDetailReport;

        if (report == null) throw new ArgumentException("Invalid model type");

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Attendance Leave Detail Report");

            worksheet.Column(1).Width = 10;
            worksheet.Column(2).Width = 15;
            worksheet.Column(3).Width = 25;
            worksheet.Column(4).Width = 20;
            worksheet.Column(5).Width = 20;
            worksheet.Column(6).Width = 20;
            worksheet.Column(7).Width = 20;
            worksheet.Column(8).Width = 30;
            worksheet.Column(9).Width = 50;
            worksheet.Column(10).Width = 70;

            var headerStyle = workbook.Style;
            headerStyle.Font.Bold = true;
            headerStyle.Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;

            int currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "No";
            worksheet.Cell(currentRow, 2).Value = "NIK";
            worksheet.Cell(currentRow, 3).Value = "Name";
            worksheet.Cell(currentRow, 4).Value = "Company";
            worksheet.Cell(currentRow, 5).Value = "Organization";
            worksheet.Cell(currentRow, 6).Value = "Position";
            worksheet.Cell(currentRow, 7).Value = "Title";
            worksheet.Cell(currentRow, 8).Value = "Date";
            worksheet.Cell(currentRow, 9).Value = "Leave";
            worksheet.Cell(currentRow, 10).Value = "Description";

            //if (!report.SelectedMonth.HasValue || !report.SelectedYear.HasValue || (!report.CompanyKey.HasValue && report.CompanyKey != Guid.Empty))
            //    throw new ArgumentException("Month and year are required for leave detail report.");

            //Calculate date range for the monthly report
            //var cutoff = await GetCutOffConfiguration(report.CompanyKey.Value, report.SelectedYear.Value);
            //var (startDay, endDay) = CutOffDateRange.GetCutOffDays(cutoff, report.SelectedMonth.Value);
            //var (startDate, endDate) = CutOffDateRange.CalculateCutOffDateRange(report.SelectedMonth.Value, report.SelectedYear.Value, startDay, endDay);

            //int dateColumn = 8;
            //var currentDate = startDate;
            //while (currentDate <= endDate)
            //{
            //    worksheet.Cell(currentRow, dateColumn).Value = currentDate.Day;
            //    dateColumn++;
            //    currentDate = currentDate.AddDays(1);
            //}

            //var leaveCodes = report.LeaveDetailReports
            //                       .SelectMany(x => x.LeaveTotals.Keys)
            //                       .Distinct()
            //                       .OrderBy(x => x)
            //                       .ToList();

            ////Leave totals
            //int leaveColoumn = dateColumn;
            //foreach (var leaveCode in leaveCodes)
            //{
            //    worksheet.Cell(currentRow, leaveColoumn).Value = leaveCode;
            //    leaveColoumn++;
            //}

            //worksheet.Range(1, 1, 1, leaveColoumn - 1).Style = headerStyle;

            currentRow = 2;
            int rowIndex = 1;
            foreach (var item in report.LeaveDetailReports)
            {
                worksheet.Cell(currentRow, 1).Value = rowIndex;
                worksheet.Cell(currentRow, 2).Value = item.NIK;
                worksheet.Cell(currentRow, 3).Value = item.EmployeeName;
                worksheet.Cell(currentRow, 4).Value = item.CompanyName;
                worksheet.Cell(currentRow, 5).Value = item.OrganizationName;
                worksheet.Cell(currentRow, 6).Value = item.PositionName;
                worksheet.Cell(currentRow, 7).Value = item.TitleName;
                worksheet.Cell(currentRow, 8).Value = item.Date.ToString("mm/dd/yyyy");
                worksheet.Cell(currentRow, 9).Value = item.LeaveName;
                worksheet.Cell(currentRow, 10).Value = item.Description ?? String.Empty;

                //Daily attendance codes
                //dateColumn = 8;
                //foreach (var dailyRecap in item.DailyRecaps)
                //{
                //    worksheet.Cell(currentRow, dateColumn).Value = dailyRecap.Code;
                //    worksheet.Cell(currentRow, dateColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                //    dateColumn++;
                //}

                //Leave totals
                //leaveColoumn = dateColumn;
                //foreach (var leaveCode in leaveCodes)
                //{
                //    var leaveTotal = item.LeaveTotals.GetValueOrDefault(leaveCode, 0);
                //    worksheet.Cell(currentRow, leaveColoumn).Value = leaveTotal;
                //    leaveColoumn++;
                //}

                currentRow++;
                rowIndex++;
            }

            // Apply borders to all used cells
            var usedRange = worksheet.RangeUsed();
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Convert workbook to byte array
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return await Task.FromResult(stream.ToArray());
            }
        }
    }

    public Task<byte[]> GenerateAttendanceEarlyOutDetailReportXlsx<T>(T model)
    {
        var report = model as PermitDetailReport;

        if (report?.EarlyOutDetailReports == null || !report.EarlyOutDetailReports.Any()) 
            throw new ArgumentException("Invalid model type, Early Out Detail Report Data is null");

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Attendance Early Out Detail Report");

            int currentRow = 1;

            // Set all headers
            var headers = new Dictionary<int, string>
            {
                {1, "No"}, {2, "NIK"}, {3, "Name"}, {4, "Company"}, {5, "Organization"},
                {6, "Position"}, {7, "Title"}, {8, "Date"}, {9, "Time Out"}, {10, "Reason"}
            };

            foreach (var header in headers)
            {
                worksheet.Cell(currentRow, header.Key).Value = header.Value;
                worksheet.Cell(currentRow, header.Key).Style.Font.SetFontSize(12)
                        .Font.SetFontColor(XLColor.White)
                        .Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            }

            currentRow++;

            int rowIndex = 1;
            foreach (var item in report.EarlyOutDetailReports)
            {
                worksheet.Cell(currentRow, 1).Value = rowIndex;
                worksheet.Cell(currentRow, 2).Value = item.NIK;
                worksheet.Cell(currentRow, 3).Value = item.EmployeeName;
                worksheet.Cell(currentRow, 4).Value = item.CompanyName;
                worksheet.Cell(currentRow, 5).Value = item.OrganizationName;
                worksheet.Cell(currentRow, 6).Value = item.PositionName;
                worksheet.Cell(currentRow, 7).Value = item.TitleName;
                worksheet.Cell(currentRow, 8).Value = item.DateSubmission.ToString("dd-MM-yyyy");
                worksheet.Cell(currentRow, 9).Value = item.TimeOut.ToString("HH:mm");
                worksheet.Cell(currentRow, 10).Value = item.Reason;

                currentRow++;
                rowIndex++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Add borders to all cells
            var dataRange = worksheet.Range(1, 1, currentRow - 1, 10);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            using (var memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                return Task.FromResult(memoryStream.ToArray());
            }
        }
    }

    public Task<byte[]> GenerateAttendanceOutPermitDetailReportXlsx<T>(T model)
    {
        var report = model as PermitDetailReport;

        if (report?.OutPermitDetailReports == null || !report.OutPermitDetailReports.Any())
            throw new ArgumentException("Invalid model type, Out Office Report Data is null");

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Attendance Out Office Detail Report");

            int currentRow = 1;

            // Set all headers
            var headers = new Dictionary<int, string>
            {
                {1, "No"}, {2, "NIK"}, {3, "Name"}, {4, "Company"}, {5, "Organization"},
                {6, "Position"}, {7, "Title"}, {8, "Date"}, {9, "Time Out"}, {10, "Back To Office"}, {11, "Reason"}
            };

            foreach (var header in headers)
            {
                worksheet.Cell(currentRow, header.Key).Value = header.Value;
                worksheet.Cell(currentRow, header.Key).Style.Font.SetFontSize(12)
                        .Font.SetFontColor(XLColor.White)
                        .Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            }

            currentRow++;

            int rowIndex = 1;
            foreach(var item in report.OutPermitDetailReports)
            {
                worksheet.Cell(currentRow, 1).Value = rowIndex;
                worksheet.Cell(currentRow, 2).Value = item.NIK;
                worksheet.Cell(currentRow, 3).Value = item.EmployeeName;
                worksheet.Cell(currentRow, 4).Value = item.CompanyName;
                worksheet.Cell(currentRow, 5).Value = item.OrganizationName;
                worksheet.Cell(currentRow, 6).Value = item.PositionName;
                worksheet.Cell(currentRow, 7).Value = item.TitleName;
                worksheet.Cell(currentRow, 8).Value = item.DateSubmission.ToString("dd-MM-yyyy");
                worksheet.Cell(currentRow, 9).Value = item.TimeOut.ToString("HH:mm");
                worksheet.Cell(currentRow, 10).Value = item.BackToOffice.ToString("HH:mm");
                worksheet.Cell(currentRow, 11).Value = item.Reason;

                currentRow++;
                rowIndex++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Add borders to all cells
            var dataRange = worksheet.Range(1, 1, currentRow - 1, 10);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            using (var memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                return Task.FromResult(memoryStream.ToArray());
            }
        }
    }

    public async Task<byte[]> GenerateShiftScheduleDetailReportXlsx<T>(T model)
    {
        var report = model as ShiftScheduleDetailReport;

        if (report == null) throw new ArgumentException("Invalid model type");
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Shift Schedule Detail Report");

            worksheet.Column(1).Width = 10; //No
            worksheet.Column(2).Width = 15; //NIK
            worksheet.Column(3).Width = 25; //Name
            worksheet.Column(4).Width = 20; //Company
            worksheet.Column(5).Width = 20; //Organization
            worksheet.Column(6).Width = 20; //Position
            worksheet.Column(7).Width = 20; //Title

            var headerStyle = worksheet.Style;
            headerStyle.Font.Bold = true;
            headerStyle.Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var subHeaderStyle = worksheet.Style;
            subHeaderStyle.Font.Bold = true;
            subHeaderStyle.Fill.SetBackgroundColor(XLColor.FromHtml("#f4f4f4"));
            subHeaderStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            subHeaderStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;

            int currentRow = 1;
            worksheet.Cell(1, 1).Value = "No";
            worksheet.Cell(1, 2).Value = "NIK";
            worksheet.Cell(1, 3).Value = "Name";
            worksheet.Cell(1, 4).Value = "Company";
            worksheet.Cell(1, 5).Value = "Organization";
            worksheet.Cell(1, 6).Value = "Position";
            worksheet.Cell(1, 7).Value = "Title";

            if (!report.SelectedMonth.HasValue || !report.SelectedYear.HasValue || (!report.CompanyKey.HasValue && report.CompanyKey != Guid.Empty))
                throw new ArgumentException("Month and year are required for shift schedule detail report.");

            //Calculate date range for the monthly report
            var cutoff = await GetCutOffConfiguration(report.CompanyKey.Value, report.SelectedYear.Value);
            var (startDay, endDay) = CutOffDateRange.GetCutOffDays(cutoff, report.SelectedMonth.Value);
            var (startDate, endDate) = CutOffDateRange.CalculateCutOffDateRange(report.SelectedMonth.Value, report.SelectedYear.Value, startDay, endDay);

            int dateColumn = 8;
            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                worksheet.Cell(currentRow, dateColumn).Value = currentDate.Day;
                dateColumn++;
                currentDate = currentDate.AddDays(1);
            }

            // Apply header styling
            worksheet.Range(1, 1, 2, dateColumn - 1).Style = headerStyle;

            currentRow = 2;
            int rowIndex = 1;
            foreach (var item in report.ShiftScheduleDetailReports)
            {
                worksheet.Cell(currentRow, 1).Value = rowIndex;
                worksheet.Cell(currentRow, 2).Value = item.NIK;
                worksheet.Cell(currentRow, 3).Value = item.EmployeeName;
                worksheet.Cell(currentRow, 4).Value = item.CompanyName;
                worksheet.Cell(currentRow, 5).Value = item.OrganizationName;
                worksheet.Cell(currentRow, 6).Value = item.PositionName;
                worksheet.Cell(currentRow, 7).Value = item.TitleName;

                //Daily shift schedules name
                dateColumn = 8;
                foreach (var dailyRecap in item.DailyShiftSchedules)
                {
                    worksheet.Cell(currentRow, dateColumn).Value = dailyRecap.ShiftName;
                    worksheet.Cell(currentRow, dateColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    dateColumn++;
                }

                currentRow++;
                rowIndex++;
            }

            //Add borders to all cells
            var dateRange = worksheet.Range(1, 1, currentRow - 1, dateColumn - 1);
            dateRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dateRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            worksheet.Rows().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return await Task.FromResult(stream.ToArray());
            }
        }
    }

    public Task<byte[]> GenerateOvertimeLetterDetailReportXlsx<T>(T model)
    {
        var report = model as OvertimeLetterDetailReport;

        if (report == null) throw new ArgumentException("Invalid model type");

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Overtime Letter Detail Report");

            int currentRow = 1;

            // Set all headers
            var headers = new Dictionary<int, string>
            {
                {1, "No"}, {2, "NIK"}, {3, "Name"}, {4, "Company"}, {5, "Organization"}, {6, "Position"}, {7, "Title"},
                {8, "Date"}, {9, "Time In"}, {10, "Time Out"}, {11, "OT IN"}, {12, "OT OUT"}, {13, "Actual Overtime"}, {14, "Multiple Overtime"}
            };

            foreach (var header in headers)
            {
                worksheet.Cell(currentRow, header.Key).Value = header.Value;
                worksheet.Cell(currentRow, header.Key).Style.Font.SetFontSize(12)
                        .Font.SetFontColor(XLColor.White)
                        .Fill.SetBackgroundColor(XLColor.FromHtml("#dd4b39"));
            }

            currentRow++;

            int rowIndex = 1;
            foreach(var item in report.OvertimeLetterDetailReports)
            {
                worksheet.Cell(currentRow, 1).Value = rowIndex;
                worksheet.Cell(currentRow, 2).Value = item.NIK;
                worksheet.Cell(currentRow, 3).Value = item.EmployeeName;
                worksheet.Cell(currentRow, 4).Value = item.CompanyName;
                worksheet.Cell(currentRow, 5).Value = item.OrganizationName;
                worksheet.Cell(currentRow, 6).Value = item.PositionName;
                worksheet.Cell(currentRow, 7).Value = item.TitleName;
                worksheet.Cell(currentRow, 8).Value = item.DateSubmission.ToString("dd-MM-yyyy");
                worksheet.Cell(currentRow, 9).Value = item.TimeIn.ToString("HH:mm");
                worksheet.Cell(currentRow, 10).Value = item.TimeOut.ToString("HH:mm");
                worksheet.Cell(currentRow, 11).Value = item.OvertimeIn.ToString("HH:mm");
                worksheet.Cell(currentRow, 12).Value = item.OvertimeOut.ToString("HH:mm");
                worksheet.Cell(currentRow, 13).Value = item.RealOvertime.ToString("HH:mm");
                worksheet.Cell(currentRow, 14).Value = item.AccumlativeOvertime.ToString("HH:mm");

                currentRow++;
                rowIndex++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Add borders to all cells
            var dataRange = worksheet.Range(1, 1, currentRow - 1, 14);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            using (var memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                return Task.FromResult(memoryStream.ToArray());
            }
        }
    }

    private async Task<CutOffListItem> GetCutOffConfiguration(Guid companyKey, int year)
    {
        var cutoffResult = await _mediator.Send(new GetCutOffsQuery([c => c.CompanyKey == companyKey &&
                                                                          c.YearPeriod == year]));

        return cutoffResult.CutOffs.FirstOrDefault() ??
            throw new ArgumentException("CutOff configuration not found for specified company and year");
    }
}
