using MySql.Data.MySqlClient;

namespace VillageNewbies;

public partial class AddCustomerPage : ContentPage
{
	public AddCustomerPage()
	{
		InitializeComponent();
	}

    // asiakkaan vienti tietokantaan
    private async void LisaaAsiakas_Clicked(object sender, EventArgs e)
    {
        // jos kent�t tyhj�t ja yritet��n tallentaa
        if (string.IsNullOrWhiteSpace(etunimi.Text) ||
        string.IsNullOrWhiteSpace(sukunimi.Text) ||
        string.IsNullOrWhiteSpace(l�hiosoite.Text) ||
        string.IsNullOrWhiteSpace(postinro.Text) ||
        string.IsNullOrWhiteSpace(s�hk�posti.Text) ||
        string.IsNullOrWhiteSpace(puhelinnro.Text))
        {
            // N�yt� varoitusikkuna
            await DisplayAlert("T�ytt�m�tt�m�t tiedot", "T�yt� kaikki asiakastiedot ennen l�hett�mist�.", "OK");
            return; // Lopeta metodin suoritus t�h�n
        }

       



        var uusiAsiakas = new Asiakas
        {
            etunimi = etunimi.Text,
            sukunimi = sukunimi.Text,
            lahiosoite = l�hiosoite.Text,
            postinro = int.Parse(postinro.Text),
            email = s�hk�posti.Text,
            puhelinnro = puhelinnro.Text
        };


        var databaseAccess = new DatabaseAccess();
        await databaseAccess.LisaaAsiakasTietokantaan(uusiAsiakas);

        // lis�� t�h�n: palaa edelliselle sivulle tai anna k�ytt�j�lle palaute onnistuneesta lis�yksest�
        
        etunimi.Text = "";
        sukunimi.Text = "";
        l�hiosoite.Text = "";
        postinro.Text = "";
        toimipaikka.Text = "";
        s�hk�posti.Text = "";
        puhelinnro.Text = "";
    }
      
       

    public class DatabaseAccess
    {
        public async Task LisaaAsiakasTietokantaan(Asiakas uusiAsiakas)
        {
            string connectionString = "server=localhost;database=vn;user=;password=;";
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var query = "INSERT INTO asiakas (postinro, etunimi, sukunimi, lahiosoite, email, puhelinnro) VALUES (@Postinro, @Etunimi, @Sukunimi, @Lahiosoite, @Email, @Puhelinnro)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        
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
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}

    
