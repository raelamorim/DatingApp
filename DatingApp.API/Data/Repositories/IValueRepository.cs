using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Models;

namespace DatingApp.API.Data.Repositories
{
    public interface IValueRepository
    {
         Task create(Value value);
         Task<Value> findById(int id);
         Task<List<Value>> findAll();
         Task updateById(Value value);
         Task deleteById(int id);
    }
}