using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoIntegradorIfood.ClassesAuxiliares;

internal class Token
{
    public  string? accessToken { get; set; }
    public  string? refreshToken { get; set; }
    public  string? type { get; set; }
    public  int expiresIn { get; set; }

    public static string? TokenDaSessao { get; set; }
    public Token()
    {
        
    }
}
