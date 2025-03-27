using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using UPlant.Models;

namespace UPlant.ViewComponents
{
    public class MenuMessageViewComponent : ViewComponent
    {
        public MenuMessageViewComponent() { }

        public IViewComponentResult Invoke(string filter)
        {
            var messages = GetData();
            return View(messages);
        }
        private List<Message> GetData()
        {
            var messages = new List<Message>();

            messages.Add(new Message
            {
                Id = 1,
                UserId = "Pippo", //((ClaimsPrincipal)User).GetUserProperty(CustomClaimTypes.NameIdentifier),
                DisplayName = "Support Team",
                AvatarURL = "/img/user1-128x128.jpg",
                ShortDesc = "Why not buy a new awesome theme?",
                TimeSpan = "5 mins",
                URLPath = "#",
            });

            messages.Add(new Message
            {
                Id = 1,
                UserId = "Prova",//((ClaimsPrincipal)User).GetUserProperty(CustomClaimTypes.NameIdentifier),
                DisplayName = "Ken",
                AvatarURL = "/img/user3-128x128.jpg",
                ShortDesc = "For approval",
                TimeSpan = "15 mins",
                URLPath = "#",
            });

            return messages;
        }
    }
}