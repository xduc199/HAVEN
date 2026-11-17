using System.ComponentModel.DataAnnotations;

namespace Haven.Domain.Privacy;

public enum PrivacyRequestType
{
    [Display(Name = "Truy cập dữ liệu của tôi")]
    Access = 1,
    [Display(Name = "Cập nhật dữ liệu")]
    Correction = 2,
    [Display(Name = "Xóa dữ liệu hoặc tài khoản")]
    Deletion = 3,
    [Display(Name = "Hạn chế xử lý dữ liệu")]
    Restriction = 4,
    [Display(Name = "Rút lại sự đồng ý xử lý dữ liệu")]
    ConsentWithdrawal = 5,
    [Display(Name = "Phản ánh hoặc khiếu nại")]
    Complaint = 6,
    [Display(Name = "Yêu cầu khác")]
    Other = 7
}
