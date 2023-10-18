using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;

namespace Schwack.Listner
{
    internal class Program
    {
        public static Settings settings = new Settings();
        public static List<Movie> movies = new List<Movie>();
        public static HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            Console.Title = "fishie's Schwack Reader";

            Console.WriteLine("Reading Settings");

            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json")))
            {
                Console.WriteLine("Settings file is missing :(.");
                Environment.Exit(404);
            }

            IConfiguration configuration = new ConfigurationBuilder()
           .SetBasePath(AppDomain.CurrentDomain.BaseDirectory) // Set the base path to your application's root directory
           .AddJsonFile("appsettings.json") // Specify the JSON file to read
           .Build();

            var movie = configuration.GetSection("CinemaConfig");
            var tele = configuration.GetSection("TeleBotConfiguration");

            settings.BotToken = tele["BotToken"];
            settings.ChatID = tele["ChatID"];
            settings.MovieName = movie["MovieName"];

            Console.WriteLine("Sending Test Notification");
            //await TestSeatBuy();

            //await SendMsg($"Bot Started Listning For The Movie : {settings.MovieName}");

            Console.WriteLine("Test Notification Sent. If you didnt get it check your settings file and read the project readme for more details on how to setup this bot.");
            Stopwatch stopwatch = new Stopwatch();

            while (true)
            {
                stopwatch.Start();
                Console.Clear();
                Console.WriteLine("Checking Website for updates");

                var html = await client.GetStringAsync("https://www.cinema.mv/schedule");
                ReadHtml(html);
                await CheckForMovie();
                stopwatch.Stop();
                Console.WriteLine($"Last Checked {DateTime.Now}");
                Console.WriteLine($"Nect Check   {DateTime.Now.AddMinutes(30)}");
                TimeSpan elapsed = stopwatch.Elapsed;

                await Task.Delay(TimeSpan.FromMinutes(30));

            }
        }

