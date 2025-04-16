using Saga.DomainShared.Models;
using Saga.DomainShared.Enums;
using Saga.DomainShared.Interfaces;

namespace Saga.Infrastructure.Services;

public class NavigationService : INavigationService
{
    public async Task<IEnumerable<NavigationMenu>> GetDefaultNavigation()
    {
        List<NavigationMenu> navs = [];

        navs.AddRange(GetHumanResourceNavigation());
        navs.AddRange(GetCustomerResourceNavigation());
        navs.AddRange(GetSystemNavigation());

        return await Task.FromResult(navs);
    }

    public async Task<IEnumerable<AccessNavigationMenu>> GetAccessNavigation()
    {
        List<AccessNavigationMenu> navs = [];
        return await Task.FromResult(navs);
    }

    public async Task<AccessNavigationMenu> CheckAccessNavigation(Uri uri)
    {
        return await Task.FromResult(new AccessNavigationMenu());
    }

    private static string LinkVoid => "javascript:void(0)";

    private static List<NavigationMenu> GetHumanResourceNavigation()
        => [
            new(){
                 Category = NavigationCategory.Extension,
                 Title = "HRM",
                 Icon = "fa fa-people-group",
                 AbsolutePath = LinkVoid,
                 SubNavigations = [
                    #region Organization
                     new(){
                         Category = NavigationCategory.Module,
                         Title = "Organization",
                         Icon = "fa fa-bank",
                         AbsolutePath = LinkVoid,
                         SubNavigations = [
                             new(){
                                 Category = NavigationCategory.Menu,
                                 Title = "Organization",
                                 Icon = "ki-outline ki-menu",
                                 AbsolutePath = LinkVoid,
                                 SubNavigations = [
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Company",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Organization/Company/Index",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Organization",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Organization/Organization/Index",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Title",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Organization/Title/Index",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Position",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Organization/Position/Index",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Grade",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Organization/Grade/Index",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Branch",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Organization/Branch/Index",
                                         SubNavigations = []
                                     }
                                ]
                             },
                             new(){
                                 Category = NavigationCategory.Menu,
                                 Title = "Report",
                                 Icon = "ki-menu ki-outline ki-paper-clip",
                                 AbsolutePath = LinkVoid,
                                 SubNavigations = [
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Company Structure",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Organization/OrganizationStructure/Index",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Company Policy",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Organization/CompanyPolicy/Report",
                                         SubNavigations = []
                                     }
                                ]
                             },
                             new(){
                                 Category = NavigationCategory.Menu,
                                 Title = "Setting",
                                 Icon = "ki-outline ki-setting",
                                 AbsolutePath = LinkVoid,
                                 SubNavigations = [
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Title Qualification",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Organization/TitleQualification/Index",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Company Policy",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Organization/CompanyPolicy/Index",
                                         SubNavigations = []
                                     }
                                ]
                             }
                        ]
                     },
                     #endregion

