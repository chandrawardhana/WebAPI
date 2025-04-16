using Saga.Domain.Enums;

namespace Saga.DomainShared.Extensions;

public static class DocumentNumberGenerator
{
    private static readonly string[] RomanMonths =
    {
        "I", "II", "III", "IV", "V", "VI", "VII", 
        "VIII", "IX", "X", "XI", "XII"
    };

    public static string GenerateDocumentNumber(DocumentNumberType documentType, DateTime date)
    {
        string prefix = GetDocumentPrefix(documentType);
        string romanMonth = RomanMonths[date.Month - 1];
        string year = date.Year.ToString();
        string sequenceNumber = GenerateSequenceNumber();

        return $"{prefix}/{romanMonth}/{year}/{sequenceNumber}";
    }

    private static string GetDocumentPrefix(DocumentNumberType documentType)
    {
        return documentType switch
        {
            DocumentNumberType.LeavePermit => "LV",
            DocumentNumberType.LatePermit => "LT",
            DocumentNumberType.EarlyOutPermit => "ET",
            DocumentNumberType.OutPermit => "OT",
            DocumentNumberType.OvertimeLetter => "OL",
            _ => throw new ArgumentException($"Unsupported document type: {documentType}")
        };
    }

    private static string GenerateSequenceNumber()
    {
        // Generate a 6-digit random number
        Random random = new();
        return random.Next(100000, 999999).ToString();
    }
}
