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
using Npgsql;
using System.Xml.Linq;

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
                Console.WriteLine($"refreshToken: {propriedadesAPIWithToken.refreshToken}");

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
                    Console.WriteLine(pullingAtual.orderId);
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

    public static async Task SetPedido()
    {
        Token.TokenDaSessao = "eyJraWQiOiJlZGI4NWY2Mi00ZWY5LTExZTktODY0Ny1kNjYzYmQ4NzNkOTMiLCJ0eXAiOiJKV1QiLCJhbGciOiJSUzUxMiJ9.eyJzdWIiOiJiZDg2MmYwNy0zYTgxLTRkZTYtYWM5Ni05NzJiNjZhNDljZTciLCJvd25lcl9uYW1lIjoiZ3VpbGhlcm1ldGVzdGVzIiwiaXNzIjoiaUZvb2QiLCJjbGllbnRfaWQiOiJjYzQ0Y2Q2MS1jYmI3LTQ0MjQtOTE5Yi1hM2RmNDI4N2FlYzEiLCJhcHBfbmFtZSI6Imd1aWxoZXJtZXRlc3Rlcy10ZXN0ZS1kIiwiYXVkIjpbInNoaXBwaW5nIiwiY2F0YWxvZyIsInJldmlldyIsImZpbmFuY2lhbCIsIm1lcmNoYW50IiwibG9naXN0aWNzIiwiZ3JvY2VyaWVzIiwiZXZlbnRzIiwib3JkZXIiLCJvYXV0aC1zZXJ2ZXIiXSwic2NvcGUiOlsic2hpcHBpbmciLCJjYXRhbG9nIiwicmV2aWV3IiwibWVyY2hhbnQiLCJsb2dpc3RpY3MiLCJncm9jZXJpZXMiLCJldmVudHMiLCJvcmRlciIsImNvbmNpbGlhdG9yIl0sInR2ZXIiOiJ2MiIsIm1lcmNoYW50X3Njb3BlIjpbIjkzNjIwMThhLTZhZTItNDM5Yy05NjhiLWE0MDE3N2EwODVlYTptZXJjaGFudCIsIjkzNjIwMThhLTZhZTItNDM5Yy05NjhiLWE0MDE3N2EwODVlYTpvcmRlciIsIjkzNjIwMThhLTZhZTItNDM5Yy05NjhiLWE0MDE3N2EwODVlYTpjYXRhbG9nIiwiOTM2MjAxOGEtNmFlMi00MzljLTk2OGItYTQwMTc3YTA4NWVhOmNvbmNpbGlhdG9yIiwiOTM2MjAxOGEtNmFlMi00MzljLTk2OGItYTQwMTc3YTA4NWVhOnJldmlldyIsIjkzNjIwMThhLTZhZTItNDM5Yy05NjhiLWE0MDE3N2EwODVlYTpsb2dpc3RpY3MiLCI5MzYyMDE4YS02YWUyLTQzOWMtOTY4Yi1hNDAxNzdhMDg1ZWE6c2hpcHBpbmciLCI5MzYyMDE4YS02YWUyLTQzOWMtOTY4Yi1hNDAxNzdhMDg1ZWE6Z3JvY2VyaWVzIiwiOTM2MjAxOGEtNmFlMi00MzljLTk2OGItYTQwMTc3YTA4NWVhOmV2ZW50cyJdLCJleHAiOjE3MTA4OTQ0OTgsImlhdCI6MTcxMDg3Mjg5OCwianRpIjoiYmQ4NjJmMDctM2E4MS00ZGU2LWFjOTYtOTcyYjY2YTQ5Y2U3OmNjNDRjZDYxLWNiYjctNDQyNC05MTliLWEzZGY0Mjg3YWVjMSIsIm1lcmNoYW50X3Njb3BlZCI6dHJ1ZX0.NHM8aJf30JkHejtBuy5tDsT84PTodUGc-SgKkVAYn85AYm7LiNffarc_rquOBPaY9WnVesR41yBZBzzc0A_Gh3-uzPgEEZ-cOUQO7j6uoKTSIuto92G_v5ImcAvdzbRUOIJ1MCBufhYaulU8pjm51LkoK0uAWnxGFgjIoeoahpc";
        string url = @"https://merchant-api.ifood.com.br/order/v1.0/orders/c9af0c20-8ff4-4d77-8b39-5e5382c4bcd6";
        try
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token.TokenDaSessao);
            HttpResponseMessage response = await client.GetAsync(url);

            if(Convert.ToInt32(response.StatusCode) == 404)
            {
                throw new HttpRequestException("Pedido Não Encontrado");
            }


            string leituraDoPedido = await response.Content.ReadAsStringAsync();
            PedidoCompleto pedidoCompletoTotal = JsonSerializer.Deserialize<PedidoCompleto>(leituraDoPedido);

            pedidocompleto pedidocompletoDB = JsonSerializer.Deserialize<pedidocompleto>(leituraDoPedido);

            //setar o id_pedido de cada objeto relacionado para inserção no banco
           

            Console.WriteLine(pedidoCompletoTotal.delivery.id_pedido);

            //fazer o insert no banco de dados separando todo o pedido em tabelas relacionadas
            using (var db = new ApplicationDbContext())
            {
                //primeiro insere na coluna pedidocomplero
               
                string jsonContent = JsonSerializer.Serialize(pedidocompletoDB);
                // db.pedidocompleto.Add(pedidocompletoDB);

                //segundo insere na coluna delivery relacionando com o id do pedido 
                pedidoCompletoTotal.delivery.id_pedido = pedidoCompletoTotal.id;
                db.delivery.Add(pedidoCompletoTotal.delivery);
                db.SaveChanges();

                //terceito insere na coluna deliveryaddress relacionando com o delivery
                pedidoCompletoTotal.delivery.deliveryAddress.id_delivery = pedidoCompletoTotal.delivery.id;
                db.deliveryaddress.Add(pedidoCompletoTotal.delivery.deliveryAddress);
                db.SaveChanges();

                //quarto insere na coluna coordinates relacionando com o deliveryaddress 
                pedidoCompletoTotal.delivery.deliveryAddress.coordinates.id_DeliveryAddress = pedidoCompletoTotal.delivery.deliveryAddress.id;
                db.coordinates.Add(pedidoCompletoTotal.delivery.deliveryAddress.coordinates);
                db.SaveChanges();

                //quinto insere na tabela merchant 
                pedidoCompletoTotal.merchant.id_pedido = pedidoCompletoTotal.id;
                db.merchant.Add(pedidoCompletoTotal.merchant);
                db.SaveChanges();

                //sexto faz a inserção na tabela create table Customer relacionando com o id do pedido
                pedidoCompletoTotal.customer.id_pedido = pedidoCompletoTotal.id;
                db.customer.Add(pedidoCompletoTotal.customer);

                //setimo faz a inserção na tabela phone relacionando com a coluna id_db da tabela customer
                pedidoCompletoTotal.customer.phone.id_customer = pedidoCompletoTotal.customer.id_customer;
                db.phone.Add(pedidoCompletoTotal.customer.phone);
                db.SaveChanges();
                //TEM QUE CORRIGIR O PQ NÂO RA MUDANDO A SERIAL PRIMARY KEY DO CUSTOMER

                //oitavo insere um array de itens fazerndo um loop para uma inserção de cada vez
                foreach (var items in pedidoCompletoTotal.items)
                {

                }

               await Console.Out.WriteLineAsync($"ID depois de inserido no DB ---->  {pedidoCompletoTotal.delivery.deliveryAddress.id}");
            }



          // await Console.Out.WriteLineAsync(pedidocompletoDB.customer.documentNumber);
           // await Console.Out.WriteLineAsync(leituraDoPedido);
        }
        /*catch (PostgresException ex)// when (ex.MessageText.Contains("duplicar valor da chave viola a restrição de unicidade"))
        {
            await Console.Out.WriteLineAsync(ex.SqlState);
        }*/
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
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
