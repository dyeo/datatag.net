namespace Datatag.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var fs = new StreamReader("test.dt"))
            {
                var result = Parser.Read(fs.ReadToEnd());
                var resultString = Parser.Write(result, EncodeSettings.Pretty);
                Console.WriteLine(resultString);

                result = Parser.Read(resultString);
                resultString = Parser.Write(result, EncodeSettings.Pretty);
                Console.WriteLine(resultString);
            }
        }
    }
}