                    #region Employee
                     new(){
                         Category = NavigationCategory.Module,
                         Title = "Employee",
                         Icon = "fa fa-user-group",
                         AbsolutePath = LinkVoid,
                         SubNavigations = [
                             new(){
                                 Category = NavigationCategory.Menu,
                                 Title = "Employee",
                                 Icon = "ki-outline ki-user-tick",
                                 AbsolutePath = LinkVoid,
                                 SubNavigations = [
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Employee",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Employee/Employee/Index",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Employee Transfer",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Employee/EmployeeTransfer/Index",
                                         SubNavigations = []
                                    },
                                    // new(){
                                    //     Category = NavigationCategory.SubMenu,
                                    //     Title = "Import Employee",
                                    //     Icon = string.Empty,
                                    //     AbsolutePath = "/Employee/EmployeeImport/Index",
                                    //     SubNavigations = []
                                    //}
                                ]
                            },
                             new(){
                                 Category = NavigationCategory.Menu,
                                 Title = "Report",
                                 Icon = "ki-menu ki-outline ki-paper-clip",
                                 AbsolutePath = LinkVoid,
                                 SubNavigations = [
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Employee",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Employee/EmployeeReport/Filter",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Curriculum Vitae",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Employee/CurriculumVitaeReport/Filter",
                                         SubNavigations = []
                                    }
                                ]
                            },
                             new(){
                                 Category = NavigationCategory.Menu,
                                 Title = "Graph",
                                 Icon = "ki-outline ki-graph-up",
                                 AbsolutePath = LinkVoid,
                                 SubNavigations = [
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Turn Over",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Employee/GraphTurnOver/Filter",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Man Power",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Employee/GraphManPower/Filter",
                                         SubNavigations = []
                                    }
                                ]
                            },
                             new(){
                                 Category = NavigationCategory.Menu,
                                 Title = "Setting",
                                 Icon = "ki-outline ki-setting",
                                 AbsolutePath = LinkVoid,
                                 SubNavigations = [
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Education",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Employee/Education/Index",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Hobby",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Employee/Hobby/Index",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Language",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Employee/Language/Index",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Skill",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Employee/Skill/Index",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Nationality",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Employee/Nationality/Index",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Religion",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Employee/Religion/Index",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Ethnic",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Employee/Ethnic/Index",
                                         SubNavigations = []
                                    }
                                ]
                            }
                        ]
                    },
                     #endregion

                    #region Attendance
                     new(){
                         Category = NavigationCategory.Module,
                         Title = "Attendance",
                         Icon = "fa fa-calendar-days",
                         AbsolutePath = LinkVoid,
                         SubNavigations = [
                             new(){
                                 Category = NavigationCategory.Menu,
                                 Title = "Data Retrive",
                                 Icon = "fa fa-fingerprint",
                                 AbsolutePath = LinkVoid,
                                 SubNavigations = [
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Finger Print",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/DataRetrive/FingerPrint",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Attendance Point",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/DataRetrive/AttendancePoint",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Import Finger Log",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/DataRetrive/ImportAttlog",
                                         SubNavigations = []
                                     }
                                ]
                             },
                             new(){
                                 Category = NavigationCategory.Menu,
                                 Title = "Attendance",
                                 Icon = "ki-outline ki-user-tick",
                                 AbsolutePath = LinkVoid,
                                 SubNavigations = [
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Leave Submission",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/LeaveSubmission/Index",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Late Permit",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/LatePermit/Index",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Early Out Permit",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/EarlyOutPermit/Index",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Out Permit",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/OutPermit/Index",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Overtime Letter",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/OvertimeLetter/Index",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Approval Request",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/AttendanceApproval/Index",
                                         SubNavigations = []
                                     }
                                ]
                             },
                             new(){
                                 Category = NavigationCategory.Menu,
                                 Title = "Calculation",
                                 Icon = "fa fa-bolt",
                                 AbsolutePath = LinkVoid,
                                 SubNavigations = [
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Calculation",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/Calculation/Index",
                                         SubNavigations = []
                                     }
                                ]
                             },
                             new(){
                                 Category = NavigationCategory.Menu,
                                 Title = "Report",
                                 Icon = "ki-menu ki-outline ki-paper-clip",
                                 AbsolutePath = LinkVoid,
                                 SubNavigations = [
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Daily",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/AttendanceReport/Daily",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Weekly",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/AttendanceReport/Weekly",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Monthly",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/AttendanceReport/Monthly",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Recapitulation",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/AttendanceReport/Recapitulation",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Late Arrival Detail",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/AttendanceReport/LateDetail",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Permit Detail",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/AttendanceReport/PermitDetail",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Leave Detail",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/AttendanceReport/LeaveDetail",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Overtime Detail",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/AttendanceReport/OvertimeDetail",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Shift Schedule",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/AttendanceReport/ShiftSchedule",
                                         SubNavigations = []
                                    }
                                ]
                             },
                             new(){
                                 Category = NavigationCategory.Menu,
                                 Title = "Graph",
                                 Icon = "ki-outline ki-graph-up",
                                 AbsolutePath = LinkVoid,
                                 SubNavigations = [
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Present",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/AttendanceGraph/Present",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Not Present",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/AttendanceGraph/NotPresent",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Working Hour",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/AttendanceGraph/WorkingHour",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Overtime",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/AttendanceGraph/Overtime",
                                         SubNavigations = []
                                    }
                                 ]
                             },
                             new(){
                                 Category = NavigationCategory.Menu,
                                 Title = "Setting",
                                 Icon = "ki-outline ki-setting",
                                 AbsolutePath = LinkVoid,
                                 SubNavigations = [
                                    new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Holiday",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/Holiday/Index",
                                         SubNavigations = []
                                    },
                                    new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Master Leave",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/MasterLeave/Index",
                                         SubNavigations = []
                                    },
                                    new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "CutOff",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/CutOff/Index",
                                         SubNavigations = []
                                    },
                                    new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Standard Working",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/StandardWorking/Index",
                                         SubNavigations = []
                                    },
                                    new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Shift",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/Shift/Index",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Shift Schedule",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/Shift/ShiftSchedule",
                                         SubNavigations = []
                                    },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Attendance Point",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/AttendancePoint/Index",
                                         SubNavigations = []
                                    },
                                    new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Overtime Rate",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/OvertimeRate/Index",
                                         SubNavigations = []
                                    },
                                    new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Finger Print Machine",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Attendance/FingerPrint/Index",
                                         SubNavigations = []
                                    }
                                ]
                             }
                        ]
                    }
                     #endregion

                    #region Payroll
                     ,new(){
                         Category = NavigationCategory.Module,
                         Title = "Payroll",
                         Icon = "fa fa-money-bills",
                         AbsolutePath = LinkVoid,
                         SubNavigations = [
                             new(){
                                 Category = NavigationCategory.Menu,
                                 Title = "Payroll",
                                 Icon = "fa fa-money-bill-1-wave",
                                 AbsolutePath = LinkVoid,
                                 SubNavigations = [
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Employee Component",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Payroll/EmployeeComponent/Index",
                                         SubNavigations = []
                                     }
                                     ,new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Other Transaction",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Payroll/OtherTransaction/Index",
                                         SubNavigations = []
                                     }
                                     ,new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Calculation",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Payroll/PayrollCalculation/Index",
                                         SubNavigations = []
                                     }
                                ]
                             }
                             ,new(){
                                 Category = NavigationCategory.Menu,
                                 Title = "Reports",
                                 Icon = "ki-menu ki-outline ki-paper-clip",
                                 AbsolutePath = LinkVoid,
                                 SubNavigations = [
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Recapitulation",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Payroll/PayrollRecapitulationReport/Index",
                                         SubNavigations = []
                                     }
                                     ,new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Payslip",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Payroll/PayrollPayslipReport/Index",
                                         SubNavigations = []
                                     }
                                     ,new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Annual",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Payroll/PayrollAnnualReport/Index",
                                         SubNavigations = []
                                     }
                                     ,new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "BPJS",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Payroll/PayrollBpjsReport/Index",
                                         SubNavigations = []
                                     }
                                     ,new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "PPH21",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Payroll/PayrollTaxReport/Index",
                                         SubNavigations = []
                                     }
                                     ,new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "SPT",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Payroll/PayrollSPTReport/Index",
                                         SubNavigations = []
                                     }
                                     ,new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Graph",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Payroll/PayrollGraphReport/Index",
                                         SubNavigations = []
                                     }
                                     ,new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Fleksible Report",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Payroll/PayrollFleksibleReport/Index",
                                         SubNavigations = []
                                     }
                                ]
                             }
                             //,new(){
                             //    Category = NavigationCategory.Menu,
                             //    Title = "Benefit",
                             //    Icon = "fa fa-leaf",
                             //    AbsolutePath = LinkVoid,
                             //    SubNavigations = [
                             //        //new(){
                             //        //    Category = NavigationCategory.SubMenu,
                             //        //    Title = "Employee Component",
                             //        //    Icon = string.Empty,
                             //        //    AbsolutePath = "/Attendance/DataRetrive/FingerPrint",
                             //        //    SubNavigations = []
                             //        //}
                             //   ]
                             //}
                             ,new(){
                                 Category = NavigationCategory.Menu,
                                 Title = "Setting",
                                 Icon = "ki-outline ki-setting",
                                 AbsolutePath = LinkVoid,
                                 SubNavigations = [
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Allowance",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Payroll/Allowance/Index",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "BPJS",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Payroll/BpjsConfig/Index",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "Payslip Template",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Payroll/PayslipTemplate/Index",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "TER",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Payroll/AverageEffectiveRate/Index",
                                         SubNavigations = []
                                     },
                                     new(){
                                         Category = NavigationCategory.SubMenu,
                                         Title = "PPH21",
                                         Icon = string.Empty,
                                         AbsolutePath = "/Payroll/PayrollPphConfig/Index",
                                         SubNavigations = []
                                     }
                                ]
                             }
                        ]
                     }
                    #endregion
                ]
            }
    ];

    private static List<NavigationMenu> GetCustomerResourceNavigation()
        => [];

    private static List<NavigationMenu> GetSystemNavigation()
        => [
        new()
        {
            Category = NavigationCategory.Extension,
            Title = "Global",
            Icon = "fa fa-arrows-left-right-to-line",
            AbsolutePath = LinkVoid,
            SubNavigations = [
                new(){
                    Category = NavigationCategory.SubMenu,
                    Title = "Country",
                    Icon = "fa fa-globe",
                    AbsolutePath = "/Organization/Country/Index",
                    SubNavigations = []
                },
                new(){
                    Category = NavigationCategory.SubMenu,
                    Title = "Province",
                    Icon = "fa fa-globe",
                    AbsolutePath = "/Organization/Province/Index",
                    SubNavigations = []
                },
                new(){
                    Category = NavigationCategory.SubMenu,
                    Title = "City",
                    Icon = "fa fa-globe",
                    AbsolutePath = "/Organization/City/Index",
                    SubNavigations = []
                },
                new(){
                    Category = NavigationCategory.SubMenu,
                    Title = "Bank",
                    Icon = "fa fa-bank",
                    AbsolutePath = "/Organization/Bank/Index",
                    SubNavigations = []
                },
                new(){
                    Category = NavigationCategory.SubMenu,
                    Title = "Currency",
                    Icon = "fa fa-money-bill-1",
                    AbsolutePath = "/Organization/Currency/Index",
                    SubNavigations = []
                }
            ]
        },

        new()
        {
            Category = NavigationCategory.Extension,
            Title = "Administrator",
            Icon = "fa fa-user-shield",
            AbsolutePath = LinkVoid,
            SubNavigations = [
                 new(){
                    Category = NavigationCategory.SubMenu,
                    Title = "User Management",
                    Icon = "fa fa-users",
                    AbsolutePath = "/System/UserManagement/Index",
                    SubNavigations = []
                },
                 new(){
                    Category = NavigationCategory.SubMenu,
                    Title = "Navigation Access",
                    Icon = "fa fa-bars",
                    AbsolutePath = "/System/NavigationAccess/Index",
                    SubNavigations = []
                },
                 new(){
                    Category = NavigationCategory.SubMenu,
                    Title = "Organization Access",
                    Icon = "fa fa-bars-staggered",
                    AbsolutePath = "/System/OrganizationAccess/Index",
                    SubNavigations = []
                }
            ]
        },

        new()
        {
            Category = NavigationCategory.Extension,
            Title = "System",
            Icon = "fa fa-gears",
            AbsolutePath = LinkVoid,
            SubNavigations = [
                 new(){
                     Category = NavigationCategory.Module,
                     Title = "Maintenance",
                     Icon = "fa fa-arrows-left-right-to-line",
                     AbsolutePath = LinkVoid,
                     SubNavigations = [
                         new(){
                             Category = NavigationCategory.SubMenu,
                             Title = "Backup",
                             Icon = string.Empty,
                             AbsolutePath = "/System/Maintenance/Backup",
                             SubNavigations = []
                        },
                         new(){
                             Category = NavigationCategory.SubMenu,
                             Title = "Restore",
                             Icon = string.Empty,
                             AbsolutePath = "/System/Maintenance/Restore",
                             SubNavigations = []
                        }
                    ]
                },
                 new(){
                     Category = NavigationCategory.Module,
                     Title = "Logs",
                     Icon = "fa fa-arrows-left-right-to-line",
                     AbsolutePath = LinkVoid,
                     SubNavigations = [
                         new(){
                             Category = NavigationCategory.SubMenu,
                             Title = "Audit Trail",
                             Icon = string.Empty,
                             AbsolutePath = "/System/SystemLog/AuditTrail",
                             SubNavigations = []
                        },
                         new(){
                             Category = NavigationCategory.SubMenu,
                             Title = "User Changed",
                             Icon = string.Empty,
                             AbsolutePath = "/System/SystemLog/UserChanged",
                             SubNavigations = []
                        }
                    ]
                }
            ]
        }
    ];
}
