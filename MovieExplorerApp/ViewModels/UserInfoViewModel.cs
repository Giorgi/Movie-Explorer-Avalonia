using IdentityModel.OidcClient;

namespace MovieExplorerApp.ViewModels
{
    public class UserInfoViewModel : ViewModelBase
    {
        public LoginResult LoginResult { get; }

        public UserInfoViewModel(LoginResult loginResult)
        {
            LoginResult = loginResult;
            FullName = loginResult.User.Identity.Name;
        }

        public string FullName { get; set; }
    }
}