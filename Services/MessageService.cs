using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChatApp.Backend.Models;
using ChatApp.Backend.Models.Dtos;
using ChatBackend.Data;
using Microsoft.Extensions.Logging;

namespace ChatBackend.Services
{
    public class MessageService : IMessageService
    {
        private readonly MessagingContext _context;
        private readonly ILogger<MessageService> _logger;

        public MessageService(MessagingContext context, ILogger<MessageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Yardımcı metod: Veritabanına log kaydı ekler.
        private async Task LogToDatabaseAsync(string level, string message)
        {
            try
            {
                var log = new Log
                {
                    Level = level,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                };
                _context.Logs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging to database.");
            }
        }

        // Mesajı ID'sine göre siler. Eğer mesaj bulunamazsa false döner.
        public async Task<bool> DeleteMessage(Guid messageId)
        {
            _logger.LogInformation("Attempting to delete message with Id: {MessageId}", messageId);
            await LogToDatabaseAsync("Info", $"DeleteMessage requested for MessageId: {messageId}");

            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
            if (message == null)
            {
                _logger.LogWarning("Message with Id: {MessageId} not found.", messageId);
                await LogToDatabaseAsync("Warning", $"Message not found for deletion. MessageId: {messageId}");
                return false;
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Message with Id: {MessageId} deleted successfully.", messageId);
            await LogToDatabaseAsync("Info", $"Message deleted successfully. MessageId: {messageId}");
            return true;
        }

        // Tüm mesajları DTO olarak döner (alıcıya göre filtreleme yapılır).
        public async Task<IEnumerable<MessageDto>> GetMessages(string receiver)
        {
            _logger.LogInformation("Fetching messages for receiver: {Receiver}", receiver);
            await LogToDatabaseAsync("Info", $"Fetching messages for receiver: {receiver}");

            try
            {
                var messages = await _context.Messages
                    .Where(m => m.Receiver.Equals(receiver, StringComparison.OrdinalIgnoreCase))
                    .ToListAsync();

                return messages.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching messages for receiver: {Receiver}", receiver);
                await LogToDatabaseAsync("Error", $"Error fetching messages for receiver: {receiver}");
                throw;
            }
        }

        // Yeni mesajı ekler ve kaydeder. Geriye MessageDto döner.
        public async Task<MessageDto> SendMessage(MessageDto messagedto)
        {
            _logger.LogInformation("Sending message from: {Sender} to {Receiver}", messagedto.Sender, messagedto.Receiver);
            await LogToDatabaseAsync("Info", $"Sending message from: {messagedto.Sender} to {messagedto.Receiver}");

            // Gönderen ve Alıcının ID formatını doğrula.
            if (!Guid.TryParse(messagedto.Sender.ToString(), out Guid senderId))
            {
                throw new InvalidOperationException("Invalid sender ID format.");
            }
            if (!Guid.TryParse(messagedto.Receiver.ToString(), out Guid receiverId))
            {
                throw new InvalidOperationException("Invalid receiver ID format.");
            }

            // Kullanıcıların veritabanında kayıtlı olup olmadığını kontrol et.
            bool senderExists = await _context.Users.AnyAsync(u => u.Id == senderId);
            bool receiverExists = await _context.Users.AnyAsync(u => u.Id == receiverId);
            if (!senderExists || !receiverExists)
            {
                string errorMessage = "Message cannot be sent because either sender or receiver does not exist in the database.";
                _logger.LogError(errorMessage);
                await LogToDatabaseAsync("Error", errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            try
            {
                // DTO'yu entity'ye çevir.
                var messageEntity = MapToEntity(messagedto);
                _context.Messages.Add(messageEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Message sent successfully. MessageId: {MessageId}", messageEntity.Id);
                await LogToDatabaseAsync("Info", $"Message sent successfully. MessageId: {messageEntity.Id}");

                return MapToDto(messageEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message from: {Sender} to {Receiver}", messagedto.Sender, messagedto.Receiver);
                await LogToDatabaseAsync("Error", $"Error sending message from: {messagedto.Sender} to {messagedto.Receiver}");
                throw;
            }
        }

        // Yardımcı metot: Message entity'sini MessageDto'ya mapler.
        private MessageDto MapToDto(Message message)
        {
            return new MessageDto
            {
                Id = message.Id,
                Content = message.Content,
                Sender = Guid.Parse(message.Sender),
                Receiver = Guid.Parse(message.Receiver),
                CreatedAt = message.Timestamp
            };
        }

        // Yardımcı metot: MessageDto'yu Message entity'sine mapler.
        private Message MapToEntity(MessageDto dto)
        {
            return new Message
            {
                Content = dto.Content,
                // Entity'de Sender ve Receiver string olarak saklanıyorsa:
                Sender = dto.Sender.ToString(),
                Receiver = dto.Receiver.ToString(),
                // Eğer mesaj oluşturma zamanını şimdi yapmak isterseniz DateTime.UtcNow kullanılabilir.
                Timestamp = dto.CreatedAt
            };
        }

        // Tüm mesajları getirir.
        public async Task<IEnumerable<MessageDto>> GetMessages()
        {
            _logger.LogInformation("Fetching all messages.");
            try
            {
                var messages = await _context.Messages.ToListAsync();
                return messages.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all messages.");
                throw;
            }
        }

    }
}