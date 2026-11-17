using System.ComponentModel.DataAnnotations;

namespace Haven.Domain.Privacy;

public enum PrivacyRequestStatus
{
    [Display(Name = "Mới tiếp nhận")]
    New = 1,
    [Display(Name = "Đang xử lý")]
    InProgress = 2,
    [Display(Name = "Đã hoàn tất")]
    Completed = 3,
    [Display(Name = "Không thể thực hiện")]
    Declined = 4
}
