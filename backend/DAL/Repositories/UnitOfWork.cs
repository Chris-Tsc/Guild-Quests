using DAL.Data;
using DAL.Repositories.Interfaces;

namespace DAL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbc;

        public UnitOfWork(AppDbContext dbc)
        {
            _dbc = dbc;
        }

        public async Task SaveChangesAsync()
        {
            await _dbc.SaveChangesAsync();
        }
    }
}
