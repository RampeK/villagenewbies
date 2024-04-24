using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace VillageNewbies;

public partial class AddCustomerPage : ContentPage
{
    private Asiakas _asiakas; 

    public AddCustomerPage()
    {
        InitializeComponent();
    }
    

    public AddCustomerPage(Asiakas asiakas) : this()
    {
            _asiakas = asiakas;

        if (_asiakas != null)
        {
            etunimi.Text = _asiakas.etunimi;
            sukunimi.Text = _asiakas.sukunimi;
            l�hiosoite.Text = _asiakas.lahiosoite;
            postinro.Text = _asiakas.postinro.ToString(); 
            toimipaikka.Text = _asiakas.toimipaikka;
            puhelinnro.Text = _asiakas.puhelinnro;
            s�hk�posti.Text = _asiakas.email;
            
        }

    }


    // asiakkaan vienti tietokantaan
    
    private async void LisaaAsiakas_Clicked(object sender, EventArgs e)
    {
        var puhelinnumero = puhelinnro.Text;
        string toimipaikkaValue = toimipaikka.Text;

        // Tarkistetaan, onko puhelinnumero tyhj�
        if (string.IsNullOrWhiteSpace(puhelinnumero))
        {
            await DisplayAlert("Virheellinen puhelinnumero", "Sy�t� kelvollinen puhelinnumero.", "OK");
            return;
        }

        // Tarkistetaan, sis�lt��k� puhelinnumero kirjaimia
        if (!puhelinnumero.All(char.IsDigit))
        {
            await DisplayAlert("Virheellinen puhelinnumero", "Sy�t� kelvollinen puhelinnumero.", "OK");
            return;
        }

        // Tarkistetaan puhelinnumeron muoto
        var puhelinnumeroRegex = new Regex(@"^(\+358\d{9}|0\d{9})$");
        var puhelinnumeroOK = puhelinnumeroRegex.IsMatch(puhelinnumero);

        if (!puhelinnumeroOK)
        {
            await DisplayAlert("Virheellinen puhelinnumero", "Sy�t� puhelinnumero muodossa +358451234567 tai 0451234567.", "OK");
            return;
        }

        var postinumero = postinro.Text;

        // Tarkistetaan, onko postinumero tyhj�
        if (string.IsNullOrWhiteSpace(postinumero))
        {
            await DisplayAlert("Virheellinen postinumero", "Sy�t� kelvollinen postinumero.", "OK");
            return;
        }

        // Tarkistetaan, sis�lt��k� postinumero kirjaimia
        if (!postinumero.All(char.IsDigit))
        {
            await DisplayAlert("Virheellinen postinumero", "Postinumeron tulee olla numeerinen.", "OK");
            return;
        }

        // Tarkistetaan postinumeron pituus
        if (postinumero.Length != 5)
        {
            await DisplayAlert("Virheellinen postinumero", "Postinumeron tulee olla 5 numeron pituinen.", "OK");
            return;

        }
                
        var email = s�hk�posti.Text;
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        var emailOK = emailRegex.IsMatch(email);

        if (!emailOK)
        {
            await DisplayAlert("Virheellinen s�hk�postiosoite", "Sy�t� kelvollinen s�hk�postiosoite.", "OK");
            return;
        }

        // Tarkistetaan, ettei s�hk�posti sis�ll� v�lily�ntej�
        if (email.Contains(" "))
        {
            await DisplayAlert("Virheellinen s�hk�postiosoite", "S�hk�postiosoite ei saa sis�lt�� v�lily�ntej�.", "OK");
            return;
        }

        



        if (string.IsNullOrWhiteSpace(etunimi.Text) ||
            string.IsNullOrWhiteSpace(sukunimi.Text) ||
            string.IsNullOrWhiteSpace(l�hiosoite.Text) ||
            string.IsNullOrWhiteSpace(postinro.Text) ||
            string.IsNullOrWhiteSpace(toimipaikkaValue) ||
            string.IsNullOrWhiteSpace(s�hk�posti.Text) ||
            string.IsNullOrWhiteSpace(puhelinnro.Text))
        {
            await DisplayAlert("T�ytt�m�tt�m�t tiedot", "T�yt� kaikki asiakastiedot ennen l�hett�mist�.", "OK");
            return;
        }
        
        var uusiAsiakas = new Asiakas
        {
            etunimi = etunimi.Text,
            sukunimi = sukunimi.Text,
            lahiosoite = l�hiosoite.Text,
            postinro = postinro.Text,
            email = s�hk�posti.Text,
            puhelinnro = puhelinnro.Text
        };

        var databaseAccess = new DatabaseAccess();
        // Tarkistetaan, onko asiakas jo olemassa tietokannassa
        bool asiakasOlemassa = await databaseAccess.OnkoAsiakasOlemassa(uusiAsiakas.puhelinnro);
        if (!asiakasOlemassa)
        {
            await databaseAccess.LisaaAsiakasTietokantaan(uusiAsiakas, toimipaikkaValue);
            await DisplayAlert("Asiakas lis�tty", "Uusi asiakas on onnistuneesti lis�tty.", "OK");
        }
        else
        {
            await DisplayAlert("Asiakas on jo olemassa", "Asiakkaan tiedot ovat jo tietokannassa.", "OK");
        }

        // Tyhjennet��n kent�t
        etunimi.Text = "";
        sukunimi.Text = "";
        l�hiosoite.Text = "";
        postinro.Text = "";
        toimipaikka.Text = "";
        s�hk�posti.Text = "";
        puhelinnro.Text = "";

        await Navigation.PopAsync();
    }
    
     

