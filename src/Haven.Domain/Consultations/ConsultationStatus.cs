namespace Haven.Domain.Consultations;

using System.ComponentModel.DataAnnotations;

public enum ConsultationStatus
{
    [Display(Name = "Mới tiếp nhận")] New,
    [Display(Name = "Đang xử lý")] InProgress,
    [Display(Name = "Đã hoàn tất")] Completed,
    [Display(Name = "Đã hủy")] Cancelled
}
