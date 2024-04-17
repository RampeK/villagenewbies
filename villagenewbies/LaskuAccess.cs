﻿using MySql.Data.MySqlClient;
using VillageNewbies;

public class LaskuAccess
{
    private string ConnectionString;

    public LaskuAccess()
    {
        string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
        var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));
        DotNetEnv.Env.Load(projectRoot);
        var env = Environment.GetEnvironmentVariables();
        ConnectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
    }

    public async Task<List<Lasku>> FetchAllLaskutAsync()
    {
        var laskut = new List<Lasku>();

        using (var connection = new MySqlConnection(ConnectionString))
        {
            await connection.OpenAsync();

            using (var command = new MySqlCommand("SELECT * FROM lasku;", connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var lasku = new Lasku
                    {
                        LaskuId = reader.GetInt32(reader.GetOrdinal("lasku_id")),
                        VarausId = reader.GetInt32(reader.GetOrdinal("varaus_id")),
                        Summa = reader.GetDouble(reader.GetOrdinal("summa")),
                        Alv = reader.GetDouble(reader.GetOrdinal("alv")),
                        Maksettu = reader.GetBoolean(reader.GetOrdinal("maksettu"))
                    };


                    laskut.Add(lasku);
                }
            }
        }

        return laskut;
    }

    public async Task<int> TallennaLasku(Lasku uusiLasku, byte[] pdfContent)
    {
        using (var connection = new MySqlConnection(ConnectionString))
        {
            await connection.OpenAsync();

            string query = @"
            INSERT INTO lasku (varaus_id, summa, alv, maksettu, pdf_content) 
            VALUES (@VarausId, @Summa, @Alv, @Maksettu, @PdfContent);
            SELECT LAST_INSERT_ID();";

            using (var command = new MySqlCommand(query, connection))
            {
                // Asetetaan parametrit SQL-komennolle.
                command.Parameters.AddWithValue("@VarausId", uusiLasku.VarausId);
                command.Parameters.AddWithValue("@Summa", uusiLasku.Summa);
                command.Parameters.AddWithValue("@Alv", uusiLasku.Alv);
                command.Parameters.AddWithValue("@Maksettu", uusiLasku.Maksettu);
                command.Parameters.AddWithValue("@PdfContent", pdfContent);

                // Suorita komento, joka lisää laskun ja palauttaa luodun laskun ID:n.
                int laskuId = Convert.ToInt32(await command.ExecuteScalarAsync());
                return laskuId; // Palauta laskun ID.
            }
        }
    }
}