    public class DatabaseAccess
    {
        
        public async Task LisaaTaiPaivitaAsiakas(Asiakas uusiAsiakas, string toimipaikka)
        {
            if (await OnkoAsiakasOlemassa(uusiAsiakas.puhelinnro))
            {
                // Asiakas on jo olemassa, joten p�ivitet��n tiedot
                await TallennaAsiakasTietokantaan(uusiAsiakas);
            }
            else
            {
                // Asiakasta ei ole olemassa, joten lis�t��n uusi
                await LisaaAsiakasTietokantaan(uusiAsiakas, toimipaikka);
            }
        }
        
        public async Task LisaaAsiakasTietokantaan(Asiakas uusiAsiakas, string toimipaikka)
        {


            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    
                    if (!await OnkoPostinumeroOlemassa(uusiAsiakas.postinro))
                    {
                        await LisaaPostinumero(uusiAsiakas.postinro, toimipaikka); 
                    }
                    
                    var query = "INSERT INTO asiakas (postinro, etunimi, sukunimi, lahiosoite, email, puhelinnro) VALUES (@Postinro, @Etunimi, @Sukunimi, @Lahiosoite, @Email, @Puhelinnro)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        Debug.WriteLine(uusiAsiakas.postinro);
                        command.Parameters.AddWithValue("@Postinro", uusiAsiakas.postinro);
                        command.Parameters.AddWithValue("@Etunimi", uusiAsiakas.etunimi);
                        command.Parameters.AddWithValue("@Sukunimi", uusiAsiakas.sukunimi);
                        command.Parameters.AddWithValue("@Lahiosoite", uusiAsiakas.lahiosoite);
                        command.Parameters.AddWithValue("@Email", uusiAsiakas.email);
                        command.Parameters.AddWithValue("@Puhelinnro", uusiAsiakas.puhelinnro);

                        await command.ExecuteNonQueryAsync();
                    }


                }
                catch (Exception ex)
                {
                    // K�sittely mahdollisille poikkeuksille
                    Debug.WriteLine(ex.Message);
                }
            }
        }

  

        public async Task<bool> OnkoAsiakasOlemassa(string puhelinnro) // tarkistetaan, onk asiakas jo tietokannassa
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\.."));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var tarkistusQuery = "SELECT COUNT(*) FROM asiakas WHERE puhelinnro = @Puhelinnro";

                using (var command = new MySqlCommand(tarkistusQuery, connection))
                {
                    command.Parameters.AddWithValue("@Puhelinnro", puhelinnro);
                    var tulos = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return tulos > 0;
                }
            }
        }

        public async Task TallennaAsiakasTietokantaan(Asiakas asiakas)
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
            using (var connection = new MySqlConnection(connectionString))

            {
                try
                {
                    await connection.OpenAsync();
                    var query = @"UPDATE asiakas 
                          SET etunimi = @etunimi, 
                              sukunimi = @sukunimi, 
                              lahiosoite = @lahiosoite, 
                              postinro = @postinro, 
                              email = @email, 
                              puhelinnro = @puhelinnro 
                          WHERE asiakas_id = @asiakas_id;";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@etunimi", asiakas.etunimi);
                        command.Parameters.AddWithValue("@sukunimi", asiakas.sukunimi);
                        command.Parameters.AddWithValue("@lahiosoite", asiakas.lahiosoite);
                        command.Parameters.AddWithValue("@postinro", asiakas.postinro);
                        command.Parameters.AddWithValue("@email", asiakas.email);
                        command.Parameters.AddWithValue("@puhelinnro", asiakas.puhelinnro);
                        command.Parameters.AddWithValue("@asiakas_id", asiakas.asiakas_id);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    // K�sittely mahdollisille poikkeuksille
                    Console.WriteLine(ex.Message);
                }
            }

        }


        public async Task<string> HaeToimipaikkaPostinronPerusteella(string postinro)
        {
            string toimipaikka = "";
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();
            string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    var query = "SELECT toimipaikka FROM posti WHERE postinro = @postinro;";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@postinro", postinro);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                toimipaikka = reader.GetString("toimipaikka");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // K�sittely mahdollisille poikkeuksille
                    Console.WriteLine(ex.Message);
                }
            }
            return toimipaikka;
        }


        public async Task<bool> OnkoPostinumeroOlemassa(string postinro)
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT COUNT(*) FROM posti WHERE postinro = @Postinro";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Postinro", postinro);
                    
                    var tulos = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return tulos > 0;
                }
            }
        }

        public async Task LisaaPostinumero(string postinro, string toimipaikka)
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = "INSERT INTO posti (postinro, toimipaikka) VALUES (@Postinro, @Toimipaikka)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Postinro", postinro);
                    command.Parameters.AddWithValue("@Toimipaikka", toimipaikka);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }   
}


