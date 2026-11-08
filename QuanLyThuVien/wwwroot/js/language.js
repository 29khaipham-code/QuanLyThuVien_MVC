(function () {
    "use strict";

    // Từ điển dịch giao diện dùng chung. Dữ liệu sách/độc giả trong CSDL không bị thay đổi.
    const translations = {
        "/ 5 cuốn": "/5 books",
        "0 cuốn": "0 book",
        "1 cuốn": "1 book",
        "2 cuốn": "2 books",
        "3 cuốn": "3 books",
        "4 cuốn": "4 books",
        "5 cuốn": "5 books",
        "6 cuốn": "6 books",

        "Khoa học tự nhiên": "Natural Science",
        "Lịch sử - Địa lý": "History - Geography",
        "Kỹ năng sống": "Life Skills",
        "Thiếu nhi": "Children",
        "Tất cả": "All",

        "Trang chủ": "Home",
        "Mượn trả": "Borrow & Return",
        "Đặt trước": "Reservations",
        "Tìm sách, tác giả...": "Search books, authors...",



        "Không gian kết nối tri thức, khám phá những trang sách và nuôi dưỡng niềm đam mê đọc.": "A space to connect with knowledge, explore the pages of books, and nurture a passion for reading.",
        "Thư viện": "Library",
        "HỆ THỐNG THƯ VIỆN": "LIBRARY SYSTEM",
        "© 2026 - Hệ thống quản lý thư viện.": "© 2026 - Library Management System.",
        "Tri thức bắt đầu từ một trang sách.": "Knowledge begins with a page.",
        "KHÔNG GIAN TRI THỨC": "KNOWLEDGE SPACE",
        "Chào mừng đến với thư viện": "Welcome to the Library",
        "Tra cứu sách giấy và sách điện tử ngay trên website.": "Search for printed books and e-books directly on the website.",
        "Sách mới về": "New Arrivals",
        "Cập nhật những đầu sách mới nhất của thư viện.": "Discover the latest additions to the library.",
        "Mượn ngay hôm nay": "Borrow Today",
        "Lọc nhanh những cuốn còn sẵn trên kệ để đến mượn.": "Quickly filter available books on the shelves and borrow them.",
        "Tra cứu sách": "Search Books",
        "Xem sách mới": "View New Arrivals",
        "Xem sách còn bản": "View Available Books",
        "Sắp đến hạn (3 ngày)": "Due Soon (3 Days)",
        "Xin chào,": "Hello,",
        "Quá hạn": "Overdue",
        "Phạt chưa thanh toán": "Unpaid Fines",
        "Sách mới nhất": "Latest Books",
        "Vừa được bổ sung vào thư viện": "Recently Added to the Library",
        "Xem tất cả": "View All",
        "Được quan tâm nhiều": "Most Popular",
        "Những cuốn được mượn nhiều nhất": "The Most Borrowed Books",
        "Sách điện tử": "E-books",
        "Đọc trực tuyến mọi lúc mọi nơi": "Read Online Anytime, Anywhere",
        "Khám phá theo chủ đề": "Explore by Category",
        "Văn học Việt Nam": "Vietnamese Literature",
        "Văn học nước ngoài": "Foreign Literature",
        "Công nghệ thông tin": "Information Technology",



        "Khám phá thế giới": "Explore the World of",
        "sách": "Books",
        "Tìm kiếm tài liệu, giáo trình và sách điện tử. Hãy bắt đầu hành trình khám phá tri thức của bạn.": "Search for documents, textbooks, and e-books. Start your journey of knowledge discovery.",
        "Tìm kiếm sách": "Search Books",
        "Sử dụng bộ lọc để tìm cuốn sách bạn cần": "Use filters to find the book you need",
        "Từ khóa": "Keyword",
        "Thể loại": "Category",
        "Nhà xuất bản": "Publisher",
        "Sắp xếp": "Sort",
        "Tên sách, tác giả, ISBN": "Book title, author, ISBN",
        "Mới nhất": "Newest",
        "Năm xuất bản": "Publication Year",
        "Còn sách": "Available",
        "Tìm thấy": "Found",
        "đầu sách": "books",
        "Còn 1/1 bản": "1/1 copy available",
        "Còn 1/2 bản": "1/2 copies available",
        "Còn 2/2 bản": "2/2 copies available",
        "Còn 1/3 bản": "2/2 copies available",
        "Còn 2/3 bản": "2/2 copies available",
        "Còn 3/3 bản": "2/2 copies available",
        "Tạm hết bản": "Temporarily unavailable",




        "Thư viện điện tử": "Digital Library",
        "KHÔNG GIAN TRI THỨC SỐ": "DIGITAL KNOWLEDGE SPACE",
        "Khám phá và đọc những cuốn sách yêu thích ngay trên thiết bị của bạn. Tri thức luôn sẵn sàng ở bất cứ đâu.": "Discover and read your favorite books right on your device. Knowledge is always available wherever you are.",
        "Lọc sách theo tên, tác giả hoặc thể loại bạn quan tâm.": "Filter books by title, author, or category of interest.",
        "Tìm kiếm sách điện tử": "Search E-books",
        "Tìm kiếm": "Search",
        "Sách của tôi": "My Books",
        "KẾT QUẢ TÌM KIẾM": "SEARCH RESULTS",
        "Tìm thấy": "Found",
        "đầu sách có bản điện tử": "books available in digital format",
        "Tất cả thể loại": "All Categories",
        "Sách điện tử của tôi": "My E-books",
        "Thông tin quyền đọc": "Reading Access Information",
        "Mỗi quyền đọc có thời hạn": "Each reading access is valid for",
        "14 ngày": "14 days",
        "Bạn có thể giữ tối đa": "You can hold up to",
        "5 quyền đọc": "5 reading accesses",
        "cùng lúc": "at the same time",
        "Khi hết hạn, bạn cần đăng ký đọc lại tại trang chi tiết sách.": "When access expires, you need to register again on the book details page.",
        "HIỆN TẠI": "CURRENTLY",
        "Quyền đọc đang hiệu lực": "Active Reading Access",
        "Những cuốn sách bạn có thể tiếp tục đọc trong thời hạn cho phép.": "Books you can continue reading within the permitted period.",
        "Bạn chưa có quyền đọc nào": "You Don't Have Any Reading Access Yet",
        "Hãy khám phá thư viện điện tử, chọn một cuốn sách yêu thích và đăng ký đọc online để bắt đầu.": "Explore the digital library, choose your favorite book, and register for online reading to get started.",
        "Khám phá thư viện": "Explore Library",
        "Quyền đọc đang dùng": "Active Reading Access",
        "Quản lý những cuốn sách bạn đã đăng ký đọc, theo dõi thời hạn và tiếp tục hành trình khám phá tri thức.": "Manage the books you have registered to read, track expiration dates, and continue your journey of knowledge discovery.",
        "TỦ SÁCH CÁ NHÂN": "PERSONAL BOOKSHELF",




        "Theo dõi những cuốn sách bạn đang mượn và thời hạn trả sách.": "Track the books you have borrowed and their return deadlines.",
        "Thời hạn mỗi lượt": "Loan Period",
        "Đang mượn": "Currently Borrowed",
        "Thư viện của bạn": "Your Library",
        "Lịch sử mượn": "Borrowing History",
        "Tiền phạt": "Fines",
        "Bạn chưa mượn cuốn sách nào": "You haven't borrowed any books yet",
        "Hãy khám phá kho sách của thư viện và tìm đọc những cuốn sách mà bạn yêu thích.": "Explore the library's book collection and find books you love to read.",
        "Khám phá sách": "Explore Books",
        "ngày": "days",




        "Quản lý những cuốn sách bạn đã đặt trước và theo dõi tình trạng giữ chỗ.": "Manage the books you have reserved and track your reservation status.",
        "Thông tin đặt trước": "Reservation Information",
        "Theo dõi yêu cầu": "Track Requests",
        "Đang hiệu lực": "Active",
        "Bạn chưa đặt trước cuốn sách nào": "You haven't reserved any books yet",
        "Đặt trước sách": "Reserve Books",
        "Bạn có thể đặt trước tối đa": "You can reserve up to",
        "3 cuốn": "3 books",
        "cùng lúc": "at the same time",
        "Khi có sách, thư viện giữ chỗ cho bạn": "When a book becomes available, the library will hold it for you for",
        "3 ngày": "3 days",
        "Quá hạn mà chưa đến nhận thì yêu cầu sẽ tự hủy.": "If you don't collect the book before the deadline, your reservation will be automatically canceled.",
        "Hãy mở trang chi tiết của một cuốn sách đang hết bản và chọn chức năng Đặt trước để đăng ký giữ chỗ.": "Open the details page of a book that is currently unavailable and select the Reserve option to place a reservation.",






        "KHÔNG GIAN ĐỌC SÁCH": "READING SPACE",
        "Thông tin cá nhân": "Personal Information",
        "Thông tin cơ bản và liên hệ của bạn": "Your basic and contact information",
        "Tên đăng nhập": "Username",
        "Ngày sinh": "Date of Birth",
        "Địa chỉ": "Address",
        "Họ và tên": "Full Name",
        "Giới tính": "Gender",
        "Số điện thoại": "Phone Number",
        "Thẻ thư viện": "Library Card",
        "Thông tin và quyền sử dụng thẻ của bạn": "Your library card information and privileges",
        "Còn hiệu lực": "Valid",
        "Loại độc giả": "Reader Type",
        "Ngày đăng ký": "Registration Date",
        "Tổng lượt mượn": "Total Borrowings",
        "Phạt chưa nộp": "Unpaid Fines",
        "JPG, PNG hoặc WEBP. Dung lượng tối đa 2 MB.": "JPG, PNG, or WEBP. Maximum file size: 2 MB.",
        "Chọn ảnh đại diện": "Choose Profile Picture",
        "Quản lý thông tin cá nhân, tài khoản và thẻ thư viện của bạn tại thư viện.": "Manage your personal information, account, and library card.",
        "Hồ sơ": "Personal",
        "cá nhân": "Profile",
        "Chỉnh sửa": "Edit",
        "Đổi mật khẩu": "Change Password",
        "Cập nhật ảnh": "Update Photo",
        "Mã độc giả": "Reader ID",
        "Quyền mượn": "Borrowing Privileges",
        "Hạn thẻ": "Card Expiration Date",
        "Sinh viên": "Student",
        "Tối đa 5 cuốn, mỗi lượt 14 ngày": "Up to 5 books, 14 days per loan",
        "Đăng xuất": "Log Out",
        "Hồ sơ cá nhân": "Personal Profile",
        "Thông báo": "Notifications",
        "Bạn đã cập nhật mọi thông báo": "You're all caught up on notifications",
        "Chưa có thông báo": "No notifications yet",
        "Khi có thông báo mới, chúng sẽ xuất hiện tại đây.": "New notifications will appear here.",
        "Mở trung tâm thông báo": "Open Notification Center",
        "BẢO MẬT TÀI KHOẢN": "ACCOUNT SECURITY",
        "Cập nhật mật khẩu mới để bảo vệ tài khoản và thông tin thư viện của bạn.": "Update your password to protect your account and library information.",
        "Bảo vệ tài khoản": "Account Protection",
        "Hãy sử dụng mật khẩu đủ mạnh và không chia sẻ mật khẩu với người khác.": "Use a strong password and do not share it with others.",
        "Tối thiểu 8 ký tự": "At least 8 characters",
        "Có cả chữ và số": "Contains both letters and numbers",
        "Không sử dụng mật khẩu quá dễ đoán": "Avoid using easily guessed passwords",
        "Thông tin mật khẩu": "Password Information",
        "Nhập thông tin bên dưới để thay đổi mật khẩu.": "Enter the information below to change your password.",
        "Mật khẩu hiện tại": "Current Password",
        "Mật khẩu mới": "New Password",
        "Từ 8 ký tự trở lên, gồm cả chữ và số.": "At least 8 characters, including both letters and numbers.",
        "Nhập lại mật khẩu mới": "Confirm New Password",
        "Hủy": "Cancel",
        "Nhập mật khẩu mới": "Enter new password",
        "Nhập mật khẩu hiện tại": "Enter current password",
        "Theo dõi những cập nhật mới nhất từ thư viện": "Stay up to date with the latest library updates",
        "Lọc thông báo": "Filter Notifications",
        "Loại thông báo": "Notification Type",
        "Trạng thái": "Status",
        "Hiện tại không có thông báo nào phù hợp với bộ lọc của bạn.": "There are currently no notifications matching your filters.",
        "thông báo": "notifications",
        "Có": "You have",
        "thông báo chưa đọc": "unread notifications",
        "Đọc tất cả": "Mark All as Read",
        "Xóa đã đọc": "Delete Read Notifications",
        "Tất cả trạng thái": "All Statuses",
        "Chưa đọc": "Unread",
        "Đã đọc": "Read",
        "Tất cả thông báo": "All Notifications",
        "Sắp đến hạn trả": "Due Soon",
        "Quá hạn trả": "Overdue",
        "Sách đặt trước đã sẵn sàng": "Reserved Book Available",
        "Sắp hết hạn giữ chỗ": "Reservation Expiring Soon",
        "Thẻ sắp hết hạn": "Library Card Expiring Soon",
        "Quyền đọc sắp hết hạn": "Reading Access Expiring Soon",
        "Giới thiệu sách": "Book Description",
        "Những nguyên tắc cơ bản trong giao tiếp và xây dựng quan hệ.": "The basic principles of communication and relationship building.",
        "Khám phá nội dung và những điều thú vị trong cuốn sách": "Explore the contents and interesting insights of this book",
        "Đọc online": "Read Online",
        "Phiên bản sách điện tử": "E-book Version",
        "Thao tác": "Actions",
        "Sách đang có sẵn trên kệ, bạn có thể đến thư viện mượn trực tiếp.": "This book is currently available on the shelf. You can borrow it directly from the library.",
        " Đã đặt:": " Reserved:",
        "Thông tin sách thư viện": "Library Book Information",
        "Cho phép tải xuống": "Download Allowed",
        "tái bản lần": "Reprint",
        "Sẵn sàng": "Available",
        "Tình trạng bản giấy": "Physical Copy Status",
        "Chưa có ảnh bìa": "No Cover Image Available",
        "Phiên bản PDF": "PDF Version"
    };

    function getLanguage() {
        const item = document.cookie.split("; ").find(x => x.startsWith("site-language="));
        return item ? decodeURIComponent(item.split("=").slice(1).join("=")) : "vi";
    }

    function translateText(value) {
        if (!value || !value.trim()) return value;

        // Dịch nguyên văn các nhãn ngắn; chỉ thay thế trong câu đối với cụm đủ dài.
        // Điều này tránh tạo câu lai Việt-Anh như "đặt Reservations", "sách Books".
        const trimmed = value.trim();
        if (Object.prototype.hasOwnProperty.call(translations, trimmed)) {
            return value.replace(trimmed, translations[trimmed]);
        }

        let result = value;
        const keys = Object.keys(translations)
            .filter(key => key.length >= 8)
            .sort((a, b) => b.length - a.length);

        for (const key of keys) {
            if (!key) continue;

            // Chỉ thay khi ranh giới không nằm giữa chữ/số, tránh thay một phần từ.
            // Cho phép khoảng trắng/newline trong Razor khác với câu trong từ điển.
            const escaped = key
                .split(/\s+/)
                .map(part => part.replace(/[.*+?^${}()|[\]\\]/g, "\\$&"))
                .join("\\s+");
            const pattern = new RegExp(`(?<![\\p{L}\\p{N}])${escaped}(?![\\p{L}\\p{N}])`, "gu");
            result = result.replace(pattern, translations[key]);
        }

        return result;
    }

    function translatePage() {
        const language = getLanguage();
        document.documentElement.lang = language;

        if (language !== "en") return;

        const walker = document.createTreeWalker(
            document.body,
            NodeFilter.SHOW_TEXT,
            {
                acceptNode(node) {
                    if (!node.nodeValue || !node.nodeValue.trim()) return NodeFilter.FILTER_REJECT;
                    const parent = node.parentElement;
                    if (!parent || ["SCRIPT", "STYLE", "TEXTAREA", "CODE", "PRE"].includes(parent.tagName)) {
                        return NodeFilter.FILTER_REJECT;
                    }
                    return NodeFilter.FILTER_ACCEPT;
                }
            }
        );

        const textNodes = [];
        while (walker.nextNode()) textNodes.push(walker.currentNode);
        textNodes.forEach(node => {
            const translated = translateText(node.nodeValue);
            if (translated !== node.nodeValue) node.nodeValue = translated;
        });

        document.querySelectorAll("input[placeholder], input[value], input[aria-label], button[aria-label], textarea[placeholder], [title], [aria-label], img[alt]").forEach(element => {
            ["placeholder", "value", "aria-label", "title", "alt"].forEach(attribute => {
                const value = element.getAttribute(attribute);
                if (value) {
                    const translated = translateText(value);
                    if (translated !== value) {
                        element.setAttribute(attribute, translated);
                    }
                }
            });
        });
    }

    document.addEventListener("DOMContentLoaded", translatePage);
    if (document.readyState !== "loading") translatePage();

    // Hỗ trợ các phần giao diện được tải thêm bằng AJAX/partial view.
    const observer = new MutationObserver(() => {
        if (getLanguage() === "en") translatePage();
    });
    observer.observe(document.documentElement, {
        childList: true,
        subtree: true
    });
})();
