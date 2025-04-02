using ChatApp.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatBackend.Data
{
    public class MessagingContext : DbContext
    {
        public MessagingContext(DbContextOptions<MessagingContext> options) : base(options)
        {
        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Log> Logs { get; set; }
         public DbSet<MessageLog> MessageLogs { get; set; }
    }
}