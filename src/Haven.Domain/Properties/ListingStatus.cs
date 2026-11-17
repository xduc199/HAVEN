namespace Haven.Domain.Properties;

using System.ComponentModel.DataAnnotations;

public enum ListingStatus
{
    [Display(Name = "Bản nháp")] Draft = 1,
    [Display(Name = "Chờ duyệt")] Pending = 2,
    [Display(Name = "Đã duyệt")] Approved = 3,
    [Display(Name = "Bị từ chối")] Rejected = 4,
    [Display(Name = "Đã ẩn")] Hidden = 5
}
