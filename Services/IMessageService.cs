// filepath: ChatBackend/Services/IMessageService.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Backend.Models.Dtos;
using System;

namespace ChatBackend.Services
{
    public interface IMessageService
    {
        // Yeni mesajı ekler ve kaydeder. Geriye MessageDto döner.
        Task<MessageDto> SendMessage(MessageDto messageDto);
        // Belirtilen alıcıya ait mesajların DTO listesini döner.
        Task<IEnumerable<MessageDto>> GetMessages(string receiver);
        // Mesajı ID'sine göre siler. Başarı durumunu döner.
        Task<bool> DeleteMessage(Guid messageId);
        // Tüm mesajların DTO listesini döner.
        Task<IEnumerable<MessageDto>> GetMessages();
    }
}