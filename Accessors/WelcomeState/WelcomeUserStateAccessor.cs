using Microsoft.Bot.Builder;
using System;

namespace NASABot
{
    /// Initializes a new instance of the <see cref="WelcomeUserStateAccessors"/> class.
    public class WelcomeUserStateAccessors
    {
        public WelcomeUserStateAccessors(UserState userState)
        {
            this.UserState = userState ?? throw new ArgumentNullException(nameof(userState));
        }

        public IStatePropertyAccessor<bool> DidBotWelcomeUser { get; set; }

        public UserState UserState { get; }
    }
}
