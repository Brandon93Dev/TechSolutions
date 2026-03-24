using TechSolutions_IPS_HW.Models.Email;
using TechSolutions_IPS_HW.Models.Enums;
using TechSolutions_IPS_HW.Repositories.Interfaces;

namespace TechSolutions_IPS_HW.Services.MailerServices
{
    /// <summary>
    /// Database-backed email queue. stopres email send requests to the EmailQueueEntries table
    /// so they survive application restarts and can be retried on failure.
    /// <remarks>I previously stored this in memory but then quickly realised mails went sending during 
    /// testing, since i kept stopping the service to make changes. </remarks>
    /// </summary>
    public class EmailQueue
    {
        private readonly IEmailQueueRepository _emailQueueRepository;

        public EmailQueue(IEmailQueueRepository emailQueueRepository)
        {
            _emailQueueRepository = emailQueueRepository;
        }


        //Creates a new queued item with the provided email message details and saves it to the database.
        //The item is initialised as Pending.
        public async ValueTask EnqueueAsync(EmailMessage message)
        {
            await _emailQueueRepository.EnqueueAsync(new EmailQueueEntry
            {
                ToAddress = message.To,
                Subject = message.Subject,
                Body = message.Body,
                Status = EmailQueueStatus.Pending,
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow,
                NextRetryAt = DateTime.UtcNow
            });
        }
    }
}
