using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using DatingApp.API.Helpers;

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
        public async Task<User> GetUser(int id) => 
            await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);
        public async Task<PagedList<User>> GetUsers(UserParams userParams) 
        {
            var users = _context.Users.Include(p => p.Photos)
                .Where(u => u.Id != userParams.UserId)
                .Where(u => u.Gender == userParams.Gender)
                .OrderByDescending(u => u.LastActive)
                .AsQueryable();

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
    }
}