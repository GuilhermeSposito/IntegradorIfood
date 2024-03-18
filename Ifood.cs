using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ProjetoIntegradorIfood.ClassesAuxiliares;
using static System.Net.WebRequestMethods;
using System.Timers;
using System.Net.Http.Headers;
using ProjetoIntegradorIfood.data;
using System.Reflection;
using System.Net.Mime;

namespace ProjetoIntegradorIfood;

internal class Ifood
{

    public static event EventHandler? OnPulling;
    private static System.Timers.Timer aTimer;

    public static async Task Autorizacao()
    {
        string url = "https://merchant-api.ifood.com.br/authentication/v1.0/oauth/";
        try
        {
            using (HttpClient client = new HttpClient())
            {
                FormUrlEncodedContent formData = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("clientId", UserCodes.clientId)
                    });

                HttpResponseMessage response = await client.PostAsync($"{url}userCode", formData);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("\nErro ao acessar o user code\n");
                }

                string jsonContent = await response.Content.ReadAsStringAsync();
                UserCodeReturnFromAPI codesOfVerif = JsonSerializer.Deserialize<UserCodeReturnFromAPI>(jsonContent);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nConecção feita com sucesso, peça para o Cliente colocar o código {codesOfVerif.userCode} no portal do parceito. E Depois coloque o código que o ifood retorna para ele!!\n");
                Console.ForegroundColor = ConsoleColor.White;


                Console.WriteLine("\n Insira o código gerado pelo ifood!\n");

                string? codeFromMenu = Console.ReadLine();

                FormUrlEncodedContent formDataToGetTheToken = new FormUrlEncodedContent(new[]
                     {
                        new KeyValuePair<string, string>("grantType", "authorization_code"),
                        new KeyValuePair<string, string>("clientId", UserCodes.clientId),
                        new KeyValuePair<string, string>("clientSecret", UserCodes.clientSecret),
                        new KeyValuePair<string, string>("authorizationCode", codeFromMenu),
                        new KeyValuePair<string, string>("authorizationCodeVerifier", codesOfVerif.authorizationCodeVerifier),

                    });

                HttpResponseMessage responseWithToken = await client.PostAsync($"{url}token", formDataToGetTheToken);
                if (!responseWithToken.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("\nErro ao acessar o token de acesso\n");
                }

                string jsonObjTokenFromAPI = await responseWithToken.Content.ReadAsStringAsync();
                Token propriedadesAPIWithToken = JsonSerializer.Deserialize<Token>(jsonObjTokenFromAPI);

                Token.TokenDaSessao = propriedadesAPIWithToken.accessToken;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nAcesso Aceito\n");
                Console.WriteLine($"Token:\t {Token.TokenDaSessao} \n\n");

                Console.ForegroundColor = ConsoleColor.White;
            }


        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static async Task Pulling()
    {
        string url = @"https://merchant-api.ifood.com.br/order/v1.0/events";

        try
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token.TokenDaSessao);
            HttpResponseMessage reponse = await client.GetAsync($"{url}:polling");

            int statusCode = (int)reponse.StatusCode;
            if (statusCode != 200)
            {
                throw new NullReferenceException("Nenhum novo pedido encontrado");
            }

            string jsonContent = await reponse.Content.ReadAsStringAsync();
            List<Pedido>? pedidos = JsonSerializer.Deserialize<List<Pedido>>(jsonContent);


            using (var dbContex = new ApplicationDbContext())
            {
                var pullingsNaBase = dbContex.pulling.ToList();
                foreach (var pullingAtual in pedidos)
                {
                    var confereSeJaExiste = pullingsNaBase.Any((p) => p.id.Contains(pullingAtual.id));

                    if (!confereSeJaExiste)
                    {
                        dbContex.pulling.Add(new data.Pulling() { id = pullingAtual.id });
                        dbContex.SaveChanges();
                    }

                }

                var pulingsToJson = JsonSerializer.Serialize(pullingsNaBase);
                StringContent content = new StringContent(pulingsToJson, Encoding.UTF8, "application/json");

                await client.PostAsync($"{url}/acknowledgment", content);

            }
            
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message, ex.StackTrace);
        }



    }

    public static void SetTimer()//set timer pra fazer o acionamento a cada 30 segundos do pulling
    {

        aTimer = new System.Timers.Timer(10000);
        aTimer.Elapsed += OnTimedEvent;
        aTimer.AutoReset = true;
        aTimer.Enabled = true;
    }

    private static void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        Pulling();
    }

}
