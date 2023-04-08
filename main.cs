using ExampleBot.Core;

namespace ExampleBot
{
    internal class main
    {
        static void Main(string[] args)
        {
            new Bot().MainAsync().GetAwaiter().GetResult();
        }
    }
}