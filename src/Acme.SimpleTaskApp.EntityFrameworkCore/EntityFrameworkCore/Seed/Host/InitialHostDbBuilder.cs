namespace Acme.SimpleTaskApp.EntityFrameworkCore.Seed.Host
{
    public class InitialHostDbBuilder
    {
        private readonly SimpleTaskAppDbContext _context;

        public InitialHostDbBuilder(SimpleTaskAppDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            new DefaultEditionCreator(_context).Create();
            new DefaultLanguagesCreator(_context).Create();
            new HostRoleAndUserCreator(_context).Create();
            new DefaultSettingsCreator(_context).Create();

            _context.SaveChanges();
        }
    }
}
