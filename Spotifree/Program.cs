using Leaf.xNet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Search;
using System.Text.RegularExpressions;
using YoutubeExplode.Converter;
using System.Threading;
using BetterConsole;
using Console = BetterConsole.BConsole;

namespace GUIDownloader
{
    internal class Program
    {
        public static Color colorsheme1 = Color.Magenta;
        public static Color colorsheme2 = Color.Blue;
        public static string nom = "";
        public static string artiste = "";
        public static string lien = "";
        private static readonly string OutputDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Output");
        private static async Task Main(string[] args)
        {
            Console.Title = "Spotifree Downloader | By Mighty#0943";
            LoadingScreen();
            JArray playlistArray;
            List<string> musi = new List<string>(5);
            try
            {
                Thread.Sleep(2000);
                Console.TypeGradient("[>] Enter your choice: ", colorsheme1, colorsheme2, 40);
                string res = (Console.ReadKey().KeyChar.ToString());
                Console.Delete(30);
                if (res == "1")
                {
                    if (!Directory.Exists("Temp"))
                    {
                        Directory.CreateDirectory("Temp");
                    }
                    Console.Clear();
                    AltScreen();
                    Console.WriteLine("\n\n");
                    string prompt = "[>] Enter the playlist ID: https://open.spotify.com/playlist/";
                    Console.TypeGradient(prompt, colorsheme1, colorsheme2, 20);
                    string ID = Console.ReadLine();
                    int ypos = Console.CursorTop - 1;
                    int xpos = Console.CursorLeft;
                    Console.SetCursorPosition(prompt.Length + ID.Length, ypos);
                    Console.Delete(prompt.Length + ID.Length, 4);
                    Thread.Sleep(250);
                    string PlaylistName = "";
                    int TracksNumber = 0;
                    PlaylistName = GetPlaylistName(ID);
                    PlaylistName = PlaylistName.Trim();
                    if (!Directory.Exists(@"Output\" + PlaylistName))
                    {
                        Directory.CreateDirectory(@"Output\" + PlaylistName);
                    }
                    prompt = "[+] Selected Playlist: " + PlaylistName + ".";
                    Console.TypeGradient(prompt + "\n", colorsheme1, colorsheme2, 20);
                    HttpRequest tokenRequest = new HttpRequest();
                    tokenRequest.UserAgent = Http.ChromeUserAgent();
                    String tokenJson = tokenRequest.Get("https://open.spotify.com/get_access_token?reason=transport&productType=web_player").ToString();
                    HttpRequest getSpotifyPlaylist = new HttpRequest();
                    JObject spotifyJsonToken = JObject.Parse(tokenJson);
                    String spotifyToken = spotifyJsonToken.SelectToken("accessToken").ToString();
                    getSpotifyPlaylist.AddHeader("Authorization", "Bearer " + spotifyToken);
                    String playlist = getSpotifyPlaylist.Get("https://api.spotify.com/v1/playlists/" + ID + "/tracks").ToString();
                    JObject jobject = JObject.Parse(playlist);
                    playlistArray = JArray.Parse(jobject.SelectToken("items").ToString());
                    TracksNumber = int.Parse(jobject.SelectToken("total").ToString());
                    prompt = "[+] Available Tracks: " + TracksNumber;
                    Console.TypeGradient(prompt + "\n", colorsheme1, colorsheme2, 20);
                    Console.WriteLine("\n");
                    prompt = "[>] Do you want to download this playlist? (y/n): ";
                    Console.TypeGradient(prompt, colorsheme1, colorsheme2, 40);
                    string rep = Console.ReadKey().KeyChar.ToString();
                    Console.Delete(prompt.Length + 5, 20);
                    if (rep == "y")
                    {
                        for (int i = 0; i < TracksNumber; i++)
                        {
                            Console.Title = $"Spotifree By Mighty#0943 | Downloading {PlaylistName} | ETA: {i}/{TracksNumber}";
                            int num2 = i + 1;
                            Console.Clear();
                            AltScreen();
                            HttpRequest w = new HttpRequest();
                            nom = playlistArray[i].SelectToken("track").SelectToken("name").ToString();
                            artiste = (playlistArray[i].SelectToken("track").SelectToken("artists")[0].SelectToken("name").ToString());
                            musi.Add(nom);
                            var client = new WebClient();
                            string sen = "\n[+] Downloading: " + artiste + " - " + nom + "...";
                            Console.TypeGradient(sen, colorsheme1, colorsheme2, 20);
                            try
                            {
                                string imageurl = playlistArray[i].SelectToken("track").SelectToken("album").SelectToken("images")[0].SelectToken("url").ToString();
                                client.DownloadFile(imageurl, "Temp\\" + artiste + " - " + nom + ".jpg");
                            }
                            catch (Exception ex)
                            {
                                if (!File.Exists("ErrorLogs.txt"))
                                {
                                    File.Create("ErrorLogs.txt");
                                }
                                File.WriteAllText("ErrorLogs.txt", ex.ToString());
                            }
                            var youtube = new YoutubeClient();

                            await foreach (var result in youtube.Search.GetResultsAsync(artiste + " - " + nom))
                            {
                                switch (result)
                                {
                                    case VideoSearchResult video:
                                        {
                                            try
                                            {
                                                if (video.Title.ToString().Contains("clip") || video.Title.ToString().Contains("video"))
                                                {

                                                }
                                                if (File.Exists("Output" + @"\" + PlaylistName + @"\" + num2 + "- " + artiste + " - " + nom + ".mp3"))
                                                {
                                                    sen = "\n[+]" + artiste + " - " + nom + " is already downloaded, skipping...";
                                                    Console.TypeGradient(sen, colorsheme1, colorsheme2, 10);
                                                    break;
                                                }
                                                var youtube2 = new YoutubeClient();

                                                var id = video.Id;
                                                lien = "https://www.youtube.com/watch?v=" + id.ToString();

                                                // Download video as mp3
                                                Directory.CreateDirectory(OutputDirectoryPath);
                                                Directory.CreateDirectory(Path.Combine(OutputDirectoryPath + @"\" + PlaylistName));
                                                var outputFilePath = Path.Combine(OutputDirectoryPath + @"\" + PlaylistName + @"\" + num2 + "- " + artiste + " - " + nom + ".mp3");
                                                await youtube.Videos.DownloadAsync(lien, outputFilePath, o => o
                                                    .SetFormat("mp3") // override format
                                                    .SetPreset(ConversionPreset.UltraFast) // change preset
                                                    .SetFFmpegPath(Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg.exe"))// custom FFmpeg location
                                                );
                                            }
                                            catch (Exception ex)
                                            {
                                                if (!File.Exists("ErrorLogs.txt"))
                                                {
                                                    File.Create("ErrorLogs.txt");
                                                }
                                                File.WriteAllText("ErrorLogs.txt", ex.ToString());
                                            }
                                            // Edit mp3 metadata
                                            try
                                            {
                                                sen = "\n[+] Writing Metadatas...";
                                                Console.TypeGradient(sen, colorsheme1, colorsheme2, 40);
                                                using (var meta = TagLib.File.Create(OutputDirectoryPath + @"\" + PlaylistName + @"\" + num2 + "- " + artiste + " - " + nom + ".mp3"))
                                                {
                                                    meta.Tag.Performers = new[] { artiste };
                                                    meta.Tag.Title = nom;
                                                    meta.Tag.Genres = new[] { "Spotifree By Mighty" };
                                                    meta.Tag.AlbumArtists = new[] { artiste };
                                                    meta.Tag.Album = playlistArray[i].SelectToken("track").SelectToken("album").SelectToken("name").ToString();
                                                    meta.Save();
                                                    TagLib.Picture pic = new TagLib.Picture
                                                    {
                                                        Type = TagLib.PictureType.FrontCover,
                                                        Description = "Cover",
                                                        MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg
                                                    };
                                                    MemoryStream ms = new MemoryStream();
                                                    Image image = Image.FromFile("Temp\\" + artiste + " - " + nom + ".jpg");
                                                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                                    ms.Position = 0;
                                                    pic.Data = TagLib.ByteVector.FromStream(ms);
                                                    meta.Tag.Pictures = new TagLib.IPicture[] { pic };
                                                    meta.Save();
                                                    ms.Close();
                                                    sen = "\n[+] Done!";
                                                    Console.TypeGradient(sen, colorsheme1, colorsheme2, 40);
                                                    Thread.Sleep(1000);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                if (!File.Exists("ErrorLogs.txt"))
                                                {
                                                    File.Create("ErrorLogs.txt");
                                                }
                                                File.WriteAllText("ErrorLogs.txt", ex.ToString());
                                            }
                                            break;
                                        }
                                }
                                break;
                            }
                        }

                    }
                    EndScreen();
                }
                if (res == "2")
                {
                    try
                    {
                        Console.Clear();
                        AltScreen();
                        Thread.Sleep(2000);
                        string sen = "[+] Enter album's ID: https://open.spotify.com/albums/";
                        Console.TypeGradient(sen, colorsheme1, colorsheme2, 40);
                        string ID = Console.ReadLine();
                        HttpRequest tokenRequest = new HttpRequest();
                        tokenRequest.UserAgent = Http.ChromeUserAgent();
                        String tokenJson = tokenRequest.Get("https://open.spotify.com/get_access_token?reason=transport&productType=web_player").ToString();
                        HttpRequest getSpotifyPlaylist = new HttpRequest();
                        JObject spotifyJsonToken = JObject.Parse(tokenJson);
                        String spotifyToken = spotifyJsonToken.SelectToken("accessToken").ToString();
                        getSpotifyPlaylist.AddHeader("Authorization", "Bearer " + spotifyToken);
                        String playlist = getSpotifyPlaylist.Get("https://api.spotify.com/v1/albums/" + ID).ToString();
                        JObject jobject = JObject.Parse(playlist);
                        playlistArray = JArray.Parse(jobject.SelectToken("tracks").SelectToken("items").ToString());
                        string art1 = jobject.SelectToken("artists")[0].SelectToken("name").ToString();
                        string coverURL = jobject.SelectToken("images")[0].SelectToken("url").ToString();
                        string albumName = jobject.SelectToken("name").ToString();
                        if (!Directory.Exists(@"Output\" + albumName))
                        {
                            Directory.CreateDirectory(@"Output\" + albumName);
                        }
                        int test = int.Parse(jobject.SelectToken("total_tracks").ToString());
                        string prompt = "[+] Selected album: " + albumName;
                        Console.TypeGradient(prompt + "\n", colorsheme1, colorsheme2, 20);
                        prompt = "[+] Total tracks: " + test;
                        Console.TypeGradient(prompt + "\n", colorsheme1, colorsheme2, 20);
                        Console.WriteLine("\n");
                        prompt = "[>] Do you want to download this album? (y/n): ";
                        Console.TypeGradient(prompt, colorsheme1, colorsheme2, 40);
                        string rep = Console.ReadKey().KeyChar.ToString();
                        Console.Delete(prompt.Length + 5, 20);
                        if (rep == "y")
                        {
                            for (int i = 0; i < test; i++)
                            {
                                int num2 = i + 1;
                                Console.Title = $"Spotifree By Mighty#0943 | Downloading {albumName} | ETA: {i}/{test}";
                                Console.Clear();
                                AltScreen();
                                HttpRequest w = new HttpRequest();
                                nom = playlistArray[i].SelectToken("name").ToString();
                                artiste = art1;
                                musi.Add(nom);
                                var client = new WebClient();
                                sen = "\n[+] Downloading: " + artiste + " - " + nom + "...";
                                Console.TypeGradient(sen, colorsheme1, colorsheme2, 20);
                                try
                                {
                                    string imageurl = playlistArray[i].SelectToken("track").SelectToken("album").SelectToken("images")[0].SelectToken("url").ToString();
                                    client.DownloadFile(imageurl, "Temp\\" + artiste + " - " + nom + ".jpg");
                                }
                                catch (Exception ex)
                                {
                                    if (!File.Exists("ErrorLogs.txt"))
                                    {
                                        File.Create("ErrorLogs.txt");
                                    }
                                    File.WriteAllText("ErrorLogs.txt", ex.ToString());
                                }
                                var youtube = new YoutubeClient();

                                await foreach (var result in youtube.Search.GetResultsAsync(artiste + " - " + nom))
                                {
                                    switch (result)
                                    {
                                        case VideoSearchResult video:
                                            {
                                                try
                                                {
                                                    if (video.Title.ToString().Contains("clip") || video.Title.ToString().Contains("video"))
                                                    {

                                                    }
                                                    if (File.Exists("Output" + @"\" + albumName + @"\" + num2 + "- " + artiste + " - " + nom + ".mp3"))
                                                    {
                                                        sen = "\n[+]" + artiste + " - " + nom + " is already downloaded, skipping...";
                                                        Console.TypeGradient(sen, colorsheme1, colorsheme2, 10);
                                                        break;
                                                    }
                                                    var youtube2 = new YoutubeClient();

                                                    var id = video.Id;
                                                    lien = "https://www.youtube.com/watch?v=" + id.ToString();

                                                    // Download video as mp3
                                                    Directory.CreateDirectory(OutputDirectoryPath);
                                                    Directory.CreateDirectory(Path.Combine(OutputDirectoryPath + @"\" + albumName));
                                                    var outputFilePath = Path.Combine(OutputDirectoryPath + @"\" + albumName + @"\" + num2 + "- " + artiste + " - " + nom + ".mp3");
                                                    await youtube.Videos.DownloadAsync(lien, outputFilePath, o => o
                                                        .SetFormat("mp3") // override format
                                                        .SetPreset(ConversionPreset.UltraFast) // change preset
                                                        .SetFFmpegPath(Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg.exe"))// custom FFmpeg location
                                                    );
                                                }
                                                catch (Exception ex)
                                                {
                                                    if (!File.Exists("ErrorLogs.txt"))
                                                    {
                                                        File.Create("ErrorLogs.txt");
                                                    }
                                                    File.WriteAllText("ErrorLogs.txt", ex.ToString());
                                                }
                                                // Edit mp3 metadata
                                                try
                                                {
                                                    var id = video.Id;
                                                    sen = "\n[+] Writing Metadatas...";
                                                    Console.TypeGradient(sen, colorsheme1, colorsheme2, 40);
                                                    var idMatch = Regex.Match(id, @"^(?<artist>.*?)-(?<title>.*?)$");
                                                    var artist = idMatch.Groups["artist"].Value.Trim();
                                                    var title = idMatch.Groups["title"].Value.Trim();
                                                    using (var meta = TagLib.File.Create(OutputDirectoryPath + @"\" + albumName + @"\" + num2 + "- " + artiste + " - " + nom + ".mp3"))
                                                    {
                                                        meta.Tag.Performers = new[] { art1 };
                                                        meta.Tag.Title = nom;
                                                        meta.Tag.Genres = new[] { "Spotifree By Mighty" };
                                                        meta.Tag.Album = jobject.SelectToken("name").ToString();
                                                        meta.Tag.TrackCount = uint.Parse(num2.ToString());
                                                        meta.Tag.Copyright = jobject.SelectToken("label").ToString();
                                                        meta.Save();
                                                        TagLib.Picture pic = new TagLib.Picture
                                                        {
                                                            Type = TagLib.PictureType.FrontCover,
                                                            Description = "Cover",
                                                            MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg
                                                        };
                                                        MemoryStream ms = new MemoryStream();
                                                        Image image = Image.FromFile(OutputDirectoryPath + artiste + " - " + nom + ".jpg");
                                                        image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                                        ms.Position = 0;
                                                        pic.Data = TagLib.ByteVector.FromStream(ms);
                                                        meta.Tag.Pictures = new TagLib.IPicture[] { pic };
                                                        meta.Save();
                                                        ms.Close();
                                                        sen = "\n[+] Done!";
                                                        Console.TypeGradient(sen, colorsheme1, colorsheme2, 40);
                                                        Thread.Sleep(1000);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    if (!File.Exists("ErrorLogs.txt"))
                                                    {
                                                        File.Create("ErrorLogs.txt");
                                                    }
                                                    File.WriteAllText("ErrorLogs.txt", ex.ToString());
                                                }
                                                break;
                                            }
                                    }
                                    break;
                                }
                            }
                            EndScreen();

                        }
                    }
                    catch (Exception ex)
                    {
                        if (!File.Exists("ErrorLogs.txt"))
                        {
                            File.Create("ErrorLogs.txt");
                        }
                        File.WriteAllText("ErrorLogs.txt", ex.ToString());
                    }
                }
                if(res == "3")
                {
                    Console.Clear();
                    Credits();
                }
            }
            catch (Exception ex)
            {
                if (!File.Exists("ErrorLogs.txt"))
                {
                    File.Create("ErrorLogs.txt");
                }
                File.WriteAllText("ErrorLogs.txt", ex.ToString());
            }
        }
        public static string GetPlaylistName(string ID)
        {
            HttpRequest tokenRequest = new HttpRequest();
            tokenRequest.UserAgent = Http.ChromeUserAgent();
            String tokenJson = tokenRequest.Get("https://open.spotify.com/get_access_token?reason=transport&productType=web_player").ToString();
            HttpRequest getSpotifyPlaylist = new HttpRequest();
            JObject spotifyJsonToken = JObject.Parse(tokenJson);
            String spotifyToken = spotifyJsonToken.SelectToken("accessToken").ToString();
            getSpotifyPlaylist.AddHeader("Authorization", "Bearer " + spotifyToken);
            String playlist2 = getSpotifyPlaylist.Get("https://api.spotify.com/v1/playlists/" + ID + "/").ToString();
            File.WriteAllText("Result.txt", playlist2);
            JObject jobject = JObject.Parse(playlist2);
            string PlaylistName = jobject.SelectToken("name").ToString();
            return PlaylistName;
        }
        public static void MainScreen()
        {
            string screen = @"

                           

                           ███████╗██████╗  ██████╗ ████████╗██╗███████╗██████╗ ███████╗███████╗
                           ██╔════╝██╔══██╗██╔═══██╗╚══██╔══╝██║██╔════╝██╔══██╗██╔════╝██╔════╝
                           ███████╗██████╔╝██║   ██║   ██║   ██║█████╗  ██████╔╝█████╗  █████╗  
                           ╚════██║██╔═══╝ ██║   ██║   ██║   ██║██╔══╝  ██╔══██╗██╔══╝  ██╔══╝  
                           ███████║██║     ╚██████╔╝   ██║   ██║██║     ██║  ██║███████╗███████╗
                           ╚══════╝╚═╝      ╚═════╝    ╚═╝   ╚═╝╚═╝     ╚═╝  ╚═╝╚══════╝╚══════╝
                           
                                                      By Mighty#0943
                          
                          
";
            Console.WriteGradient(screen, colorsheme1, colorsheme2);
            Console.WriteGradient("                                              [1] Download a SPOTIFY playlist\n", colorsheme1, colorsheme2);
            Console.WriteGradient("                                              [2] Download a SPOTIFY album\n", colorsheme1, colorsheme2);
            Console.WriteGradient("                                              [3] View credits\n", colorsheme1, colorsheme2);
            Console.WriteLine("\n\n\n");
            Console.CursorVisible = true;
        }
        public static void AltScreen()
        {
            string screen = @"

                          
                          ███████╗██████╗  ██████╗ ████████╗██╗███████╗██████╗ ███████╗███████╗
                          ██╔════╝██╔══██╗██╔═══██╗╚══██╔══╝██║██╔════╝██╔══██╗██╔════╝██╔════╝
                          ███████╗██████╔╝██║   ██║   ██║   ██║█████╗  ██████╔╝█████╗  █████╗  
                          ╚════██║██╔═══╝ ██║   ██║   ██║   ██║██╔══╝  ██╔══██╗██╔══╝  ██╔══╝  
                          ███████║██║     ╚██████╔╝   ██║   ██║██║     ██║  ██║███████╗███████╗
                          ╚══════╝╚═╝      ╚═════╝    ╚═╝   ╚═╝╚═╝     ╚═╝  ╚═╝╚══════╝╚══════╝
                          
                                                     By Mighty#0943
                          

";
            Console.WriteGradient(screen, colorsheme1, colorsheme2);
        }
        public static void LoadingScreen()
        {
            string screen = @"




                         
                          ███████╗██████╗  ██████╗ ████████╗██╗███████╗██████╗ ███████╗███████╗
                          ██╔════╝██╔══██╗██╔═══██╗╚══██╔══╝██║██╔════╝██╔══██╗██╔════╝██╔════╝
                          ███████╗██████╔╝██║   ██║   ██║   ██║█████╗  ██████╔╝█████╗  █████╗  
                          ╚════██║██╔═══╝ ██║   ██║   ██║   ██║██╔══╝  ██╔══██╗██╔══╝  ██╔══╝  
                          ███████║██║     ╚██████╔╝   ██║   ██║██║     ██║  ██║███████╗███████╗
                          ╚══════╝╚═╝      ╚═════╝    ╚═╝   ╚═╝╚═╝     ╚═╝  ╚═╝╚══════╝╚══════╝ (v.2)
                          
                                                     By Mighty#0943
                          
                                                        Loading...

";
            Console.AnimateColor(screen, 30, 3000, colorsheme1, colorsheme2);
            Console.Clear();
            MainScreen();
        }
        public static void Credits()
        {
            string screen = @"




                         
                          ███████╗██████╗  ██████╗ ████████╗██╗███████╗██████╗ ███████╗███████╗
                          ██╔════╝██╔══██╗██╔═══██╗╚══██╔══╝██║██╔════╝██╔══██╗██╔════╝██╔════╝
                          ███████╗██████╔╝██║   ██║   ██║   ██║█████╗  ██████╔╝█████╗  █████╗  
                          ╚════██║██╔═══╝ ██║   ██║   ██║   ██║██╔══╝  ██╔══██╗██╔══╝  ██╔══╝  
                          ███████║██║     ╚██████╔╝   ██║   ██║██║     ██║  ██║███████╗███████╗
                          ╚══════╝╚═╝      ╚═════╝    ╚═╝   ╚═╝╚═╝     ╚═╝  ╚═╝╚══════╝╚══════╝ (v.2)
                          
                                                     By Mighty#0943


                                        [+] GitHub: https://github.com/MightyTea
                                        [+] Twitter: https://twitter.com/cmoimighty
                                        [+] Discord: https://discord.gg/pvYGnM63

";
            Console.AnimateRainbow(screen, 30, 10000);
            Console.Clear();
            MainScreen();
        }
        public static void EndScreen()
        {
            Console.Clear();
            string screen = @"


                                                 Finished your download!

                         
                          ███████╗██████╗  ██████╗ ████████╗██╗███████╗██████╗ ███████╗███████╗
                          ██╔════╝██╔══██╗██╔═══██╗╚══██╔══╝██║██╔════╝██╔══██╗██╔════╝██╔════╝
                          ███████╗██████╔╝██║   ██║   ██║   ██║█████╗  ██████╔╝█████╗  █████╗  
                          ╚════██║██╔═══╝ ██║   ██║   ██║   ██║██╔══╝  ██╔══██╗██╔══╝  ██╔══╝  
                          ███████║██║     ╚██████╔╝   ██║   ██║██║     ██║  ██║███████╗███████╗
                          ╚══════╝╚═╝      ╚═════╝    ╚═╝   ╚═╝╚═╝     ╚═╝  ╚═╝╚══════╝╚══════╝ (v.2)
                          
                                                     By Mighty#0943


                                        [+] GitHub: https://github.com/MightyTea
                                        [+] Twitter: https://twitter.com/cmoimighty
                                        [+] Discord: https://discord.gg/pvYGnM63

";
            Console.AnimateRainbow(screen, 30, 10000);
            Console.Clear();
            MainScreen();
        }
    }
}