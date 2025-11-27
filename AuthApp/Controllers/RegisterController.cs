using AuthApp.Data;
using AuthApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AuthApp.Controllers
{
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegisterController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAccount(Account model)
        {
            // Cờ (flag) để theo dõi xem có lỗi trùng lặp nào xảy ra hay không
            bool hasDuplicateError = false;

            // 1. KIỂM TRA VALIDATION TỪ MODEL (Bắt buộc điền đầy đủ trường)
            // Nếu Validation thất bại (thiếu trường), trả lại View ngay
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // 2. KIỂM TRA TRÙNG LẶP USERNAME
            if (await _context.Accounts.AnyAsync(a => a.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username already exists. Please select another Username.");
                hasDuplicateError = true;
            }

            // 3. KIỂM TRA TRÙNG LẶP EMAIL
            if (await _context.Accounts.AnyAsync(a => a.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already exists. Please select another Email.");
                hasDuplicateError = true;
            }

            // 4. KIỂM TRA TRÙNG LẶP PHONE NUMBER
            if (await _context.Accounts.AnyAsync(a => a.PhoneNumber == model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Phonenumber already exists. Please select another Phonenumber");
                hasDuplicateError = true;
            }

            // 5. NẾU CÓ BẤT KỲ LỖI TRÙNG LẶP NÀO
            if (hasDuplicateError)
            {
                // Trả lại View để hiển thị tất cả các lỗi trùng lặp đã thu thập
                return View("Index", model);
            }

            // 6. ĐĂNG KÝ THÀNH CÔNG (Chỉ chạy khi không có lỗi Required và không có lỗi Trùng lặp)

            // Lưu ý: Trong ứng dụng thực tế,  cần mã hóa mật khẩu trước khi lưu.
            _context.Accounts.Add(model);
            await _context.SaveChangesAsync();

            // Chuyển hướng đến trang Đăng nhập sau khi thành công
            return RedirectToAction("Index", "Login");
        }
    }
}