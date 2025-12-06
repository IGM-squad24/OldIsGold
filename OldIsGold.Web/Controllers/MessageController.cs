using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OldIsGold.DAL.Data;
using OldIsGold.DAL.Models;

namespace OldIsGold.Web.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // Get unique conversations
            var sentMessages = await _context.Messages
                .Where(m => m.SenderId == userId)
                .Select(m => m.ReceiverId)
                .Distinct()
                .ToListAsync();

            var receivedMessages = await _context.Messages
                .Where(m => m.ReceiverId == userId)
                .Select(m => m.SenderId)
                .Distinct()
                .ToListAsync();

            var allContactIds = sentMessages.Union(receivedMessages).Distinct().ToList();

            var conversations = new List<dynamic>();
            foreach (var contactId in allContactIds)
            {
                var lastMessage = await _context.Messages
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .Where(m => (m.SenderId == userId && m.ReceiverId == contactId) ||
                               (m.SenderId == contactId && m.ReceiverId == userId))
                    .OrderByDescending(m => m.SentDate)
                    .FirstOrDefaultAsync();

                if (lastMessage != null)
                {
                    var unreadCount = await _context.Messages
                        .CountAsync(m => m.SenderId == contactId && m.ReceiverId == userId && !m.IsRead);

                    conversations.Add(new
                    {
                        Contact = contactId == lastMessage.SenderId ? lastMessage.Sender : lastMessage.Receiver,
                        LastMessage = lastMessage,
                        UnreadCount = unreadCount
                    });
                }
            }

            ViewBag.Conversations = conversations.OrderByDescending(c => c.LastMessage.SentDate).ToList();
            return View();
        }

        public async Task<IActionResult> Conversation(string contactId)
        {
            var userId = _userManager.GetUserId(User);
            var contact = await _userManager.FindByIdAsync(contactId);

            if (contact == null)
            {
                return NotFound();
            }

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Item)
                .Where(m => (m.SenderId == userId && m.ReceiverId == contactId) ||
                           (m.SenderId == contactId && m.ReceiverId == userId))
                .OrderBy(m => m.SentDate)
                .ToListAsync();

            // Mark messages as read
            var unreadMessages = messages.Where(m => m.ReceiverId == userId && !m.IsRead).ToList();
            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }
            if (unreadMessages.Any())
            {
                await _context.SaveChangesAsync();
            }

            ViewBag.Contact = contact;
            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string receiverId, int? itemId, string content)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var message = new Message
            {
                SenderId = userId,
                ReceiverId = receiverId,
                ItemId = itemId,
                Content = content,
                SentDate = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Message sent!";
            return RedirectToAction(nameof(Conversation), new { contactId = receiverId });
        }
    }
}
