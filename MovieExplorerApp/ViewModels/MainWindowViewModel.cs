using Avalonia.Controls;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using MovieExplorerApp.Services;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MovieExplorerApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IMoviesService moviesService;

        private bool isBusy;
        private string? searchText;
        private MovieViewModel? selectedMovie;
        private MovieViewModel currentMovie;
        private UserInfoViewModel? user;

        private readonly OidcClientOptions oktaSignInOptions = new()
        {
            Authority = "https://{yourOktaDomain}",
            ClientId = "{ClientId}",
            RedirectUri = "http://127.0.0.1:8090/callback",
            PostLogoutRedirectUri = "http://127.0.0.1:8090/",
            Scope = "openid profile",
            Browser = new SystemBrowser(8090),
            ProviderInformation = new ProviderInformation
            {
                IssuerName = "https://{yourOktaDomain}",
                AuthorizeEndpoint = "https://{yourOktaDomain}/oauth2/v1/authorize",
                TokenEndpoint = "https://{yourOktaDomain}/oauth2/v1/token",
                
                EndSessionEndpoint = "https://{yourOktaDomain}/oauth2/v1/logout",
            },
            LoadProfile = false,
            Policy = new Policy
            {
                Discovery = new DiscoveryPolicy
                {
                    RequireKeySet = false
                }
            }
        };

        private readonly OidcClient oidcClient;

        public MainWindowViewModel()
        {
            oidcClient = new OidcClient(oktaSignInOptions);
        }

        public MainWindowViewModel(IMoviesService moviesService)
        {
            this.moviesService = moviesService;

            this.WhenAnyValue(x => x.SearchText)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Throttle(TimeSpan.FromMilliseconds(400))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(SearchMovies!);

            LoginCommand = ReactiveCommand.CreateFromTask(Login);
            LogoutCommand = ReactiveCommand.CreateFromTask(Logout);

            oidcClient = new OidcClient(oktaSignInOptions);
        }

        private async Task Login()
        {
            var result = await oidcClient.LoginAsync();

            if (result.IsError)
            {
                var messageBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow("Error", result.ErrorDescription, ButtonEnum.Ok, Icon.Error, WindowStartupLocation.CenterOwner);
                await messageBoxStandardWindow.Show();
            }
            else
            {
                User = new UserInfoViewModel(result);
            }
        }

        private async Task Logout()
        {
            var result = await oidcClient.LogoutAsync(new LogoutRequest
            {
                IdTokenHint = User.LoginResult.IdentityToken
            });
            User = null;
        }

        public UserInfoViewModel? User
        {
            get => user;
            set => this.RaiseAndSetIfChanged(ref user, value);
        }

        public string? SearchText
        {
            get => searchText;
            set => this.RaiseAndSetIfChanged(ref searchText, value);
        }

        public bool IsBusy
        {
            get => isBusy;
            set => this.RaiseAndSetIfChanged(ref isBusy, value);
        }

        public MovieViewModel CurrentMovie
        {
            get => currentMovie;
            set => this.RaiseAndSetIfChanged(ref currentMovie, value);
        }

        public MovieViewModel? SelectedMovie
        {
            get => selectedMovie;
            set => LoadMovie(value.Id);
        }

        private async void LoadMovie(int valueId)
        {
            CurrentMovie = new MovieViewModel(await moviesService.GetMovie(valueId));
            await CurrentMovie.LoadPoster();
        }

        public ObservableCollection<MovieViewModel> PopularMovies { get; } = new();

        public ObservableCollection<MovieViewModel> SearchResults { get; } = new();


        public ICommand LoginCommand { get; set; }
        
        public ICommand LogoutCommand { get; set; }

        public override async void OnOpened()
        {
            var popularMovies = await moviesService.GetPopularMovies();

            foreach (var movie in popularMovies)
            {
                PopularMovies.Add(new MovieViewModel(movie));
            }

            LoadCovers(PopularMovies);
        }


        private async void SearchMovies(string search)
        {
            IsBusy = true;
            SearchResults.Clear();

            var albums = await moviesService.DiscoverMovies(search);

            foreach (var album in albums)
            {
                var vm = new MovieViewModel(album);

                SearchResults.Add(vm);
            }

            LoadCovers(SearchResults);

            IsBusy = false;
        }

        private async void LoadCovers(ObservableCollection<MovieViewModel> movies)
        {
            foreach (var model in movies)
            {
                await model.LoadCover();
            }
        }
    }
}
