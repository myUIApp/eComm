using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Text;

namespace Mango.Services.EmailAPI.Services
{
    public class EmailService : IEmailService
    {
        private DbContextOptions<AppDbContext> _dbOptions;

        public EmailService(DbContextOptions<AppDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        public async Task EmailCardAndLog(CartDto cartDto)
        {
            StringBuilder message = new StringBuilder();

            message.AppendLine("<br/>Cart email requested");
            message.AppendLine("<br/> Total " + cartDto.CartHeader.CartTotal);
            message.Append("<br/>");
            message.Append("<ul>");
            foreach (var item in cartDto.CartDetails)
            {
                message.Append("<li>");
                message.Append(item.Product.Name+" X "+item.Count);
                message.Append("</li>");
            }
            message.Append("</ul>");

            await LogAndEmai(message.ToString(), cartDto.CartHeader.Email);
        }

        public async Task LogOrderPlaced(RewardsMessage rewardsMessage)
        {
            string message = "New order placed.<br/> Order ID : " + rewardsMessage.OrderId;
            await LogAndEmai(message, "abhinaw.kumar2022@gmail.com");
        }

        public async Task RegisterUserEmailAndLog(string email)
        {
            string message = "User Registration Successful.<br/> Email : " + email;
            await LogAndEmai(message, "abhinaw.kumar2022@gmail.com");
        }

        private async Task<bool> LogAndEmai(string message, string email)
        {
            try
            {
                EmailLogger emailLog = new EmailLogger()
                {
                    Email = email,
                    EmailSent= DateTime.Now,
                    Message = message
                };
                await using var _db = new AppDbContext(_dbOptions);
                await _db.EmailLoggers.AddAsync(emailLog);
                await _db.SaveChangesAsync();
                return true;

            }
            catch (Exception ex)
            {
                return false;
             }
        }
    }
}
