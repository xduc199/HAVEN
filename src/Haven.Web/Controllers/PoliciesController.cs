using Haven.Web.Services;
using Haven.Web.ViewModels.Policies;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[Route("")]
public sealed class PoliciesController(IConfiguration configuration) : Controller
{
    private sealed record PolicyEdition(
        string Version,
        DateOnly EffectiveDate,
        string Title,
        string Summary,
        PolicySectionViewModel[] Sections);

    private static readonly DateOnly FirstEditionDate = new(2026, 11, 17);
    private static readonly DateOnly CurrentEditionDate = PolicyVersions.EffectiveDate;

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, PolicyEdition>> Documents =
        new Dictionary<string, IReadOnlyDictionary<string, PolicyEdition>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Privacy"] = Editions(
                new("1.0.0", FirstEditionDate, "Chính sách quyền riêng tư", "Thông tin về dữ liệu HAVEN xử lý, mục đích sử dụng và cách gửi yêu cầu liên quan đến dữ liệu cá nhân.", [
                    Section("1. HAVEN và phạm vi chính sách", "HAVEN là website cho phép người dùng tạo tài khoản, đăng và tìm kiếm tin bất động sản, lưu tin yêu thích và gửi yêu cầu tư vấn. HAVEN không nhận thanh toán, ký hợp đồng hay thay mặt các bên thực hiện giao dịch bất động sản. Chính sách này áp dụng khi bạn sử dụng website.", "Phiên bản 1.0.0 có hiệu lực từ ngày 17/11/2026."),
                    Section("2. Dữ liệu được xử lý", "Khi đăng ký, HAVEN xử lý tên hiển thị, email đăng nhập và thông tin xác thực được Identity lưu dưới dạng bảo vệ. Hồ sơ có thể có số điện thoại nếu bạn tự bổ sung.", "Tin đăng gồm nội dung và thông tin bất động sản do người đăng cung cấp; ảnh và video do người dùng tải lên được lưu trong kho tệp của ứng dụng cùng metadata.", "Yêu cầu tư vấn gồm tên, email, số điện thoại và nội dung liên hệ do người gửi nhập. HAVEN cũng lưu tin yêu thích, yêu cầu tư vấn và lịch sử đồng ý chính sách gắn với tài khoản."),
                    Section("3. Mục đích và căn cứ sử dụng", "Dữ liệu tài khoản được dùng để xác thực, bảo vệ tài khoản và cung cấp các chức năng đăng tin, quản lý tin, yêu thích và gửi yêu cầu tư vấn. Nội dung tin được dùng để kiểm duyệt, tìm kiếm và hiển thị theo trạng thái công khai.", "Thông tin liên hệ trong yêu cầu tư vấn được chuyển cho chủ tin liên quan và quản trị viên để phản hồi, xử lý. Thông tin đó không hiển thị công khai.", "HAVEN không tích hợp trong mã nguồn hiện tại công cụ phân tích quảng cáo, pixel tiếp thị hoặc dịch vụ gửi email. Cơ sở xử lý cụ thể cần được chủ thể vận hành xác nhận trước khi đưa website vào hoạt động thực tế."),
                    Section("4. Thông tin công khai và quyền truy cập", "Tin đã duyệt có thể hiển thị công khai gồm nội dung tin, vị trí, giá, diện tích, hình ảnh/video và tên hiển thị người đăng. Email, số điện thoại và nội dung yêu cầu tư vấn không được công khai bởi trang tin.", "Người dùng chỉ xem và quản lý dữ liệu tài khoản, yêu cầu tư vấn của mình theo chức năng được cấp. Quản trị viên được phân quyền có thể truy cập dữ liệu cần thiết để kiểm duyệt và xử lý yêu cầu."),
                    Section("5. Lưu trữ, chia sẻ và bảo vệ", "Dữ liệu được lưu trong cơ sở dữ liệu và kho tệp do ứng dụng cấu hình. Thời hạn lưu cụ thể phụ thuộc chính sách vận hành và nghĩa vụ lưu trữ được chủ dự án xác lập; HAVEN hiện chưa có chức năng tự động xóa tài khoản hoặc lịch tự xóa media.", "HAVEN không khai báo đối tác nhận dữ liệu bên thứ ba trong source hiện tại. Dữ liệu chỉ được cung cấp cho bên khác khi cần để thực hiện chức năng bạn yêu cầu, theo yêu cầu hợp pháp của cơ quan có thẩm quyền hoặc khi chủ dự án xác lập căn cứ phù hợp."),
                    Section("6. Quyền và yêu cầu của bạn", "Bạn có thể xem dữ liệu hồ sơ, gửi yêu cầu truy cập, chỉnh sửa, hạn chế xử lý, xóa dữ liệu hoặc rút lại sự đồng ý tại mục Quyền riêng tư của tôi. Yêu cầu được ghi nhận để quản trị viên xem xét; gửi yêu cầu không đồng nghĩa mọi dữ liệu sẽ bị xóa tự động.", "Một số dữ liệu có thể cần được giữ lại để bảo vệ an toàn hệ thống, xử lý tranh chấp hoặc đáp ứng nghĩa vụ pháp luật. Quản trị viên sẽ ghi nhận kết quả xử lý trong hệ thống."),
                    Section("7. Cookie và thay đổi chính sách", "HAVEN sử dụng cookie thiết yếu cho đăng nhập, chống giả mạo yêu cầu và thông báo tạm thời của website. Không có cookie phân tích hoặc quảng cáo được cấu hình trong source hiện tại. Xem Chính sách cookie để biết chi tiết.", "Khi nội dung chính sách thay đổi, phiên bản mới được công bố tại website. Phiên bản và thời điểm đồng ý của tài khoản được lưu để tra cứu."),
                    Section("8. Thông tin đơn vị vận hành", "Tên đơn vị, địa chỉ và kênh hỗ trợ chưa được chủ dự án cung cấp nên website hiển thị trạng thái chưa cấu hình. Chủ dự án cần điền mục Haven:Legal trong cấu hình môi trường trước khi phát hành chính thức." )
                ]),
                new(PolicyVersions.Privacy, CurrentEditionDate, "Chính sách quyền riêng tư", "Chính sách giải thích dữ liệu được xử lý khi bạn tạo tài khoản, sử dụng hồ sơ, đăng tin, gửi yêu cầu tư vấn hoặc yêu cầu hỗ trợ.", [
                    Section("1. Phạm vi và đơn vị vận hành", "Chính sách áp dụng cho người truy cập và người có tài khoản HAVEN. Website cung cấp công cụ tìm kiếm, hiển thị tin bất động sản, quản lý tài khoản và tin đăng, yêu thích, so sánh, gửi yêu cầu tư vấn và gửi yêu cầu về dữ liệu.", "Thông tin pháp nhân vận hành và kênh liên hệ hiện chưa được cấu hình. Vì vậy, tài liệu này mô tả chức năng của ứng dụng; chủ dự án cần xác nhận danh tính bên chịu trách nhiệm xử lý dữ liệu và cập nhật thông tin liên hệ trước khi phát hành chính thức."),
                    Section("2. Dữ liệu ứng dụng xử lý", "Tài khoản gồm tên hiển thị, email đăng nhập và thông tin xác thực do ASP.NET Core Identity quản lý. Hồ sơ có thể có số điện thoại và ảnh đại diện nếu người dùng tự bổ sung. Mật khẩu không được hiển thị trong giao diện.", "Tin đăng gồm tiêu đề, mô tả, loại hình, mục đích mua bán/cho thuê, giá, diện tích, vị trí, thông tin phòng, hình ảnh và video do người đăng nhập. Ảnh tin được xác thực về phần mở rộng, MIME type, chữ ký nội dung và dung lượng; video cũng có giới hạn định dạng, số lượng và dung lượng.", "Khi gửi yêu cầu tư vấn, người dùng nhập tên liên hệ, email, số điện thoại, tin đăng liên quan và nội dung. Trung tâm quyền riêng tư lưu loại yêu cầu, mã tin nếu có, nội dung, trạng thái và kết quả xử lý. Ứng dụng cũng lưu lịch sử chấp thuận chính sách và danh sách yêu thích gắn với tài khoản."),
                    Section("3. Mục đích xử lý", "Thông tin tài khoản được dùng để đăng nhập, quản lý hồ sơ và phân quyền. Thông tin tin đăng được dùng để lưu, kiểm duyệt, tìm kiếm và hiển thị những tin được duyệt. Thông tin liên hệ trong yêu cầu tư vấn được dùng để quản trị viên xem xét và xử lý yêu cầu.", "Dữ liệu yêu cầu quyền riêng tư được dùng để hiển thị lịch sử cho chính người gửi và hỗ trợ quản trị viên ghi nhận trạng thái xử lý. Nhật ký đồng ý chính sách hỗ trợ tra cứu tài liệu và phiên bản người dùng đã chấp thuận."),
                    Section("4. Thông tin hiển thị công khai", "Tin có trạng thái đã duyệt được hiển thị công khai theo các trường của tin đăng. Hồ sơ công khai của người đăng hiển thị tên hiển thị, ảnh đại diện và các tin đã duyệt của tài khoản đó.", "Email đăng nhập, số điện thoại hồ sơ và nội dung yêu cầu tư vấn không được đưa vào trang hồ sơ công khai. Người đăng không nên đưa thông tin cá nhân của mình hoặc người khác vào phần mô tả, ảnh hay video nếu không cần thiết."),
                    Section("5. Nơi lưu trữ, quyền truy cập và bên thứ ba", "Ứng dụng sử dụng cơ sở dữ liệu SQL Server và kho tệp được cấu hình cho ảnh/video. Source hiện tại không xác định thời hạn lưu trữ chung, lịch tự động xóa tài khoản hoặc tự động xóa tệp media. Thời hạn cụ thể cần được đơn vị vận hành xác lập phù hợp với mục đích và nghĩa vụ pháp luật.", "Quản trị viên được phân quyền có thể truy cập dữ liệu cần thiết để kiểm duyệt tin và xử lý yêu cầu. Các yêu cầu trong Trung tâm quyền riêng tư chỉ người gửi và quản trị viên có quyền xem.", "Trang quản trị hiện tải phông chữ từ Google Fonts. Trình duyệt có thể gửi yêu cầu kỹ thuật đến nhà cung cấp khi tải tài nguyên này. Source không khai báo công cụ analytics, pixel quảng cáo hay dịch vụ gửi email; cấu hình khi triển khai thực tế cần được rà soát lại."),
                    Section("6. Cookie và công nghệ lưu trữ", "Ứng dụng cấu hình cookie xác thực Haven.Auth và cookie chống giả mạo Haven.Antiforgery. Cookie xác thực được đánh dấu HttpOnly và SameSite=Lax; chính sách Secure được áp dụng luôn trong môi trường production và phụ thuộc giao thức trong Development. MVC cũng dùng TempData để thông báo một lần sau thao tác.", "Trong mã ứng dụng hiện tại không tìm thấy việc sử dụng localStorage hoặc sessionStorage cho dữ liệu người dùng. Xem Chính sách cookie để biết thêm về chức năng, tùy chọn trình duyệt và tác động khi chặn cookie."),
                    Section("7. Quyền của bạn và cách gửi yêu cầu", "Pháp luật có thể trao cho chủ thể dữ liệu các quyền như được biết, truy cập, chỉnh sửa, yêu cầu xóa, hạn chế hoặc phản đối xử lý, rút lại sự đồng ý khi căn cứ đó được áp dụng, khiếu nại và yêu cầu bảo vệ quyền lợi. Việc áp dụng từng quyền phụ thuộc pháp luật và hoàn cảnh xử lý cụ thể.", "Người đã đăng nhập có thể gửi yêu cầu tại mục Quyền riêng tư của tôi. Biểu mẫu ghi nhận yêu cầu để quản trị viên xem xét và cập nhật trạng thái/kết quả; hiện biểu mẫu không tự thực hiện việc sửa, xóa hay hạn chế dữ liệu. Việc gửi yêu cầu không làm mất các quyền theo pháp luật và không đồng nghĩa yêu cầu đã được chấp thuận."),
                    Section("8. Bảo vệ dữ liệu và dữ liệu trẻ em", "Ứng dụng áp dụng các kiểm soát trong source như xác thực, phân quyền, antiforgery và kiểm tra tệp tải lên. Những kiểm soát này không phải cam kết an toàn tuyệt đối; biện pháp vận hành, sao lưu, lưu trữ và ứng phó sự cố phải được đơn vị triển khai xác nhận riêng.", "HAVEN chưa có bước xác minh độ tuổi hoặc quy trình riêng cho dữ liệu trẻ em. Người giám hộ cần cân nhắc trước khi trẻ cung cấp thông tin hoặc đăng tải nội dung."),
                    Section("9. Cập nhật chính sách và liên hệ", "Khi chính sách thay đổi, phiên bản và ngày hiệu lực được hiển thị tại đầu tài liệu. Lịch sử chấp thuận được lưu cho tài khoản theo chức năng hiện có.", "Tên đơn vị vận hành, email, số điện thoại và địa chỉ hiện chưa được cấu hình. Các thông tin này cần được bổ sung trước khi HAVEN vận hành với người dùng thực tế.")
                ])),

            ["Terms"] = Editions(
                new("1.0.0", FirstEditionDate, "Điều khoản sử dụng", "Các nguyên tắc sử dụng nền tảng và trách nhiệm khi tạo tài khoản, đăng hoặc tìm kiếm tin bất động sản.", [
                    Section("1. Phạm vi dịch vụ", "HAVEN cung cấp chức năng tạo và đăng tin bất động sản, tìm kiếm, lưu yêu thích, so sánh và gửi yêu cầu tư vấn. Website không xử lý thanh toán, đặt cọc, ký kết hợp đồng hoặc thay mặt người dùng môi giới hay bán bất động sản."),
                    Section("2. Tài khoản", "Bạn cần cung cấp thông tin chính xác, giữ bí mật thông tin đăng nhập và chịu trách nhiệm về hoạt động phát sinh từ tài khoản. Không chia sẻ tài khoản hoặc mạo danh người khác."),
                    Section("3. Nội dung người dùng", "Người đăng chịu trách nhiệm về quyền sử dụng, độ chính xác và tính hợp pháp của tin, ảnh và video. Không đăng nội dung giả mạo, lừa đảo, xâm phạm quyền của người khác, chứa mã độc hoặc thông tin bị pháp luật cấm."),
                    Section("4. Kiểm duyệt và trạng thái", "Tin mới có thể được lưu nháp hoặc gửi chờ duyệt. Chỉ tin đã duyệt mới xuất hiện công khai. HAVEN có thể từ chối, ẩn hoặc yêu cầu chỉnh sửa nội dung theo quy chế đăng tin và quy trình kiểm duyệt."),
                    Section("5. Giới hạn vai trò HAVEN", "Thông tin trên website do người dùng cung cấp và cần được các bên tự kiểm chứng. HAVEN không xác nhận quyền sở hữu, tình trạng pháp lý, chất lượng hay khả năng giao dịch của bất động sản và không phải bên tham gia giao dịch."),
                    Section("6. Ngừng sử dụng và liên hệ", "Bạn có thể ngừng sử dụng website và gửi yêu cầu về tài khoản tại trang Quyền riêng tư của tôi. Việc xử lý nội dung, dữ liệu và khiếu nại được thực hiện theo chính sách liên quan và quy định áp dụng.")
                ]),
                new(PolicyVersions.Terms, CurrentEditionDate, "Điều khoản sử dụng", "Điều khoản quy định vai trò của HAVEN, trách nhiệm tài khoản, nội dung tin và cách nền tảng xử lý nội dung vi phạm.", [
                    Section("1. Phạm vi và chấp thuận", "Điều khoản áp dụng khi bạn truy cập hoặc sử dụng website HAVEN. Khi tạo tài khoản, biểu mẫu yêu cầu bạn xác nhận đã đọc và đồng ý với Điều khoản sử dụng cùng Chính sách quyền riêng tư. Việc chấp thuận được ghi nhận cùng phiên bản và thời điểm trong tài khoản.", "Điều khoản mô tả cách ứng dụng hoạt động; không thay thế thỏa thuận riêng giữa người mua, người thuê, người bán hoặc người cho thuê."),
                    Section("2. Vai trò của HAVEN", "HAVEN cung cấp công cụ đăng tin, tìm kiếm, hiển thị thông tin, quản lý yêu thích, so sánh và gửi yêu cầu tư vấn. Trong phần mềm hiện có, HAVEN không nhận tiền đặt cọc hoặc thanh toán, không ký hợp đồng bất động sản thay người dùng và không xác nhận quyền sở hữu hay tình trạng pháp lý của tài sản.", "Mô tả trên phản ánh chức năng của ứng dụng, không tự xác định phân loại pháp lý của đơn vị vận hành. Các bên phải tự kiểm tra thông tin, giấy tờ và điều kiện giao dịch trước khi thỏa thuận."),
                    Section("3. Tài khoản và bảo mật", "Bạn cần cung cấp thông tin đúng, cập nhật thông tin hồ sơ khi cần và bảo vệ thông tin đăng nhập. Không dùng tài khoản để mạo danh người khác, can thiệp trái phép vào hệ thống hoặc thực hiện hành vi vi phạm pháp luật.", "Nếu phát hiện tài khoản bị truy cập trái phép, hãy gửi yêu cầu tại mục Quyền riêng tư của tôi. HAVEN chưa cấu hình số điện thoại hoặc email hỗ trợ công khai."),
                    Section("4. Quyền và trách nhiệm của người đăng", "Người đăng phải có quyền hoặc căn cứ phù hợp để đăng tải tin, hình ảnh, video và thông tin liên quan. Người đăng chịu trách nhiệm về tính chính xác, cập nhật và hợp pháp của nội dung mình cung cấp; không được giả mạo chủ sở hữu, môi giới hoặc bên liên quan.", "Người đăng có thể quản lý tin qua khu vực Tin đăng của tôi theo trạng thái của tin. Tính năng chỉnh sửa và xóa phụ thuộc trạng thái cụ thể được nêu trong Quy chế đăng tin."),
                    Section("5. Nội dung và hành vi bị cấm", "Không đăng tin sai sự thật, lừa đảo, trùng lặp có chủ đích, nội dung xâm phạm quyền riêng tư hoặc sở hữu trí tuệ, thông tin liên hệ của người khác khi chưa được phép, mã độc, quảng cáo không liên quan hay nội dung bị pháp luật cấm.", "Không sử dụng website để thu thập trái phép dữ liệu, gây gián đoạn dịch vụ, né kiểm duyệt hoặc gây thiệt hại cho người dùng khác."),
                    Section("6. Kiểm duyệt và biện pháp xử lý", "Tin được tạo ở trạng thái nháp, sau đó có thể gửi chờ duyệt. Chỉ tin đã duyệt mới được hiển thị công khai. Tin có thể bị từ chối hoặc ẩn theo quy trình quản trị hiện có; lý do từ chối được hiển thị trong khu vực quản lý tin.", "Khi có căn cứ về nội dung vi phạm, HAVEN có thể yêu cầu chỉnh sửa, từ chối hoặc ẩn tin theo chức năng hiện có, và áp dụng biện pháp với tài khoản theo quy định hệ thống. Việc đánh giá được thực hiện dựa trên thông tin có được, không phải xác nhận tính pháp lý của bất động sản."),
                    Section("7. Yêu cầu tư vấn và giao dịch giữa các bên", "Yêu cầu tư vấn được gửi qua chức năng tương ứng và có thể chứa thông tin liên hệ do bạn nhập. Người dùng cần chỉ cung cấp thông tin cần thiết. Các bên tự trao đổi, thẩm định giấy tờ, thương lượng và lập thỏa thuận phù hợp; HAVEN không thể hiện chức năng thanh toán hoặc ký kết giao dịch trên website."),
                    Section("8. Quyền sở hữu trí tuệ", "Tên gọi, logo, giao diện và nội dung do HAVEN tạo ra thuộc quyền của chủ thể có quyền tương ứng. Người đăng giữ quyền đối với nội dung họ cung cấp và chịu trách nhiệm có quyền sử dụng nội dung đó.", "Khi đăng tin, bạn cho phép hệ thống lưu trữ, xử lý và hiển thị nội dung trong phạm vi cần thiết để cung cấp chức năng đăng tin, kiểm duyệt và tìm kiếm. Phạm vi này không chuyển quyền sở hữu nội dung sang HAVEN."),
                    Section("9. Thông tin, tính sẵn sàng và trách nhiệm", "HAVEN hiển thị thông tin do người dùng cung cấp. Người xem cần xác minh giá, địa điểm, quyền sử dụng, hồ sơ pháp lý và hiện trạng tài sản trực tiếp với các bên liên quan.", "Điều khoản không loại trừ trách nhiệm hoặc quyền của người dùng trong trường hợp pháp luật không cho phép loại trừ. HAVEN không cam kết website luôn hoạt động liên tục hoặc mọi thông tin người dùng đăng đều chính xác."),
                    Section("10. Quyền riêng tư", "Dữ liệu được xử lý theo Chính sách quyền riêng tư và Chính sách cookie. Bạn có thể gửi yêu cầu liên quan đến dữ liệu tại Trung tâm quyền riêng tư; yêu cầu được ghi nhận để xem xét và không tự động thực hiện thao tác xóa hoặc chỉnh sửa."),
                    Section("11. Thay đổi điều khoản", "Phiên bản hiện hành và ngày hiệu lực được hiển thị ở đầu trang. Khi nội dung được cập nhật, HAVEN công bố phiên bản mới trên website. Việc áp dụng yêu cầu chấp thuận lại, nếu có, phải tuân theo quy trình và quy định pháp luật tương ứng."),
                    Section("12. Pháp luật và phản ánh", "Việc sử dụng HAVEN chịu sự điều chỉnh của pháp luật Việt Nam trong phạm vi áp dụng. Bạn có thể gửi phản ánh hoặc yêu cầu tại mục Quyền riêng tư của tôi. Thông tin đơn vị vận hành và kênh liên hệ ngoài website sẽ được bổ sung khi được chủ dự án xác nhận.")
                ])),

            ["PostingRules"] = Editions(
                new("1.0.0", FirstEditionDate, "Quy chế đăng tin", "Yêu cầu đối với nội dung, hình ảnh, trạng thái duyệt và việc quản lý tin bất động sản.", [
                    Section("1. Điều kiện nội dung", "Tin đăng cần mô tả rõ bất động sản, mục đích mua bán hoặc cho thuê, loại bất động sản, giá, diện tích và địa điểm. Người đăng chịu trách nhiệm về tính chính xác, quyền đăng tải và việc cập nhật nội dung."),
                    Section("2. Ảnh và video", "Chỉ tải lên nội dung bạn có quyền sử dụng và có liên quan đến tin. Không đăng hình ảnh gây hiểu lầm, xâm phạm riêng tư, bản quyền, chứa thông tin cá nhân không cần thiết hoặc nội dung trái pháp luật. Loại tệp và giới hạn dung lượng được kiểm tra tại biểu mẫu đăng tin."),
                    Section("3. Nội dung không được phép", "Không đăng tin giả, trùng lặp nhằm gây nhiễu, lừa đảo, phân biệt đối xử trái pháp luật, quảng cáo dịch vụ không liên quan, thông tin liên hệ của người khác hoặc nội dung bị cấm."),
                    Section("4. Kiểm duyệt", "Tin được lưu ở trạng thái nháp, chờ duyệt, đã duyệt, bị từ chối hoặc đã ẩn. Tin chỉ được công khai sau khi duyệt. Khi bị từ chối, người đăng có thể xem lý do, chỉnh sửa và gửi duyệt lại. HAVEN có thể ẩn tin cần xem xét."),
                    Section("5. Báo cáo vi phạm", "Người dùng có thể gửi phản ánh tại trang Tiếp nhận phản ánh và khiếu nại hoặc từ mục Quyền riêng tư của tôi. Hãy cung cấp đường dẫn/mã tin, mô tả sự việc và tài liệu liên quan nếu có.")
                ]),
                new(PolicyVersions.PostingRules, CurrentEditionDate, "Quy chế đăng tin", "Quy định cách tạo, gửi duyệt, cập nhật và xử lý tin bất động sản trên HAVEN.", [
                    Section("1. Phạm vi và điều kiện đăng", "Quy chế áp dụng cho cá nhân tạo hoặc quản lý tin trên HAVEN. Bạn cần đăng nhập để tạo tin và phải có quyền hoặc căn cứ hợp pháp phù hợp để giới thiệu bất động sản. Việc có thể đăng tin trên website không thay thế điều kiện pháp lý riêng của hoạt động kinh doanh, môi giới hoặc giao dịch nếu pháp luật yêu cầu."),
                    Section("2. Thông tin cần cung cấp", "Biểu mẫu đăng tin yêu cầu thông tin như tiêu đề, mô tả, loại hình bất động sản, mục đích mua bán hoặc cho thuê, giá, diện tích, địa điểm và một số đặc điểm của tài sản. Chỉ cung cấp thông tin liên hệ hoặc địa chỉ chi tiết ở mức cần thiết và có quyền cung cấp.", "Giá, diện tích, địa chỉ, tình trạng pháp lý, quy hoạch và quyền của bên thứ ba phải được mô tả trung thực. Nếu thông tin thay đổi, người đăng cần cập nhật tin theo khả năng quản lý được cung cấp."),
                    Section("3. Tiêu đề và mô tả", "Tiêu đề phải nhận diện được bất động sản, không gây hiểu nhầm hoặc dùng tuyên bố không có căn cứ. Mô tả cần phân biệt thông tin đã xác minh với thông tin tham khảo; không lặp từ khóa nhằm thao túng tìm kiếm, không chèn quảng cáo không liên quan."),
                    Section("4. Hình ảnh và video", "Ảnh phải liên quan đến bất động sản, không gây hiểu lầm và không xâm phạm quyền của người khác. Ứng dụng chấp nhận ảnh JPEG, PNG hoặc WebP, tối đa 10 ảnh mỗi tin và 10 MB mỗi ảnh. Video được giới hạn tối đa 3 tệp, định dạng MP4 hoặc WebM và tối đa 100 MB mỗi tệp.", "Không tải lên hình ảnh/video có watermark của bên khác nếu không được phép, tài liệu nhạy cảm, mã QR dẫn đến nội dung nguy hiểm hoặc hình ảnh nhận diện người khác khi chưa có căn cứ phù hợp."),
                    Section("5. Nội dung không được phép", "Không đăng tin giả, tài sản không có căn cứ giới thiệu, giả mạo chủ sở hữu hoặc người môi giới, lừa đảo, trùng lặp nhằm gây nhiễu, xâm phạm quyền riêng tư hoặc sở hữu trí tuệ, phân biệt đối xử trái pháp luật, nội dung bị cấm hoặc quảng cáo không liên quan."),
                    Section("6. Trạng thái và kiểm duyệt", "Tin mới được tạo dưới dạng bản nháp. Khi gửi duyệt, tin chuyển sang chờ duyệt; chỉ tin đã duyệt mới xuất hiện công khai. Quản trị viên có thể duyệt, từ chối kèm lý do hoặc ẩn tin theo chức năng quản trị hiện có.", "Các trạng thái trên website gồm Bản nháp, Chờ duyệt, Đã duyệt, Bị từ chối và Đã ẩn. Trạng thái có thể ảnh hưởng đến khả năng xem, sửa hoặc gửi duyệt lại."),
                    Section("7. Chỉnh sửa và xóa tin", "Chủ tin có thể chỉnh sửa tin ở trạng thái bản nháp, bị từ chối hoặc đã duyệt. Tin đã duyệt sau khi chỉnh sửa cần được gửi duyệt lại theo quy trình hiện có.", "Chức năng xóa của người dùng hiện áp dụng cho tin nháp. Tin đang chờ duyệt hoặc đã ẩn không thể chỉnh sửa từ biểu mẫu người dùng; hãy gửi yêu cầu qua Trung tâm quyền riêng tư nếu cần HAVEN xem xét."),
                    Section("8. Phản ánh và xử lý vi phạm", "Người dùng đã đăng nhập có thể phản ánh tin qua trang Phản ánh và khiếu nại/Quyền riêng tư của tôi. Với phản ánh tin, biểu mẫu yêu cầu mã tin; nội dung cần nêu vấn đề cụ thể. Hiện biểu mẫu không hỗ trợ đính kèm tệp.", "Khi có căn cứ, tin có thể bị từ chối hoặc ẩn. Hành vi vi phạm có thể được chuyển đến quản trị viên xem xét theo quyền quản trị hiện có; quy chế này không đặt ra thời hạn xử lý cụ thể."),
                    Section("9. Trách nhiệm các bên", "Người đăng chịu trách nhiệm về nội dung và quyền sử dụng tài liệu đã cung cấp. HAVEN kiểm duyệt nội dung theo quy trình của nền tảng nhưng việc duyệt tin không phải xác nhận quyền sở hữu, chất lượng, tính pháp lý hay khả năng giao dịch của tài sản."),
                    Section("10. Cập nhật quy chế", "Phiên bản và ngày hiệu lực hiện hành được hiển thị ở đầu trang. Quy chế có thể được cập nhật để phản ánh thay đổi của chức năng hoặc yêu cầu pháp luật; các phiên bản đã công bố có thể được xem nếu còn được hệ thống lưu.")
                ])),

            ["Complaints"] = Editions(
                new("1.0.0", FirstEditionDate, "Tiếp nhận phản ánh và khiếu nại", "Hướng dẫn gửi phản ánh về tin đăng, quyền riêng tư, tài khoản hoặc quá trình sử dụng HAVEN.", [
                    Section("1. Nội dung tiếp nhận", "Bạn có thể phản ánh tin sai sự thật, dấu hiệu lừa đảo, xâm phạm quyền, nội dung không phù hợp hoặc vấn đề liên quan đến quyền riêng tư và dịch vụ HAVEN."),
                    Section("2. Cách gửi", "Đăng nhập và gửi yêu cầu tại mục Quyền riêng tư của tôi. Với phản ánh tin đăng, chọn loại “Phản ánh tin đăng”, nhập mã tin nếu biết và mô tả cụ thể. Không gửi mật khẩu, mã xác thực hoặc dữ liệu nhạy cảm không cần thiết."),
                    Section("3. Tiếp nhận và xem xét", "Yêu cầu được lưu riêng tư và chỉ người gửi cùng quản trị viên có quyền truy cập. Trạng thái ban đầu là mới tiếp nhận; quản trị viên có thể chuyển sang đang xử lý, hoàn tất hoặc từ chối và ghi kết quả."),
                    Section("4. Bổ sung thông tin", "Hãy ghi thông tin liên hệ phù hợp trong hồ sơ tài khoản để HAVEN có thể yêu cầu làm rõ. HAVEN chưa cấu hình kênh hỗ trợ bên ngoài; thông tin liên hệ chính thức sẽ xuất hiện khi chủ dự án bổ sung cấu hình.")
                ]),
                new(PolicyVersions.Complaints, CurrentEditionDate, "Phản ánh và khiếu nại", "Gửi yêu cầu về tin đăng đáng ngờ, tài khoản, quyền riêng tư hoặc vấn đề kỹ thuật theo kênh hiện có.", [
                    Section("1. Những vấn đề có thể gửi", "Bạn có thể phản ánh tin có thông tin không chính xác, dấu hiệu lừa đảo, mạo danh, nội dung có thể xâm phạm quyền hoặc vi phạm Quy chế đăng tin. Bạn cũng có thể gửi yêu cầu về tài khoản, dữ liệu cá nhân hoặc lỗi khi sử dụng website."),
                    Section("2. Kênh tiếp nhận hiện có", "Biểu mẫu được cung cấp tại mục Quyền riêng tư của tôi và yêu cầu người gửi đăng nhập. HAVEN chưa cấu hình email, số điện thoại, địa chỉ hoặc biểu mẫu công khai khác; các trường thông tin hỗ trợ trên trang sẽ hiển thị là chưa được cấu hình cho đến khi chủ dự án cung cấp.", "Với phản ánh về tin đăng, biểu mẫu yêu cầu mã tin. Nếu vấn đề liên quan đến trang khác, hãy mô tả đường dẫn và thao tác dẫn đến sự cố trong nội dung yêu cầu."),
                    Section("3. Thông tin nên cung cấp", "Nêu sự việc cụ thể, thời điểm phát hiện, đường dẫn hoặc mã tin liên quan và lý do bạn cho rằng cần xem xét. Biểu mẫu hiện nhận nội dung văn bản tối đa 3.000 ký tự và không hỗ trợ tải tài liệu hoặc ảnh đính kèm.", "Không gửi mật khẩu, mã OTP, thông tin thanh toán hoặc giấy tờ cá nhân đầy đủ nếu không cần thiết. Chỉ chia sẻ dữ liệu liên quan trực tiếp đến yêu cầu."),
                    Section("4. Quy trình được hỗ trợ", "Sau khi gửi, yêu cầu được lưu để người gửi xem lại trong tài khoản và quản trị viên xem xét. Trạng thái có thể là Mới tiếp nhận, Đang xử lý, Đã hoàn tất hoặc Không thể thực hiện; quản trị viên có thể ghi kết quả xử lý.", "Hệ thống không quy định thời hạn phản hồi hoặc cơ chế tự động ưu tiên. Trang này không cam kết một thời gian giải quyết cụ thể."),
                    Section("5. Quyền riêng tư của yêu cầu", "Yêu cầu gắn với tài khoản và nội dung không hiển thị công khai. Người gửi xem được yêu cầu của mình; quản trị viên được phân quyền có thể truy cập để xử lý. Hãy tham khảo Chính sách quyền riêng tư về mục đích và cách lưu dữ liệu."),
                    Section("6. Trách nhiệm của người phản ánh", "Cung cấp thông tin trung thực, có liên quan và không dùng cơ chế phản ánh để quấy rối, vu khống hoặc xâm phạm quyền của người khác. Việc gửi phản ánh không đồng nghĩa nội dung bị báo cáo đã được xác định là vi phạm."),
                    Section("7. Liên hệ và trường hợp khẩn cấp", "Chưa có kênh hỗ trợ bên ngoài được cấu hình trong ứng dụng. Nếu nội dung phản ánh liên quan nguy cơ tức thời hoặc hành vi có dấu hiệu vi phạm pháp luật, bạn nên liên hệ cơ quan có thẩm quyền phù hợp; biểu mẫu HAVEN không thay thế kênh khẩn cấp hoặc thủ tục khiếu nại theo pháp luật.")
                ])),

            ["CookiePolicy"] = Editions(
                new("1.0.0", FirstEditionDate, "Chính sách cookie", "Các cookie phục vụ đăng nhập, chống giả mạo yêu cầu và trạng thái thông báo của website.", [
                    Section("1. Cookie thiết yếu", "Cookie đăng nhập duy trì phiên xác thực; cookie chống giả mạo hỗ trợ bảo vệ biểu mẫu; cookie thông báo tạm thời giúp hiển thị kết quả thao tác. Các cookie này phục vụ chức năng cốt lõi và được cấu hình với thuộc tính bảo mật phù hợp."),
                    Section("2. Cookie không thiết yếu", "Source hiện tại không cấu hình cookie phân tích, quảng cáo hoặc theo dõi của bên thứ ba. Nếu chức năng này được bổ sung, chính sách và cơ chế lựa chọn sẽ được cập nhật trước khi sử dụng."),
                    Section("3. Quản lý cookie", "Bạn có thể xóa hoặc chặn cookie trong trình duyệt. Việc này có thể khiến đăng nhập hoặc một số biểu mẫu không hoạt động đúng.")
                ]),
                new(PolicyVersions.CookiePolicy, CurrentEditionDate, "Chính sách cookie", "Giải thích cookie mà HAVEN cấu hình, mục đích sử dụng và ảnh hưởng khi trình duyệt chặn cookie.", [
                    Section("1. Cookie là gì?", "Cookie là dữ liệu nhỏ mà website lưu trong trình duyệt để duy trì trạng thái hoặc hỗ trợ chức năng giữa các yêu cầu. Cookie khác với localStorage và sessionStorage; trong mã ứng dụng hiện tại không tìm thấy việc lưu dữ liệu người dùng vào hai kho này."),
                    Section("2. Cookie thiết yếu trên HAVEN", "Ứng dụng cấu hình cookie xác thực Haven.Auth để duy trì phiên đăng nhập và cookie Haven.Antiforgery để hỗ trợ xác thực biểu mẫu chống giả mạo yêu cầu. Hai cookie này được đánh dấu HttpOnly và SameSite=Lax; cookie Secure luôn bật trong production, còn Development phụ thuộc giao thức đang dùng.", "MVC sử dụng TempData để hiển thị thông báo một lần sau thao tác; provider mặc định có thể lưu dữ liệu này trong cookie. Thuộc tính và vòng đời thực tế có thể phụ thuộc phiên bản ASP.NET Core và cấu hình triển khai."),
                    Section("3. Cookie phân tích, quảng cáo và dịch vụ ngoài", "Không tìm thấy mã cấu hình Google Analytics, pixel quảng cáo hoặc công cụ theo dõi trong ứng dụng HAVEN hiện tại. Tuy nhiên, khu vực quản trị tải phông chữ từ Google Fonts. Đây là yêu cầu tải tài nguyên bên ngoài; chính sách này không khẳng định nhà cung cấp không thể nhận dữ liệu kỹ thuật của yêu cầu hoặc không đặt cookie trong mọi tình huống."),
                    Section("4. Quản lý cookie", "HAVEN hiện chưa có bảng điều khiển đồng ý hoặc tùy chọn tắt riêng từng nhóm cookie. Bạn có thể xóa hoặc chặn cookie qua cài đặt trình duyệt. Nếu chặn cookie xác thực, bạn không thể duy trì đăng nhập; nếu chặn cookie chống giả mạo, một số biểu mẫu gửi dữ liệu có thể không hoạt động."),
                    Section("5. Thời hạn và cập nhật", "Cookie xác thực được cấu hình với thời hạn phiên tám giờ và gia hạn trượt khi người dùng hoạt động. Cookie chống giả mạo không được ứng dụng đặt thời hạn riêng trong cấu hình hiện tại. Cookie TempData phụ thuộc hành vi mặc định của framework.", "Nếu hạ tầng hoặc tích hợp bên thứ ba thay đổi, đơn vị vận hành cần rà soát cookie thực tế trên trình duyệt và cập nhật chính sách này. Phiên bản và ngày hiệu lực được hiển thị ở đầu trang."),
                    Section("6. Liên hệ", "Thông tin đơn vị vận hành và kênh hỗ trợ chưa được cấu hình. Hãy xem Chính sách quyền riêng tư để biết trạng thái thông tin liên hệ và cách gửi yêu cầu trong hệ thống.")
                ]))
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
        ViewData["MetaDescription"] = "Tìm hiểu cách HAVEN hỗ trợ khám phá, đăng và quản lý tin bất động sản.";
        return View("About");
    }

    private IActionResult Render(string key, string? version)
    {
        var currentVersion = PolicyVersions.CurrentFor(key);
        var document = Documents[key].GetValueOrDefault(version ?? currentVersion);
        if (document is null) return NotFound();

        ViewData["Title"] = document.Title;
        ViewData["MetaDescription"] = document.Summary;
        var legal = configuration.GetSection("Haven:Legal");
        string Value(string name) => string.IsNullOrWhiteSpace(legal[name]) ? "Chưa được chủ dự án cấu hình" : legal[name]!;
        var otherPolicies = Documents
            .Select(item => (item.Key, item.Value[PolicyVersions.CurrentFor(item.Key)].Title))
            .Where(item => item.Key != key)
            .ToArray();

        return View("Document", new PolicyPageViewModel(
            key,
            document.Title,
            document.Summary,
            document.Version,
            document.EffectiveDate,
            document.Sections,
            otherPolicies,
            Value("OperatorName"),
            Value("SupportEmail"),
            Value("SupportPhone"),
            Value("SupportAddress")));
    }

    private static IReadOnlyDictionary<string, PolicyEdition> Editions(params PolicyEdition[] editions) =>
        editions.ToDictionary(edition => edition.Version, StringComparer.Ordinal);

    private static PolicySectionViewModel Section(string heading, params string[] paragraphs) => new(heading, paragraphs);
}
