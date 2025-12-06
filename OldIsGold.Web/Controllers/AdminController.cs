using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OldIsGold.DAL.Data;
using OldIsGold.DAL.Models;

namespace OldIsGold.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalItems = await _context.Items.CountAsync();
            var pendingItems = await _context.Items.CountAsync(i => i.Status == ItemStatus.Pending);
            var totalOrders = await _context.Orders.CountAsync();
            var pendingReports = await _context.Reports.CountAsync(r => r.Status == ReportStatus.Pending);

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalItems = totalItems;
            ViewBag.PendingItems = pendingItems;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.PendingReports = pendingReports;

            var recentUsers = await _context.Users.OrderByDescending(u => u.JoinDate).Take(5).ToListAsync();
            ViewBag.RecentUsers = recentUsers;

            return View();
        }

        public async Task<IActionResult> Users(string? search)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FullName.Contains(search) || u.Email!.Contains(search));
            }

            var users = await query.OrderByDescending(u => u.JoinDate).ToListAsync();
            ViewBag.Search = search;

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            // Log admin action
            var adminId = _userManager.GetUserId(User);
            var log = new AdminLog
            {
                AdminId = adminId!,
                Action = "Ban User",
                TargetType = "User",
                TargetId = userId,
                Description = $"Banned user: {user.FullName}",
                Date = DateTime.Now
            };
            _context.AdminLogs.Add(log);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User {user.FullName} has been banned.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            // Log admin action
            var adminId = _userManager.GetUserId(User);
            var log = new AdminLog
            {
                AdminId = adminId!,
                Action = "Unban User",
                TargetType = "User",
                TargetId = userId,
                Description = $"Unbanned user: {user.FullName}",
                Date = DateTime.Now
            };
            _context.AdminLogs.Add(log);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User {user.FullName} has been unbanned.";
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> Items(ItemStatus? status)
        {
            var query = _context.Items
                .Include(i => i.Category)
                .Include(i => i.Seller)
                .Include(i => i.Images)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }

            var items = await query.OrderByDescending(i => i.CreatedDate).ToListAsync();
            ViewBag.StatusFilter = status;

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveItem(int itemId)
        {
            var item = await _context.Items.FindAsync(itemId);
            if (item == null)
            {
                return NotFound();
            }

            item.Status = ItemStatus.Approved;
            item.ApprovedDate = DateTime.Now;

            // Log admin action
            var adminId = _userManager.GetUserId(User);
            var log = new AdminLog
            {
                AdminId = adminId!,
                Action = "Approve Item",
                TargetType = "Item",
                TargetId = itemId.ToString(),
                Description = $"Approved item: {item.Title}",
                Date = DateTime.Now
            };
            _context.AdminLogs.Add(log);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Item approved successfully!";
            return RedirectToAction(nameof(Items));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectItem(int itemId)
        {
            var item = await _context.Items.FindAsync(itemId);
            if (item == null)
            {
                return NotFound();
            }

            item.Status = ItemStatus.Rejected;

            // Log admin action
            var adminId = _userManager.GetUserId(User);
            var log = new AdminLog
            {
                AdminId = adminId!,
                Action = "Reject Item",
                TargetType = "Item",
                TargetId = itemId.ToString(),
                Description = $"Rejected item: {item.Title}",
                Date = DateTime.Now
            };
            _context.AdminLogs.Add(log);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Item rejected.";
            return RedirectToAction(nameof(Items));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int itemId)
        {
            var item = await _context.Items
                .Include(i => i.Images)
                .FirstOrDefaultAsync(i => i.ItemId == itemId);

            if (item == null)
            {
                return NotFound();
            }

            // Log admin action
            var adminId = _userManager.GetUserId(User);
            var log = new AdminLog
            {
                AdminId = adminId!,
                Action = "Delete Item",
                TargetType = "Item",
                TargetId = itemId.ToString(),
                Description = $"Deleted item: {item.Title}",
                Date = DateTime.Now
            };
            _context.AdminLogs.Add(log);

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Item deleted successfully!";
            return RedirectToAction(nameof(Items));
        }

        public async Task<IActionResult> Reports(ReportStatus? status)
        {
            var query = _context.Reports
                .Include(r => r.Item)
                .Include(r => r.Reporter)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            var reports = await query.OrderByDescending(r => r.ReportDate).ToListAsync();
            ViewBag.StatusFilter = status;

            return View(reports);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveReport(int reportId, string action)
        {
            var report = await _context.Reports
                .Include(r => r.Item)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);

            if (report == null)
            {
                return NotFound();
            }

            var adminId = _userManager.GetUserId(User);
            report.Status = ReportStatus.Resolved;
            report.ReviewedByAdminId = adminId;

            if (action == "delete_item" && report.Item != null)
            {
                report.Item.Status = ItemStatus.Rejected;
                
                var log = new AdminLog
                {
                    AdminId = adminId!,
                    Action = "Resolve Report - Delete Item",
                    TargetType = "Report",
                    TargetId = reportId.ToString(),
                    Description = $"Resolved report and rejected item: {report.Item.Title}",
                    Date = DateTime.Now
                };
                _context.AdminLogs.Add(log);
            }
            else
            {
                var log = new AdminLog
                {
                    AdminId = adminId!,
                    Action = "Resolve Report - Dismiss",
                    TargetType = "Report",
                    TargetId = reportId.ToString(),
                    Description = "Resolved report - no action taken",
                    Date = DateTime.Now
                };
                _context.AdminLogs.Add(log);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Report resolved.";
            return RedirectToAction(nameof(Reports));
        }

        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        public async Task<IActionResult> Logs()
        {
            var logs = await _context.AdminLogs
                .Include(l => l.Admin)
                .OrderByDescending(l => l.Date)
                .Take(100)
                .ToListAsync();

            return View(logs);
        }
    }
}
