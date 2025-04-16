namespace Saga.Domain.Entities.Systems;

[Table("tbmdatetimeduration", Schema = "System")]
public class DateTimeDuration
{
    public int Year { get; set; }
    public int Month { get; set; }

    // Helper method to calculate duration between two dates
    private static DateTimeDuration CalculateDuration(DateTime startDate, DateTime endDate)
    {
        int years = endDate.Year - startDate.Year;
        int months = endDate.Month - startDate.Month;

        if (months < 0)
        {
            years--;
            months += 12;
        }

        // Adjust for day of month
        if (endDate.Day < startDate.Day)
        {
            months--;
            if (months < 0)
            {
                years--;
                months += 12;
            }
        }

        return new DateTimeDuration
        {
            Year = years,
            Month = months
        };
    }

    // Extension method for Employee to get long of joined Employee
    public static DateTimeDuration GetLongOfJoin(Employee employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        return CalculateDuration(employee.HireDate, DateTime.Now);
    }

    // Extension method for EmployeePersonal to get age
    public static DateTimeDuration GetAge(EmployeePersonal employeePersonal)
    {
        if (employeePersonal == null)
            throw new ArgumentNullException(nameof(employeePersonal));

        return CalculateDuration(employeePersonal.DateOfBirth, DateTime.Now);
    }

    // Override ToString for easy formatting
    public override string ToString()
    {
        if (Year == 0)
            return $"{Month} month{(Month != 1 ? "s" : "")}";
        if (Month == 0)
            return $"{Year} year{(Year != 1 ? "s" : "")}";
        return $"{Year} year{(Year != 1 ? "s" : "")} {Month} month{(Month != 1 ? "s" : "")}";
    }
}
