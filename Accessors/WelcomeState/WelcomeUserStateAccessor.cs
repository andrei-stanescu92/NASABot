using Microsoft.Bot.Builder;
using NASABot.Models;
using System;
using System.Collections.Generic;

namespace NASABot
{
    /// Initializes a new instance of the <see cref="WelcomeUserStateAccessors"/> class.
    public class WelcomeUserStateAccessors
    {
        public UserState UserState { get; }
        public WelcomeUserStateAccessors(UserState userState)
        {
            this.UserState = userState ?? throw new ArgumentNullException(nameof(userState));
        }

        public IStatePropertyAccessor<bool> DidBotWelcomeUser { get; set; }

        public IStatePropertyAccessor<string> UserProfile { get; set; }

        public IStatePropertyAccessor<IList<MarsRoverPhoto>> UserData { get; set; }
    }
}
