using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoIntegradorIfood.ClassesAuxiliares;

internal class UserCodeReturnFromAPI
{
    public string? userCode { get; set; }
    public string? authorizationCodeVerifier { get; set; }
    public string? verificationUrl { get; set; }
    public string? verificationUrlComplete { get; set; }
    public int expiresIn { get; set; }

    public UserCodeReturnFromAPI() { }


}
