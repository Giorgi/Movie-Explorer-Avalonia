using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using LibVLCSharp.Shared;
using ReactiveUI;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;
using YouTubeApiSharp;

namespace MovieExplorerApp.ViewModels
{
    public class MovieViewModel : ViewModelBase
    {
        private readonly LibVLC libVlc = new LibVLC();
        static readonly HttpClient HttpClient = new();

        private readonly Movie? movie;
        private readonly SearchMovie? searchMovie;

        private Bitmap? cover;
        private Bitmap? poster;
        private MediaPlayer mediaPlayer;

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

        public int VoteCount => movie.VoteCount;
        
        public double VoteAverage => movie.VoteAverage;
        
        public DateTime ReleaseDate => movie.ReleaseDate.Value;

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

        public MediaPlayer MediaPlayer
        {
            get => mediaPlayer;
            private set => this.RaiseAndSetIfChanged(ref mediaPlayer, value);
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

        public void GetYouTubeUrl()
        {
            if (movie!.Videos.Results.Any())
            {
                var urls = DownloadUrlResolver.GetDownloadUrls($"https://www.youtube.com/watch?v={movie.Videos.Results[0].Key}");

                var videoInfo = urls.FirstOrDefault(info => info.VideoType == VideoType.Mp4);
                MediaPlayer = new MediaPlayer(new Media(libVlc, new Uri(videoInfo.DownloadUrl)));
                //MediaPlayer.Play();
            }
        }
    }
}