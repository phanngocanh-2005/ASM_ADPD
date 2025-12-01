using System.Collections.Generic;
using System.Linq;
using AuthApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AuthApp.Patterns.Adapter
{
    /// <summary>
    /// Adapter chuyển danh sách Teacher sang danh sách SelectListItem để hiển thị trên UI.
    /// Ý tưởng Adapter pattern:
    /// - Source: IEnumerable&lt;Teacher&gt; (model trong domain)
    /// - Target: IEnumerable&lt;SelectListItem&gt; (dữ liệu dùng cho dropdown ASP.NET MVC)
    /// - Adapter: TeacherSelectListAdapter đứng giữa, chuyển đổi định dạng.
    /// </summary>
    // Interface mô tả chức năng adapter cho Teacher
    public interface ITeacherSelectListAdapter
    {
        IEnumerable<SelectListItem> Adapt(IEnumerable<Teacher> teachers);
    }

    // Adapter cụ thể triển khai logic chuyển đổi
    public class TeacherSelectListAdapter : ITeacherSelectListAdapter
    {
        public IEnumerable<SelectListItem> Adapt(IEnumerable<Teacher> teachers)
        {
            return teachers
                // Sắp xếp theo tên cho dễ nhìn trên UI
                .OrderBy(t => t.FullName)
                // Tạo SelectListItem: Text hiển thị, Value gửi về server
                .Select(t => new SelectListItem($"{t.FullName} ({t.TeacherCode})", t.Id.ToString()));
        }
    }
}


