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
using System.Security.Cryptography.X509Certificates;
using System.IO;

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
        Token.TokenDaSessao = "eyJraWQiOiJlZGI4NWY2Mi00ZWY5LTExZTktODY0Ny1kNjYzYmQ4NzNkOTMiLCJ0eXAiOiJKV1QiLCJhbGciOiJSUzUxMiJ9.eyJzdWIiOiJiZDg2MmYwNy0zYTgxLTRkZTYtYWM5Ni05NzJiNjZhNDljZTciLCJvd25lcl9uYW1lIjoiZ3VpbGhlcm1ldGVzdGVzIiwiaXNzIjoiaUZvb2QiLCJjbGllbnRfaWQiOiJjYzQ0Y2Q2MS1jYmI3LTQ0MjQtOTE5Yi1hM2RmNDI4N2FlYzEiLCJhcHBfbmFtZSI6Imd1aWxoZXJtZXRlc3Rlcy10ZXN0ZS1kIiwiYXVkIjpbInNoaXBwaW5nIiwiY2F0YWxvZyIsInJldmlldyIsImZpbmFuY2lhbCIsIm1lcmNoYW50IiwibG9naXN0aWNzIiwiZ3JvY2VyaWVzIiwiZXZlbnRzIiwib3JkZXIiLCJvYXV0aC1zZXJ2ZXIiXSwic2NvcGUiOlsic2hpcHBpbmciLCJjYXRhbG9nIiwicmV2aWV3IiwibWVyY2hhbnQiLCJsb2dpc3RpY3MiLCJncm9jZXJpZXMiLCJldmVudHMiLCJvcmRlciIsImNvbmNpbGlhdG9yIl0sInR2ZXIiOiJ2MiIsIm1lcmNoYW50X3Njb3BlIjpbIjkzNjIwMThhLTZhZTItNDM5Yy05NjhiLWE0MDE3N2EwODVlYTptZXJjaGFudCIsIjkzNjIwMThhLTZhZTItNDM5Yy05NjhiLWE0MDE3N2EwODVlYTpvcmRlciIsIjkzNjIwMThhLTZhZTItNDM5Yy05NjhiLWE0MDE3N2EwODVlYTpjYXRhbG9nIiwiOTM2MjAxOGEtNmFlMi00MzljLTk2OGItYTQwMTc3YTA4NWVhOmNvbmNpbGlhdG9yIiwiOTM2MjAxOGEtNmFlMi00MzljLTk2OGItYTQwMTc3YTA4NWVhOnJldmlldyIsIjkzNjIwMThhLTZhZTItNDM5Yy05NjhiLWE0MDE3N2EwODVlYTpsb2dpc3RpY3MiLCI5MzYyMDE4YS02YWUyLTQzOWMtOTY4Yi1hNDAxNzdhMDg1ZWE6c2hpcHBpbmciLCI5MzYyMDE4YS02YWUyLTQzOWMtOTY4Yi1hNDAxNzdhMDg1ZWE6Z3JvY2VyaWVzIiwiOTM2MjAxOGEtNmFlMi00MzljLTk2OGItYTQwMTc3YTA4NWVhOmV2ZW50cyJdLCJleHAiOjE3MTEwNzAzNzEsImlhdCI6MTcxMTA0ODc3MSwianRpIjoiYmQ4NjJmMDctM2E4MS00ZGU2LWFjOTYtOTcyYjY2YTQ5Y2U3OmNjNDRjZDYxLWNiYjctNDQyNC05MTliLWEzZGY0Mjg3YWVjMSIsIm1lcmNoYW50X3Njb3BlZCI6dHJ1ZX0.K6i31FGzFFaJmc2nxCN5u3s9pNGaBr_SfsAkQpBj_zY4Ve7BQ_oPX-j5p80rszThN0fw-VPm-teQFwN5T0E7X3itab2cOklALgxHjy0Um5DP0xcV1IW4ywj6E49rtlkAVBUe9KNa2AiVe-zV2gaMZE7x9N9PzBzQJiqZyeaF50I";
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

                    await Console.Out.WriteLineAsync(pullingAtual.fullCode);

                    if (!confereSeJaExiste) //só entra aqui caso o pulling não existir
                    {
                        dbContex.pulling.Add(new data.Pulling() { id = pullingAtual.id });
                        dbContex.SaveChanges();
                        await SetPedido(pullingAtual.orderId, pullingAtual.fullCode);
                    }

                    var order = dbContex.pedidocompleto.Where(p => p.id == pullingAtual.orderId).FirstOrDefault();

                    if (order != null)
                    {
                        order.StatusCode = pullingAtual.fullCode;
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

    public static async Task SetPedido(string orderId, string statusCode = "PLACED")
    {
        // Token.TokenDaSessao = "eyJraWQiOiJlZGI4NWY2Mi00ZWY5LTExZTktODY0Ny1kNjYzYmQ4NzNkOTMiLCJ0eXAiOiJKV1QiLCJhbGciOiJSUzUxMiJ9.eyJzdWIiOiJiZDg2MmYwNy0zYTgxLTRkZTYtYWM5Ni05NzJiNjZhNDljZTciLCJvd25lcl9uYW1lIjoiZ3VpbGhlcm1ldGVzdGVzIiwiaXNzIjoiaUZvb2QiLCJjbGllbnRfaWQiOiJjYzQ0Y2Q2MS1jYmI3LTQ0MjQtOTE5Yi1hM2RmNDI4N2FlYzEiLCJhcHBfbmFtZSI6Imd1aWxoZXJtZXRlc3Rlcy10ZXN0ZS1kIiwiYXVkIjpbInNoaXBwaW5nIiwiY2F0YWxvZyIsInJldmlldyIsImZpbmFuY2lhbCIsIm1lcmNoYW50IiwibG9naXN0aWNzIiwiZ3JvY2VyaWVzIiwiZXZlbnRzIiwib3JkZXIiLCJvYXV0aC1zZXJ2ZXIiXSwic2NvcGUiOlsic2hpcHBpbmciLCJjYXRhbG9nIiwicmV2aWV3IiwibWVyY2hhbnQiLCJsb2dpc3RpY3MiLCJncm9jZXJpZXMiLCJldmVudHMiLCJvcmRlciIsImNvbmNpbGlhdG9yIl0sInR2ZXIiOiJ2MiIsIm1lcmNoYW50X3Njb3BlIjpbIjkzNjIwMThhLTZhZTItNDM5Yy05NjhiLWE0MDE3N2EwODVlYTptZXJjaGFudCIsIjkzNjIwMThhLTZhZTItNDM5Yy05NjhiLWE0MDE3N2EwODVlYTpvcmRlciIsIjkzNjIwMThhLTZhZTItNDM5Yy05NjhiLWE0MDE3N2EwODVlYTpjYXRhbG9nIiwiOTM2MjAxOGEtNmFlMi00MzljLTk2OGItYTQwMTc3YTA4NWVhOmNvbmNpbGlhdG9yIiwiOTM2MjAxOGEtNmFlMi00MzljLTk2OGItYTQwMTc3YTA4NWVhOnJldmlldyIsIjkzNjIwMThhLTZhZTItNDM5Yy05NjhiLWE0MDE3N2EwODVlYTpsb2dpc3RpY3MiLCI5MzYyMDE4YS02YWUyLTQzOWMtOTY4Yi1hNDAxNzdhMDg1ZWE6c2hpcHBpbmciLCI5MzYyMDE4YS02YWUyLTQzOWMtOTY4Yi1hNDAxNzdhMDg1ZWE6Z3JvY2VyaWVzIiwiOTM2MjAxOGEtNmFlMi00MzljLTk2OGItYTQwMTc3YTA4NWVhOmV2ZW50cyJdLCJleHAiOjE3MTA5NTk3NDIsImlhdCI6MTcxMDkzODE0MiwianRpIjoiYmQ4NjJmMDctM2E4MS00ZGU2LWFjOTYtOTcyYjY2YTQ5Y2U3OmNjNDRjZDYxLWNiYjctNDQyNC05MTliLWEzZGY0Mjg3YWVjMSIsIm1lcmNoYW50X3Njb3BlZCI6dHJ1ZX0.NO1_-hgj4h4XeN8bZXIWBKztACHnJZzWYnuvDClluXzjYE6b7sm7wwzbMow7wOHRHgkGjRkHduiUVNAFB7-yULunwX350PLRiIGxuBf_cFUyK1_xvO_M14p59s4yGkntobm6pj57ZH1MxPnbxTw4Rgftqc7eQCW54cfhdebbO7s";
        string url = $"https://merchant-api.ifood.com.br/order/v1.0/orders/{orderId}";
        try
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token.TokenDaSessao);
            HttpResponseMessage response = await client.GetAsync(url);

            if (Convert.ToInt32(response.StatusCode) == 404)
            {
                throw new HttpRequestException("Pedido Não Encontrado");
            }


            string leituraDoPedido = await response.Content.ReadAsStringAsync();
            PedidoCompleto? pedidoCompletoTotal = JsonSerializer.Deserialize<PedidoCompleto>(leituraDoPedido);

            pedidocompleto? pedidocompletoDB = JsonSerializer.Deserialize<pedidocompleto>(leituraDoPedido);

            //setar o id_pedido de cada objeto relacionado para inserção no banco
            //fazer o insert no banco de dados separando todo o pedido em tabelas relacionadas

            using (var db = new ApplicationDbContext())
            {
                //primeiro insere na coluna pedidocompleto (primeiro verifica se o pedido já existe)
                if (db.pedidocompleto.Find(pedidocompletoDB.id) != null)
                {
                    throw new Exception("Pedido já encontrado no banco de dados");
                }

                //caso exista vai ser inserido o pedido no banco de dados
                pedidocompletoDB.StatusCode = statusCode;
                string jsonContent = JsonSerializer.Serialize(pedidocompletoDB);
                db.pedidocompleto.Add(pedidocompletoDB);
                db.SaveChanges();

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

                //sexto faz a inserção na tabela Customer relacionando com o id do pedido (Porém verifica se já existe antes) (Só vai ser inserido o phone também se já não existir o customer)
                var customerUnique_IdSerch = db.customer.Find(pedidoCompletoTotal.customer.id_pedido);
                if (customerUnique_IdSerch == null)
                {
                    pedidoCompletoTotal.customer.id_pedido = pedidoCompletoTotal.id;
                    db.customer.Add(pedidoCompletoTotal.customer);
                    db.SaveChanges();

                    //setimo faz a inserção na tabela phone relacionando com a coluna id_db da tabela customer 
                    pedidoCompletoTotal.customer.phone.id_customer_pedido = pedidoCompletoTotal.customer.id_pedido;
                    db.phone.Add(pedidoCompletoTotal.customer.phone);
                    db.SaveChanges();
                }

                //oitavo insere um array de itens fazerndo um loop para uma inserção de cada vez
                foreach (var item in pedidoCompletoTotal.items)
                {
                    item.id_pedido = pedidocompletoDB.id;
                    db.items.Add(item);
                    db.SaveChanges();
                }

                //nono insere na tabela total relacionando o id do pedido com a coluna id_pedido da tabela total
                pedidoCompletoTotal.total.id_pedido = pedidocompletoDB.id;
                db.total.Add(pedidoCompletoTotal.total);
                db.SaveChanges();

                //decimo insere na tabela Payments relacionando a coluna id_pedido com a tabela pedidototal
                pedidoCompletoTotal.payments.id_pedido = pedidocompletoDB.id;
                db.payments.Add(pedidoCompletoTotal.payments);
                db.SaveChanges();

                //decimo primeiro faz um for e insere na tabela methods relacionando as colunas payments_id com o id do paymant e id_pedido relacionando com o id da coluna pedidocompleto 
                foreach (var method in pedidoCompletoTotal.payments.methods)
                {

                    method.id_pedido = pedidocompletoDB.id;
                    method.payments_id = pedidoCompletoTotal.payments.id;
                    db.methods.Add(method);
                    db.SaveChanges();

                }

                //decimo segundo insere na tabela additionalinfo para depois poder relacionar a tabela metadata com a additionalinfo
                pedidoCompletoTotal.additionalInfo.id_pedido = pedidocompletoDB.id;
                db.additionalinfo.Add(pedidoCompletoTotal.additionalInfo);
                db.SaveChanges();

                //decimo terceiro insere na tabela metadata relacionando com a tabela id do pedidototal e id da addicionalinfo 
                pedidoCompletoTotal.additionalInfo.metadata.id_pedido = pedidocompletoDB.id;
                pedidoCompletoTotal.additionalInfo.metadata.id_additionalinfo = pedidoCompletoTotal.additionalInfo.id;
                db.metadata.Add(pedidoCompletoTotal.additionalInfo.metadata);
                db.SaveChanges();

                Console.ForegroundColor = ConsoleColor.Green;
                await Console.Out.WriteLineAsync("Pedido inserido na base de dados");
                Console.ForegroundColor = ConsoleColor.White;

            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public static async Task GetPedido(string pedido_id)
    {
        string path = @"C:\Users\gui-c\OneDrive\Área de Trabalho\primeiro\testeSeriliazeJson.json";
        try
        {
            using (var db = new ApplicationDbContext())
            {

                var resultado = from a in db.pedidocompleto
                                join b in db.items on a.id equals b.id_pedido
                                join c in db.payments on a.id equals c.id_pedido
                                join d in db.methods on c.id equals d.payments_id
                                join e in db.total on a.id equals e.id_pedido
                                where a.id == pedido_id
                                group new { a, b, c, d, e } by a into grupo
                                select new
                                {
                                    Pedido = grupo.Key,
                                    Items = grupo.Select(x => x.b).ToList(),
                                    Payments = new
                                    {
                                        IdPedido = grupo.Select(p => p.c.id_pedido).FirstOrDefault(),
                                        Prepaid = grupo.Select(p => p.c.prepaid).FirstOrDefault(),
                                        Pending = grupo.Select(p => p.c.pending).FirstOrDefault(),
                                        Methods = grupo.Select(x => x.d).Take(1).ToList(),
                                    },
                                    Total = grupo.Select(p => p.e).FirstOrDefault()


                                };

                string pedidoSerializado = JsonSerializer.Serialize(resultado);

                using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    JsonSerializer.Serialize(stream, resultado);
                }

            }

        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync(ex.Message);
        }


    }

    public static async void SetTimer()//set timer pra fazer o acionamento a cada 30 segundos do pulling
    {

        aTimer = new System.Timers.Timer(10000);
        aTimer.Elapsed += OnTimedEvent;
        aTimer.AutoReset = true;
        aTimer.Enabled = true;

        await Console.Out.WriteLineAsync();
    }

    private static async void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        await Pulling();
    }

}
