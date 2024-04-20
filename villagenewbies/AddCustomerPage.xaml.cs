using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace VillageNewbies;

public partial class AddCustomerPage : ContentPage
{
    private Asiakas _asiakas; // J�senmuuttuja lis�t��n t�ss�, t�m� lis�tty koodi

    public AddCustomerPage()
    {
        InitializeComponent();
    }
        //lis�tty koodi

    public AddCustomerPage(Asiakas asiakas) : this()
    {
            _asiakas = asiakas;
        // T�yt� kent�t asiakkaan tiedoilla

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
        var puhelinnumeroRegex = new Regex(@"^(\+358\d{9}|0\d{9})$");
        var puhelinnumeroOK = puhelinnumeroRegex.IsMatch(puhelinnumero);

        string toimipaikkaValue = toimipaikka.Text;

        if (!puhelinnumeroOK)
        {
            await DisplayAlert("Virheellinen puhelinnumero", "Sy�t� puhelinnumero muodossa +358451234567 tai 0451234567.", "OK");
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
            toimipaikka = toimipaikka.Text,
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
    }
    
    private async void TallennaAsiakkaanTietoja_Clicked(object sender, EventArgs e)
    {
        // Tarkistetaan, onko sy�tetty puhelinnumero oikeassa muodossa
        var puhelinnumero = puhelinnro.Text;
        var puhelinnumeroRegex = new Regex(@"^(\+358\d{9}|0\d{9})$");
        var puhelinnumeroOK = puhelinnumeroRegex.IsMatch(puhelinnumero);
        if (!puhelinnumeroOK)
        {
            await DisplayAlert("Virheellinen puhelinnumero", "Sy�t� puhelinnumero muodossa +358451234567 tai 0451234567.", "OK");
            return;
        }
        
        // Tarkistetaan, ettei mik��n kentt� ole tyhj�
        if (string.IsNullOrWhiteSpace(etunimi.Text) ||
            string.IsNullOrWhiteSpace(sukunimi.Text) ||
            string.IsNullOrWhiteSpace(l�hiosoite.Text) ||
            string.IsNullOrWhiteSpace(postinro.Text) ||
            string.IsNullOrWhiteSpace (toimipaikka.Text) ||
            string.IsNullOrWhiteSpace(s�hk�posti.Text) ||
            string.IsNullOrWhiteSpace(puhelinnro.Text))
        {
            await DisplayAlert("T�ytt�m�tt�m�t tiedot", "T�yt� kaikki asiakastiedot ennen l�hett�mist�.", "OK");
            return;
        }
        
        // P�ivitet��n _asiakas-olion tiedot
        if (_asiakas == null)
        {
            _asiakas = new Asiakas();
        }

        _asiakas.etunimi = etunimi.Text;
        _asiakas.sukunimi = sukunimi.Text;
        _asiakas.lahiosoite = l�hiosoite.Text;
        _asiakas.postinro = postinro.Text;
        _asiakas.toimipaikka = toimipaikka.Text;
        _asiakas.email = s�hk�posti.Text;
        _asiakas.puhelinnro = puhelinnro.Text;

        var databaseAccess = new DatabaseAccess();
        
        // Tarkistetaan, onko asiakas jo olemassa tietokannassa
        bool asiakasOlemassa = await databaseAccess.OnkoAsiakasOlemassa(_asiakas.puhelinnro);
        if (!asiakasOlemassa || _asiakas.asiakas_id == 0)
        {
            await databaseAccess.LisaaAsiakasTietokantaan(_asiakas, toimipaikka.Text);
            await DisplayAlert("Asiakas lis�tty", "Uusi asiakas on onnistuneesti lis�tty.", "OK");
        }
        else
        {
            await databaseAccess.TallennaAsiakasTietokantaan(_asiakas);
            await DisplayAlert("Tiedot p�ivitetty", "Asiakkaan tiedot on p�ivitetty onnistuneesti.", "OK");
        }
        
        // Tyhjennet��n kent�t
        etunimi.Text = "";
        sukunimi.Text = "";
        l�hiosoite.Text = "";
        postinro.Text = "";
        toimipaikka.Text = "";
        s�hk�posti.Text = "";
        puhelinnro.Text = "";

        await Navigation.PopAsync(); // paluu edelliselle sivulle tallennuksen j�lkeen
    }
   

    // Asiakkaan poistaminen tietokannasta
    private async void PoistaAsiakasTietokannasta_Clicked(object sender, EventArgs e)
    {
        var vastaus = await DisplayAlert("Vahvista poisto", "Haluatko varmasti poistaa t�m�n asiakkaan?", "Kyll�", "Ei");
        if (vastaus)
        {
            var databaseAccess = new DatabaseAccess();
            await databaseAccess.PoistaAsiakasTietokannasta(_asiakas.asiakas_id);
            await DisplayAlert("Poistettu", "Asiakas on poistettu onnistuneesti.", "OK");
            // Palaa tarvittaessa edelliselle sivulle
            await Navigation.PopAsync();
        }
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
                        await LisaaPostinumero(uusiAsiakas.postinro, toimipaikka); // Olettaen, ett� uusiAsiakas sis�lt�� paikkakunnan
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


        // Asiakkaan tietojen muokkaus

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

        //asiakkaan poisto tietokannasta
        public async Task PoistaAsiakasTietokannasta(int asiakasId)
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

                    var query = "DELETE FROM asiakas WHERE asiakas_id = @AsiakasId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AsiakasId", asiakasId);
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


