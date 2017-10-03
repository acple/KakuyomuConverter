using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContextFreeTasks;
using static Parsec.Parser;
using static Parsec.Text;

namespace KakuyomuConverter
{
    internal class Program
    {
        private static async Task Main(string[] args)
            => await Run(args);

        private static async ContextFreeTask Run(string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentNullException("input target file.");

            var path = args[0];
            var text = await ReadFile(path);
            var converted = Convert(text);
            var newfile = Path.GetFileNameWithoutExtension(path) + "_converted" + Path.GetExtension(path);
            await WriteFile(converted, newfile);
        }

        private static async ContextFreeTask<string> ReadFile(string path)
        {
            using (var input = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(input, Encoding.UTF8))
                return await reader.ReadToEndAsync();
        }

        private static async ContextFreeTask WriteFile(string text, string path)
        {
            using (var output = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(output, Encoding.UTF8))
                await writer.WriteAsync(text);
        }

        private static string Convert(string source)
        {
            var open = Char('《').Repeat(2);
            var close = Char('》').Repeat(2);

            var emphasized =
                from _ in open
                from emphasis in ManyTill(Any(), close).ToStr()
                select $"｜{emphasis}《{new string('﹅', emphasis.Length)}》";

            var vertical = Char('|').Map(_ => '｜').ToStr();
            var parser = Many(emphasized | vertical | Any().ToStr())
                .Map(x => string.Join(string.Empty, x));

            return parser.Parse(source);
        }
    }
}
