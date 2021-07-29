using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace FirstCode_Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        public MailJetSettings _mailjetSettings { get; set; }
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(email,subject,htmlMessage);
        }

        public async Task Execute(string email, string subject, string body)
        {
            _mailjetSettings = _configuration.GetSection("MailJet").Get<MailJetSettings>();
            MailjetClient client = new MailjetClient(_mailjetSettings.ApiKey,_mailjetSettings.SecretKey)
            {
                //Version = ApiVersion.V3_1,
            };
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
             .Property(Send.Messages, new JArray {
     new JObject {
      {
       "From",
       new JObject {
        {"Email", "mridhapower@protonmail.com"},
        {"Name", "Ashraful"}
       }
      }, {
       "To",
       new JArray {
        new JObject {
         {
          "Email",
          email
         }, {
          "Name",
          "DOtNet"
         }
        }
       }
      }, {
       "Subject",
       subject
      }, {
       "HTMLPart",
       body
      }
     }
             });
            /*MailjetResponse response = */await client.PostAsync(request);
        }
    }
}
