using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using MovieExplorerApp.Services;
using ReactiveUI;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;

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

    public class MovieViewModel : ViewModelBase
    {
        static readonly HttpClient HttpClient = new();

        private readonly Movie? movie;
        private readonly SearchMovie? searchMovie;

        private Bitmap? cover;
        private Bitmap? poster;

        public MovieViewModel(SearchMovie searchMovie)
        {
            this.searchMovie = searchMovie;
        }

        public MovieViewModel(Movie movie)
        {
            this.movie = movie;
        }

        public int Id => searchMovie.Id;

        public string Title => searchMovie?.Title ?? movie?.Title;

        public string Overview => searchMovie.Overview;

        public Bitmap? Cover
        {
            get => cover;
            private set => this.RaiseAndSetIfChanged(ref cover, value);
        }

        public Bitmap? Poster
        {
            get => poster;
            private set => this.RaiseAndSetIfChanged(ref poster, value);
        }

        public List<Cast> Cast => movie!.Credits.Cast;

        public async Task LoadCover()
        {
            var data = await HttpClient.GetByteArrayAsync($"https://image.tmdb.org/t/p/w500{searchMovie?.BackdropPath}");

            var memoryStream = new MemoryStream(data);
            Cover = await Task.Run(() => Bitmap.DecodeToWidth(memoryStream, 400));
        }

        public async Task LoadPoster()
        {
            var data = await HttpClient.GetByteArrayAsync($"https://image.tmdb.org/t/p/w500{movie!.PosterPath}");

            var memoryStream = new MemoryStream(data);
            Poster = await Task.Run(() => Bitmap.DecodeToWidth(memoryStream, 400));
        }
    }
}
