using System;
using System.Collections.Generic;

namespace QC_Management.Models;

public class ExternalProgram
{
    public int Id { get; set; }

    // Năm của chương trình ngoại kiểm (ví dụ 2026)
    public int Year { get; set; }

    // Tên chương trình
    public string Name { get; set; } = null!;

    // Mô tả/ghi chú
    public string? Description { get; set; }

    // Nhà cung cấp/nguồn ngoại kiểm (tùy chọn)
    public string? Vendor { get; set; }

    // Trạng thái (active/inactive)
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    // Tập các kết quả thuộc chương trình này
    public virtual ICollection<ExternalResult> Results { get; set; } = new List<ExternalResult>();
}