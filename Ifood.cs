using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ProjetoIntegradorIfood.ClassesAuxiliares;
using static System.Net.WebRequestMethods;

namespace ProjetoIntegradorIfood;

internal class Ifood
{

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
                string jsonContent = await response.Content.ReadAsStringAsync();
                UserCodeReturnFromAPI codesOfVerif = JsonSerializer.Deserialize<UserCodeReturnFromAPI>(jsonContent);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Conecção feita com sucesso, peça para o Cliente colocar o código {codesOfVerif.userCode} no portal do parceito. Assim que ele colocar De ok!");
                Console.ForegroundColor = ConsoleColor.White;

                Console.WriteLine($"userCode: {codesOfVerif.userCode}" +
                    $"\nauthorizationCodeVerifier: {codesOfVerif.authorizationCodeVerifier}" +
                    $"\nverificationUrl: {codesOfVerif.verificationUrl} " +
                    $"\nverificationUrlComplete: {codesOfVerif.verificationUrlComplete}" +
                    $"\nexpiresIn: {codesOfVerif.expiresIn}");

                Console.WriteLine("\n Insira o código gerado pelo ifood!");

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
                    throw new HttpRequestException();
                }

                string jsonObjTokenFromAPI = await responseWithToken.Content.ReadAsStringAsync();
                Token propAPIWithToken = JsonSerializer.Deserialize<Token>(jsonObjTokenFromAPI);

                Token.TokenDaSessao = propAPIWithToken.accessToken;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Acesso Aceito");
                Console.WriteLine($"Token Atual da Sessão: \n {Token.TokenDaSessao}");
                Console.ForegroundColor = ConsoleColor.White;
            }


        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

}
