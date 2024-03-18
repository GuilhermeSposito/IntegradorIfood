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
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJraWQiOiJlZGI4NWY2Mi00ZWY5LTExZTktODY0Ny1kNjYzYmQ4NzNkOTMiLCJ0eXAiOiJKV1QiLCJhbGciOiJSUzUxMiJ9.eyJzdWIiOiJiZDg2MmYwNy0zYTgxLTRkZTYtYWM5Ni05NzJiNjZhNDljZTciLCJvd25lcl9uYW1lIjoiZ3VpbGhlcm1ldGVzdGVzIiwiaXNzIjoiaUZvb2QiLCJjbGllbnRfaWQiOiJjYzQ0Y2Q2MS1jYmI3LTQ0MjQtOTE5Yi1hM2RmNDI4N2FlYzEiLCJhcHBfbmFtZSI6Imd1aWxoZXJtZXRlc3Rlcy10ZXN0ZS1kIiwiYXVkIjpbInNoaXBwaW5nIiwiY2F0YWxvZyIsInJldmlldyIsImZpbmFuY2lhbCIsIm1lcmNoYW50IiwibG9naXN0aWNzIiwiZ3JvY2VyaWVzIiwiZXZlbnRzIiwib3JkZXIiLCJvYXV0aC1zZXJ2ZXIiXSwic2NvcGUiOlsic2hpcHBpbmciLCJjYXRhbG9nIiwicmV2aWV3IiwibWVyY2hhbnQiLCJsb2dpc3RpY3MiLCJncm9jZXJpZXMiLCJldmVudHMiLCJvcmRlciIsImNvbmNpbGlhdG9yIl0sInR2ZXIiOiJ2MiIsIm1lcmNoYW50X3Njb3BlIjpbIjkzNjIwMThhLTZhZTItNDM5Yy05NjhiLWE0MDE3N2EwODVlYTptZXJjaGFudCIsIjkzNjIwMThhLTZhZTItNDM5Yy05NjhiLWE0MDE3N2EwODVlYTpvcmRlciIsIjkzNjIwMThhLTZhZTItNDM5Yy05NjhiLWE0MDE3N2EwODVlYTpjYXRhbG9nIiwiOTM2MjAxOGEtNmFlMi00MzljLTk2OGItYTQwMTc3YTA4NWVhOmNvbmNpbGlhdG9yIiwiOTM2MjAxOGEtNmFlMi00MzljLTk2OGItYTQwMTc3YTA4NWVhOnJldmlldyIsIjkzNjIwMThhLTZhZTItNDM5Yy05NjhiLWE0MDE3N2EwODVlYTpsb2dpc3RpY3MiLCI5MzYyMDE4YS02YWUyLTQzOWMtOTY4Yi1hNDAxNzdhMDg1ZWE6c2hpcHBpbmciLCI5MzYyMDE4YS02YWUyLTQzOWMtOTY4Yi1hNDAxNzdhMDg1ZWE6Z3JvY2VyaWVzIiwiOTM2MjAxOGEtNmFlMi00MzljLTk2OGItYTQwMTc3YTA4NWVhOmV2ZW50cyJdLCJleHAiOjE3MTA4MTM0MzEsImlhdCI6MTcxMDc5MTgzMSwianRpIjoiYmQ4NjJmMDctM2E4MS00ZGU2LWFjOTYtOTcyYjY2YTQ5Y2U3OmNjNDRjZDYxLWNiYjctNDQyNC05MTliLWEzZGY0Mjg3YWVjMSIsIm1lcmNoYW50X3Njb3BlZCI6dHJ1ZX0.Fj1hwMifV2Ch1zlmKbQ9gel9q6gxLcbSW_6ULdndepufJzF_yKhg2OSZPkrXOxvMNHeKVMRHRdi8cA27ZD_qGKVFZRJGi_Qo7v5u25odty3GNflqzwR3r1swjli_ql-u-Ni8IImugLXQsN7XSgm-zbORIo6wxQWcVW-d7hutCwE");
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
