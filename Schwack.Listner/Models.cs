namespace Schwack.Listner
{
    public class Settings
    {
        public string BotToken { get; set; }
        public string ChatID { get; set; }
        public string MovieName { get; set; }
    }


    public class Movie
    {
        public string Title { get; set; }
        public string Time { get; set; }
        public string Location { get; set; }
        public string Url { get; set; }
    }
}
