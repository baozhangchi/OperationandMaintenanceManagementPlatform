// See https://aka.ms/new-console-template for more information

using CommandLine;


Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(o =>
    {
        // TODO
        // 从SourceFile压缩包解压文件到一个缓存目录，然后复制解压结果到TargetFolder，复制完成后启动ExePath，如果有Arguments带上Arguments
    });

public class Options
{
    [Option('s', "source", Required = true)]
    public string SourceFile { get; set; }

    [Option('t', "target", Required = true)]
    public string TargetFolder { get; set; }

    [Option('e', "exe", Required = true)] public string ExePath { get; set; }

    [Option('a', "args", Required = false)]
    public string Arguments { get; set; }
}