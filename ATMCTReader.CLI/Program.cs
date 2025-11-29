using Spectre.Console.Cli;

var app = new CommandApp<ProcessCardCommand>();
await app.RunAsync(args);