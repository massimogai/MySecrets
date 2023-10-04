using EntityFrameworkCore.EncryptColumn.Extension;
using EntityFrameworkCore.EncryptColumn.Interfaces;
using EntityFrameworkCore.EncryptColumn.Util;
using Microsoft.EntityFrameworkCore;
using MySecrets.Model;
using DbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace ScuolaRegionale.Services
{
    public class SecretsDbContext : DbContext
    {
        public DbSet<MySecret> Secrets { get; set; }
        public DbSet<User> Users { get; set; }
        private IEncryptionProvider _encryptionProvider;
    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            _encryptionProvider = new GenerateEncryptionProvider("b14ca5898a4e4133bbce2ea2315a1916");
            modelBuilder.UseEncryption(this._encryptionProvider);
        }

   

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
       
            string? databaseurl=Environment.GetEnvironmentVariable("DATABASE_URL");
            Console.WriteLine("DB URL:"+databaseurl);
        
            string connectionString;
            if (databaseurl == null)
            {
                connectionString = "Host=localhost;Database=mypi;Username=postgres;Password=askme1st";
            }
            else
            {
                Uri dbUri = new Uri(databaseurl);
            
                String username = dbUri.UserInfo.Split(":")[0];
                String password = dbUri.UserInfo.Split(":")[1];
                int port=dbUri.Port;
                string host=dbUri.Host;
                string dbname = dbUri.AbsolutePath.Substring(1);
                connectionString = $"Host={host};Database={dbname};Username={username};Password={password}";
            }
       
            optionsBuilder.UseNpgsql(connectionString);
            optionsBuilder.EnableSensitiveDataLogging(true);
        }
        //postgres://vakjjpygrcmifm:3c3e828a0022a77f7685884b5bdd26522256fbaf53d0cbfba0833b36cc546e7c@ec2-54-155-110-181.eu-west-1.compute.amazonaws.com:5432/d20m20ds2505g6

        public  void Initialize()
        {
            Console.WriteLine("Creazione DB");
            // Database.EnsureDeleted();
            Database.EnsureCreated();
       
        }
    }
}