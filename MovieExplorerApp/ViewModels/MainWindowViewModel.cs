using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Web;
using MovieExplorerApp.Services;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace MovieExplorerApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IMoviesService moviesService;

        private bool isBusy;
        private string? searchText;
        private MovieViewModel? selectedMovie;
        private MovieViewModel currentMovie;

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


        public MovieViewModel? SelectedMovie
        {
            get => selectedMovie;
            set => LoadMovie(value.Id);
        }

        public MovieViewModel CurrentMovie
        {
            get => currentMovie;
            set => this.RaiseAndSetIfChanged(ref currentMovie, value);
        }

        private async void LoadMovie(int valueId)
        {
            CurrentMovie = new MovieViewModel(await moviesService.GetMovie(valueId));
            await CurrentMovie.LoadPoster();
        }

        public ObservableCollection<MovieViewModel> PopularMovies { get; } = new();
        public ObservableCollection<MovieViewModel> SearchResults { get; } = new();

        public MainWindowViewModel()
        {

        }

        public MainWindowViewModel(IMoviesService moviesService)
        {
            this.moviesService = moviesService;

            this.WhenAnyValue(x => x.SearchText)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Throttle(TimeSpan.FromMilliseconds(400))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(SearchMovies!);
        }

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
