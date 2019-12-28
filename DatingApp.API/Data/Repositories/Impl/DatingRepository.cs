using System.Reflection.Metadata;
using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using DatingApp.API.Helpers;
using System.Collections.Generic;

namespace DatingApp.API.Data.Repositories.Impl
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        { 
            _context = context;
        }
        public void Add<T>(T entity) where T : class => _context.AddAsync(entity);
        public void Delete<T>(T entity) where T : class => _context.Remove(entity);
        public async Task<User> GetUser(int id) => await _context.Users
            .Include(p => p.Photos)
            .Include(l => l.Likers)
            .Include(l => l.Likees)
            .FirstOrDefaultAsync(u => u.Id == id);
        public async Task<PagedList<User>> GetUsers(UserParams userParams) 
        {
            var users = _context.Users.Include(p => p.Photos)
                .Where(u => u.Id != userParams.UserId)
                .Where(u => u.Gender == userParams.Gender)
                .OrderByDescending(u => u.LastActive)
                .AsQueryable();

            if (userParams.Likees)
                users = users.Where(u => u.Likers.Any(l => l.LikerId == userParams.UserId));

            if (userParams.Likers)
                users = users.Where(u => u.Likees.Any(l => l.LikeeId == userParams.UserId));

            if (userParams.MinAge !=18 || userParams.MaxAge !=99)
            {
                var minDateBirthday = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDateBirthday = DateTime.Today.AddYears(-userParams.MinAge);
                users = users.Where(u => 
                    u.DateOfBirth >= minDateBirthday && u.DateOfBirth <= maxDateBirthday);
            }

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, 
                                            userParams.PageSize);
        }

        public async Task<bool> SaveAll() => await _context.SaveChangesAsync() > 0;
        public async Task<Photo> GetPhoto(int id) => 
            await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
        public async Task<Photo> GetMainPhotoForUser(int userId) => 
            await _context.Photos
                .Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
        public async Task<Like> GetLike(int userId, int recipientId) => 
            await _context.Likes.FirstOrDefaultAsync(u => 
                u.LikerId == userId && u.LikeeId == recipientId);

        public async Task<Message> GetMessage(int id) => 
            await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(m => m.RecipientId == messageParams.UserId
                                                && m.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where(m => m.SenderId == messageParams.UserId
                                                && m.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(m => m.RecipientId == messageParams.UserId
                                                && m.RecipientDeleted == false 
                                                && m.IsRead == false);
                    break;
            }

            messages = messages.OrderByDescending(d => d.MessageSent);
            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId) => 
            await _context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(m => m.RecipientId == userId && m.RecipientDeleted == false && m.SenderId == recipientId
                    || m.RecipientId == recipientId && m.SenderDeleted == false && m.SenderId == userId)
                .OrderByDescending(m => m.MessageSent)
                .ToListAsync();
    }
}