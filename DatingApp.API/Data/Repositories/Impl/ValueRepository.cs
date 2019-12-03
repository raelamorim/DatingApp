using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data.Repositories
{
    public class ValueRepository : IValueRepository
    {
        public ValueRepository(DataContext context)
        {
            _context = context;
        }

        private readonly DataContext _context;

        public async Task create(Value value)
        {
            await _context.Values.AddAsync(value);
            await save();
        }
        public async Task deleteById(int id) {
            var valueToDelete = await _context.Values.FirstOrDefaultAsync(x => x.Id == id);
            _context.Values.Remove(valueToDelete);
            await save();
        }
        public async Task<List<Value>> findAll() => await _context.Values.ToListAsync();
        public async Task<Value> findById(int id) => await _context.Values.FirstOrDefaultAsync(x => x.Id == id);
        public async Task updateById(Value value)
        {
            var valueToUpdate = await _context.Values.FirstOrDefaultAsync(x => x.Id == value.Id);
            valueToUpdate.Name = value.Name;
            await save();
        }

        private async Task save() {
            await _context.SaveChangesAsync();
        }
    }
}