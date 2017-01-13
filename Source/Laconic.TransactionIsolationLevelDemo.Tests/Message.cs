namespace Laconic.TransactionIsolationLevelDemo.Tests
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return $"{{Id = {Id}, Text = \"{Text}\"}}";
        }
    }
}