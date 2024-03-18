using ProjetoIntegradorIfood;
using ProjetoIntegradorIfood.ClassesAuxiliares;
Console.ForegroundColor = ConsoleColor.White;

while (true)
{
    try
    {
        //Menu
        Console.WriteLine($"1 - Autorizar App\n");

        switch (Convert.ToInt32(Console.ReadLine()))
        {
            case 1:
                await Ifood.Autorizacao();
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

}