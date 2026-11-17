using Haven.Web.ViewModels.Policies;
using Haven.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[Route("")]
public sealed class PoliciesController(IConfiguration configuration) : Controller
{

    private sealed record PolicyEdition(string Version, DateOnly EffectiveDate, string Title, PolicySectionViewModel[] Sections);

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, PolicyEdition>> Documents =
        new Dictionary<string, IReadOnlyDictionary<string, PolicyEdition>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Privacy"] = new Dictionary<string, PolicyEdition>(StringComparer.Ordinal) { ["1.0.0"] = new("1.0.0", new(2026, 11, 17), "Chính sách quyền riêng tư", [
                Section("1. HAVEN và phạm vi chính sách", "HAVEN là website cho phép người dùng tạo tài khoản, đăng và tìm kiếm tin bất động sản, lưu tin yêu thích và gửi yêu cầu tư vấn. HAVEN không nhận thanh toán, ký hợp đồng hay thay mặt các bên thực hiện giao dịch bất động sản. Chính sách này áp dụng khi bạn sử dụng website.", "Phiên bản 1.0.0 có hiệu lực từ ngày 17/11/2026."),
                Section("2. Dữ liệu được xử lý", "Khi đăng ký, HAVEN xử lý tên hiển thị, email đăng nhập và thông tin xác thực được Identity lưu dưới dạng bảo vệ. Hồ sơ có thể có số điện thoại nếu bạn tự bổ sung.", "Tin đăng gồm nội dung và thông tin bất động sản do người đăng cung cấp; ảnh và video do người dùng tải lên được lưu trong kho tệp của ứng dụng cùng metadata.", "Yêu cầu tư vấn gồm tên, email, số điện thoại và nội dung liên hệ do người gửi nhập. HAVEN cũng lưu tin yêu thích, yêu cầu tư vấn và lịch sử đồng ý chính sách gắn với tài khoản."),
                Section("3. Mục đích và căn cứ sử dụng", "Dữ liệu tài khoản được dùng để xác thực, bảo vệ tài khoản và cung cấp các chức năng đăng tin, quản lý tin, yêu thích và gửi yêu cầu tư vấn. Nội dung tin được dùng để kiểm duyệt, tìm kiếm và hiển thị theo trạng thái công khai.", "Thông tin liên hệ trong yêu cầu tư vấn được chuyển cho chủ tin liên quan và quản trị viên để phản hồi, xử lý. Thông tin đó không hiển thị công khai.", "HAVEN không tích hợp trong mã nguồn hiện tại công cụ phân tích quảng cáo, pixel tiếp thị hoặc dịch vụ gửi email. Cơ sở xử lý cụ thể cần được chủ thể vận hành xác nhận trước khi đưa website vào hoạt động thực tế."),
                Section("4. Thông tin công khai và quyền truy cập", "Tin đã duyệt có thể hiển thị công khai gồm nội dung tin, vị trí, giá, diện tích, hình ảnh/video và tên hiển thị người đăng. Email, số điện thoại và nội dung yêu cầu tư vấn không được công khai bởi trang tin.", "Người dùng chỉ xem và quản lý dữ liệu tài khoản, yêu cầu tư vấn của mình theo chức năng được cấp. Quản trị viên được phân quyền có thể truy cập dữ liệu cần thiết để kiểm duyệt và xử lý yêu cầu."),
                Section("5. Lưu trữ, chia sẻ và bảo vệ", "Dữ liệu được lưu trong cơ sở dữ liệu và kho tệp do ứng dụng cấu hình. Thời hạn lưu cụ thể phụ thuộc chính sách vận hành và nghĩa vụ lưu trữ được chủ dự án xác lập; HAVEN hiện chưa có chức năng tự động xóa tài khoản hoặc lịch tự xóa media.", "HAVEN không khai báo đối tác nhận dữ liệu bên thứ ba trong source hiện tại. Dữ liệu chỉ được cung cấp cho bên khác khi cần để thực hiện chức năng bạn yêu cầu, theo yêu cầu hợp pháp của cơ quan có thẩm quyền hoặc khi chủ dự án xác lập căn cứ phù hợp."),
                Section("6. Quyền và yêu cầu của bạn", "Bạn có thể xem dữ liệu hồ sơ, gửi yêu cầu truy cập, chỉnh sửa, hạn chế xử lý, xóa dữ liệu hoặc rút lại sự đồng ý tại mục Quyền riêng tư của tôi. Yêu cầu được ghi nhận để quản trị viên xem xét; gửi yêu cầu không đồng nghĩa mọi dữ liệu sẽ bị xóa tự động.", "Một số dữ liệu có thể cần được giữ lại để bảo vệ an toàn hệ thống, xử lý tranh chấp hoặc đáp ứng nghĩa vụ pháp luật. Quản trị viên sẽ ghi nhận kết quả xử lý trong hệ thống."),
                Section("7. Cookie và thay đổi chính sách", "HAVEN sử dụng cookie thiết yếu cho đăng nhập, chống giả mạo yêu cầu và thông báo tạm thời của website. Không có cookie phân tích hoặc quảng cáo được cấu hình trong source hiện tại. Xem Chính sách cookie để biết chi tiết.", "Khi nội dung chính sách thay đổi, phiên bản mới được công bố tại website. Phiên bản và thời điểm đồng ý của tài khoản được lưu để tra cứu."),
                Section("8. Thông tin đơn vị vận hành", "Tên đơn vị, địa chỉ và kênh hỗ trợ chưa được chủ dự án cung cấp nên website hiển thị trạng thái chưa cấu hình. Chủ dự án cần điền mục Haven:Legal trong cấu hình môi trường trước khi phát hành chính thức." )
            ]) },
            ["Terms"] = new Dictionary<string, PolicyEdition>(StringComparer.Ordinal) { ["1.0.0"] = new("1.0.0", new(2026, 11, 17), "Điều khoản sử dụng", [
                Section("1. Phạm vi dịch vụ", "HAVEN cung cấp chức năng tạo và đăng tin bất động sản, tìm kiếm, lưu yêu thích, so sánh và gửi yêu cầu tư vấn. Website không xử lý thanh toán, đặt cọc, ký kết hợp đồng hoặc thay mặt người dùng môi giới hay bán bất động sản."),
                Section("2. Tài khoản", "Bạn cần cung cấp thông tin chính xác, giữ bí mật thông tin đăng nhập và chịu trách nhiệm về hoạt động phát sinh từ tài khoản. Không chia sẻ tài khoản hoặc mạo danh người khác."),
                Section("3. Nội dung người dùng", "Người đăng chịu trách nhiệm về quyền sử dụng, độ chính xác và tính hợp pháp của tin, ảnh và video. Không đăng nội dung giả mạo, lừa đảo, xâm phạm quyền của người khác, chứa mã độc hoặc thông tin bị pháp luật cấm."),
                Section("4. Kiểm duyệt và trạng thái", "Tin mới có thể được lưu nháp hoặc gửi chờ duyệt. Chỉ tin đã duyệt mới xuất hiện công khai. HAVEN có thể từ chối, ẩn hoặc yêu cầu chỉnh sửa nội dung theo quy chế đăng tin và quy trình kiểm duyệt."),
                Section("5. Giới hạn vai trò HAVEN", "Thông tin trên website do người dùng cung cấp và cần được các bên tự kiểm chứng. HAVEN không xác nhận quyền sở hữu, tình trạng pháp lý, chất lượng hay khả năng giao dịch của bất động sản và không phải bên tham gia giao dịch."),
                Section("6. Ngừng sử dụng và liên hệ", "Bạn có thể ngừng sử dụng website và gửi yêu cầu về tài khoản tại trang Quyền riêng tư của tôi. Việc xử lý nội dung, dữ liệu và khiếu nại được thực hiện theo chính sách liên quan và quy định áp dụng.")
            ]) },
            ["PostingRules"] = new Dictionary<string, PolicyEdition>(StringComparer.Ordinal) { ["1.0.0"] = new("1.0.0", new(2026, 11, 17), "Quy chế đăng tin", [
                Section("1. Điều kiện nội dung", "Tin đăng cần mô tả rõ bất động sản, mục đích mua bán hoặc cho thuê, loại bất động sản, giá, diện tích và địa điểm. Người đăng chịu trách nhiệm về tính chính xác, quyền đăng tải và việc cập nhật nội dung."),
                Section("2. Ảnh và video", "Chỉ tải lên nội dung bạn có quyền sử dụng và có liên quan đến tin. Không đăng hình ảnh gây hiểu lầm, xâm phạm riêng tư, bản quyền, chứa thông tin cá nhân không cần thiết hoặc nội dung trái pháp luật. Loại tệp và giới hạn dung lượng được kiểm tra tại biểu mẫu đăng tin."),
                Section("3. Nội dung không được phép", "Không đăng tin giả, trùng lặp nhằm gây nhiễu, lừa đảo, phân biệt đối xử trái pháp luật, quảng cáo dịch vụ không liên quan, thông tin liên hệ của người khác hoặc nội dung bị cấm."),
                Section("4. Kiểm duyệt", "Tin được lưu ở trạng thái nháp, chờ duyệt, đã duyệt, bị từ chối hoặc đã ẩn. Tin chỉ được công khai sau khi duyệt. Khi bị từ chối, người đăng có thể xem lý do, chỉnh sửa và gửi duyệt lại. HAVEN có thể ẩn tin cần xem xét."),
                Section("5. Báo cáo vi phạm", "Người dùng có thể gửi phản ánh tại trang Tiếp nhận phản ánh và khiếu nại hoặc từ mục Quyền riêng tư của tôi. Hãy cung cấp đường dẫn/mã tin, mô tả sự việc và tài liệu liên quan nếu có.")
            ]) },
            ["Complaints"] = new Dictionary<string, PolicyEdition>(StringComparer.Ordinal) { ["1.0.0"] = new("1.0.0", new(2026, 11, 17), "Tiếp nhận phản ánh và khiếu nại", [
                Section("1. Nội dung tiếp nhận", "Bạn có thể phản ánh tin sai sự thật, dấu hiệu lừa đảo, xâm phạm quyền, nội dung không phù hợp hoặc vấn đề liên quan đến quyền riêng tư và dịch vụ HAVEN."),
                Section("2. Cách gửi", "Đăng nhập và gửi yêu cầu tại mục Quyền riêng tư của tôi. Với phản ánh tin đăng, chọn loại “Phản ánh tin đăng”, nhập mã tin nếu biết và mô tả cụ thể. Không gửi mật khẩu, mã xác thực hoặc dữ liệu nhạy cảm không cần thiết."),
                Section("3. Tiếp nhận và xem xét", "Yêu cầu được lưu riêng tư và chỉ người gửi cùng quản trị viên có quyền truy cập. Trạng thái ban đầu là mới tiếp nhận; quản trị viên có thể chuyển sang đang xử lý, hoàn tất hoặc từ chối và ghi kết quả."),
                Section("4. Bổ sung thông tin", "Hãy ghi thông tin liên hệ phù hợp trong hồ sơ tài khoản để HAVEN có thể yêu cầu làm rõ. HAVEN chưa cấu hình kênh hỗ trợ bên ngoài; thông tin liên hệ chính thức sẽ xuất hiện khi chủ dự án bổ sung cấu hình.")
            ]) },
            ["CookiePolicy"] = new Dictionary<string, PolicyEdition>(StringComparer.Ordinal) { ["1.0.0"] = new("1.0.0", new(2026, 11, 17), "Chính sách cookie", [
                Section("1. Cookie thiết yếu", "Cookie đăng nhập duy trì phiên xác thực; cookie chống giả mạo hỗ trợ bảo vệ biểu mẫu; cookie thông báo tạm thời giúp hiển thị kết quả thao tác. Các cookie này phục vụ chức năng cốt lõi và được cấu hình với thuộc tính bảo mật phù hợp."),
                Section("2. Cookie không thiết yếu", "Source hiện tại không cấu hình cookie phân tích, quảng cáo hoặc theo dõi của bên thứ ba. Nếu chức năng này được bổ sung, chính sách và cơ chế lựa chọn sẽ được cập nhật trước khi sử dụng."),
                Section("3. Quản lý cookie", "Bạn có thể xóa hoặc chặn cookie trong trình duyệt. Việc này có thể khiến đăng nhập hoặc một số biểu mẫu không hoạt động đúng.")
            ]) }
        };

    [HttpGet("Privacy")]
    public IActionResult Privacy(string? version = null) => Render("Privacy", version);
    [HttpGet("Terms")]
    public IActionResult Terms(string? version = null) => Render("Terms", version);
    [HttpGet("PostingRules")]
    public IActionResult PostingRules(string? version = null) => Render("PostingRules", version);
    [HttpGet("Complaints")]
    public IActionResult Complaints(string? version = null) => Render("Complaints", version);
    [HttpGet("CookiePolicy")]
    public IActionResult CookiePolicy(string? version = null) => Render("CookiePolicy", version);

    [HttpGet("About")]
    public IActionResult About()
    {
        ViewData["Title"] = "Giới thiệu HAVEN";
        return View("About");
    }

    private IActionResult Render(string key, string? version)
    {
        var currentVersion = PolicyVersions.CurrentFor(key);
        var document = Documents[key].GetValueOrDefault(version ?? currentVersion);
        if (document is null) return NotFound();
        ViewData["Title"] = document.Title;
        var legal = configuration.GetSection("Haven:Legal");
        string Value(string name) => string.IsNullOrWhiteSpace(legal[name]) ? "Chưa được chủ dự án cấu hình" : legal[name]!;
        return View("Document", new PolicyPageViewModel(key, document.Title, document.Version, document.EffectiveDate,
            document.Sections, Documents.Select(item => (item.Key, item.Value[PolicyVersions.CurrentFor(item.Key)].Title)).Where(item => item.Key != key).ToArray(),
            Value("OperatorName"), Value("SupportEmail"), Value("SupportPhone"), Value("SupportAddress")));
    }

    private static PolicySectionViewModel Section(string heading, params string[] paragraphs) => new(heading, paragraphs);
}
