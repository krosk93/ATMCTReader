using System.ComponentModel;
using System.Text;
using ATMCTReader.Models;
using ATMCTReader.Parser;
using Spectre.Console;
using Spectre.Console.Cli;

internal sealed class ProcessCardCommand : AsyncCommand<ProcessCardCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("Path of the binary file containing a bin dump of a ATMCT card")]
        [CommandArgument(0, "<path>")]
        public required string Path { get; init; }

        [CommandOption("-p|--pretty")]
        [DefaultValue(false)]
        public bool Pretty { get; init; }
    }

    static ArraySegment<byte> GetLine(byte[] card, int address)
    {
        return new ArraySegment<byte>(card, address, 0x10);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        using BinaryReader reader = new(File.Open(settings.Path, FileMode.Open));
        byte[] card = reader.ReadBytes(1024);
        Card c = CardParser.ParseCard(card);
        
        if (settings.Pretty)
        {
            AnsiConsole.MarkupLine($" [bold][white on orangered1]  ATM [/] {c.Authority.Name}[/]");
            AnsiConsole.MarkupLine(" [grey85 on grey85]      [/] Autoritat Territorial de la Mobilitat");
            AnsiConsole.MarkupLine(" [grey85 on grey85]      [/] [bold]Pwned![/]");
            AnsiConsole.WriteLine();
        }
        AnsiConsole.MarkupLine($"[bold]Targeta:[/] {c.CardId}");
        AnsiConsole.WriteLine($"         {c.AccountId}");

        
        if(!settings.Pretty) {
            int curAddress = 0;
            for (int i = 0; i < 48; i++)
            {
                if (((curAddress + 0x10) % 0x40) == 0) curAddress += 0x10;
                StringBuilder sb = new(Convert.ToString(curAddress, 16).PadLeft(4, '0') + ": ");
                StringBuilder hex = new("      ");
                bool[] found = new bool[16];
                var byteSegment = new ArraySegment<byte>(card, curAddress, 0x10);
                var bytes = byteSegment.Reverse().ToArray();
                curAddress += 0x10;
                for(int j = 0; j < 16; j++)
                {
                    hex.Append(Convert.ToString(bytes[j], 16).PadLeft(2, '0')).Append("      ");
                    sb.Append(Convert.ToString(bytes[j], 2).PadLeft(8, '0'));
                }
                sb.Replace("10000110", "[red]10000110[/]");
                AnsiConsole.MarkupLine(hex.ToString());
                AnsiConsole.MarkupLine(sb.ToString());
            }
        }
        if(!string.IsNullOrWhiteSpace(c.Owner))
            AnsiConsole.MarkupLine($"[bold]Propietari:[/] {c.Owner}");
        else
            AnsiConsole.MarkupLine("[bold]Propietari:[/] Targeta anònima");

        AnsiConsole.MarkupLine($"[bold]Caducitat:[/] {c.ExpireDate:dd/MM/yyyy}");
        
        AnsiConsole.MarkupLine($"[bold]Perfil:[/] {c.Profile.Name} [bold]EA:[/] {c.EA} [bold]M:[/] {c.M.ToString().PadLeft(4, '0')}");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine($"[bold]Darrera validació:[/] Feta el {c.LastValidation.Instant:dd/MM/yyyy} a les {c.LastValidation.Instant:HH:mm} amb l'operador {c.LastValidation.Company.Name} des de {c.LastValidation.Stop.Name} ({c.LastValidation.Line.Name}) Zona: {c.LastValidation.Zone.Name}");
        if(!settings.Pretty)
        {
            var bytes = GetLine(card, 0x100).Reverse().ToArray();
            AnsiConsole.WriteLine(string.Join("      ", bytes.Select(x => Convert.ToString(x, 16).PadLeft(2, '0'))));
            AnsiConsole.Markup($"[red]{Convert.ToString(bytes[0], 2).PadLeft(8, '0')}[/]");
            AnsiConsole.Markup($"[blue]{Convert.ToString(bytes[1] >> 2, 2).PadLeft(6, '0')}[/]");
            AnsiConsole.Markup($"[yellow]{Convert.ToString(bytes[1] & 0x3).PadLeft(2, '0')}[/]");
            AnsiConsole.Markup($"[yellow]{Convert.ToString(bytes[2] >> 5, 2).PadLeft(3, '0')}[/]");
            AnsiConsole.Markup($"[green]{Convert.ToString(bytes[2] & 31, 2).PadLeft(5, '0')}[/]");
            AnsiConsole.Markup($"[aqua]{Convert.ToString(bytes[3] >> 4, 2).PadLeft(4, '0')}[/]");
            AnsiConsole.Markup($"[magenta3]{Convert.ToString(bytes[3] & 15, 2).PadLeft(4, '0')}[/]");
            AnsiConsole.Markup($"[springgreen2]{Convert.ToString(bytes[4], 2).PadLeft(8, '0')}[/]");
            AnsiConsole.Markup($"[fuchsia]{Convert.ToString(bytes[5], 2).PadLeft(8, '0')}[/]");
            AnsiConsole.Markup($"[fuchsia]{Convert.ToString(bytes[6] >> 1, 2).PadLeft(7, '0')}[/]");
            AnsiConsole.Markup($"[indianred]{Convert.ToString(bytes[6] & 1, 2)}[/]");
            AnsiConsole.Markup($"[indianred]{Convert.ToString(bytes[7] >> 1, 2).PadLeft(7, '0')}[/]");
            AnsiConsole.Markup($"[aquamarine1]{Convert.ToString(bytes[7] & 1, 2)}[/]");
            AnsiConsole.Markup($"[aquamarine1]{Convert.ToString(bytes[8], 2).PadLeft(8, '0')}[/]");
            AnsiConsole.Markup($"[aquamarine1]{Convert.ToString(bytes[9] >> 6, 2).PadLeft(2, '0')}[/]");
            AnsiConsole.Write(Convert.ToString(bytes[9] & 63, 2).PadLeft(6, '0'));
            AnsiConsole.Write(Convert.ToString(bytes[10], 2).PadLeft(8, '0'));
            AnsiConsole.Write(Convert.ToString(bytes[11], 2).PadLeft(8, '0'));
            AnsiConsole.Write(Convert.ToString(bytes[12], 2).PadLeft(8, '0'));
            AnsiConsole.Write(Convert.ToString(bytes[13], 2).PadLeft(8, '0'));
            AnsiConsole.Write(Convert.ToString(bytes[14], 2).PadLeft(8, '0'));
            AnsiConsole.Write(Convert.ToString(bytes[15], 2).PadLeft(8, '0'));
            AnsiConsole.WriteLine();

            AnsiConsole.Markup("[red]checksum[/]");
            AnsiConsole.Markup("[blue]mmmmmm[/]");
            AnsiConsole.Markup("[yellow]hhhhh[/]");
            AnsiConsole.Markup("[green]DDDDD[/]");
            AnsiConsole.Markup("[aqua]MMMM[/]");
            AnsiConsole.Markup("[magenta3]AAAA[/]");
            AnsiConsole.Markup("[springgreen2]zzzzzzzz[/]");
            AnsiConsole.Markup("[fuchsia]ppppppppppppppp[/]");
            AnsiConsole.Markup("[indianred]oooooooo[/]");
            AnsiConsole.Markup("[aquamarine1]lllllllllll[/]");
            AnsiConsole.Markup(" ");

            AnsiConsole.WriteLine();
        }


        AnsiConsole.WriteLine();
            
        if(!settings.Pretty) {
            AnsiConsole.MarkupLine($"[bold]Títol actual:[/] {c.CurrentTicket.Type.Name} ({c.CurrentTicket.NumberOfZones} zones)");
            
            var bytes = GetLine(card, 0x140).Reverse().ToArray();
            AnsiConsole.MarkupLine(string.Join("      ", bytes.Select(x => Convert.ToString(x, 16).PadLeft(2, '0'))));
            AnsiConsole.Markup($"[red]{Convert.ToString(bytes[0], 2).PadLeft(8, '0')}[/]");
            AnsiConsole.Markup($"{Convert.ToString(bytes[1], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"{Convert.ToString(bytes[2], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"{Convert.ToString(bytes[3], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"{Convert.ToString(bytes[4], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"[turquoise2]{Convert.ToString(bytes[5] >> 2, 2).PadLeft(6, '0')}[/]");
            AnsiConsole.Markup($"{Convert.ToString(bytes[5] & 3).PadLeft(2, '0')}");
            AnsiConsole.Markup($"{Convert.ToString(bytes[6], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"{Convert.ToString(bytes[7], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"{Convert.ToString(bytes[8], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"[lightsalmon3]{Convert.ToString(bytes[9], 2).PadLeft(8, '0')}[/]");
            AnsiConsole.Markup($"[lightsalmon3]{Convert.ToString(bytes[10], 2).PadLeft(8, '0')}[/]");
            AnsiConsole.Markup($"{Convert.ToString(bytes[11], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"{Convert.ToString(bytes[12], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"[chartreuse1]{Convert.ToString(bytes[13] >> 2, 2).PadLeft(6, '0')}[/]");
            AnsiConsole.Markup($"[cornflowerblue]{Convert.ToString(bytes[13] & 3, 2).PadLeft(2, '0')}[/]");
            AnsiConsole.Markup($"[cornflowerblue]{Convert.ToString(bytes[14], 2).PadLeft(8, '0')}[/]");
            AnsiConsole.Markup($"{Convert.ToString(bytes[15], 2).PadLeft(8, '0')}");
            AnsiConsole.WriteLine();
            AnsiConsole.Markup("[red]checksum[/]");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("[turquoise2]nnnnnn[/]  ");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("[lightsalmon3]iiiiiiiiiiiiiiii[/]");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("[chartreuse1]vvvvvv[/]");
            AnsiConsole.Markup("[cornflowerblue]tttttttttt[/]");
            AnsiConsole.Markup("        ");
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine($"[bold]Estat Targeta:[/]");
            AnsiConsole.MarkupLine($"[bold]Viatges restants:[/] {c.CurrentTicket.TripsLeft}");
            AnsiConsole.MarkupLine($"[bold]Caducitat títol:[/] {(c.CurrentTicket.ExpireDate.HasValue ? c.CurrentTicket.ExpireDate.Value.ToString("dd/MM/yyyy") : "Sense caducitat")}");
            AnsiConsole.MarkupLine($"[bold]Zona primera validació:[/] {c.CurrentTicket.FirstZone.Name}");
            
            bytes = GetLine(card, 0x150).Reverse().ToArray();
            AnsiConsole.MarkupLine(string.Join("      ", bytes.Select(x => Convert.ToString(x, 16).PadLeft(2, '0'))));
            AnsiConsole.Markup($"[red]{Convert.ToString(bytes[0], 2).PadLeft(8, '0')}[/]");
            AnsiConsole.Markup($"{Convert.ToString(bytes[1], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"{Convert.ToString(bytes[2] >> 4, 2).PadLeft(4, '0')}");
            AnsiConsole.Markup($"[springgreen2]{Convert.ToString(bytes[2] & 15, 2).PadLeft(4, '0')}[/]");
            AnsiConsole.Markup($"[springgreen2]{Convert.ToString(bytes[3] >> 4, 2).PadLeft(4, '0')}[/]");
            AnsiConsole.Markup($"{Convert.ToString(bytes[3] & 15, 2).PadLeft(4, '0')}");
            AnsiConsole.Markup($"{Convert.ToString(bytes[4], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"{Convert.ToString(bytes[5], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"{Convert.ToString(bytes[6], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"[chartreuse1]{Convert.ToString(bytes[7] >> 1, 2).PadLeft(7, '0')}[/]");
            AnsiConsole.Markup($"{Convert.ToString(bytes[7] & 1, 2)}");
            AnsiConsole.Markup($"{Convert.ToString(bytes[8], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"{Convert.ToString(bytes[9], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"{Convert.ToString(bytes[10], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"{Convert.ToString(bytes[11], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"{Convert.ToString(bytes[12] >> 1, 2).PadLeft(7, '0')}");
            AnsiConsole.Markup($"[green]{Convert.ToString(bytes[12] & 1, 2)}[/]");
            AnsiConsole.Markup($"[green]{Convert.ToString(bytes[13] >> 4, 2).PadLeft(4, '0')}[/]");
            AnsiConsole.Markup($"[aqua]{Convert.ToString(bytes[13] & 15, 2).PadLeft(4, '0')}[/]");
            AnsiConsole.Markup($"{Convert.ToString(bytes[14], 2).PadLeft(8, '0')}");
            AnsiConsole.Markup($"{Convert.ToString(bytes[15], 2).PadLeft(8, '0')}");
            AnsiConsole.WriteLine();
            AnsiConsole.Markup("[red]checksum[/]");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("    ");
            AnsiConsole.Markup("[springgreen2]zzzzzzzz[/]");
            AnsiConsole.Markup("    ");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("[chartreuse1]vvvvvvv[/]");
            AnsiConsole.Markup(" ");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("       ");
            AnsiConsole.Markup("[green]DDDDD[/]");
            AnsiConsole.Markup("[aqua]MMMM[/]");
            AnsiConsole.Markup("        ");
            AnsiConsole.Markup("        ");
            AnsiConsole.WriteLine();
        } 
        else
        {
            var statusTable = new Table();
            statusTable.Title = new TableTitle("[bold]Estat de la targeta[/]");
            statusTable.AddColumn("[bold]Títol actual[/]");
            statusTable.AddColumn("[bold]Zones[/]");
            statusTable.AddColumn("[bold]Viatges restants[/]");
            statusTable.AddColumn("[bold]Caducitat títol[/]");
            statusTable.AddColumn("[bold]Zona primera validació[/]");
            statusTable.AddRow(
                c.CurrentTicket.Type.Name, 
                c.CurrentTicket.NumberOfZones.ToString(), 
                c.CurrentTicket.TripsLeft.ToString(), 
                c.CurrentTicket.ExpireDate.HasValue 
                ? (
                    c.CurrentTicket.ExpireDate.Value > DateTime.Now 
                    ? c.CurrentTicket.ExpireDate.Value.ToString("dd/MM/yyyy")
                    : $"[bold red]!! {c.CurrentTicket.ExpireDate.Value:dd/MM/yyyy} !![/]"
                ) : "Sense caducitat", c.CurrentTicket.FirstZone.Name);
            AnsiConsole.Write(statusTable);

        }
        AnsiConsole.WriteLine();

        Table rechargeTable = new Table();
        if (settings.Pretty)
        {
            rechargeTable.AddColumn("[bold]Data[/]");
            rechargeTable.AddColumn("[bold]Títol[/]");
            rechargeTable.Title = new TableTitle("[bold]Recàrregues[/]");
        }
        if (settings.Pretty)
            foreach (var topUp in c.TopUps.OrderByDescending(t => t.Date)) {
                rechargeTable.AddRow(topUp.Date.ToString("dd/MM/yyyy"), topUp.Type.Name);
            }
        else
        {
            foreach (var (topUp, i) in c.TopUps.Select((el, i) => (el, i))) {
                AnsiConsole.WriteLine($"Recàrrega {i + 1} de {topUp.Type.Name} feta el {topUp.Date:dd/MM/yyyy}");
                var bytes = GetLine(card, 0x240 + 0x10 * i).Reverse().ToArray();
                AnsiConsole.WriteLine(string.Join("      ", bytes.Select(x => Convert.ToString(x, 16).PadLeft(2, '0'))));
                AnsiConsole.Markup($"[red]{Convert.ToString(bytes[0], 2).PadLeft(8, '0')}[/]");
                AnsiConsole.Markup($"{Convert.ToString(bytes[1] >> 1, 2).PadLeft(7, '0')}");
                AnsiConsole.Markup($"[green]{Convert.ToString(bytes[1] & 1, 2)}[/]");
                AnsiConsole.Markup($"[green]{Convert.ToString(bytes[2] >> 4, 2).PadLeft(4, '0')}[/]");
                AnsiConsole.Markup($"[aqua]{Convert.ToString(bytes[2] & 15, 2).PadLeft(4, '0')}[/]");
                AnsiConsole.Markup($"[magenta3]{Convert.ToString(bytes[3] >> 4, 2).PadLeft(4, '0')}[/]");
                AnsiConsole.Markup($"{Convert.ToString(bytes[3] & 15, 2).PadLeft(4, '0')}");
                AnsiConsole.Markup($"{Convert.ToString(bytes[4], 2).PadLeft(8, '0')}");
                AnsiConsole.Markup($"{Convert.ToString(bytes[5], 2).PadLeft(8, '0')}");
                AnsiConsole.Markup($"{Convert.ToString(bytes[6] >> 2, 2).PadLeft(6, '0')}");
                AnsiConsole.Markup($"[cornflowerblue]{Convert.ToString(bytes[6] & 3, 2).PadLeft(2, '0')}[/]");
                AnsiConsole.Markup($"[cornflowerblue]{Convert.ToString(bytes[7], 2).PadLeft(8, '0')}[/]");
                AnsiConsole.Markup($"{Convert.ToString(bytes[8], 2).PadLeft(8, '0')}");
                AnsiConsole.Markup($"{Convert.ToString(bytes[9], 2).PadLeft(8, '0')}");
                AnsiConsole.Markup($"{Convert.ToString(bytes[10], 2).PadLeft(8, '0')}");
                AnsiConsole.Markup($"{Convert.ToString(bytes[11], 2).PadLeft(8, '0')}");
                AnsiConsole.Markup($"{Convert.ToString(bytes[12], 2).PadLeft(8, '0')}");
                AnsiConsole.Markup($"{Convert.ToString(bytes[13], 2).PadLeft(8, '0')}");
                AnsiConsole.Markup($"{Convert.ToString(bytes[14], 2).PadLeft(8, '0')}");
                AnsiConsole.Markup($"{Convert.ToString(bytes[15], 2).PadLeft(8, '0')}");
                AnsiConsole.WriteLine();
                AnsiConsole.Markup("[red]checksum[/]");
                AnsiConsole.Markup("       ");
                AnsiConsole.Markup("[green]DDDDD[/]");
                AnsiConsole.Markup("[aqua]MMMM[/]");
                AnsiConsole.Markup("[magenta3]AAAA[/]");
                AnsiConsole.Markup("    ");
                AnsiConsole.Markup("        ");
                AnsiConsole.Markup("        ");
                AnsiConsole.Markup("      ");
                AnsiConsole.Markup("[cornflowerblue]tttttttttt[/]");
                AnsiConsole.WriteLine();
            }
        }

        if (settings.Pretty) AnsiConsole.Write(rechargeTable);
        else AnsiConsole.WriteLine();

        Table tripsTable = new Table();
        if(settings.Pretty)
        {
            tripsTable
                .AddColumn("[bold]Operador[/]")
                .AddColumn("[bold]Data[/]")
                .AddColumn("[bold]Origen[/]")
                .AddColumn("[bold]Linia[/]")
                .AddColumn("[bold]Vehicle[/]")
                .AddColumn("[bold]Zona[/]")
                .AddColumn("[bold]Passatgers[/]")
                .AddColumn("[bold]És transbord?[/]")
                .Title = new TableTitle("[bold]Darreres 10 validacions[/]");
        }
        if(settings.Pretty) {
            foreach (var validation in c.Validations.OrderByDescending(v => v.Instant))
                tripsTable.AddRow(validation.Company.Name, validation.Instant.ToString("dd/MM/yyyy HH:mm"), validation.Stop.Name, validation.Line.Name, validation.Vehicle.ToString(), validation.Zone.Name, validation.Passengers.ToString(), validation.IsTransfer ? "Sí" : "No");
        } else {
            foreach (var (validation, i) in c.Validations.Select((v, i) => (v, i)))
            {

                AnsiConsole.WriteLine($"Viatge amb {validation.Company.Name} el {validation.Instant:dd/MM/yyyy} a les {validation.Instant:HH:mm} amb el vehicle {validation.Vehicle} des de {validation.Stop.Name} ({validation.Line.Name}) Zona: {validation.Zone.Name}{(validation.Passengers > 1 ? $" - {validation.Passengers} passatgers" : "")}");
                
                var address = 0x2c0 + (i / 3 * 0x40) + (i % 3 * 0x10);
                var bytes = GetLine(card, address).Reverse().ToArray();
                AnsiConsole.WriteLine(string.Join("      ", bytes.Select(x => Convert.ToString(x, 16).PadLeft(2, '0'))));
                AnsiConsole.Markup($"[red]{Convert.ToString(bytes[0], 2).PadLeft(8, '0')}[/]");
                AnsiConsole.Markup($"[blue]{Convert.ToString(bytes[1] >> 2, 2).PadLeft(6, '0')}[/]");
                AnsiConsole.Markup($"[yellow]{Convert.ToString(bytes[1] & 3, 2).PadLeft(2, '0')}[/]");
                AnsiConsole.Markup($"[yellow]{Convert.ToString(bytes[2] >> 5, 2).PadLeft(3, '0')}[/]");
                AnsiConsole.Markup($"[green]{Convert.ToString(bytes[2] & 31, 2).PadLeft(5, '0')}[/]");
                AnsiConsole.Markup($"[aqua]{Convert.ToString(bytes[3] >> 4, 2).PadLeft(4, '0')}[/]");
                AnsiConsole.Markup($"[magenta3]{Convert.ToString(bytes[3] & 15, 2).PadLeft(4, '0')}[/]");
                AnsiConsole.Markup($"[maroon]{Convert.ToString(bytes[4], 2).PadLeft(8, '0')}[/]");
                AnsiConsole.Markup($"[maroon]{Convert.ToString(bytes[5] >> 6, 2).PadLeft(2, '0')}[/]");
                AnsiConsole.Markup($"[springgreen2]{Convert.ToString(bytes[5] & 63, 2).PadLeft(6, '0')}[/]");
                AnsiConsole.Markup($"[springgreen2]{Convert.ToString(bytes[6] >> 6, 2).PadLeft(2, '0')}[/]");
                AnsiConsole.Markup($"[fuchsia]{Convert.ToString(bytes[6] & 63, 2).PadLeft(6, '0')}[/]");
                AnsiConsole.Markup($"[fuchsia]{Convert.ToString(bytes[7], 2).PadLeft(8, '0')}[/]");
                AnsiConsole.Markup($"[fuchsia]{Convert.ToString(bytes[8] >> 7, 2)}[/]");
                AnsiConsole.Markup($"[indianred]{Convert.ToString(bytes[8] & 127, 2).PadLeft(7, '0')}[/]");
                AnsiConsole.Markup($"[indianred]{Convert.ToString(bytes[9] >> 7, 2)}[/]");
                AnsiConsole.Markup($"[aquamarine1]{Convert.ToString(bytes[9] & 127, 2).PadLeft(7, '0')}[/]");
                AnsiConsole.Markup($"[aquamarine1]{Convert.ToString(bytes[10] >> 4, 2).PadLeft(4, '0')}[/]");
                AnsiConsole.Write(Convert.ToString((bytes[10] >> 2) & 3, 2).PadLeft(2, '0'));
                AnsiConsole.Markup($"[lime]{Convert.ToString((bytes[10] & 2) >> 1, 2)}[/]");
                AnsiConsole.Write(Convert.ToString(bytes[10] & 1, 2));
                AnsiConsole.Write(Convert.ToString(bytes[11], 2).PadLeft(8, '0'));
                AnsiConsole.Write(Convert.ToString(bytes[12], 2).PadLeft(8, '0'));
                AnsiConsole.Write(Convert.ToString(bytes[13], 2).PadLeft(8, '0'));
                AnsiConsole.Markup($"[mediumpurple3]{Convert.ToString(bytes[14] >> 5, 2).PadLeft(3, '0')}[/]");
                AnsiConsole.Markup($"{Convert.ToString(bytes[14] & 31, 2).PadLeft(5, '0')}");
                AnsiConsole.Write(Convert.ToString(bytes[15] >> 1, 2).PadLeft(7, '0'));
                AnsiConsole.Markup($"[lime]{Convert.ToString(bytes[15] & 1, 2)}[/]");
                AnsiConsole.WriteLine();

                AnsiConsole.Markup("[red]checksum[/]");
                AnsiConsole.Markup("[blue]mmmmmm[/]");
                AnsiConsole.Markup("[yellow]hhhhh[/]");
                AnsiConsole.Markup("[green]DDDDD[/]");
                AnsiConsole.Markup("[aqua]MMMM[/]");
                AnsiConsole.Markup("[magenta3]AAAA[/]");
                AnsiConsole.Markup("[maroon]bbbbbbbbbb[/]");
                AnsiConsole.Markup("[springgreen2]zzzzzzzz[/]");
                AnsiConsole.Markup("[fuchsia]ppppppppppppppp[/]");
                AnsiConsole.Markup("[indianred]oooooooo[/]");
                AnsiConsole.Markup("[aquamarine1]lllllllllll[/]");
                AnsiConsole.Markup("  ");
                AnsiConsole.Markup("[lime]t[/]");
                AnsiConsole.Markup(" ");
                AnsiConsole.Markup("        ");
                AnsiConsole.Markup("        ");
                AnsiConsole.Markup("        ");
                AnsiConsole.Markup("[mediumpurple3]nnn[/]");
                AnsiConsole.Markup("     ");
                AnsiConsole.Markup("       ");
                AnsiConsole.Markup("[lime]t[/]");
                AnsiConsole.WriteLine();
            }
        }
        
        if(settings.Pretty) AnsiConsole.Write(tripsTable);
        return 0;
    }
}