        static void ReadHtml(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var divs = doc.DocumentNode.SelectNodes("//div[@class='caption']");

            if (divs != null)
            {
                Console.ForegroundColor = ConsoleColor.Blue;

                foreach (var div in divs)
                {
                    Movie movie = new Movie();
                    movie.Title = div.SelectSingleNode(".//div[@class='name']").InnerText;
                    movie.Time = div.SelectNodes(".//div[@class='info']")[0].InnerText;
                    movie.Location = div.SelectNodes(".//div[@class='info']")[1].InnerText;
                    movie.Url = div.SelectSingleNode(".//a[@class='button primary large']").GetAttributeValue("href", "");

                    Console.WriteLine(movie.Title + " " + movie.Time + " " + movie.Location);
                    movies.Add(movie);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Website is down for updates");
            }
            Console.ResetColor();
        }

        /// <summary>
        /// go thorugh all movies until it sees the movie we want and announces it to the group chat
        /// </summary>
        /// <returns></returns>
        static async Task CheckForMovie()
        {
            foreach (var movie in movies)
            {
                if (movie.Title.ToLower().Contains(settings.MovieName.ToLower()))
                {
                    string Msg = $"MOVIE ON SALE!!🚨🚨🚨\n{movie.Title}\n{movie.Time}\n{movie.Location},\nhttps://www.cinema.mv{movie.Url}";
                    await SendMsg(Msg);

                    //commenting out the code for auto buying tickets
                    //try
                    //{
                    //    //await CheckSeats(movie);
                    //}
                    //catch
                    //{
                    //    await SendMsg("ERROR WHILE BOOKIN!! DEV PULL TF UP");
                    //}
                }
            }
        }

        static async Task CheckSeats(Movie movie)
        {
            var html = await client.GetStringAsync("https://cinema.mv" + movie.Url);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var ulElement = doc.DocumentNode.SelectSingleNode("//ul[@class='float-clear']");

            var links = ulElement.SelectNodes(".//a");

            foreach (var link in links)
            {
                string href = link.GetAttributeValue("href", "");

                if (href != "")
                {
                    HtmlNode aElement = link;

                    var timeSpan = aElement.SelectSingleNode(".//span[@class='time']");
                    var typeSpan = aElement.SelectSingleNode(".//span[@class='type']");

                    if (timeSpan != null && typeSpan != null)
                    {
                        string time = timeSpan.InnerText;
                        string type = typeSpan.InnerText;

                        string[] parts = href.Split('/');

                        string movie_id = parts[parts.Length - 1];


                        if (type.Contains("MLE"))
                        {
                            await SendMsg($"Attempting to Snipe MLE Tickets {time}");
                            Console.WriteLine($"Attempting to Snipe MLE Tickets {time}");

                            await Snipe(new List<string> { "B12", "B13" }, "phone_number", "email@gmail.com", "name", movie_id);
                        }
                        else
                        {
                            await SendMsg($"Attempting to Snipe HML Tickets {time}");
                            Console.WriteLine($"Attempting to Snipe HML Tickets {time}");

                            await Snipe(new List<string> { "B12", "B13" }, "phone_number", "email@gmail.com", "name", movie_id);
                            
                        }
                    }
                }
            }
        }


        static async Task TestSeatBuy()
        {
            var html = await client.GetStringAsync("https://cinema.mv/movie/the-creator/20904");

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var ulElement = doc.DocumentNode.SelectSingleNode("//ul[@class='float-clear']");

            var links = ulElement.SelectNodes(".//a");

            //we test on the show thats gonna be shown last so that there will be time for the tickets to expire
            //otherwise everytime we run the bot it will buy up the tickets on the closest date and
            //the real customers wont be able to buy tickets and also the cinema will lose money
            //its always good to be carefull and responsible when using bots and not abuse it to cause harm dot com
            string href = links.LastOrDefault().GetAttributeValue("href", "");

            if (href != "")
            {
                HtmlNode aElement = links.LastOrDefault();

                var timeSpan = aElement.SelectSingleNode(".//span[@class='time']");
                var typeSpan = aElement.SelectSingleNode(".//span[@class='type']");

                if (timeSpan != null && typeSpan != null)
                {
                    string time = timeSpan.InnerText;
                    string type = typeSpan.InnerText;

                    string[] parts = href.Split('/');

                    string movie_id = parts[parts.Length - 1];

                    if (type.Contains("MLE"))
                    {
                        await SendMsg($"Attempting to Snipe MLE Tickets {time}");
                        Console.WriteLine($"Attempting to Snipe MLE Tickets {time}");

                        await Snipe(new List<string> { "B12", "B13" }, "phone_number", "email@gmail.com", "name", movie_id);
                    }
                    else
                    {
                        await SendMsg($"Attempting to Snipe HML Tickets {time}");
                        Console.WriteLine($"Attempting to Snipe HML Tickets {time}");

                        await Snipe(new List<string> { "B12", "B13" }, "phone_number", "email@gmail.com", "name", movie_id);
                    }
                }
            }

        }

        /// <summary>
        /// generate form request data 
        /// </summary>
        /// <param name="Seats"></param>
        /// <param name="Phone"></param>
        /// <param name="Email"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        static Dictionary<string, string> RequestData(List<string> Seats, string Phone, string Email, string Name)
        {
            Dictionary<string, string> formData = new Dictionary<string, string>();

            for (int i = 0; i < Seats.Count; i++)
            {
                formData.Add($"seat[{i}]", Seats[i]);
            }

            formData.Add("name", Name);
            formData.Add("email", Email);
            formData.Add("phone", Phone);
            formData.Add("book", "Book");

            return formData;
        }

        /// <summary>
        /// submit the snipe request
        /// </summary>
        static async Task Snipe(List<string> Seats, string Phone, string Email, string Name, string Movie_Id)
        {
            await SendMsg($"Attempting to Snipe {ConcatenateListElements(Seats)}");
            Console.WriteLine($"Attempting to Snipe {ConcatenateListElements(Seats)}");
            var formData = RequestData(Seats, Phone, Email, Name);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://cinema.mv/book/" + Movie_Id),
                Content = new FormUrlEncodedContent(formData),
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                if (body.Contains("There was an error trying to book your ticket!"))
                {
                    await SendMsg($"BOOK FAILED 😔😔😔");
                }
                else
                {
                    await SendMsg($"Tickets Secured 🎉🎉🎉");
                }
            }
        }

        static string ConcatenateListElements(List<string> inputList)
        {
            return string.Join(", ", inputList);
        }

        #region Notify

        /// <summary>
        /// send a telegram notification when tickets are available 
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="isError"></param>
        /// <returns></returns>
        public static async Task SendMsg(string Text)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Reporting to Telegram");
            string botToken = Program.settings.BotToken;
            string chatId = Program.settings.ChatID;

            string url = $"https://api.telegram.org/bot{botToken}/sendMessage";

            using (var form = new MultipartFormDataContent())
            {
                form.Add(new StringContent(chatId), "chat_id");
                form.Add(new StringContent(Text), "text");

                var response = await Program.client.PostAsync(url, form);

                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }

            Console.ResetColor();
        }

        #endregion
    }
}