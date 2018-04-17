using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using CryptoMarket.Models;
using WebGrease.Css.Extensions;

namespace CryptoMarket.Source.Managers {
    /// <summary>
    /// Manager, provides personal messaging functionality
    /// </summary>
    public class PersonalMessagesManager {

        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor, with DI of DB context
        /// </summary>
        /// <param name="context"></param>
        public PersonalMessagesManager(ApplicationDbContext context) {
            _context = context;
        }

        /// <summary>
        /// Send message from user id, to user id with text
        /// </summary>
        /// <param name="fromUserId">Sender User Id</param>
        /// <param name="toUserId">Recipient User Id</param>
        /// <param name="text">Message Text</param>
        /// <returns></returns>
        public bool SendMessage(string fromUserId, string toUserId, string text) {
            var message = new Message {
                SenderUserId = fromUserId,
                RecipientUserId = toUserId,
                Readed = false,
                SendDateTime = DateTime.UtcNow,
                Text = text
            };

            _context.PersonalMessages.Add(message);

            return _context.SaveChanges() > 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="senderUserId"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetLastConversations(string senderUserId) {
            var returnData = new Dictionary<string, string>();

            returnData.AddRange(_context.PersonalMessages
                .Where(_ => _.SenderUserId == senderUserId)
                .DistinctBy(_ => _.RecipientUserId)
                .ToDictionary(conversationUser => conversationUser.RecipientUserId, conversationUser => _context.Users.First(_ => _.Id.ToString() == conversationUser.RecipientUserId).UserName));

            returnData.AddRange(_context.PersonalMessages
                .Where(_ => _.RecipientUserId == senderUserId)
                .DistinctBy(_ => _.SenderUserId)
                .ToDictionary(conversationUser => conversationUser.SenderUserId, conversationUser => _context.Users.First(_ => _.Id.ToString() == conversationUser.SenderUserId).UserName));

            return returnData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userIdSender"></param>
        /// <param name="userIdRecipient"></param>
        /// <param name="startFrom"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public IEnumerable<Message> GetConversation(string userIdSender, string userIdRecipient, int startFrom = 0, int take = 10) {
            var allMessages = new List<Message>();
            // Get all messages, where 

            allMessages.AddRange(_context.PersonalMessages.Where(_ => _.RecipientUserId == userIdRecipient && _.SenderUserId == userIdSender));
            allMessages.AddRange(_context.PersonalMessages.Where(_ => _.RecipientUserId == userIdSender && _.SenderUserId == userIdRecipient));

            // Reading all messages
            //_context.PersonalMessages.Where(_ => _.RecipientUserId == userIdRecipient).ForEach(_ => _.Readed = true);

           // _context.SaveChanges();

            return allMessages.OrderBy(_ => _.SendDateTime);
        } 

        /// <summary>
        /// 
        /// </summary>
        public class Message {
            /// <summary>
            /// 
            /// </summary>
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public Guid Id { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string SenderUserId { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string RecipientUserId { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public bool Readed { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public DateTime SendDateTime { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public DateTime? ReadDateTime { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string Text { get; set; }
        }

         

    }
}