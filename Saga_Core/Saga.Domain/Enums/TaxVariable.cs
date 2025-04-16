

namespace Saga.Domain.Enums;

/// <summary>
/// ashari.herman 2025-03-07 slipi jakarta
/// </summary>

public enum TaxVariable
{
    [Display(Name = "NONE")]
    None,

    [Display(Name = "GAJI ATAU UANG PENSIUN BERKALA")]
    Salary = 1,

    [Display(Name = "TUNJANGAN LAINNYA, UANG LEMBUR, DAN SEGALANYA")]
    Allowance = 3,

    [Display(Name = "HONORARIUM DAN IMBALAN LAIN SEJENISNYA")]
    Honorarium = 4,

    [Display(Name = "PREMI ASURANSI YANG DIBAYARKAN PEMBERI KERJA")]
    PremiInsurance = 5,

    [Display(Name = "PENERIMAAN DALAM BENTUK NATURA DAN KENIKMATAN LAINNYA YANG DIKENAKAN PEMOTONGAN PPh PASAL 21")]
    Natura = 6,

    [Display(Name = "TANTIEM, BONUS, GRATIFIKASI, JASA PRODUKSI DAN THR")]
    Bonus = 7,

    [Display(Name = "IURAN TERKAIT PENSIUN ATAU HARI TUA")]
    Pensiun = 10,

    [Display(Name = "ZAKAT/SUMBANGAN KEAGAMAAN YANG BERSIFAT WAJIB YANG DIBAYARKAN MELALUI PEMBERI KERJA")]
    Zakat = 11,
}
