using System.Linq;
using CryptoMarket.Models;

namespace CryptoMarket.Source.Managers{
    public class EmergencyMessagesManager{
        public class EmergencyMessageState{
            public bool IsActive { get; set; }
            public string Text { get; set; }
        }

        public static EmergencyMessageState Get(){
            using (var context = new ApplicationDbContext()){
                if (context.EmergencyMessages.Any(messages => messages.Active)){
                    return new EmergencyMessageState{
                        IsActive = true,
                        Text = context.EmergencyMessages.First(message => message.Active).Text
                    };

                }
                return new EmergencyMessageState{
                    IsActive = false
                };
            }
        }

    }
}